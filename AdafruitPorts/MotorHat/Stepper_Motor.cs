using System;

namespace AdafruitPorts.MotorHat{
	
	public class Stepper_Motor
	{
		int MICROSTEPS = 8;
		int[] MICROSTEP_CURVE = {0, 50, 98, 142, 180, 212, 236, 250, 255};
		int revsteps;
		double sec_per_step;
		int steppingcounter;
		int currentstep;
		int motornum;
		Motor_Hat MC;
		int PWMA, AIN2, AIN1, PWMB, BIN2, BIN1;

		public Stepper_Motor(Motor_Hat controller, int motor, int steps = 200){
			MC = controller;
			revsteps = steps;
			motornum = motor;
			sec_per_step = 0.1;
			steppingcounter = 0;
			currentstep = 0;

			motor -= 1;

			if (motor == 0){
				PWMA = 8;
				AIN2 = 9;
				AIN1 = 10;
				PWMB = 13;
				BIN2 = 12;
				BIN1 = 11;
			}
			else if (motor == 1){
				PWMA = 2;
				AIN2 = 3;
				AIN1 = 4;
				PWMB = 7;
				BIN2 = 6;
				BIN1 = 5;
			}
			else{
				Console.WriteLine("MotorHAT Stepper must be between 1 and 2 inclusive");
				return;
			}
		}
		public void SetSpeed(int rpm){
			sec_per_step = 60.0 / (revsteps * rpm);
			steppingcounter = 0;
		}
		private int oneStep(int dir, int style){
			int pwm_a = 255, pwm_b = 255;

			// first determine what sort of stepping procedure we're up to
			if (style == Motor_Hat.SSINGLE) {
				if ((currentstep/(MICROSTEPS/2) % 2) != 0) {
					// we're at an odd step, weird
					if (dir == Motor_Hat.MFORWARD) {
						currentstep += MICROSTEPS / 2;
					} else {
						currentstep -= MICROSTEPS / 2;
					}
				} else {
					// go to next even step
					if (dir == Motor_Hat.MFORWARD) {
						currentstep += MICROSTEPS;
					} else {
						currentstep -= MICROSTEPS;
					}
				}
			}
			if (style == Motor_Hat.SDOUBLE) {
				if ((currentstep / (MICROSTEPS / 2) % 2)==0) {
					// we're at an even step, weird
					if (dir == Motor_Hat.MFORWARD) {
						currentstep += MICROSTEPS / 2;
					} else {
						currentstep -= MICROSTEPS / 2;
					}
				} else {
					// go to next odd step
					if (dir == Motor_Hat.MFORWARD) {
						currentstep += MICROSTEPS;
					} else {
						currentstep -= MICROSTEPS;
					}
				}
			}
			if (style == Motor_Hat.SINTERLEAVE) {
				if (dir == Motor_Hat.MFORWARD) {
					currentstep += MICROSTEPS / 2;
				} else {
					currentstep -= MICROSTEPS / 2;
				}
			}

			if (style == Motor_Hat.SMICROSTEP) {
				if (dir == Motor_Hat.MFORWARD) {
					currentstep += 1;
				} else {
					currentstep -= 1;
				}

				// go to next 'step' and wrap around
				currentstep += MICROSTEPS * 4;
				currentstep %= MICROSTEPS * 4;

				pwm_a = pwm_b = 0;
				if ((currentstep >= 0) && (currentstep < MICROSTEPS)) {
					pwm_a = MICROSTEP_CURVE [MICROSTEPS - currentstep];
					pwm_b = MICROSTEP_CURVE [currentstep];
				} else if ((currentstep >= MICROSTEPS) && (currentstep < MICROSTEPS * 2)) {
					pwm_a = MICROSTEP_CURVE [currentstep - MICROSTEPS];
					pwm_b = MICROSTEP_CURVE [MICROSTEPS * 2 - currentstep];
				} else if ((currentstep >= MICROSTEPS * 2) && (currentstep < MICROSTEPS * 3)) {
					pwm_a = MICROSTEP_CURVE [MICROSTEPS * 3 - currentstep];
					pwm_b = MICROSTEP_CURVE [currentstep - MICROSTEPS * 2];
				} else if ((currentstep >= MICROSTEPS * 3) && (currentstep < MICROSTEPS * 4)) {
					pwm_a = MICROSTEP_CURVE [currentstep - MICROSTEPS * 3];
					pwm_b = MICROSTEP_CURVE [MICROSTEPS * 4 - currentstep];
				}
			}

			// go to next 'step' and wrap around
			currentstep += MICROSTEPS * 4;
			currentstep %= MICROSTEPS * 4;

			// only really used for microstepping, otherwise always on!
			MC._pwm.SetPwm(PWMA, 0, pwm_a*16);
			MC._pwm.SetPwm (PWMB, 0, pwm_b * 16);
			int[] coils = {0, 0, 0, 0};
			int[] newCoils = null;
			if (style == Motor_Hat.SMICROSTEP){
				if ((currentstep >= 0) && (currentstep < MICROSTEPS)) {
					newCoils = new int[] { 1, 1, 0, 0 };
				} else if ((currentstep >= MICROSTEPS) && (currentstep < MICROSTEPS * 2)) {
					newCoils = new int[] { 0, 1, 1, 0 };
				} else if ((currentstep >= MICROSTEPS * 2) && (currentstep < MICROSTEPS * 3)) {
					newCoils = new int[] { 0, 0, 1, 1 };
				} else if ((currentstep >= MICROSTEPS * 3) && (currentstep < MICROSTEPS * 4)) {
					newCoils = new int[] { 1, 0, 0, 1 };
				} else {
					Console.WriteLine ("Unknown error. Printing vars for troubleshooting\n" +
					"Currentstep: {0}\n" +
					"MICROSTEPS: {1}", currentstep, MICROSTEPS);
				}

				Array.Copy(newCoils, 0, coils, 1, 4);
			}
			else{
				int[][] step2coils = new int[8][]{ 	
					new int[]{1, 0, 0, 0}, 
					new int[]{1, 1, 0, 0},
					new int[]{0, 1, 0, 0},
					new int[]{0, 1, 1, 0},
					new int[]{0, 0, 1, 0},
					new int[]{0, 0, 1, 1},
					new int[]{0, 0, 0, 1},
					new int[]{1, 0, 0, 1} 
				};
					coils = step2coils[currentstep/(MICROSTEPS/2)];
			}
			Console.WriteLine ("coils state = " + coils);
			MC.setPin (AIN2, coils [0]);
			MC.setPin (BIN1, coils [1]);
			MC.setPin (AIN1, coils [2]);
			MC.setPin (BIN2, coils [3]);

			return currentstep;
		}
		public void step(int steps, int direction, int stepstyle){
			double s_per_s = sec_per_step;
			int lateststep = 0;

			if (stepstyle == Motor_Hat.SINTERLEAVE){
				s_per_s = s_per_s / 2.0;
			}
			if (stepstyle == Motor_Hat.SMICROSTEP){
				s_per_s /= MICROSTEPS;
				steps *= MICROSTEPS;
			}
			Console.WriteLine (s_per_s + " sec per step");

			for(int s = 0; s < 4;s++){
				lateststep = oneStep(direction, stepstyle);
				System.Threading.Thread.Sleep(Convert.ToInt32(s_per_s));
			}
				if (stepstyle == Motor_Hat.SMICROSTEP){
						// this is an edge case, if we are in between full steps, lets just keep going
						// so we end on a full step
				while ((lateststep != 0) && (lateststep != MICROSTEPS)){
					lateststep = oneStep(direction, stepstyle);
					System.Threading.Thread.Sleep(Convert.ToInt32(s_per_s));
				}
			}
		}
	}
}