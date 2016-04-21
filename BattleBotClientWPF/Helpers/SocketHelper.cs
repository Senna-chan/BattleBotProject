using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using BattleBotClientWPF.Views;

namespace BattleBotClientWPF
{
	public class SocketHelper
    {
        private Socket _socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private IPEndPoint _ipendpoint;
        public SocketHelper(IPAddress ipaddress)
        {
            ConnectToSocket(new IPEndPoint(ipaddress, App.BattleBotServerPort));
        }
        private void ConnectToSocket(IPEndPoint ip)
        {
            
            try
            {
                _socket.Connect(ip);
            }
            catch (Exception e)
            {
                var dialog = new OkCancelDialog();
                dialog.Message.Text = e.Message + "\nDo you want to reconnect?";
                dialog.CancelButton.Content = "No";
                dialog.OkButton.Content = "Yes";
                dialog.ShowDialog();
                if (dialog.DialogResult.Value && dialog.DialogResult.Value)
                {
                    App.setupSocketConnection();
                }
                else
                {
                    MainWindow.CloseAppS();
                }
            }
            Properties.Settings.Default.IPAddress = ip.Address.ToString();
            Properties.Settings.Default.Save();
            _ipendpoint = ip;
        }
        
//        public void SendToServer(string dataToSend)
//        {
//            if (Reconnect())
//            {
//                byte[] data = Encoding.ASCII.GetBytes(dataToSend);
//                _socket.Send(data);
//                Console.WriteLine("Data send! Data: \"" + dataToSend + "\"");
//                _socket.Close();
//                Thread.Sleep(App.sleepTime);
//            }
//        }

        public void SendToServer(string dataToSend)
        {
//            if (Reconnect())
//            {
                byte[] data = Encoding.ASCII.GetBytes(dataToSend);
                _socket.Send(data);
                Console.WriteLine("Data send! Data: \"" + dataToSend + "\"");
//                _socket.Close();
                Thread.Sleep(App.sleepTime);
//            }
        }

        public void Close()
        {
            _socket.Close();
        }

        public bool Reconnect()
        {
            _socket.Close();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                _socket.Connect(_ipendpoint);
            }
            catch
            {
                Console.WriteLine("Error reconnecting to the socket");
                return false;
            }
            return true;
        }
	}
}

