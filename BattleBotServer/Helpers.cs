using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using AdafruitPorts;
using AdafruitPorts.MotorHat;
using Raspberry.IO.Components.Controllers.Pca9685;
using Raspberry.IO.GeneralPurpose;
using Raspberry.IO.InterIntegratedCircuit;

namespace BattleBotServer
{
    public class Helpers
    {
        private static I2cDriver driver = new I2cDriver(ConnectorPin.P1Pin3.ToProcessor(), ConnectorPin.P1Pin5.ToProcessor());
        public static Motor_Hat mh = new Motor_Hat(driver, 0x60, 50);
        public static MotorHelper DC1Helper = new MotorHelper(mh.GetMotor(1)); 
        public static MotorHelper DC2Helper = new MotorHelper(mh.GetMotor(2));
        public static MotorHelper DC3Helper = new MotorHelper(mh.GetMotor(3));
        public static MotorHelper DC4Helper = new MotorHelper(mh.GetMotor(4));


        public static void TestMotors()
        {
            DC1Helper.Forward(25);
            DC2Helper.Forward(25);
            DC3Helper.Forward(25);
            DC4Helper.Forward(25);
            Thread.Sleep(1000);
            DC1Helper.Stop();
            DC2Helper.Stop();
            DC3Helper.Stop();
            DC4Helper.Stop();
        }
        public static bool IsLinux
        {
            get
            {
                var p = (int) Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        

        public static bool isRaspberry()
        {
            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var hostName = Dns.GetHostName();
            var fqdn = "";
            if (!hostName.Contains(domainName))
                fqdn = hostName + "." + domainName;
            else
                fqdn = hostName;
            return fqdn == "Natsukipi" || fqdn == "Natsukipi.local";
        }

        public static int GetServoValue(int input)
        {
            return map(input, -100, 100, 104, 521);
        }

        private static int map(int x, int in_min, int in_max, int out_min, int out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }


        public static string GetPiTemp()
        {
            var output = RunProgram("cat", "/sys/class/thermal/thermal_zone0/temp");
            return RunProgramSuccesed(output) ? output.Substring(0, 2) : output;
        }

        public static string GetPiBatLevel()
        {
            var output = RunProgram("mopicli", "-v");
            return RunProgramSuccesed(output) ? (int.Parse(output.Substring(24))/1000).ToString() : output;
        }

        public static bool RunProgramSuccesed(string output)
        {
            return !output.StartsWith("An error occured. Error: ");
        }

        public static string GetPiLoad()
        {
            var output = RunProgram("cat", "/proc/loadavg");
            return RunProgramSuccesed(output) ? output.Split(' ')[0] : output;
        }

        public static bool CheckWiFiUplink()
        {
            var output = RunProgram("iwconfig", "wlan0 | grep Link");
            if (RunProgramSuccesed(output))
            {
                //output = output.Split('\n')[6];
                Console.WriteLine(output);
            }
            return false;
        }

        public static string RunProgram(string program, string parameters = null)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = program,
                    Arguments = parameters,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                }
            };
            try
            {
                proc.Start();
                return proc.StandardOutput.ReadToEnd();
            }
            catch (Exception e)
            {
                return "An error occured. Error: " + e.Message;
            }
        }
        
        public static int[] motorCalcsTank(int speed, int wheelPos1, int wheelPos2, int servoX, int servoY)
        {
            int DC1motorSpeed = 0, DC2motorSpeed = 0;
            if (speed > 0)
            {
                DC1motorSpeed = Convert.ToInt32(Math.Floor(speed*2.55 - wheelPos1*2.55));
                DC2motorSpeed = Convert.ToInt32(Math.Floor(speed*2.55 - wheelPos2*2.55));
            }
            else if (speed < 0)
            {
                DC1motorSpeed = Convert.ToInt32(Math.Floor(speed*2.55 + wheelPos1*2.55));
                DC2motorSpeed = Convert.ToInt32(Math.Floor(speed*2.55 + wheelPos2*2.55));
            }
            if (speed == 0)
            {
                if (wheelPos1 > 0)
                {
                    DC1motorSpeed = Convert.ToInt32(Math.Floor(wheelPos1*2.55*-1 + 1));
                    DC2motorSpeed = Convert.ToInt32(Math.Floor(wheelPos1*2.55));
                }
                if (wheelPos2 > 0)
                {
                    DC1motorSpeed = Convert.ToInt32(Math.Floor(wheelPos2*2.55));
                    DC2motorSpeed = Convert.ToInt32(Math.Floor(wheelPos2*2.55*-1 + 1));
                }
            }
            if (DC1motorSpeed < 0) DC3Helper.Backward(DC1motorSpeed);
            if (DC1motorSpeed > 0) DC3Helper.Forward(DC1motorSpeed);
            if (DC1motorSpeed == 0) DC3Helper.Stop();
            if (DC2motorSpeed < 0) DC2Helper.Backward(DC2motorSpeed);
            if (DC2motorSpeed > 0) DC2Helper.Forward(DC2motorSpeed);
            if (DC2motorSpeed == 0) DC2Helper.Stop();

            MovePanTilt(servoX, servoY);

            return new[] {0, DC1motorSpeed, DC2motorSpeed};
        }

        /// <summary>
        ///     Moves the PanTilt System
        /// </summary> 
        /// <param name="PanTiltX">PanTilt X Axix</param>
        /// <param name="PanTiltY">PanTilt Y Axis</param>
        public static void MovePanTilt(int PanTiltX, int PanTiltY) // Doing servo stuff is so darn easy
        {
            var PanX = GetServoValue(PanTiltX);
            var PanY = GetServoValue(PanTiltY);
            mh._pwm.SetPwm(15, 0, PanX);
            mh._pwm.SetPwm(14, 0, PanY);
        }

        public static void ShutDown()
        {
            Console.WriteLine("Shutting Down");
            Console.WriteLine("Switching Server To Client communication off");
            Console.WriteLine("Switching of Motor and Servo hat outputs");
            DC1Helper.Stop();
            DC2Helper.Stop();
            DC3Helper.Stop();
            DC4Helper.Stop();
            for (var i = 0; i < 12; i++)
            {
                mh._pwm.SetFull(i, false);
            }
            Console.WriteLine("If no error then outputs have been disabled");
            Console.WriteLine("Bye bye");
            Environment.Exit(1);
        }

        public static void ResetMotorHat()
        {
            DC1Helper = null;
            DC2Helper = null;
            DC3Helper = null;
            DC4Helper = null;
            mh = null;
            driver = null;
            driver = new I2cDriver(ConnectorPin.P1Pin3.ToProcessor(), ConnectorPin.P1Pin5.ToProcessor());
            mh = new Motor_Hat(driver, 0x60, 50);
            DC1Helper = new MotorHelper(mh.GetMotor(1));
            DC2Helper = new MotorHelper(mh.GetMotor(2));
            DC3Helper = new MotorHelper(mh.GetMotor(3));
            DC4Helper = new MotorHelper(mh.GetMotor(4));
            Console.WriteLine("Resetted Motor Hat");
        }
    }
}