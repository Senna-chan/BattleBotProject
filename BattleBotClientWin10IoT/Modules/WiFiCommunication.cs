using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleBotClientWin10IoT.Modules
{
    class WiFiCommunication
    {
        public delegate void RecievedDataEventHandler(object source, EventArgs args);
        public event RecievedDataEventHandler RecievedData;
        protected virtual void OnDataRecieved()
        {
            RecievedData?.Invoke(this, EventArgs.Empty);
        }



        public WiFiCommunication()
        {
            
        }

    }
}
