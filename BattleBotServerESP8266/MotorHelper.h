#ifndef _MOTORHELPER_h
#define _MOTORHELPER_h

#include <Esp.h>
class MotorHelperClass
{
 public:
	MotorHelperClass(int pwmpin, int PF, int PB);
	void Forward(int speed);
	void Backward(int speed);
	void Stop(void);
  private:
	  int _pwmpin;
	  int _PF;
	  int _PB;
};

extern MotorHelperClass MotorHelper;

#endif

