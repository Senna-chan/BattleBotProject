#!/usr/bin/python
import Adafruit_BMP.BMP085 as BMP085
from Adafruit_LSM303 import *
from L3GD20 import *
import math
class tenDOFclass(object):
    alt0 = 0
    accX0 = 0
    accY0 = 0
    accZ0 = 0
    magX0 = 0
    magY0 = 0
    magZ0 = 0
    gyroX0 = 0
    gyroY0 = 0
    gyroZ0 = 0
    temp0 = 0

    alt = 0
    accX = 0
    accY = 0
    accZ = 0
    magX = 0
    magY = 0
    magZ = 0
    gyroX = 0
    gyroY = 0
    gyroZ = 0
    temp = 0
    roll = 0
    pitch = 0
    heading = 0

    def __init__(self):
        self.L3GS20Sensor = L3GD20(L3GD20.L3DS20_RANGE_2000DPS_2)
        self.LSM303Sensor = LSM303(True) 
        self.BMP085Sensor = BMP085.BMP085(mode=BMP085.BMP085_ULTRAHIGHRES)

    def reset10dof(self):
        self.L3GS20Sensor = L3GD20(L3GD20.L3DS20_RANGE_2000DPS_2)
        self.LSM303Sensor = LSM303(True) 
        self.BMP085Sensor = BMP085.BMP085(mode=BMP085.BMP085_ULTRAHIGHRES)
    def Get10DOFValues(self):
        LSM303vals = self.LSM303Sensor.read()
        L3GD20vals = self.L3GS20Sensor.read()
        orientation = self.getOrientation()
        self.roll = orientation[0];
        self.pitch = orientation[1]
        self.heading = orientation[2];
        self.temp =  self.BMP085Sensor.read_temperature()   + self.temp0
        self.alt =   self.BMP085Sensor.read_altitude()      + self.alt0
        self.accX =  LSM303vals[0][0]                       + self.accX0
        self.accY =  LSM303vals[0][1]                       + self.accY0
        self.accZ =  LSM303vals[0][2]                       + self.accZ0
        self.magX =  LSM303vals[1][0]                       + self.magX0
        self.magY =  LSM303vals[1][1]                       + self.magY0
        self.magZ =  LSM303vals[1][2]                       + self.magZ0
        self.gyroX = L3GD20vals[0]                          + self.gyroX0
        self.gyroY = L3GD20vals[1]                          + self.gyroY0
        self.gyroZ = L3GD20vals[2]                          + self.gyroZ0

    def Calibrate(self, alt00 = False, acc00 = False, mag00 = False, gyro00 = True, temp00 = False):
        LSM303vals = self.LSM303Sensor.read()
        L3GD20vals = self.L3GS20Sensor.read()
        self.temp = self.BMP085Sensor.read_temperature()
        self.alt = self.BMP085Sensor.read_altitude()
        self.accX = LSM303vals[0][0]
        self.accY = LSM303vals[0][1]
        self.accZ = LSM303vals[0][2]
        self.magX = LSM303vals[1][0]
        self.magY = LSM303vals[1][1]
        self.magZ = LSM303vals[1][2]
        self.gyroX = L3GD20vals[0]
        self.gyroY = L3GD20vals[1]
        self.gyroZ = L3GD20vals[2]
        if(alt00):
            self.alt0 = (self.alt * -1)
        if(acc00):
            self.accX0 = self.accX * -1
            self.accY0 = self.accY * -1
            self.accZ0 = self.accZ * -1
        if(mag00):
            self.magX0 = self.magX0 * -1
            self.magY0 = self.magY0 * -1
            self.magZ0 = self.magZ0 * -1
        if(gyro00):
            self.gyroX0 = self.gyroX * -1
            self.gyroY0 = self.gyroY * -1
            self.gyroZ0 = self.gyroZ * -1
        if(temp00):
            self.temp0 = self.temp * -1

    def getOrientation(self):
        LSM303vals = self.LSM303Sensor.read()
        magVals = LSM303vals[1]
        accVals = LSM303vals[0]

        PI_F = 3.14159265

        # roll: Rotation around the X-axis. -180 <= roll <= 180                                          
        # a positive roll angle is defined to be a clockwise rotation about the positive X-axis          
        #                                                                                                
        #                    y                                                                           
        #      roll = atan2(---)                                                                         
        #                    z                                                                           
        #                                                                                                
        # where:  y, z are returned value from accelerometer sensor         
      
        accx = (accVals[0] - -0.2596642733160819) / 8.512907048615071;
        accy = (accVals[1] - -0.1616872080865025) / 10.71593260608176;
        accz = (accVals[2] - -0.10080992349675334)/ 11.770471429389648;

        magx = (magVals[0] - -2.234341049193788) / 40.738412695094574;
        magy = (magVals[1] - -7.382413060772603) / 42.64382108924524;
        magz = (magVals[2] - -9.028240987770872) / 39.34425527443214;
  
        roll = math.atan2(accy, accz);

        # pitch: Rotation around the Y-axis. -180 <= roll <= 180                                         
        # a positive pitch angle is defined to be a clockwise rotation about the positive Y-axis         
        #                                                                                                
        #                                 -x                                                             
        #      pitch = atan(-------------------------------)                                             
        #                    y * sin(roll) + z * cos(roll)                                               
        #                                                                                                
        # where:  x, y, z are returned value from accelerometer sensor                                   
        if (accy * math.sin(roll) + accz * math.cos(roll) == 0):
            if(accx > 0):
                pitch = PI_F / 2
            else:
                pitch = -PI_F / 2
        else:
            pitch = math.atan(-accx / (accy * math.sin(roll) + accz * math.cos(roll)));

        # heading: Rotation around the Z-axis. -180 <= roll <= 180                                       
        # a positive heading angle is defined to be a clockwise rotation about the positive Z-axis       
        #                                                                                                
        #                                       z * sin(roll) - y * cos(roll)                            
        #   heading = atan2(--------------------------------------------------------------------------)  
        #                    x * cos(pitch) + y * sin(pitch) * sin(roll) + z * sin(pitch) * cos(roll))   
        #                                                                                                
        # where:  x, y, z are returned value from magnetometer sensor                                    
        heading = math.atan2(magz * math.sin(roll) - magy * math.cos(roll), magx * math.cos(pitch) + 
                        magy * math.sin(pitch) * math.sin(roll) + 
                        magz * math.sin(pitch) * math.cos(roll))


        # Convert angular data to degree 
        roll = roll * 180 / PI_F;
        pitch = pitch * 180 / PI_F;
        heading = heading * 180 / PI_F;

        return [roll,pitch,heading]

    def returnAHRS(self):
        return "AHRS:{0:0.2f},{1:0.2f},{2:0.2f},{3:0.2f},{4:0.2f}".format(self.temp,self.alt,self.roll,self.pitch,self.heading)
