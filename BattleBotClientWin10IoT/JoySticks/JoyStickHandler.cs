using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Gaming.Input;
using BattleBotClientWin10IoT.Helpers;
using BattleBotClientWin10IoT.Interfaces;

namespace BattleBotClientWin10IoT.JoySticks
{
    class JoyStickHandler
    {
        public IJoyStickInterface CJoyStick;
        public DeviceInformationCollection BluetoothDevices { get; set; }
        private CancellationTokenSource CancelPolling = new CancellationTokenSource();
        private bool ShouldStartPolling = false;
        public JoyStickHandler()
        {
            ConnectToAJoystick();
            PollController();
        }

        private void PollController()
        {
            new Task(PollControllerTask, CancelPolling.Token, TaskCreationOptions.LongRunning).Start();
        }

        private void PollControllerTask()
        {
            while (!CancelPolling.Token.IsCancellationRequested)
            {
                if (ShouldStartPolling)
                {
                    CJoyStick.GetControllerData();
                }
                CancelPolling.Token.WaitHandle.WaitOne(10);
            }
        }

        private async void ConnectToAJoystick()
        {
            if (VariableStorage.DeviceFormFactor == DeviceFormFactorType.Desktop)
            {
                await GetBluetoothDevices();
                if (BluetoothDevices != null && BluetoothDevices.Count > 1)
                {
                    Debug.WriteLine("We got some bluetooth devices");
                    CJoyStick = new PS4JoyStick();
                }
                else
                {
                    var dialog = new Windows.UI.Popups.MessageDialog("Could not find a PS4 controller. Are you just testing? If not then connect the PS4 controller and press No.");

                    dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
                    dialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 1 });

                    var result = await dialog.ShowAsync();
                    if ((int) result.Id == 0)
                    {
                        CJoyStick = new KeyboardJoystick();
                    }
                    else
                    {
                        await GetBluetoothDevices();
                        if (BluetoothDevices != null && BluetoothDevices.Count > 1)
                        {
                            Debug.WriteLine("We got some bluetooth devices");
                            CJoyStick = new PS4JoyStick();
                        }
                        else
                        {
                            dialog = new Windows.UI.Popups.MessageDialog("Still no PS4 controller. Bye");
                            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Ok"));
                            await dialog.ShowAsync();
                            App.Current.Exit();
                        }

                    }
                }
            }
            else if (VariableStorage.DeviceFormFactor == DeviceFormFactorType.IoT)
            {
                CJoyStick = new PiJoystick();
            }
        }

        private async Task GetBluetoothDevices()
        {
//            BluetoothDevices = await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelector());
//            var devicess = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
//            DeviceInformationCollection collection = await DeviceInformation.FindAllAsync();
        }

    }
}
