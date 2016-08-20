import math
class AHRS():
    def getOrientation(magVals, accVals):

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
  
        roll = atan2(accy, accz);

        # pitch: Rotation around the Y-axis. -180 <= roll <= 180                                         
        # a positive pitch angle is defined to be a clockwise rotation about the positive Y-axis         
        #                                                                                                
        #                                 -x                                                             
        #      pitch = atan(-------------------------------)                                             
        #                    y * sin(roll) + z * cos(roll)                                               
        #                                                                                                
        # where:  x, y, z are returned value from accelerometer sensor                                   
        if (accy * sin(roll) + accz * cos(roll) == 0):
            if(accx > 0):
                pitch = PI_F / 2
            else:
                pitch = -PI_F / 2
        else:
            pitch = atan(-accx / (accy * sin(roll) + accz * cos(roll)));

        # heading: Rotation around the Z-axis. -180 <= roll <= 180                                       
        # a positive heading angle is defined to be a clockwise rotation about the positive Z-axis       
        #                                                                                                
        #                                       z * sin(roll) - y * cos(roll)                            
        #   heading = atan2(--------------------------------------------------------------------------)  
        #                    x * cos(pitch) + y * sin(pitch) * sin(roll) + z * sin(pitch) * cos(roll))   
        #                                                                                                
        # where:  x, y, z are returned value from magnetometer sensor                                    
        heading = atan2(magz * sin(roll) - magy * cos(roll), 
                        magx * cos(pitch) + 
                        magy * sin(pitch) * sin(roll) + 
                        magz * sin(pitch) * cos(roll))


        # Convert angular data to degree 
        roll = roll * 180 / PI_F;
        pitch = pitch * 180 / PI_F;
        heading = heading * 180 / PI_F;

        return [roll,pitch,heading]