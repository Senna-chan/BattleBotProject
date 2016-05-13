#!/usr/bin/python
import socket, atexit, math, threading
from Adafruit_MotorHAT import Adafruit_MotorHAT, Adafruit_DCMotor
from MotorHelper import MotorHelper
import math
import i2clcd
import RPi.GPIO as GPIO



mh = Adafruit_MotorHAT(0x60, 50)


DC1Helper = MotorHelper(mh.getMotor(1))
DC2Helper = MotorHelper(mh.getMotor(2))
DC3Helper = MotorHelper(mh.getMotor(3))
DC4Helper = MotorHelper(mh.getMotor(4))
def MotorCalc(speed, wheelPos1, wheelPos2, servoX, servoY):
    speed = int(speed);
    wheelPos1 = int(wheelPos1)
    wheelPos2 = int(wheelPos2)
    servoX = int(servoX)
    servoY = int(servoY)
    DC1motorSpeed = 0
    DC2motorSpeed = 0
    if (speed > 0):
        DC1motorSpeed = math.floor(speed*2.55 - wheelPos1*2.55);
        DC2motorSpeed = math.floor(speed*2.55 - wheelPos2*2.55);
    elif (speed < 0):
        DC1motorSpeed = math.floor(speed*2.55 + wheelPos1*2.55);
        DC2motorSpeed = math.floor(speed*2.55 + wheelPos2*2.55);
    if (speed == 0): 
        if (wheelPos1 > 0):
            DC1motorSpeed = math.floor(wheelPos1*2.55*-1 + 1);
            DC2motorSpeed = math.floor(wheelPos1*2.55);
        if (wheelPos2 > 0):
            DC1motorSpeed = math.floor(wheelPos2*2.55);
            DC2motorSpeed = math.floor(wheelPos2*2.55*-1 + 1);
           
    DC1motorSpeed = int(DC1motorSpeed)
    DC2motorSpeed = int(DC2motorSpeed)
    if (DC1motorSpeed < 0): DC3Helper.Backward(DC1motorSpeed);
    if (DC1motorSpeed > 0): DC3Helper.Forward(DC1motorSpeed);
    if (DC1motorSpeed == 0): DC3Helper.Stop();
    if (DC2motorSpeed < 0): DC2Helper.Backward(DC2motorSpeed);
    if (DC2motorSpeed > 0): DC2Helper.Forward(DC2motorSpeed);
    if (DC2motorSpeed == 0): DC2Helper.Stop();

    MovePanTilt(servoX, servoY);

    return [0, DC1motorSpeed, DC2motorSpeed];


def MovePanTilt(PanTiltX, PanTiltY):
    PanX = GetServoValue(PanTiltX);
    PanY = GetServoValue(PanTiltY);
    mh._pwm.setPWM(15, 0, PanX);
    mh._pwm.setPWM(14, 0, PanY);


def GetServoValue(inputt):
    return map(inputt, -100, 100, 104, 521);

def map(inputt, in_min, in_max, out_min, out_max):
    return (inputt - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

def StopMotors():
    DC1Helper.Stop()
    DC2Helper.Stop()
    DC3Helper.Stop()
    DC4Helper.Stop()











speed = 0
wheelpos1 = 0
wheelpos2 = 0
servoX = 0
servoY = 0
parameters = ""
ctype = ""
command = ""
clienttype = ""
running = True;
clientConnected = False;
clientSendedHandshake = False;
s = socket.socket()
s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
def cleanup():
   print "Cleaning up the stuff for shutdown"
   s.close()
   StopMotors()

atexit.register(cleanup)
s.bind(('', 20010))

s.listen(5)                 # Now wait for client connection.
print "Awaiting senpai"
while True:
    c, addr = s.accept()     # Establish connection with client.
    print "Senpai noticed me from: "+str(addr[0])+".\nNow waiting for data"
    clientConnected = True;
    while clientConnected:
        data = c.recv(30)
        data = data.lower()
        data = data.replace("\r\n","")
        if len(data) == 0: continue
        typcom = data.split(':')
        if(len(typcom) == 2 or len(typcom) == 3):
            print("Recieved a normal command: %s",data)
            ctype = typcom[0]
            command = typcom[1]
            if(len(typcom) == 3):
                parameters = typcom[2]

            if(ctype.startswith("client") and len(typcom) == 3):
                clienttype = command;
                clientaction = parameters
                if(clientaction == "connected"):
                    clientSendedHandshake = True;
                if(clienttype == "pc"):
                    if(clientaction == "connected"):
                        print "PC just connected"
                        
                    elif(clientaction == "disconnected"):
                        print "PC just disconnected"
                        clientConnected = False;
                if(clienttype == "psp"):
                    if(clientaction == "connected"):
                        print "PSP just connected"
                    elif(clientaction == "disconnected"):
                        print "PSP just disconnected"
                        clientConnected = False;

            if(clientSendedHandshake):
                if(ctype.startswith("dc") and len(typcom) == 3):
                    motorcommands = command.split(',')
                    servocommands = parameters.split(',')
                    speed = motorcommands[0]
                    wheelpos1 = motorcommands[1]
                    wheelpos2 = motorcommands[2]
                    servoX = servocommands[0]
                    servoY = servocommands[1]
                    if(len(motorcommands) != 3 or len(servocommands) != 2):
                        print("The recieved data is not a valid drive command. Length doesn't match")
                        continue
                    MotorCalc(speed, wheelpos1, wheelpos2, servoX, servoY)
            
            else:
                c.send("Please use a handshake when you are connecting or else you are threated as a offsider")
            if(ctype.startswith("c")):
                if(command == "exit"):
                    clientConnected = False;
                    running = False;
                    cleanup()

        else:
            print "Command: "+data+" is not a valid command"
            #c.send("Command: "+data+" is not a valid command")
        if(not clientConnected): 
            print "Client has disconnected"
            c.close()
            break
    
    if(not running):
        break;

cleanup()