using AdafruitPorts.MotorHat;

namespace BattleBotServer
{
    public class MotorHelper
    {
        private readonly DC_Motor motor;

        public MotorHelper(DC_Motor dcMotor)
        {
            motor = dcMotor;
        }

        public void Forward(int speed)
        {
            motor.setSpeed(speed);
            motor.run(Motor_Hat.MFORWARD);
        }

        public void Backward(int speed)
        {
            motor.setSpeed(speed*-1);
            motor.run(Motor_Hat.MBACKWARD);
        }

        public void Stop()
        {
            motor.setSpeed(0);
            motor.run(Motor_Hat.MRELEASE);
        }
    }
}