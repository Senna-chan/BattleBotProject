#include "MotorHelper.h"


MotorHelperClass::MotorHelperClass(int pwmpin, int PF, int PB)
{
	_pwmpin = pwmpin;
	_PF = PF;
	_PB = PB;
	pinMode(_PF, OUTPUT);
	pinMode(_PB, OUTPUT);
	pinMode(_pwmpin, OUTPUT);
}

void MotorHelperClass::Forward(int speed)
{
	digitalWrite(_PB, LOW);
	digitalWrite(_PF, HIGH);
	analogWrite(_pwmpin, map(speed, 0, 100, 0, 1024));
}

void MotorHelperClass::Backward(int speed)
{
	speed = speed * -1;
	digitalWrite(_PF, LOW);
	digitalWrite(_PB, HIGH);
	analogWrite(_pwmpin, map(speed, 0, 100, 0, 1024));
}

void MotorHelperClass::Stop()
{
	digitalWrite(_PF, LOW);
	digitalWrite(_PB, LOW);
	analogWrite(_pwmpin, 0);
}