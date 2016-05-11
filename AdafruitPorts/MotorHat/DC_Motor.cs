using System;

namespace AdafruitPorts.MotorHat{
	public class DC_Motor
	{
		int motornum;
		Motor_Hat MC;
		int PWMpin, IN1pin, IN2pin;
		public DC_Motor(Motor_Hat controller, int motor)
		{
			MC = controller;
			motornum = motor;
			int pwm = 0, in1 = 0, in2 = 0;

			if (motor == 0){
				pwm = 8;
				in2 = 9;
				in1 = 10;
			}
			else if (motor == 1){
				pwm = 13;
				in2 = 12;
				in1 = 11;
			}
			else if (motor == 2){
				pwm = 2;
				in2 = 3;
				in1 = 4;
			}
			else if (motor == 3){
				pwm = 7;
				in2 = 6;
				in1 = 5;
			}
			else{
				Console.WriteLine("MotorHAT Motor must be between 1 and 4 inclusive");
				return;
			}
			PWMpin = pwm;
			IN1pin = in1;
			IN2pin = in2;
		}
		/// <summary>
		/// Sets the mode of the motor
		/// </summary>
		/// <param name="command">mode</param>
		public void run(int command){
			if (MC==null){
			return;
			}
			if (command == Motor_Hat.MFORWARD){
				MC.setPin (IN2pin, 0);
				MC.setPin (IN1pin, 1);
			}
			if (command == Motor_Hat.MBACKWARD){
				MC.setPin (IN1pin, 0);
				MC.setPin (IN2pin, 1);
			}
			if (command == Motor_Hat.MRELEASE){
				MC.setPin (IN1pin, 0);
				MC.setPin (IN2pin, 0);
			}
		}
		/// <summary>
		/// Sets the speed.
		/// </summary>
		/// <param name="speed">Speed</param>
		public void setSpeed(int speed){
			if (speed < 0){
				speed = 0;
			}
			if (speed > 255){
				speed = 255;
			}
			MC._pwm.SetPwm (PWMpin, 0, speed * 16);
		}
	}
}