using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BattleBotClientWin10IoT.Helpers;
using BattleBotClientWin10IoT.Modules;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BattleBotClientWin10IoT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InitializingPage : Page
    {
        public InitializingPage()
        {
            InitializeComponent();
            DataContext = VariableStorage.ViewModel;
            SetupSettings();
            Initialize();
        }

        private void SetupSettings()
        {
            var firstrun = Settings.GetStringSetting("firstrun");
            if (firstrun != string.Empty)
            {
                Settings.SaveSetting("firstrun", "false");
                Settings.SaveSetting("waittime", 30);
                Settings.SaveSetting("realenginehandling", true);
                Settings.SaveSetting("debug", false);
            }
        }

        private async void Initialize()
        {
            await InitMDNS();
            await VariableStorage.JoyStick.ConnectToAJoystick();
            InitWirelessCommunication();
            Frame.Navigate(typeof(MainPage));
        }
        private async Task InitMDNS()
        {
            mDNS.mDNS client = new mDNS.mDNS();
            await client.Init();
            int counter = 0;
            bool shouldConnectToCamera = true;
            do
            {
                var services = await client.List("_workstation._tcp.local.");
                await Task.Delay(250);
                if (services != null && services.Length != 0)
                {
                    foreach (var service in services)
                    {
                        if (service.Server.StartsWith("battlebotcamera"))
                        {
                            VariableStorage.BattlebotCameraAddress = services[0].Address;
                            VariableStorage.ViewModel.CameraStatus = "Camera found on: " + services[0].HostAddress;
                            shouldConnectToCamera = false;
                        }
                    }
                }
                else
                {
                    if (counter == 0)
                    {
                        VariableStorage.ViewModel.CameraStatus = "Camera not found";
                        if (Settings.GetBoolSetting("debug"))
                        {
                            shouldConnectToCamera = false;
                            continue;
                        }
                        var dialog =
                            new MessageDialog("Could not find the camera. Do you want to continue without camera?");

                        dialog.Commands.Add(new UICommand("Yes") { Id = 0 });
                        dialog.Commands.Add(new UICommand("No, try again") { Id = 1 });

                        var result = await dialog.ShowAsync();
                        if ((int)result.Id == 0) shouldConnectToCamera = false;
                    }
                    if (counter == 10)
                    {
                        var dialog = new MessageDialog("No camera for you.");
                        dialog.Commands.Add(new UICommand("Ok"));
                        await dialog.ShowAsync();
                        VariableStorage.ViewModel.CameraStatus = "Camera not found";
                        shouldConnectToCamera = false;
                    }
                    await Task.Delay(250);
                    counter++;
                }
            } while (shouldConnectToCamera);
            bool connectedToEsp = false;
            counter = 0;
            while (!connectedToEsp)
            {

                var services = await client.List("_battlebot._udp.local.");
                await Task.Delay(250);
                if (services.Length == 1) // This is garantied to be the ESP
                {
                    VariableStorage.EspAddress = services[0].Address;
                    VariableStorage.ViewModel.EspStatus = "Esp found on: " + services[0].HostAddress;
                    connectedToEsp = true;
                }

                if (counter == 0)
                {
                    if (Settings.GetBoolSetting("debug"))
                    {
                        VariableStorage.ViewModel.EspStatus = "Esp found on: 192.168.1.114 in Testing mode";
                        VariableStorage.EspAddress = IPAddress.Parse("192.168.1.114");
                        connectedToEsp = true;
                        continue;
                    }
                    var dialog = new MessageDialog("I can't seem to find the robot, please check the wifi and wires and try again.\r\nOr are you testing this?");

                    dialog.Commands.Add(new UICommand("Try again") { Id = 0 });
                    dialog.Commands.Add(new UICommand("I'm testing") { Id = 1 });
                    dialog.Commands.Add(new UICommand("Enter manualy"){Id =2});
                    var result = await dialog.ShowAsync();
                    if ((int)result.Id == 1)
                    {
                        VariableStorage.ViewModel.EspStatus = "Esp found on: 127.0.0.1 in Testing mode";
                        VariableStorage.EspAddress = IPAddress.Parse("127.0.0.1");
                        connectedToEsp = true;
                    }
                }
                if (counter == 10)
                {
                    var dialog = new MessageDialog("Too much lazing around. I can't seem to connect to the robot. Check the network for any problems and press Ok to close/restart.");

                    dialog.Commands.Add(new UICommand("Ok") { Id = 0 });

                    await dialog.ShowAsync();
                    Application.Current.Exit();
                }
                await Task.Delay(250);
                counter++;
            }
        }

        private void InitWirelessCommunication()
        {
            VariableStorage.BattleBotCommunication = new WiFiCommunication();
        }
    }
}
