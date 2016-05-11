using System;
using System.Collections.Generic;
using Raspberry.IO.Components.Controllers.Pca9685;
using Raspberry.IO.InterIntegratedCircuit;

namespace AdafruitPorts.MotorHat
{
	public class Motor_Hat
	{
		public static int MFORWARD = 1;
		public static int MBACKWARD = 2;
		public static int MBRAKE = 3;
		public static int MRELEASE = 4;

		public static int SSINGLE = 1;
		public static int SDOUBLE = 2;
		public static int SINTERLEAVE = 3;
		public static int SMICROSTEP = 4;
		List<DC_Motor> motors = new List<DC_Motor>();
		List<Stepper_Motor> steppers = new List<Stepper_Motor>();
		public Pca9685Connection _pwm;
		public I2cDeviceConnection connection;

		/// <summary>
		/// Initializes a new instance of the <see cref="Motor_Hat"/> class.
		/// </summary>
		/// <param name="addr">I2C address default is 0x60</param>
		/// <param name="freq">Frequency</param>
		public Motor_Hat(I2cDriver driver, int addr = 0x60, int freq = 1600){
			for(int i=0;i<4;i++){
				motors.Add (new DC_Motor (this, i)); // Creates 4 DC Motors and adds them to the list
			}
			steppers.Add (new Stepper_Motor (this, 1)); // Creates Stepper Motor 1
			steppers.Add (new Stepper_Motor (this, 2)); // Creates Stepper Motor 2
			connection = driver.Connect(addr);
			_pwm = new Pca9685Connection (connection); // Connects to the Motor Hat
			_pwm.SetPwmUpdateRate(freq); // Sets the frequency
		}
		public void setPin(int pin, int value){
			if ((pin < 0) || (pin > 12)){
				Console.WriteLine("PWM pin must be between 0 and 12 inclusive");
				return;
			}
			if ((value != 0) && (value != 1)){
				Console.WriteLine("Pin value must be 0 or 1!");
			}
			if (value == 0)
			{
				_pwm.SetPwm(pin, 0, 4096);
			}
			if (value == 1){
				_pwm.SetPwm(pin, 4096, 0);
			}
		}
		/// <summary>
		/// Gets a Stepper motor
		/// </summary>
		/// <returns>The stepper.</returns>
		/// <param name="num">Number of stepper you want to get</param>
		public Stepper_Motor GetStepper(int num){
			if ((num < 1) || (num > 2)){
				Console.WriteLine("MotorHAT Stepper must be between 1 and 2 inclusive");
			}
			return steppers[num-1];
		}
		public DC_Motor GetMotor(int num){
			if ((num < 1) || (num > 4)){
				Console.WriteLine ("MotorHAT Motor must be between 1 and 4 inclusive");
			}
			return motors[num-1];
		}
	}
}

