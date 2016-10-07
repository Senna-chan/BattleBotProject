#!/usr/bin/python
import socket, atexit, math, threading, sys, os, time, subprocess
from MotorHelper import MotorHelper
import math
import i2clcd
import RPi.GPIO as GPIO
GPIO.setmode(GPIO.BCM)
import netifaces as ni
import Adafruit_PCA9685
from time import sleep
#from tenDOFclass import *
import threading


os.nice(20) # High priority for better performance
servo = Adafruit_PCA9685.PCA9685(address=0x40)
motor = Adafruit_PCA9685.PCA9685(address=0x60)
motor.set_pwm_freq(1000)
servo.set_pwm_freq(60)
DC1Helper = MotorHelper(motor, 0, 16, 19)
DC2Helper = MotorHelper(motor, 1, 20, 21)
#tenDOF = tenDOFclass();
WiFiStrength = 0

isStepping = False;
isGettingWiFiStrength = False;
# Stepper Motor part
ControllPin = [22,23,24,25]
shootled = 6
GPIO.setup(shootled, GPIO.OUT)
GPIO.output(shootled, False)

for pin in ControllPin:
    GPIO.setup(pin, GPIO.OUT)
    GPIO.output(pin, False)

seq = [ [1,0,0,0],
        [1,1,0,0],
        [0,1,0,0],
        [0,1,1,0],
        [0,0,1,0],
        [0,0,1,1],
        [0,0,0,1],
        [1,0,0,1] ]

def DoSteps(amount = 4096, shoot = False):
    global isStepping
    if(not isStepping):
        isStepping = True;
        thread = threading.Thread(target=runStepper, args=([amount, shoot]))
        thread.start()                                  # Start the execution
    else:
        print "Can't do that. Stepper Motor is busy"
        
def runStepper(amount = 4096, shoot = False):
    global isStepping
    times = 0
    if(shoot):
        GPIO.output(shootled, True)
    while True:
        for halfstep in range(8):
            if(times == amount):
                if(shoot):
                    GPIO.output(shootled, False)
                isStepping = False;
                return
            #Here we switch all the output pins to the correct state
            for pin in range(4):
                GPIO.output(ControllPin[pin], seq[halfstep][pin])
            times = times + 1

            sleep(0.001)


def get_wifi_strength():
    global isGettingWiFiStrength
    if(not isGettingWiFiStrength):
        isGettingWiFiStrength = True;
        thread = threading.Thread(target=runGetWiFiStrength)
        thread.start()                                  # Start the execution
        
def runGetWiFiStrength():
    global WiFiStrength, isGettingWiFiStrength
    cmd = subprocess.Popen('iwconfig wlan0', shell=True, stdout=subprocess.PIPE)
    for line in cmd.stdout:
        if 'Link Quality' in line:
            line = line.lstrip(' ')
            line = line[13:18]
            quality = line.split('/')
            isGettingWiFiStrength = False
            WiFiStrength = str(int(round(float(quality[0]) / float(quality[1]) * 100)))

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
            DC1motorSpeed = wheelPos1 * -1
            DC2motorSpeed = wheelPos1
        if (wheelPos2 > 0):
            DC1motorSpeed = wheelPos2
            DC2motorSpeed = wheelPos2 * -1
           

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
    servo.set_pwm(14, 0, PanX);
    servo.set_pwm(15, 0, PanY);


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
   print "Motors stopped\nStopping GPIO"
   GPIO.cleanup()
   print "GPIO stopped\nEverything cleaned up\nGoodbye"

atexit.register(cleanup)


s.bind(('', 20010))
os.system("clear")
ip = ni.ifaddresses('wlan0')[2][0]['addr']
print "Battlebot server ip: "+ip
print "Starting Battlebot Server"
print "Settings Servo's on neutral"
servo.set_pwm(8, 0, GetServoValue(0));
servo.set_pwm(9, 0, GetServoValue(0));

if(len(sys.argv)>1):
    calalt = sys.argv[1];
    calacc = sys.argv[2];
    calmag = sys.argv[3];
    calgyro = sys.argv[4];
    caltemp = sys.argv[5]
else:
    calalt = True
    calacc = False
    calmag = False
    calgyro = True
    caltemp = False
print "Calibrating on the following:"
print "Temperature          "+str(caltemp)
print "Altitude:            "+str(calalt )
print "Accelerometer:       "+str(calacc )
print "Magnometer/Compass:  "+str(calmag )
print "Gyroscoop:           "+str(calgyro)
try:
    #tenDOF.Calibrate(calalt, calacc, calmag, calgyro, caltemp)
except:
    print ""
print "Calibrated"

print "Awaiting senpai"
while True:
    start = time.time()
    get_wifi_strength()
    print WiFiStrength
    data, addr = s.recvfrom(30)
    print ""
    print time.time()-start
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
                s.sendto("YouConnected", addr) # Super crude handshaking but it works (kinda)
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
            if(WiFiStrength >= 90):
                print time.time()-start
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
                print time.time()-start
            elif(WiFiStrength < 90):
                print "low wifi"
                data="E:1" # Error code 1. Low signal strength
                s.sendto(data, addr)
                
        if(ctype.startswith("c")):
            if(command == "calibrate"):
               if(len(typcom)==3):
                   CalStuff = parameters.split(',')
                   Calibrate(CalStuff[0],CalStuff[1],CalStuff[2],CalStuff[3],CalStuff[4])
            if(command == "reset10dof"):
                print "Nope"
                #tenDOF.reset10dof()

            if(command == "reload"):
                DoSteps(512, False)
            if(command == "shoot"):
                DoSteps(512, True)
            if(command == "exit"):
                clientConnected = False;
                running = False;
                cleanup()
        #try:
        #    data = tenDOF.returnAHRSGD()
        #except:
        #    data = "AHRS:0,0,0,0,0";
        data = "AHRS:0,0,0,0,0";
        s.sendto(data, addr)
        time.sleep(0.005)
        data = "GPS:lat,long,alt,speed,heading"
        s.sendto(data, addr)
        time.sleep(0.005)
        data = "MISC:c1,c2,c3,{}".format(WiFiStrength)
        s.sendto(data, addr)
        print time.time()-start
    else:
        print "Command: "+data+" is not a valid command"
        MotorCalc(0,0,0,servoX, servoY)


cleanup()