/*
 Name:		FlightController.ino
 Created:	4/23/2017 1:25:38 PM
 Author:	Natsuki

*/
#include <Joystick.h>
#include <TimedAction.h>
#include <PCF8574.h>
#include <Wire.h>

#define ON					    true
#define OFF					    false
#define PRESSED				    1
#define RELEASED			    0

#define PCF_ADDR			    B0100000	// This is the write addres of the PCF. The library will be able to use the read functions as well
// Digital pins
#define refSwitch			    4 // Used to ref up the nerf gun
#define forwardsButton			6 // Used to trigger a forwards  movement with throttle
#define backwardsButton			5 // Used to trigger a backwards movement with throttle
#define pcfInterruptPin			0 // Change this to any pin that got a interrupt for reading the PCF
#define shootInterruptPin		1 // Change this to any pin that got a interrupt for the shoot trigger

// Analog pins
#define xAxis				    2 // Tilt axis
#define yAxis				    0 // Pan axis
#define zAxis				    3 // Turn axis
#define throttlePin				1 // Mapping speed to throttle controll and using forwards and backwards button to trigger a analog axis
#define servoModePin			10 // 3 position switch for the servo mode that is connected to 3.3 v and 5v

// JoyStick buttons
#define zero2				    0
#define zero3				    1
#define zero4				    2
#define zero5				    3
#define refbutton				4
#define shootbutton			5
#define servoLock				6
#define servoStabalize			7

PCF8574 PCF_38(PCF_ADDR);

unsigned long  lastTime = 0;
volatile unsigned long count[8];
bool buttons[8];
volatile bool buttonsChanged;
volatile bool shootTriggered;
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
/*
Order of buttons on PCF8574 is:
5: hat up
7: hat right
6: hat down
4: hat left
3: 02
2: 03 
0: 04
1: 05
*/
void sendPCFButtons()
{
	Serial.println("pcf");
	uint8_t value = PCF_38.read8();

	for (uint8_t i = 0; i< 8; i++)
	{
		Serial.print(returnPinValue(value, i));
		Serial.print(",");
		buttons[i] == returnPinValue(value, i);
	}
	Serial.println();

	if		(buttons[5] && buttons[7])	JoyStick.setHatSwitch(0, 45);
	else if (buttons[7] && buttons[6])	JoyStick.setHatSwitch(0, 135);
	else if (buttons[6] && buttons[5])	JoyStick.setHatSwitch(0, 225);
	else if (buttons[5] && buttons[4])	JoyStick.setHatSwitch(0, 315);
	else if (buttons[5])				JoyStick.setHatSwitch(0, 0);
	else if (buttons[7])				JoyStick.setHatSwitch(0, 90);
	else if (buttons[6])				JoyStick.setHatSwitch(0, 180);
	else if (buttons[4])				JoyStick.setHatSwitch(0, 270);
	else								JoyStick.setHatSwitch(0, -1);

	JoyStick.setButton(zero2, buttons[3]);
	JoyStick.setButton(zero3, buttons[2]);
	JoyStick.setButton(zero4, buttons[0]);
	JoyStick.setButton(zero5, buttons[1]);

	buttonsChanged = false;
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
		JoyStick.setYAxis(map(analogRead(throttlePin), 170, 770, 1023, 511));
		Serial.print("FORWARDS: ");
		Serial.println(map(analogRead(throttlePin), 170, 770, 1023, 511));
	}
	else if (digitalRead(backwardsButton) == LOW)
	{
		JoyStick.setYAxis(map(analogRead(throttlePin), 170, 770, 0, 511));
		Serial.print("BACK: ");
		Serial.println(map(analogRead(throttlePin), 170, 770, 0, 511));
	}
	else
	{
		JoyStick.setYAxis(511);
	}
	JoyStick.setRxAxis(map(analogRead(xAxis), 320, 668, 0, 1023));
	JoyStick.setRyAxis(map(analogRead(yAxis), 250, 630, 0, 1023));
	JoyStick.setXAxis(map(analogRead(zAxis), 270, 920, 0, 1023));
//	float servoModeValue = analogRead(servoModePin) * (5.0 / 1023.0);
//	if(servoModeValue >= 4.5)
//	{
//		if(servoMode == 0 || servoMode == 1)
//		{
//			JoyStick.setButton(servoLock, PRESSED);
//			JoyStick.setButton(servoStabalize, RELEASED);
//		}
//	} 
//	else if(servoModeValue >= 3 && servoModeValue <= 4)
//	{
//		if (servoMode == 0 || servoMode == 2)
//		{
//			JoyStick.setButton(servoLock, RELEASED);
//			JoyStick.setButton(servoStabalize, PRESSED);
//		}
//	}
//	else
//	{
//		if (servoMode == 1 || servoMode == 2)
//		{
//			JoyStick.setButton(servoLock, RELEASED);
//			JoyStick.setButton(servoStabalize, RELEASED);
//		}
//	}
}

void disableShooting()
{
	JoyStick.setButton(shootbutton, LOW);
	JoyStick.sendState();
	doDisableShooting.disable();
	shootTriggered = false;
}

void catchShoot()
{
	JoyStick.setButton(shootbutton, !digitalRead(shootInterruptPin));
}

void catchPcf()
{
	if(!buttonsChanged) // Only trigger this if the previous sending is completed
		buttonsChanged = true; 
}



void setup()
{
	Serial.begin(115200);
	JoyStick.begin();
	Serial.println(F("\nJoystick Test program\n"));
	pinMode(pcfInterruptPin, INPUT_PULLUP);
	pinMode(shootInterruptPin, INPUT_PULLUP);
	pinMode(forwardsButton, INPUT_PULLUP);
	pinMode(backwardsButton, INPUT_PULLUP);
	pinMode(refSwitch, INPUT_PULLUP);
	attachInterrupt(digitalPinToInterrupt(pcfInterruptPin), catchPcf, CHANGE);
	attachInterrupt(digitalPinToInterrupt(shootInterruptPin), catchShoot, CHANGE);
	doDisplayCount = TimedAction(1000, displayCount);
	doGetDigitalReadings = TimedAction(10, getJoystickReadings);
	doDisableShooting = TimedAction(70, disableShooting);
	doDisableShooting.disable();
}

void loop()
{
	doGetDigitalReadings.check();
	doDisableShooting.check();
	if (buttonsChanged)
	{
		sendPCFButtons();
	}
}
