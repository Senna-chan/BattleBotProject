using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using BattleBotServer.Firmata;
using BattleBotServer.Models;
using Firmata.NET;

namespace BattleBotServer
{
    internal class MainClass
    {
        private static Socket socket;
        private static Socket accepted;
        private static int speed;
        private static int wheelPos1;
        private static int wheelPos2;
        private static int WheelPos;
        private static int freq = 500;
        private static IPAddress clientIP;

        public static int BattleBotServerPort = 20010;
        public static int BattleBotClientPort = 20011;

        public static int sleepTime = 50;
        public static LogWriter logWriter;

        public static volatile int LeftMotorSpeed, RightMotorSpeed;

        public static string MotorConfig = "Tank";
        public static SocketHelper socketHelper;
        public static CheckConnectionToClient CheckConnectionToClientObject;
        public static DamageModels Damage = new DamageModels();
        public static Random random = new Random();
        public static bool DoDamageSimulation = false;
        public static int DamageTimer;
        private static byte[] Buffer { get; set; }
        public static ArduinoBridge ArduinoBridge;
        private static int timeSinceLastCommand;
        private static int servoX, servoY;
        public static void Main(string[] args)
        { 
            //Helpers.MotorTest();
            Console.CancelKeyPress += Console_CancelKeyPress;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(0, BattleBotServerPort));
            logWriter = LogWriter.Instance;
            CheckConnectionToClientObject = new CheckConnectionToClient();
            ArduinoBridge = new ArduinoBridge();
//            var arduinoBridgeThread = new Thread(ArduinoBridge.PrintAnalog);
            var checkConnectionThread = new Thread(CheckConnectionToClientObject.CountTimeSinceLastCommand);
            //arduinoBridgeThread.Start();
            if (args.Length != 0)
            {
                if (args[0] == "TestMotors")
                {
                    Helpers.TestMotors();
                } 
            }
            Console.WriteLine("Awaiting Data");
            while (true)
            {
                socket.Listen(100);
                accepted = socket.Accept();
                if (clientIP == null)
                {
                    var clientIPEndPoint = accepted.RemoteEndPoint as IPEndPoint;
                    clientIP = clientIPEndPoint.Address;
                    //socketHelper = new SocketHelper(clientIPEndPoint);
                    checkConnectionThread.Start();
                }
                Buffer = new byte[accepted.SendBufferSize];
                var bytesRead = accepted.Receive(Buffer);
                var formatted = new byte[bytesRead];
                for (var i = 0; i < bytesRead; i++)
                {
                    formatted[i] = Buffer[i];
                }
                var strData = Encoding.ASCII.GetString(formatted);
                Console.WriteLine(strData);
                var typcom = strData.Split(':');
                if (typcom.Length == 2 || typcom.Length == 3)
                {
                    Console.WriteLine("Normal command recieved");
                    Console.WriteLine($"Command: {strData}");
                    var commandtype = typcom[0];
                    var command = typcom[1].ToLower();
                    string parameters = null;
                    if (typcom.Length == 3)
                    {
                        parameters = typcom[2];
                    }
                    if (commandtype == "CLIENT")
                    {
                        if (command == "disconnected")
                        {
                            CheckConnectionToClientObject.RequestStop();
                        }
                        if (command == "connected")
                        {
                            CheckConnectionToClientObject.RequestStart();
                        }
                    }
                    if (commandtype == "DC") // We are dealing with drive commands. 
                    {
                        if (typcom.Length == 3)
                        {
                            var motorpart = command.Split(',');
                            var servopart = parameters.Split(',');
                            if (motorpart.Length == 3)
                            {
                                speed = int.Parse(motorpart[0]);
                                wheelPos1 = int.Parse(motorpart[1]);
                                wheelPos2 = int.Parse(motorpart[2]);
                            }
                            else
                            {
                                Console.WriteLine("Error, can't parse motor command \"" + command + "\"");
                            }
                            if (servopart.Length == 2)
                            {
                                servoX = int.Parse(servopart[0]);
                                servoY = int.Parse(servopart[1]);
                            }
                            else
                            {
                                Console.WriteLine("Error, can't parse servo command \"" + parameters + "\"");
                            }
                            Helpers.motorCalcsTank(speed, wheelPos1, wheelPos2, servoX, servoY);
                        }
                        else
                        {
                            Console.WriteLine("Error, can't parse drive command \"" + strData + "\"");
                        }
                    }
                    if (commandtype == "L") // We are dealing with something we need to log server sided. 
                    {
                        Console.WriteLine("LOG: " + command);
                        if (parameters != null)
                        {
                            logWriter.WriteToLog(parameters, command);
                        }
                        else
                        {
                            logWriter.WriteToLog(command);
                        }
                    }
                    if (commandtype == "C")
                        // We are dealing with a system command. The array can be more than 2 if parameters are added
                    {
                        if (command == "shutdown")
                        {
                            Console.WriteLine("Ima shutting down");
                        }
                        if (command == "reboot")
                        {
                            Console.WriteLine("REEEEEEEEEEBOOT");
                        }
                        if (command == "exit")
                        {
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Can't parse \"" + strData + "\" as a command.");
                }
                // Ok we processed all our data, so lets manipulate that data to simulate damage.
                if (Damage.ControllUnit >= 25) // Control unit damage, Changes the sleeptime of the loop
                {
                    var temp = random.Next(50, Damage.ControllUnit + 50)*1.20;
                    sleepTime = (int) Math.Floor(temp);
                }
                if (DoDamageSimulation)
                {
                    if (Damage.Motor >= 25 && MotorConfig == "Normal")
                    {
                        if (speed > 0)
                        {
                            speed -= random.Next(0, Damage.Motor);
                            if (speed < 0)
                            {
                                speed = 0;
                            }
                        }
                        if (speed < 0)
                        {
                            speed += random.Next(0, Damage.Motor);
                            if (speed > 0)
                            {
                                speed = 0;
                            }
                        }
                    }

                    if (Damage.LeftMotor >= 25 && MotorConfig == "Tank")
                    {
                        if (wheelPos1 > 0)
                        {
                            wheelPos1 -= random.Next(0, Damage.LeftMotor);
                            if (wheelPos1 < 0)
                            {
                                wheelPos1 = 0;
                            }
                        }
                        if (wheelPos1 < 0)
                        {
                            wheelPos1 += random.Next(0, Damage.LeftMotor);
                            if (wheelPos1 > 0)
                            {
                                wheelPos1 = 0;
                            }
                        }
                    }
                    if (Damage.RightMotor >= 25 && MotorConfig == "Tank")
                    {
                        if (wheelPos2 > 0)
                        {
                            wheelPos2 -= random.Next(0, Damage.RightMotor);
                            if (wheelPos2 < 0)
                            {
                                wheelPos2 = 0;
                            }
                        }
                        if (wheelPos2 < 0)
                        {
                            wheelPos2 += random.Next(0, Damage.RightMotor);
                            if (wheelPos2 > 0)
                            {
                                wheelPos2 = 0;
                            }
                        }
                    }
                    DoDamageSimulation = false;
                }
                else
                {
                    if (DamageTimer == 10)
                    {
                        DoDamageSimulation = true;
                    }
                    DamageTimer++;
                }

                int[] motorSpeeds = {0, 0, 0};
                CheckConnectionToClientObject.timeSinceLastCommand = 0;
//                LeftMotorSpeed = motorSpeeds[1];
//                RightMotorSpeed = motorSpeeds[2];
//                var temperature = Helpers.GetPiTemp();
//                var load = Helpers.GetPiLoad();
//                socketHelper.SendToServer($"SYS:{temperature}|37%|{load}|{LeftMotorSpeed}|{RightMotorSpeed}");
            }
            socket.Close();
            accepted.Close();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Helpers.ShutDown();
        }

        public class CheckConnectionToClient
        {
            private volatile bool _shouldStop;
            public volatile int timeSinceLastCommand = 0;
            public void RequestStop()
            {
                _shouldStop = true;
            }

            public void RequestStart()
            {
                _shouldStop = false;
            }

            public void CountTimeSinceLastCommand()
            {
                while (!_shouldStop)
                {
                    Thread.Sleep(1);
                    timeSinceLastCommand++;
                    if (timeSinceLastCommand > 151)
                    {
                        Console.WriteLine("Didn't recieve a command for 0.151 seconds");
                        Helpers.motorCalcsTank(0, 0, 0,0,0);
                        timeSinceLastCommand = 0;
                    }
                }
            }
        }
    }
}