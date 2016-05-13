#!/usr/bin/python
from Adafruit_MotorHAT import Adafruit_MotorHAT,Adafruit_DCMotor

class MotorHelper:
    
    def __init__(self, DCMotor):
        self.motor = DCMotor;

    def Forward(self, speed):
        self.motor.setSpeed(speed)
        self.motor.run(Adafruit_MotorHAT.FORWARD)

    def Backward(self, speed):
        self.motor.setSpeed(speed *-1)
        self.motor.run(Adafruit_MotorHAT.BACKWARD)

    def Stop(self):
        self.motor.setSpeed(0)
        self.motor.run(Adafruit_MotorHAT.RELEASE)

    
