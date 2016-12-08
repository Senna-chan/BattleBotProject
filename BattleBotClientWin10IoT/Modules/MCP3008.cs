using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using BattleBotClientWin10IoT.Helpers;

namespace BattleBotClientWin10IoT
{
    public class MCP3008
    {
        private SpiDevice _MCP3008;

        public MCP3008()
        {
            ConnectToMCP3008();
        }

        private async void ConnectToMCP3008()
        {
            var spiSettings = new SpiConnectionSettings(0); //for spi bus index 0
            spiSettings.ClockFrequency = 3600000; //3.6 MHz
            spiSettings.Mode = SpiMode.Mode0;
            string spiQuery = SpiDevice.GetDeviceSelector("SPI0");
            try
            {
                VariableStorage.ViewModel.spiStatus = "Connecting to MCP3008";
                var deviceInfo = await DeviceInformation.FindAllAsync(spiQuery);
                if (deviceInfo != null && deviceInfo.Count > 0)
                {
                    _MCP3008 = SpiDevice.FromIdAsync(deviceInfo[0].Id, spiSettings).GetResults();
                    VariableStorage.ViewModel.spiStatus = "Connected to MCP3008";
                }
                else
                {
                    VariableStorage.ViewModel.spiStatus = "No SPI device found";
                }
            }
            catch (Exception ex)
            {
                VariableStorage.ViewModel.spiStatus = "Not Connected";
                Debug.WriteLine(new Exception("SPI Initialization Failed", ex));
            }
        }

        public int Read(byte adcNumber)
        {
            if (_MCP3008 == null) return -1;
            if (adcNumber > 7)
            {
                return -1;
            }
            byte channel = Convert.ToByte((8 + adcNumber) << 4);
            var transmitBuffer = new byte[3] {1, channel, 0x00};
            var receiveBuffer = new byte[3];

            _MCP3008.TransferFullDuplex(transmitBuffer, receiveBuffer);
            return ((receiveBuffer[1] & 3) << 8) + receiveBuffer[2];
        }
    }
}