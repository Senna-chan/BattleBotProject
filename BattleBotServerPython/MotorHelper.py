#!/usr/bin/python
import RPi.GPIO as GPIO
GPIO.setmode(GPIO.BCM)
import math
class MotorHelper:
    
    def __init__(self, PWM, PWMchannel, PF, PB):
        '''
        This function requires the following parameters:
        - pwm: The servohat object.
        - GPIO: The gpio object.
        - PWMchannel: What pin on the Adafruit Servohat is for the PWM?
        - PF: What pin triggers forwards?
        - PB: What pin triggers backwards?
        '''
        self.pwm = PWM
        self.pwmchannel = PWMchannel;
        self.pf = PF
        self.pb = PB
        GPIO.setup(PF, GPIO.OUT)
        GPIO.setup(PB, GPIO.OUT)
    def Forward(self, speed):
        print "Forwards: "+str(self.mapp(speed))
        GPIO.output(self.pf, GPIO.HIGH)
        GPIO.output(self.pb, GPIO.LOW)
        self.pwm.set_pwm(self.pwmchannel, 0, self.mapp(speed))
    def Backward(self, speed):
        speed = speed * -1;
        print "Backwards: "+str(self.mapp(speed))
        GPIO.output(self.pf, GPIO.LOW)
        GPIO.output(self.pb, GPIO.HIGH)
        self.pwm.set_pwm(self.pwmchannel, 0, self.mapp(speed))
    def Stop(self):
        print "STOP"
        GPIO.output(self.pf, GPIO.LOW)
        GPIO.output(self.pb, GPIO.LOW)
        self.pwm.set_pwm(self.pwmchannel, 0, 0)

    def mapp(self,inputt):
        return int(math.floor(inputt * 4096 / 100));