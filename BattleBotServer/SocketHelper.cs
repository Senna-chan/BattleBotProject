using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BattleBotServer
{
    public class SocketHelper
    {
        private IPEndPoint ipendpoint;
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public SocketHelper(IPAddress ipaddress)
        {
            connectToSocket(new IPEndPoint(ipaddress, MainClass.BattleBotClientPort));
        }

        private void connectToSocket(IPEndPoint IP)
        {
            try
            {
                socket.Connect(IP);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
            ipendpoint = IP;
        }

        public void SendToServer(string dataToSend)
        {
            Reconnect();
            var data = Encoding.ASCII.GetBytes(dataToSend);
            socket.Send(data);
            Console.WriteLine("Data send! Data: \"" + dataToSend + "\"");
            socket.Close();
            Thread.Sleep(50);
        }

        public void Reconnect()
        {
            socket.Close();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(ipendpoint);
            }
            catch
            {
                Console.WriteLine(
                    "Couldn't connect to the server.\nCheck if internet still works or if the server isn't down");
            }
        }
    }
}