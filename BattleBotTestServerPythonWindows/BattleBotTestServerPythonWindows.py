#!/usr/bin/python
import socket, atexit, math, threading, sys, os, time, random
from MotorHelper import MotorHelper
import math
import netifaces as ni
from time import sleep
import threading

import win32api,win32process,win32con

pid = win32api.GetCurrentProcessId()
handle = win32api.OpenProcess(win32con.PROCESS_ALL_ACCESS, True, pid)
win32process.SetPriorityClass(handle, win32process.REALTIME_PRIORITY_CLASS) 

current_milli_time = lambda: int(round(time.time() * 1000))

isStepping = False;
DC1Helper = MotorHelper(1)
DC2Helper = MotorHelper(2)
def DoSteps(amount = 4096, shoot = False):
    global isStepping
    if(not isStepping):
        isStepping = True;
        thread = threading.Thread(target=run, args=([amount, shoot]))
        thread.start()                                  # Start the execution
    else:
        print "Can't do that. Stepper Motor is busy"
def run(amount = 4096, shoot = False):
    global isStepping
    times = 0
    if(shoot):
        print "PEW PEW"
    while True:
        for halfstep in range(8):
            if(times == amount):
                isStepping = False;
                return
            #Here we switch all the output pins to the correct state
            for pin in range(4):
                GPIO.output(ControllPin[pin], seq[halfstep][pin])
            times = times + 1

            sleep(0.001)


def MotorCalc(speed, wheelPos1, wheelPos2, servoX, servoY):
    speed = int(speed);
    wheelPos1 = int(wheelPos1)
    wheelPos2 = int(wheelPos2)
    servoX = int(servoX)
    servoY = int(servoY)
    DC1motorSpeed = 0
    DC2motorSpeed = 0
    if (speed > 0):
        DC1motorSpeed = speed - wheelPos1
        DC2motorSpeed = speed - wheelPos2
    elif (speed < 0):
        DC1motorSpeed = speed + wheelPos1
        DC2motorSpeed = speed + wheelPos2
    if (speed == 0): 
        if (wheelPos1 > 0):
            DC1motorSpeed = wheelPos1 * -1 + 1
            DC2motorSpeed = wheelPos1
        if (wheelPos2 > 0):
            DC1motorSpeed = wheelPos2
            DC2motorSpeed = wheelPos2 * -1 + 1
           

    if (DC1motorSpeed < 0): DC1Helper.Backward(DC1motorSpeed);
    if (DC1motorSpeed > 0): DC1Helper.Forward(DC1motorSpeed);
    if (DC1motorSpeed == 0): DC1Helper.Stop();
    if (DC2motorSpeed < 0): DC2Helper.Backward(DC2motorSpeed);
    if (DC2motorSpeed > 0): DC2Helper.Forward(DC2motorSpeed);
    if (DC2motorSpeed == 0): DC2Helper.Stop();

    MovePanTilt(servoX, servoY);

    return [0, DC1motorSpeed, DC2motorSpeed];


def MovePanTilt(PanTiltX, PanTiltY):
    PanX = GetServoValue(PanTiltX);
    PanY = GetServoValue(PanTiltY);


def GetServoValue(inputt):
    return map(inputt, -100, 100, 104, 521);

def map(inputt, in_min, in_max, out_min, out_max):
    return (inputt - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

def StopMotors():
    DC1Helper.Stop()
    DC2Helper.Stop()





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
s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

def checkHandshake():
    """
        This checks if the client has sended the connect stuff. 
        If it did then return true
        Else return false and send an error back
        Error: Please use a handshake when you are connecting or else you are threated as a offsider
    """
    if(clientSendedHandshake):
        return True;
    else:
        return False;
def cleanup():
   print "Cleaning up the stuff for shutdown"
   print "Closing Socket"
   s.close()
   print "Socket closed\nStopping motors"
   StopMotors()
   print "GPIO stopped\nEverything cleaned up\nGoodbye"

atexit.register(cleanup)


s.bind(('', 20010))
os.system("clear")
print "Starting Battlebot Server"
print "Awaiting senpai"
while True:
    
    data, addr = s.recvfrom(30)
    print ""
    start = time.time()
    #tenDOF.Get10DOFValues()
    data = data.lower()
    data = data.replace("\r","")
    data = data.replace("\n","")
    if len(data) == 0: continue
    typcom = data.split(':')
    if(len(typcom) == 2 or len(typcom) == 3):
        print("Recieved a normal command: "+data)
        ctype = typcom[0]
        command = typcom[1]
        if(len(typcom) == 3):
            parameters = typcom[2]

        if(ctype.startswith("client") and len(typcom) == 3):
            clienttype = command;
            clientaction = parameters
            if(clientaction == "connected"):
                clientSendedHandshake = True;
                clientConnected = True;
                s.sendto("YouConnected", addr)
                print "Senpai noticed me from: "+str(addr[0])

            if(clientaction == "disconnected"):
                StopMotors()
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

        if(ctype.startswith("dc") and len(typcom) == 3):
            print (time.time()-start) * 1000
            checkHandshake()
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
            print (time.time()-start) * 1000
        if(ctype.startswith("c")):
            if(command == "calibrate"):
               if(len(typcom)==3):
                   CalStuff = parameters.split(',')
                   Calibrate(CalStuff[0],CalStuff[1],CalStuff[2],CalStuff[3],CalStuff[4])
            if(command == "reset10dof"):
                print "Reset 10 DOF"

            if(command == "reload"):
                DoSteps(512, False)
            if(command == "shoot"):
                DoSteps(512, True)
            if(command == "exit"):
                clientConnected = False;
                running = False;
                cleanup()
        
        data = "AHRS:{0:0.2f},{1:0.2f},{2:0.2f},{3:0.2f},{4:0.2f}".format(random.uniform(20, 30),random.uniform(0, 20),random.uniform(-180, 180),random.uniform(-180, 180),random.uniform(-180, 180))
        s.sendto(data, addr)
        time.sleep(0.005)
        data = "gps:{0:0.15f},{1:0.15f},{2:0.2f},{3:0.2f},{4:0.2f}".format(random.uniform(5, 5.5),random.uniform(40, 40.5),random.uniform(0, 30),random.uniform(0,20),random.uniform(-180, 180))
        s.sendto(data, addr)
        print (time.time()-start) * 1000
    else:
        print "Command: "+data+" is not a valid command"
        MotorCalc(0,0,0,servoX, servoY)


cleanup()