using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using BattleBotClientWin10IoT.Helpers;

namespace BattleBotClientWin10IoT.Modules
{
    public class RecievedDataEventArgs : EventArgs
    {
        public byte[] RecievedBytes { get; set; }
    }
    class WiFiCommunication
    {
        public delegate void RecievedDataEventHandler(object source, RecievedDataEventArgs args);
        public event RecievedDataEventHandler RecievedData;
        protected virtual void OnDataRecieved()
        {
            RecievedData?.Invoke(this, new RecievedDataEventArgs() { RecievedBytes = asyncBuffer });
        }
        private UdpClient BattleBotConnection;
        private IPEndPoint battlebotIp;
        private byte[] buffer = new byte[200];
        private byte[] asyncBuffer = new byte[200];

        private CancellationTokenSource CancelRecieving = new CancellationTokenSource();
        private CancellationTokenSource CancelSending   = new CancellationTokenSource();
        public WiFiCommunication()
        {
            BattleBotConnection = new UdpClient();
            BattleBotConnection.Client.Connect(VariableStorage.EspAddress, 20010);
            SendHandshake();
        }

        public void StartCommunicating()
        {
            Send("client:pc:continued"); // Now we start with communicating
            new Task(() => RecieveData(), CancelRecieving.Token, TaskCreationOptions.LongRunning).Start();
            new Task(() => SendMotorData(), CancelSending.Token,   TaskCreationOptions.LongRunning).Start();
        }

        private void SendHandshake()
        {
            var oldTimeout = BattleBotConnection.Client.ReceiveTimeout;
            BattleBotConnection.Client.ReceiveTimeout = 500;
            Send("client:pc:connected");
            try
            {
                var recievedBytes = BattleBotConnection.Client.Receive(buffer);
            }
            catch (Exception e)
            {
                throw e.GetBaseException();
            }
            Send("client:pc:paused"); // We want to connect but not send data immidiatly
            BattleBotConnection.Client.ReceiveTimeout = oldTimeout;
        }

        public void SendDriveCommand()
        {
            VariableStorage.JoyStick.CalculateSpeeds();
            var strmessage = $"DC:{VariableStorage.ViewModel.LeftMotorSpeed},{VariableStorage.ViewModel.RightMotorSpeed}:{VariableStorage.JoyStick.GetPanValue()},{VariableStorage.JoyStick.GetTiltValue()}";
            var bytemessage = Encoding.ASCII.GetBytes(strmessage);
            BattleBotConnection.SendAsync(bytemessage, bytemessage.Length, battlebotIp);
        }

        public void StopRecievingData()
        {
            CancelRecieving.Cancel();
        }
        private async Task RecieveData()
        {
            var socketArgs = new SocketAsyncEventArgs();
            socketArgs.SetBuffer(asyncBuffer,0,asyncBuffer.Length);
            while (!CancelRecieving.Token.IsCancellationRequested)
            {
                var something = await BattleBotConnection.ReceiveAsync();
                asyncBuffer = something.Buffer;
                OnDataRecieved();
            }
        }

        private async Task SendMotorData()
        {
            while (!CancelSending.Token.IsCancellationRequested)
            {
                SendDriveCommand();
                CancelSending.Token.WaitHandle.WaitOne(Settings.GetIntSetting("waittime"));
            }
        }

        public void StopCommunication()
        {
            StopRecievingData();
            Send("client:pc:disconnect");
            BattleBotConnection.Dispose();
        }

        public void Send(string command)
        {
            BattleBotConnection.Client.Send(Encoding.ASCII.GetBytes(command));
        }
    }
}
