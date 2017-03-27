using System.Net;
using BattleBotClientWin10IoT.JoySticks;
using BattleBotClientWin10IoT.Modules;
using BattleBotClientWin10IoT.ViewModels;
using mDNS;
using MjpegProcessor;

namespace BattleBotClientWin10IoT.Helpers
{
    class VariableStorage
    {
        public static volatile MainViewModel ViewModel = new MainViewModel();
        public static DeviceFormFactorType DeviceFormFactor = DeviceTypeHelper.GetDeviceFormFactorType();
        public static JoyStickHandler JoyStick = new JoyStickHandler();
        public static MjpegDecoder MjpegDecoder = new MjpegDecoder();
        public static WiFiCommunication BattleBotCommunication;
//        public static MCP3008 mcp3008;
        public static ServiceInfo[] DiscoveredServices;
        public static IPAddress BattlebotCameraAddress;
        public static IPAddress EspAddress;
    }
}
