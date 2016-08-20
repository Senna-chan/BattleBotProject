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

        public SocketHelper(IPEndPoint clientIP)
        {
            connectToSocket(clientIP);
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
            var data = Encoding.ASCII.GetBytes(dataToSend);
            try
            {
                socket.Send(data);
                Console.WriteLine("Data send! Data: \"" + dataToSend + "\"");
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't send the command.\nCheck if the client didn't crash");
            }
            Thread.Sleep(50);
        }
    }
}