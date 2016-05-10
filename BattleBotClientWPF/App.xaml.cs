using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using BattleBotClientWPF.Helpers;
using BattleBotClientWPF.Properties;
using BattleBotClientWPF.Views;
using OpenTK.Input;

namespace BattleBotClientWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static int BattleBotServerPort = 20010;
        public static int BattleBotClientPort = 20011;
        public static int sleepTime = 50;
        public static SerialPort ArduinoPort;
        public static Thread controllerThread;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static MainWindow x;

        public static ClientToServerSocketHandler ClientToServerSocketHandler = new ClientToServerSocketHandler();
        public static ControllerHandling ControllerHandler = new ControllerHandling();
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            x = new MainWindow();
            
            var socketServerThread = new Thread(ClientToServerSocketHandler.StartSocketHandling);
            //socketServerThread.Start();

            setupSocketConnection();
            SetupFirstRun();
            Console.Beep();

            var controller = Settings.Default.ControllerConfig;
            if (controller == "XboxInput")
            {
                controllerThread = new Thread(ControllerHandler.StartXBoxControllerHandling);
            }
            else /*if (controller == "ArduinoSerial")*/
            {
                controllerThread = new Thread(ControllerHandler.StartArduinoSerialControllerHandling);
            }
            controllerThread.Start();
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow = MainWindow;
        }
        public static void changeControllerHandling()
        {
            ControllerHandler.RequestStop();
            controllerThread.Abort();
            var controller = Settings.Default.ControllerConfig;
            if (controller == "XboxInput")
            {
                controllerThread = new Thread(ControllerHandler.StartXBoxControllerHandling);
            }
            else /*if (controller == "ArduinoSerial")*/
            {
                controllerThread = new Thread(ControllerHandler.StartArduinoSerialControllerHandling);
            }
            controllerThread.Start();
            ControllerHandler.RequestStart();
        }
        public class ControllerHandling
        {
            public static ControllerHandler Controller;
            public static Dictionary<string, ButtonState> OldPSButtons;
            public static int freq;
            public static byte[] vars = { 0, 0, 0 };
            public static byte controllerMode;

            public void StartArduinoSerialControllerHandling()
            {
                ArduinoPort = new SerialPort("COM3", 9600);
                if (!ArduinoPort.IsOpen)
                {
                    if (check_connection())
                    {
                        while (!_shouldStop)
                        {
                            ArduinoPort.Open();
                            var data = ArduinoPort.ReadTo("!");
                            ArduinoPort.Close();
                            data = data.Replace("1,5?", "").Replace("!", "");
                            var cmd = data.Split('=');
                            if (cmd.Length != 2) continue;
                            VariableStorage._socketHelper.SendToServer(cmd[0]);
                            Thread.Sleep(0);
                            VariableStorage._socketHelper.SendToServer(cmd[1]);
                            Thread.Sleep(10);
                        }
                    }
                }
            }
            static bool check_connection()
            {
                if (!ArduinoPort.IsOpen)
                {
                    try
                    {
                        ArduinoPort.Open();
                        ArduinoPort.WriteLine("T");
                        ArduinoPort.Close();
                    }
                    catch
                    {
                        MessageBox.Show("There was an error. Please make sure that the correct port was selected, and that the device is plugged in.");
                        return false;
                    }
                    return true;
                }
                else
                {

                    return true;
                }
            }

            public void StartXBoxControllerHandling()
            {
                while (!_shouldStop)
                {
                    if (VariableStorage.ipaddress != null)
                    {
                        Controller = new ControllerHandler(OldPSButtons, freq, "Tank", controllerMode);
                        var speed = Controller.GetSpeed();
                        var wheelpos1 = Controller.GetWheelPos1();
                        var wheelpos2 = Controller.GetWheelPos2();
                        OldPSButtons = Controller.GetPsButtons();
                        vars = Controller.GetVars();
                        SendControllerData(speed, wheelpos1, wheelpos2, Controller.GetServoX(), Controller.GetServoY());
                        VariableStorage.ViewModel.Gear = vars[1].ToString();
                        VariableStorage.ViewModel.Speed = speed.ToString();
                        VariableStorage.ViewModel.WheelSpeedLeft = wheelpos1.ToString();
                        VariableStorage.ViewModel.WheelSpeedRight = wheelpos2.ToString();
                        Thread.Sleep(20); // We do not set a changeble time here for a realistic feeling of things that the "motor" does not respond well
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

            private volatile bool _shouldStop;
        }

        public static void SendControllerData(int speed, int WheelPos1, int WheelPos2, int PanTiltX, int PanTiltY)
        {
            VariableStorage._socketHelper.SendToServer($"DC:{speed},{WheelPos1},{WheelPos2}:{PanTiltX},{PanTiltY}");
        }

        public void ExitProgram()
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Shutdown();
        }

        public static void setupSocketConnection()
        {
            var dialog = new TextDialog("Enter IP Address", "Enter IP", Settings.Default.IPAddress);
            dialog.ShowDialog();
            if (dialog.DialogResult.Value && dialog.DialogResult.Value)
            {
                string hostName = dialog.GetEnteredText();
                IPAddress IP;
                IPAddress.TryParse(hostName, out IP);
                VariableStorage._socketHelper = new SocketHelper(IP);
                VariableStorage.ipaddress = Settings.Default.IPAddress;
                VariableStorage._socketHelper.SendToServer("CLIENT:CONNECTED");
                var Mainwindow = new MainWindow();;
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                Current.MainWindow = Mainwindow;
                VariableStorage.ViewModel.WebAddress = VariableStorage.ipaddress;
                Mainwindow.Show();
                
            }
            else
            {
                MessageBox.Show("Closing Program");
                BattleBotClientWPF.MainWindow.CloseAppS(false);
            }
        }

        private static void SetupFirstRun()
        {
            Settings settings = Settings.Default;
            if (!settings.FirstRun) return;
            //settings.FirstRun = false;
            settings.HealthColor = "#FF088925";
            settings.ArmorColor = "#FF11144D";
            settings.MotorConfig = "Tank";
            settings.ControllerConfig = "XboxInput";
            settings.Frequency = 500;
            settings.Save();
        }
    }
}
 