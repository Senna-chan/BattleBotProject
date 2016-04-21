using System;
using System.Net;
using System.Net.Sockets;
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
        public static SocketServerToClient ServerToClientObject;
        public static CheckConnectionToClient CheckConnectionToClientObject;
        public static DamageModels Damage = new DamageModels();
        public static Random random = new Random();
        public static bool DoDamageSimulation = false;
        public static int DamageTimer;
        private static byte[] Buffer { get; set; }
        public static ArduinoBridge ArduinoBridge;
        private static int timeSinceLastCommand;
        public static void Main(string[] args)
        {
            //Helpers.MotorTest();
            Console.CancelKeyPress += Console_CancelKeyPress;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(0, BattleBotServerPort));
            logWriter = LogWriter.Instance;
            ServerToClientObject = new SocketServerToClient();
            CheckConnectionToClientObject = new CheckConnectionToClient();
            ArduinoBridge = new ArduinoBridge();
            var arduinoBridgeThread = new Thread(ArduinoBridge.PrintAnalog);
            var serverToClientThread = new Thread(ServerToClientObject.StartCom);
            var checkConnectionThread = new Thread(CheckConnectionToClientObject.CountTimeSinceLastCommand);
            //arduinoBridgeThread.Start();
            Console.WriteLine("Awaiting Data");
            socket.Listen(100);
            accepted = socket.Accept();
            while (true)
            {
//                if (!Helpers.CheckWiFiUplink())
//                {
////                    Helpers.DC1Helper.Stop();
////                    Helpers.DC2Helper.Stop();
////                    Helpers.DC3Helper.Stop();
////                    Helpers.DC4Helper.Stop();
////                    continue;
//                }

                

                if (clientIP == null)
                {
                    var clientIPEndPoint = accepted.RemoteEndPoint as IPEndPoint;
                    clientIP = clientIPEndPoint.Address;
                    //serverToClientThread.Start();
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
                            ServerToClientObject.RequestStop();
                        }
                        if (command == "connected")
                        {
                            ServerToClientObject.RequestStart();
                        }
                    }
                    if (commandtype == "MC") // We are dealing with motor commands. 
                    {
                        var temp = command.Split(',');
                        if (temp.Length == 4)
                        {
                            speed = int.Parse(temp[0]);
                            wheelPos1 = int.Parse(temp[1]);
                            wheelPos2 = int.Parse(temp[2]);
                            freq = int.Parse(temp[3]);
                            MotorConfig = "Tank";
                        }
                        else if (temp.Length == 3)
                        {
                            speed = int.Parse(temp[0]);
                            WheelPos = int.Parse(temp[1]);
                            freq = int.Parse(temp[2]);
                            MotorConfig = "Normal";
                        }
                        else
                        {
                            Console.WriteLine("Error, can't parse motor command \"" + command + "\"");
                        }
                    }
                    if (commandtype == "SC") // Servo Command
                    {
                        var temp = command.Split(',');
                        Helpers.MovePanTilt(int.Parse(temp[0]), int.Parse(temp[1]));
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
                if (timeSinceLastCommand < 80)
                {
                    if (MotorConfig == "Tank")
                    {
                        motorSpeeds = Helpers.motorCalcsTank(speed, wheelPos1, wheelPos2, freq);
                    }
                    else if (MotorConfig == "Normal")
                    {
                        motorSpeeds = Helpers.motorCalcsNormal(speed, WheelPos, freq);
                    }
                    CheckConnectionToClientObject.timeSinceLastCommand = 0;
                }
                else
                {
                    Console.WriteLine("No command recieved");
                    motorSpeeds = Helpers.motorCalcsTank(0, 0, 0);
                }
                LeftMotorSpeed = motorSpeeds[1];
                RightMotorSpeed = motorSpeeds[2];
                //Thread.Sleep(sleepTime);
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
                Thread.Sleep(1);
                timeSinceLastCommand++;
                if (timeSinceLastCommand > 100)
                {
                    Helpers.motorCalcsTank(0, 0, 0);
                }
            }
        }

        public class SocketServerToClient
        {
            // Volatile is used as hint to the compiler that this data
            // member will be accessed by multiple threads.
            private volatile bool _shouldStop;

            public void StartCom()
            {
                var socketHelper = new SocketHelper(clientIP);
                while (true)
                {
                    while (!_shouldStop)
                    {
                        var temp = Helpers.GetPiTemp();
                        var load = Helpers.GetPiLoad();
                        socketHelper.SendToServer($"SYS:{temp}|37%|{load}|{LeftMotorSpeed}|{RightMotorSpeed}");
                        Thread.Sleep(sleepTime);
                    }
                }
            }

            public void RequestStop()
            {
                _shouldStop = true;
            }

            public void RequestStart()
            {
                _shouldStop = false;
            }
        }
    }
}