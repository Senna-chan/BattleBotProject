using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BattleBotClientWPF.Helpers;

namespace BattleBotClientWPF
{
    public class ClientToServerSocketHandler
    {
        public static Socket socket;
        private static Socket accepted = null;
        private static byte[] Buffer;

        public static void StartSocketHandling()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(0, App.BattleBotClientPort));
            while (true)
            {
                socket.Listen(100);
                accepted = socket.Accept();
                Buffer = new byte[accepted.SendBufferSize];
                int bytesRead = accepted.Receive(Buffer);
                byte[] formatted = new byte[bytesRead];
                for (int i = 0; i < bytesRead; i++)
                {
                    formatted[i] = Buffer[i];
                }

                string strData = Encoding.ASCII.GetString(formatted);
                string[] typcom = strData.Split(':');
                if (typcom.Length == 2 || typcom.Length == 3)
                {
                    string commandtype = typcom[0];
                    string command = typcom[1];
                    if (typcom.Length == 3)
                    {
                        string parameters = typcom[2];
                    }

                    if (commandtype == "SYS")
                    {

                        if (VariableStorage.ViewModel != null)
                        {
                            var values = command.Split('|');
                            VariableStorage.ViewModel.Temperature = values[0];
                            VariableStorage.ViewModel.Load = values[1];
                            VariableStorage.ViewModel.Ram = values[2];
                            VariableStorage.ViewModel.WheelSpeedSliderLeft = int.Parse(values[3]);
                            VariableStorage.ViewModel.WheelSpeedSliderRight = int.Parse(values[4]);
                        }
                    }

                }
                else
                {
                    Console.WriteLine("Can't parse \"" + strData + "\" as a command.");
                }
                Thread.Sleep(50);
            }
        }
        public void RequestStop()
        {
            _shouldStop = true;
        }

        private volatile bool _shouldStop;
    }
}
