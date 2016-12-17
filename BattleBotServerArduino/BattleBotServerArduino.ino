/*
 Name:		BattleBotServerArduino.ino
 Created:	10/9/2016 3:08:15 PM
 Author:	Natsuki
*/

// the setup function runs once when you press reset or power the board
#include <L3G/L3G.h>
#include <Adafruit_LSM303DLHC/Adafruit_LSM303.h>
#include <Adafruit_BMP085.h>
#include <Servo.h>
#include <DualVNH5019MotorShield.h>
#include <Wire.h>
#include <ThreadController.h>
#include <TimerOne.h>
#include "ESPCOMBYTES.h"
#include "DualVNH5019MotorShield/DualVNH5019MotorShield.h"

#define ESPSTATUSLED 24

#define ESPRESETPIN 2 // This connects to the ESP reset pin incase it won't work

byte DC1MotorSpeedSerial, DC2MotorSpeedSerial, panSerial, tiltSerial;
int DC1MotorSpeed, DC2MotorSpeed, pan, tilt;

DualVNH5019MotorShield Motors = DualVNH5019MotorShield(32, 34, 30, A0, 33, 35, 31, A1);
Servo panServo, tiltServo;
Adafruit_BMP085 bmp		= Adafruit_BMP085();
Adafruit_LSM303 lsm303	= Adafruit_LSM303();
L3G				l3g		= L3G();

float seaLevelPressure, altitude, temperature;
byte M1Current, M2Current;
ThreadController threadController = ThreadController();
Thread dofthread = Thread();
Thread connectionThread = Thread();
volatile int timeSinceLastReceivedMotorData = 0;
unsigned long previousMillis = millis();
unsigned long currentMillis = millis();
bool clientConnected;

void timerCallback() {
	threadController.run();
}

void getDofData()
{
	noInterrupts();
	if (clientConnected) {
		altitude = bmp.readAltitude(seaLevelPressure);
		temperature = bmp.readTemperature();
	}
	if(false)
	{

	}
	interrupts();
}

void connectionHandler() // This is to make sure the tank doesn't go haywire when the ESP stops working
{
	if (currentMillis - previousMillis > 50) Motors.setBrakes(0, 0); // In case of connection troubles. We do not reset the esp YET
	if (currentMillis - previousMillis > 500) {
		digitalWrite(ESPRESETPIN, HIGH);
		delay(2);
		digitalWrite(ESPRESETPIN, LOW);
	}
}


void setup() {
	Serial.begin(115200);
	Serial3.begin(115200);

	bmp.begin();
	lsm303.begin();
	l3g.init();
	l3g.enableDefault();
	float rawAltitude = bmp.readAltitude();
	seaLevelPressure = bmp.readSealevelPressure(rawAltitude);
	// Threading things
	dofthread.onRun(getDofData);
	dofthread.setInterval(250);
	threadController.add(&dofthread);
	connectionThread.onRun(connectionHandler);
	connectionThread.setInterval(30);
	threadController.add(&connectionThread);
	Timer1.initialize(20000);
	Timer1.attachInterrupt(timerCallback);
	Timer1.start();
	// Threading things done
	panServo.attach(7);
	tiltServo.attach(8);
	Motors.init();
	Wire.begin();
	Serial.println("Ready");
}
void MoveMotors()
{
	M1Current = Motors.getM1CurrentMilliamps() / 1000;
	M2Current = Motors.getM2CurrentMilliamps() / 1000;
	if (!Motors.getM1Fault()) {// Check if there is a fault in the motor driver on M1
		if (M1Current < 16) { // Check if the current isn't to high
			if (DC1MotorSpeed != 0) {
				Motors.setM1Speed(DC1MotorSpeed);
			}
			else
			{
				Motors.setM1Brake(0);
			}
		}
		else
		{
			Serial.println("Warning: Motor 1 current is to high");
			Serial3.write(ESPMOTORDIAG);
			Serial3.write(0x01); // Motor 1
			Serial3.write(0x01); // Current to high
			Serial3.write(ESPMESSAGEEND);
			Motors.setBrakes(0, 0);
		}
	}
	else
	{
		Serial.println("Warning: Motor 1 has a fault");
		Serial3.write(ESPMOTORDIAG);
		Serial3.write(0x01); // Motor 1
		Serial3.write(0x02); // Fault
		Serial3.write(ESPMESSAGEEND);
		Motors.setBrakes(0, 0);
	}
	if (!Motors.getM2Fault()) {// Check if there is a fault in the motor driver on M1
		if (M2Current < 16) { // Check if the current isn't to high
			if (DC2MotorSpeed != 0) {
				Motors.setM2Speed(DC2MotorSpeed);
			}
			else
			{
				Motors.setM2Brake(0);
			}
		}
		else
		{
			Serial.println("Warning: Motor 2 current is to high");

			Serial3.write(ESPMOTORDIAG);
			Serial3.write(0x02); // Motor 1
			Serial3.write(0x01); // Current to high
			Serial3.write(ESPMESSAGEEND);
			Motors.setBrakes(0, 0);
		}
	}
	else
	{
		Serial.println("Warning: Motor 2 has a fault");
		Serial3.write(ESPMOTORDIAG);
		Serial3.write(0x02); // Motor 1
		Serial3.write(0x02); // Fault
		Serial3.write(ESPMESSAGEEND);
		Motors.setBrakes(0, 0);
	}
}

void HandleESPData()
{
	byte c = Serial3.read();
	if (c == ESPMOTOR) // Motor
	{
		DC1MotorSpeedSerial = Serial3.read();
		DC2MotorSpeedSerial = Serial3.read();
		byte endCommand = Serial3.read();
		Serial.print("DC1Serial: ");
		Serial.println(DC1MotorSpeedSerial);
		Serial.print("DC2Serial: ");
		Serial.println(DC2MotorSpeedSerial);
		Serial.print("EndCommand: ");
		Serial.println(endCommand);
		if (endCommand == ESPMESSAGEEND)
		{
			DC1MotorSpeed = map(DC1MotorSpeedSerial, 0, 200, -400, 400);
			DC2MotorSpeed = map(DC2MotorSpeedSerial, 0, 200, -400, 400);
			Serial.print("DC1: ");
			Serial.println(DC1MotorSpeed);
			Serial.print("DC2: ");
			Serial.println(DC2MotorSpeed);
			MoveMotors();
			previousMillis = millis();
		}
		else
		{// Command not recieved
			Serial.println("Motor data is not valid");
		}
	}
	if(c == ESPSERVO) // Servo
	{
		panSerial = Serial3.read();
		tiltSerial = Serial3.read();
		if (Serial3.read() == ESPMESSAGEEND) {
			pan = map(panSerial, 0, 200, 0, 180);
			tilt = map(tiltSerial, 0, 200, 0, 180);
			panServo.write(pan);
			tiltServo.write(tilt);
		}
		else
		{
			Serial.write("Error getting the pantilt values");
		}
	}
	if (c == ESPCLIENT)
	{
		c = Serial.read();
		if (c == 0x02)
		{
			if (Serial.read() == ESPMESSAGEEND) {
				Serial.println("A client has connected");
				clientConnected = true;
			}
		}
		else if (c == 0x01) {
			if (Serial.read() == ESPMESSAGEEND) {
				Serial.println("ESP is ready for data");
			}
		}
		else if (c == 0x03) {
			if (Serial.read() == ESPMESSAGEEND) {
				Serial.println("ESP lost connection to the client either with or without normal disconnect");
				clientConnected = false;
				Motors.setBrakes(0, 0);
			}
		}
	}
	Serial3.write(ESPMOTORSTAT);
	Serial3.write(M1Current);
	Serial3.write(M2Current);
	Serial3.write(ESPMESSAGEEND);
}

// the loop function runs over and over again until power down or reset
void loop() {
	currentMillis = millis();
	while (Serial3.available()) {
		HandleESPData();
	}
}