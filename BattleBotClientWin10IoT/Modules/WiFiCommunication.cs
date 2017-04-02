using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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
        public string RecievedString{ get; set; }
    }
    class WiFiCommunication
    {
        public delegate void RecievedDataEventHandler(object source, RecievedDataEventArgs args);
        public event RecievedDataEventHandler RecievedData;
        public delegate void RecievedErrorDataEventHandler(object source, RecievedDataEventArgs args);
        public event RecievedErrorDataEventHandler RecievedErrorData;
        protected virtual void OnDataRecieved()
        {
            if (RecievedString.StartsWith("error"))
            {
                RecievedErrorData?.Invoke(this, new RecievedDataEventArgs() { RecievedString = RecievedString });
            }
            else
            { 
                RecievedData?.Invoke(this, new RecievedDataEventArgs() { RecievedString = RecievedString });
            }
        }
        private UdpClient BattleBotConnection;
        private IPEndPoint battlebotIp;
        private byte[] buffer = new byte[200];
        private string RecievedString = String.Empty;

        private CancellationTokenSource CancelRecieving = new CancellationTokenSource();
        private CancellationTokenSource CancelSending = new CancellationTokenSource();
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
            new Task(() => SendMotorData(), CancelSending.Token, TaskCreationOptions.LongRunning).Start();
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
            var asyncBuffer = new byte[128];
            var socketArgs = new SocketAsyncEventArgs();
            socketArgs.SetBuffer(asyncBuffer, 0, asyncBuffer.Length);
            while (!CancelRecieving.Token.IsCancellationRequested)
            {
                var something = await BattleBotConnection.ReceiveAsync();
                asyncBuffer = something.Buffer;
                RecievedString = Encoding.ASCII.GetString(asyncBuffer);
                OnDataRecieved();
            }
        }

        private async Task SendMotorData()
        {
            while (!CancelSending.Token.IsCancellationRequested)
            {
                SendDriveCommand();
                await Task.Delay(30);
                //CancelSending.Token.WaitHandle.WaitOne(30);
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
