using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleSocketServer
{
    internal class Program
    {
        private static Socket socket;
        private static Socket accepted;
        private static IPAddress clientIP;

        public static int BattleBotServerPort = 20010;
        private static byte[] Buffer { get; set; }

        public static void Main(string[] args)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(0, BattleBotServerPort));
            Console.WriteLine("Awaiting Data");
            socket.Listen(1000);
            accepted = socket.Accept();
            while (true)
            {
                Console.WriteLine("Client Connected");
                if (clientIP == null)
                {
                    var clientIPEndPoint = accepted.RemoteEndPoint as IPEndPoint;
                    clientIP = clientIPEndPoint.Address;
                    
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
                if (strData == "exit") break;
            }
            socket.Close();
            accepted.Close();
        }
        public static long map(long x, long in_min, long in_max, long out_min, long out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
    }
}
