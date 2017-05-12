/*
 Name:		FlightController.ino
 Created:	4/23/2017 1:25:38 PM
 Author:	Natsuki

Order of buttons on PCF8574 is:
1: hat up
2: hat right
3: hat down
4: hat left
5: 02
6: 03 
7: 04
8: 05

*/
#include <Joystick.h>
#include <TimedAction.h>
#include <PCF8574.h>
#include <Wire.h>

#define ON					true
#define OFF					false
#define PRESSED				1
#define RELEASED			0

#define PCF_ADDR			B0100000	// This is the write addres of the PCF. The library will be able to use the read functions as well
// Digital pins
#define refSwitch			4 // Used to ref up the nerf gun
#define forwardsButton		9 // Used to trigger a forwards  movement with throttle
#define backwardsButton		6 // Used to trigger a backwards movement with throttle
#define pcfInterruptPin		7 // Change this to any pin that got a interrupt for reading the PCF
#define shootInterruptPin	1 // Change this to any pin that got a interrupt for the shoot trigger

// Analog pins
#define xAxis				0 // Tilt axis
#define yAxis				1 // Pan axis
#define zAxis				2 // Turn axis
#define throttlePin			3 // Mapping speed to throttle controll and using forwards and backwards button to trigger a analog axis
#define servoModePin		10 // 3 position switch for the servo mode that is connected to 3.3 v and 5v

// JoyStick buttons
#define zero2				0
#define zero3				1
#define zero4				2
#define zero5				3
#define refbutton			4
#define schootbutton		5
#define servoLock			6
#define servoStabalize		7

PCF8574 PCF_38(PCF_ADDR);

unsigned long  lastTime = 0;
volatile unsigned long count[8];
volatile bool buttons[8];
volatile bool buttonsChanged;
int servoMode = 0; // 0 is none 1 is lock 2 is stabilize

bool forwardsState = OFF;
bool backwardsState = OFF;
bool refState = OFF;

bool checkDisableShooting = false;

TimedAction doDisplayCount = TimedAction();
TimedAction doGetDigitalReadings = TimedAction();
TimedAction doDisableShooting = TimedAction();

Joystick_ JoyStick = Joystick_(3, 4, 8 , 1, true, true, false, true, true, false, false, false, false, false, false);

#pragma region Helpers
int returnPinValue(uint8_t value, uint8_t pin)
{
	return (value & (1 << pin)) > 0;
}
#pragma endregion

void sendPCFButtons()
{
	if		(buttons[0] && buttons[1])	JoyStick.setHatSwitch(0, 45);
	else if (buttons[1] && buttons[2])	JoyStick.setHatSwitch(0, 135);
	else if (buttons[2] && buttons[3])	JoyStick.setHatSwitch(0, 225);
	else if (buttons[3] && buttons[0])	JoyStick.setHatSwitch(0, 315);
	else if (buttons[0])				JoyStick.setHatSwitch(0, 360);
	else if (buttons[1])				JoyStick.setHatSwitch(0, 90);
	else if (buttons[2])				JoyStick.setHatSwitch(0, 180);
	else if (buttons[3])				JoyStick.setHatSwitch(0, 270);
	else								JoyStick.setHatSwitch(0, 0);

	JoyStick.setButton(zero2, buttons[4]);
	JoyStick.setButton(zero3, buttons[5]);
	JoyStick.setButton(zero4, buttons[6]);
	JoyStick.setButton(zero5, buttons[7]);

	JoyStick.sendState();
}

void displayCount()
{
	Serial.print(count[0]);
	for (uint8_t i = 1; i< 8; i++)
	{
		Serial.print(buttons[i]);
		Serial.print(',');
	}
	Serial.println("");
}

void getJoystickReadings()
{
	if(digitalRead(refSwitch) == HIGH)
	{
		if (refState == ON) {
			JoyStick.releaseButton(refbutton);
			refState = OFF;
		}
	}
	else
	{
		if (refState == OFF) {
			JoyStick.pressButton(refbutton);
			refState = ON;
		}
	}

	if(digitalRead(forwardsButton) == LOW)
	{
		JoyStick.setYAxis(map(analogRead(throttlePin), 0, 1023, 0, 100));
	}
	else
	{
		JoyStick.setYAxis(0);
	}

	if(digitalRead(backwardsButton) == LOW)
	{
		JoyStick.setYAxis(map(analogRead(throttlePin), 0, 1023, 0, 100) * -1);
	}
	else
	{
		JoyStick.setYAxis(0);
	}

	JoyStick.setRxAxis(map(analogRead(xAxis), 0, 1023, -100, 100));
	JoyStick.setRyAxis(map(analogRead(yAxis), 0, 1023, -100, 100));
	JoyStick.setXAxis(map(analogRead(zAxis), 0, 1023, -100, 100));
	float servoModeValue = analogRead(servoModePin) * (5.0 / 1023.0);
	if(servoModeValue >= 4.5)
	{
		if(servoMode == 0 || servoMode == 1)
		{
			JoyStick.setButton(servoLock, PRESSED);
			JoyStick.setButton(servoStabalize, RELEASED);
		}
	} 
	else if(servoModeValue >= 3 && servoModeValue <= 4)
	{
		if (servoMode == 0 || servoMode == 2)
		{
			JoyStick.setButton(servoLock, RELEASED);
			JoyStick.setButton(servoStabalize, PRESSED);
		}
	}
	else
	{
		if (servoMode == 1 || servoMode == 2)
		{
			JoyStick.setButton(servoLock, RELEASED);
			JoyStick.setButton(servoStabalize, RELEASED);
		}
	}
	JoyStick.sendState();
}

void disableShooting()
{
	JoyStick.setButton(schootbutton, LOW);
	JoyStick.sendState();
	doDisableShooting.disable();
}

void SHOOT()
{
	JoyStick.setButton(schootbutton, HIGH);
	JoyStick.sendState();
	doDisableShooting.enable();
}

void catchChange()
{
	uint8_t tmp = TWBR;
	TWBR = 2; // speed up I2C;
	interrupts();
	uint8_t value = PCF_38.read8();
	noInterrupts();
	TWBR = tmp;

	for (uint8_t i = 0; i< 8; i++)
	{
		Serial.print(returnPinValue(value, i));
		Serial.print(",");
		buttons[i] == returnPinValue(value, i);
	}
	Serial.println();
	buttonsChanged = true; 
}



void setup()
{
	Serial.begin(115200);
	JoyStick.setRxAxisRange(-100, 100);
	JoyStick.setRyAxisRange(-100, 100);
	JoyStick.setXAxisRange(-100, 100);
	JoyStick.setYAxisRange(-100, 100);
	JoyStick.begin(false);
	Serial.println(F("\nJoystick Test program\n"));
	pinMode(pcfInterruptPin, INPUT_PULLUP);
	pinMode(shootInterruptPin, INPUT_PULLUP);
	pinMode(forwardsButton, INPUT_PULLUP);
	pinMode(backwardsButton, INPUT_PULLUP);
	pinMode(refSwitch, INPUT_PULLUP);
	attachInterrupt(digitalPinToInterrupt(pcfInterruptPin), catchChange, CHANGE);
	attachInterrupt(digitalPinToInterrupt(shootInterruptPin), SHOOT, RISING);
	doDisplayCount = TimedAction(1000, displayCount);
	doGetDigitalReadings = TimedAction(10, getJoystickReadings);
	doDisableShooting = TimedAction(40, disableShooting);
	doDisableShooting.disable();
}

void loop()
{
	doDisplayCount.check();
	doGetDigitalReadings.check();
	doDisableShooting.check();
	if(buttonsChanged)
	{
		buttonsChanged = false;
		sendPCFButtons();
	}
}