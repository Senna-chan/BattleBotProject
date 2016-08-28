#!/usr/bin/python
import math
class MotorHelper:
    
    def __init__(self, motorNumber):
        self.motor = motorNumber
    def Forward(self, speed):
        print "Motor "+str(self.motor)+" Forwards: "+str(self.mapp(speed))
    def Backward(self, speed):
        speed = speed * -1;
        print "Motor "+str(self.motor)+" Backwards: "+str(self.mapp(speed))
    def Stop(self):
        print "Motor "+str(self.motor)+" STOP"

    def mapp(self,inputt):
        return int(math.floor(inputt * 4096 / 100));