/*
 Name:		BattleBotServerArduino.ino
 Created:	10/9/2016 3:08:15 PM
 Author:	Natsuki
*/

// the setup function runs once when you press reset or power the board
#include <L3G.h>
#include <Adafruit_LSM303.h>
#include <Adafruit_BMP085.h>
#include <Servo.h>
#include <Wire.h>
#include <ThreadController.h>
#include <TimerOne.h>
#include "BattleBotComBytes.h"
#include <DualVNH5019MotorShieldMega.h>

#define ESPSTATUSLED	24 // Lights up when a client has connected to the ESP
#define ESPRESETPIN	45	 // This connects to the ESP reset pin incase it won't work
// Shoot section
#define SHOOTPIN		20 // Here the transistor is connected for the shooting
#define LASERPIN		21 // This is where the laser for the ammo detection is.
#define LASERDETECTPIN	22 // This is where the phototransistor for the laser is.

#define PANSERVOPIN		8
#define TILTSERVOPIN    9
byte DC1MotorSpeedSerial, DC2MotorSpeedSerial, panSerial, tiltSerial;
int DC1MotorSpeed, DC2MotorSpeed, pan, tilt;

DualVNH5019MotorShield Motors = DualVNH5019MotorShield(32, 34, 30, A0, 33, 35, 31, A1);
Servo panServo, tiltServo, shootServo;
Adafruit_BMP085 bmp = Adafruit_BMP085();
Adafruit_LSM303 lsm303 = Adafruit_LSM303();
L3G l3g = L3G();

float seaLevelPressure, altitude, temperature;
byte M1Current, M2Current;
ThreadController threadController = ThreadController();
Thread dofthread = Thread();
Thread connectionThread = Thread();
volatile int timeSinceLastReceivedMotorData = 0;
unsigned long previousMillis = millis();
unsigned long currentMillis = millis();
bool clientConnected;
byte buffer[10];

void timerCallback()
{
	threadController.run();
}

void getDofData()
{
	noInterrupts();
	if (clientConnected)
	{
		altitude = bmp.readAltitude(seaLevelPressure);
		temperature = bmp.readTemperature();
	}
	if (false)
	{
	}
	interrupts();
}

void connectionHandler() // This is to make sure the tank doesn't go haywire when the ESP stops working
{
	if (currentMillis - previousMillis > 50 && clientConnected == true)
	{
		Serial.println("Didn't recieve data in time from the ESP");
		Motors.setBrakes(0, 0); // In case of connection troubles. We do not reset the esp YET
	}
	if (currentMillis - previousMillis > 500 && clientConnected == true)
	{
		Motors.setBrakes(0, 0);
		digitalWrite(ESPRESETPIN, HIGH);
		delay(2);
		digitalWrite(ESPRESETPIN, LOW);
	}
}


void setup()
{
	Serial.begin(115200);
	Serial3.begin(250000);

	bmp.begin();
	lsm303.begin();
	l3g.init();
	l3g.enableDefault();
	float rawAltitude = bmp.readAltitude();
	seaLevelPressure = bmp.readSealevelPressure(rawAltitude);
	// Threading things
	dofthread.onRun(getDofData);
	dofthread.setInterval(250);
	//threadController.add(&dofthread);
	connectionThread.onRun(connectionHandler);
	connectionThread.setInterval(30);
	threadController.add(&connectionThread);
	Timer1.initialize(20000);
	Timer1.attachInterrupt(timerCallback);
	Timer1.start();
	// Threading things done
	panServo.attach(PANSERVOPIN);
	tiltServo.attach(TILTSERVOPIN);
	panServo.write(90);
	tiltServo.write(90);
	Motors.init();
	Wire.begin();
	Serial.println("Ready");
}

void MoveMotors()
{
	M1Current = Motors.getM1CurrentMilliamps() / 1000;
	M2Current = Motors.getM2CurrentMilliamps() / 1000;
	if (!Motors.getM1Fault()) {// Check if there is a fault in the motor driver on M1
		if (M1Current < 16)
		{ // Check if the current isn't to high
			Motors.setM1Speed(DC1MotorSpeed);
		}
		else
		{
			Serial.println("Warning: Motor 1 current is to high");
			if (Serial3.availableForWrite())
			{
				Serial3.write(ESPMOTORDIAG);
				Serial3.write(MOTOR1);
				Serial3.write(CURRENTWARNING);
				Serial3.write(MESSAGEEND);
			}
			Motors.setBrakes(0, 0);
		}
	}
	else
	{
		Serial.println("Warning: Motor 1 has a fault");

		if (Serial3.availableForWrite()) {
			Serial3.write(ESPMOTORDIAG);
			Serial3.write(MOTOR1);
			Serial3.write(MOTORFAULT);
			Serial3.write(MESSAGEEND);
		}
		Motors.setBrakes(0, 0);
	}
	if (!Motors.getM2Fault()) {// Check if there is a fault in the motor driver on M1
		if (M2Current < 16)
		{ // Check if the current isn't to high
			Motors.setM2Speed(DC2MotorSpeed);
		}
		else
		{
			Serial.println("Warning: Motor 2 current is to high");
			if (Serial3.availableForWrite())
			{
				Serial3.write(ESPMOTORDIAG);
				Serial3.write(MOTOR2);
				Serial3.write(CURRENTWARNING);
				Serial3.write(MESSAGEEND);
			}
			Motors.setBrakes(0, 0);
		}
	}
	else
	{
		Serial.println("Warning: Motor 2 has a fault");
		if (Serial3.availableForWrite()) {
			Serial3.write(ESPMOTORDIAG);
			Serial3.write(MOTOR2);
			Serial3.write(MOTORFAULT);
			Serial3.write(MESSAGEEND);
		}
		Motors.setBrakes(0, 0);
	}
}
#pragma region ReadSerialData
bool EndMessage()
{
	if (Serial3.read() == MESSAGEEND) return true;
	return false;
}

void ReadMotorData()
{
	DC1MotorSpeedSerial = Serial3.read();
	DC2MotorSpeedSerial = Serial3.read();
	byte endCommand = Serial3.read();
	Serial.print(DC1MotorSpeedSerial);
	Serial.print(" ");
	Serial.print(DC2MotorSpeedSerial);
	Serial.print(" ");
	Serial.println(endCommand);
	if (endCommand == MESSAGEEND)
	{
		DC1MotorSpeed = map(DC1MotorSpeedSerial, 0, 200, -400, 400);
		DC2MotorSpeed = map(DC2MotorSpeedSerial, 0, 200, -400, 400);
		Serial.print(DC1MotorSpeed);
		Serial.print(" ");
		Serial.println(DC2MotorSpeed);
		if (clientConnected)
		{ // Just to be on the safe side
			MoveMotors();
		}
		previousMillis = millis();
	}
	else
	{// Command not recieved
		Serial.println("Motor data is not valid");
	}
}

void ReadServoData()
{
	panSerial = Serial3.read();
	tiltSerial = Serial3.read();
	Serial.print(panSerial, HEX);
	Serial.print(" ");
	Serial.print(tiltSerial, HEX);
	Serial.print(" ");
	if (EndMessage())
	{
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

void ReadGenericData()
{
	byte command = Serial3.read();
	switch (command)
	{
	case GENSHOOT: // Right now it's just a laser that shoots and that is handled by the Mega itself
		if (EndMessage())
		{
			digitalWrite(LASERPIN, HIGH);
			delay(3);
			digitalWrite(LASERPIN, LOW);
		}
		break;
	default: break;
	}
}

void ReadClientData()
{
	byte espclientmessage = Serial3.read();
	if (espclientmessage == CLIENTESPREADY)
	{
		if (EndMessage())
		{
			Serial.println("ESP is ready for data");
		}
	}
	else if (espclientmessage == CLIENTCONNECT)
	{
		if (EndMessage())
			Serial.println("A client has connected");
		clientConnected = true;
	}
	else if (espclientmessage == CLIENTDISCONNECT)
	{
		if (EndMessage())
		{
			Serial.println("ESP lost connection to the client either with or without normal disconnect");
			clientConnected = false;
			Motors.setBrakes(0, 0);
		}
	}
	else if (espclientmessage == CLIENTPAUSED) {
		if (EndMessage())
		{
			Serial.println("ESP client is paused");
			clientConnected = false;
			Motors.setBrakes(0, 0);
		}
	}
	else if (espclientmessage == CLIENTCONTINUED) {
		if (EndMessage())
		{
			Serial.println("ESP client is continued");
			clientConnected = true;
		}
	}
}
#pragma endregion ReadSerialData

void HandleESPData()
{
	byte c = Serial3.read();
	Serial.print(c, HEX);
	Serial.print(" ");
	switch (c)
	{
	case COMMOTOR:
		ReadMotorData();
		break;
	case COMSERVO:
		ReadServoData();
		break;
	case COMCLIENT:
		ReadClientData();
		break;
	case COMGENERIC:
		ReadGenericData();
	case (byte)0x00:
		break;
	default:
		Serial.print(c, HEX);
		Serial.println(" is a unknown command");
		break;
	}
	Serial3.write(ESPMOTORSTAT);
	Serial3.write(M1Current);
	Serial3.write(M2Current);
	Serial3.write(MESSAGEEND);
}


void loop()
{
	currentMillis = millis();
	if (Serial3.available())
	{
		HandleESPData();
		Serial.println();
	}
}
