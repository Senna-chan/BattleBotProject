/*
 Name:		BattleBotServerArduino.ino
 Created:	10/9/2016 3:08:15 PM
 Author:	Natsuki
*/

// the setup function runs once when you press reset or power the board
#include <Adafruit_10DOF.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_LSM303_U.h>
#include <Adafruit_BMP085_U.h>
#include <Servo.h>
#include <DualVNH5019MotorShieldMega.h>
#include <Wire.h>
#include <ThreadController.h>
#include <TimerOne.h>

#define ESPRESETPIN 22 // Digital pin that connects to the ESP reset pin
#define ESPSTATUSLED 24
// ESP message data
#define ESPMOTOR		0xF0
#define ESPSERVO		0xF1
#define ESPREADY		0xF2
#define ESPMESSAGEEND	0xFF


byte DC1MotorSpeedSerial, DC2MotorSpeedSerial, panSerial, tiltSerial;
int DC1MotorSpeed, DC2MotorSpeed, pan, tilt;

DualVNH5019MotorShield Motors = DualVNH5019MotorShield();
Servo panServo, tiltServo;
Adafruit_10DOF                dof = Adafruit_10DOF();
Adafruit_LSM303_Accel_Unified accel = Adafruit_LSM303_Accel_Unified(30301);
Adafruit_LSM303_Mag_Unified   mag = Adafruit_LSM303_Mag_Unified(30302);
Adafruit_BMP085_Unified       bmp = Adafruit_BMP085_Unified(18001);

float seaLevelPressure;

ThreadController controll = ThreadController();
Thread dofthread = Thread();
Thread connectionThread = Thread();
volatile int timeSinceLastReceivedMotorData = 0;
unsigned long previousMillis = millis();
unsigned long currentMillis = millis();
void timerCallback() {
	controll.run();
}

void getDofData()
{
	//Serial.println("Getting dof sensor data");
}

void connectionHandler()
{
	if (currentMillis - previousMillis > 50) Motors.setBrakes(0, 0); // In case of connection troubles. We do not reset the esp YET
	if (currentMillis - previousMillis > 500) digitalWrite(ESPRESETPIN, HIGH);
}


void setup() {
	Serial.begin(9600);
	Serial3.begin(115200);
//	if (!accel.begin())
//	{
//		/* There was a problem detecting the LSM303 ... check your connections */
//		Serial.println(F("Ooops, no LSM303 detected ... Check your wiring!"));
//		while (1);
//	}
//	if (!mag.begin())
//	{
//		/* There was a problem detecting the LSM303 ... check your connections */
//		Serial.println("Ooops, no LSM303 detected ... Check your wiring!");
//		while (1);
//	}
//	if (!bmp.begin())
//	{
//		/* There was a problem detecting the BMP180 ... check your connections */
//		Serial.println("Ooops, no BMP180 detected ... Check your wiring!");
//		while (1);
//	}

	// Threading things
	dofthread.onRun(getDofData);
	dofthread.setInterval(50);
	controll.add(&dofthread);
	connectionThread.onRun(connectionHandler);
	connectionThread.setInterval(50);
	controll.add(&connectionThread);
	Timer1.initialize(20000);
	Timer1.attachInterrupt(timerCallback);
	Timer1.start();

	panServo.attach(7);
	tiltServo.attach(8);
	Motors.init();
	Wire.begin();
	Serial.println("Ready");
}
void MoveMotors()
{
	if (!Motors.getM1Fault()) {// Check if there is a fault in the motor driver on M1
		if (Motors.getM1CurrentMilliamps() < 16000) { // Check if the current isn't to high
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
			Serial3.println("motor:1:current");
			Motors.setBrakes(0, 0);
		}
	}
	else
	{
		Serial.println("Warning: Motor 1 has a fault");
		Serial3.println("motor:1:fault");
		Motors.setBrakes(0, 0);
	}
	if (!Motors.getM2Fault()) {// Check if there is a fault in the motor driver on M1
		if (Motors.getM2CurrentMilliamps() < 16000) { // Check if the current isn't to high
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
			Serial3.println("motor:2:current");
			Motors.setBrakes(0, 0);
		}
	}
	else
	{
		Serial.println("Warning: Motor 2 has a fault");
		Serial3.println("motor:2:fault");
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
		if (Serial3.read() == 0xFF)
		{
			DC1MotorSpeed = map(DC1MotorSpeedSerial, 0, 200, -400, 400);
			DC2MotorSpeed = map(DC2MotorSpeedSerial, 0, 200, -400, 400);

			MoveMotors();
			previousMillis = millis();
			//Serial3.println("data:motor:recieved");
		}
		else
		{// Command not recieved
			Serial.println("data:motor:failed");
		}
	}
	if(c == ESPSERVO) // Servo
	{
		panSerial = Serial3.read();
		tiltSerial = Serial3.read();
		pan = map(panSerial, 0, 200, 0, 180);
		tilt = map(tiltSerial, 0, 200, 0, 180);
		panServo.write(pan);
		tiltServo.write(tilt);
	}
	if(c == ESPREADY)
	{
		Serial.println("ESP is ready for data");
	}
}

// the loop function runs over and over again until power down or reset
void loop() {
	currentMillis = millis();
	while (Serial3.available()) {
		HandleESPData();
	}
}