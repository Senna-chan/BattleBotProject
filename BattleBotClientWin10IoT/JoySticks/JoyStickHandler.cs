using BattleBotClientWin10IoT.Helpers;
using BattleBotClientWin10IoT.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace BattleBotClientWin10IoT.JoySticks
{
    class JoyStickHandler
    {
        public IJoyStickInterface CJoyStick;
        public DeviceInformationCollection BluetoothDevices { get; set; }
        private CancellationTokenSource CancelPolling = new CancellationTokenSource();
        private int lmTargetSpeed = 0;
        private int rmTargetSpeed = 0;

        public void PollController()
        {
            new Task(PollControllerTask, CancelPolling.Token, TaskCreationOptions.LongRunning).Start();
        }

        public void StopPollingController()
        {
            CancelPolling.Cancel();
        }

        private void PollControllerTask()
        {
            CancelPolling.Token.WaitHandle.WaitOne(1000);
            while (!CancelPolling.Token.IsCancellationRequested)
            {
                CJoyStick.GetControllerData();
                if (CJoyStick.GetSpeedUpGearButtonState() && VariableStorage.ViewModel.SpeedGear != 4)
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        VariableStorage.ViewModel.SpeedGear++;
                    });
                }
                else if (CJoyStick.GetSpeedDownGearButtonState() && VariableStorage.ViewModel.SpeedGear != 1)
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        VariableStorage.ViewModel.SpeedGear--;
                    });
                }
                if (CJoyStick.GetTurnSharperGearButtonState() && VariableStorage.ViewModel.TurnGear != 4)
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        VariableStorage.ViewModel.TurnGear++;
                    });
                }
                if (CJoyStick.GetTurnWeakerGearButtonState() && VariableStorage.ViewModel.TurnGear != 1)
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        VariableStorage.ViewModel.TurnGear--;
                    });
                }
                CJoyStick.PopulateOldButtons();
                CancelPolling.Token.WaitHandle.WaitOne(10);
            }
        }

        public void CalculateSpeeds()
        {
            var speedGearValue = 100 / 4 * VariableStorage.ViewModel.SpeedGear; // Used to simulate gearing(Max speed in gears
            var turnGearValue = 100 / 4 * VariableStorage.ViewModel.TurnGear;
            var speed = GeneralHelpers.MapIntToValue(CJoyStick.GetSpeedAxisPosition(), -100, 100, speedGearValue * -1, speedGearValue);
            int wheelpos = GeneralHelpers.MapIntToValue(CJoyStick.GetTurnAxisPosition(), -100, 100, turnGearValue * -1, turnGearValue);
            int lmSpeed = speed;
            int rmSpeed = speed;
            
            if (speed == 0)
            {
                if (wheelpos > 0)
                {
                    lmSpeed -= wheelpos;
                    rmSpeed += wheelpos;
                }
                else if (wheelpos < 0)
                {
                    lmSpeed -= wheelpos;
                    rmSpeed += wheelpos;
                }
            }
            else if (speed > 0)
            {
                if (wheelpos < 0)
                {
                    lmSpeed -= (wheelpos);
                }
                else if (wheelpos > 0)
                {
                    rmSpeed -= (wheelpos * -1);
                }
            }
            else if (speed < 0)
            {
                if (wheelpos < 0)
                {
                    rmSpeed -= (wheelpos * -1);
                }
                else if (wheelpos > 0)
                {
                    lmSpeed -= wheelpos;
                }
            }
            int oldlmSpeed = 100;
            int oldrmSpeed = 100;
            if (!Settings.GetBoolSetting("realenginehandling"))
            {
                oldlmSpeed = GeneralHelpers.MapIntToValue(lmSpeed, -100, 100, 0, 200);
                oldrmSpeed = GeneralHelpers.MapIntToValue(rmSpeed, -100, 100, 0, 200);
            }
            else if (Settings.GetBoolSetting("realenginehandling"))
            {
                lmTargetSpeed = GeneralHelpers.MapIntToValue(lmSpeed, -100, 100, 0, 200);
                rmTargetSpeed = GeneralHelpers.MapIntToValue(rmSpeed, -100, 100, 0, 200);
                oldlmSpeed = VariableStorage.ViewModel.LeftMotorSpeed;
                oldrmSpeed = VariableStorage.ViewModel.RightMotorSpeed;

                if (lmTargetSpeed > oldlmSpeed)
                {
                    oldlmSpeed += 5;
                }
                else if (lmTargetSpeed < oldlmSpeed)
                {
                    oldlmSpeed -= 5;
                }
                else if (lmTargetSpeed.IsBetween(oldlmSpeed - 3, oldlmSpeed + 3))
                {
                    oldlmSpeed = lmTargetSpeed;
                }
                if (rmTargetSpeed > oldrmSpeed)
                {
                    oldrmSpeed += 5;
                }
                else if (rmTargetSpeed < oldrmSpeed)
                {
                    oldrmSpeed -= 5;
                }
                else if (rmTargetSpeed.IsBetween(oldrmSpeed - 3, oldrmSpeed + 3))
                {
                    oldrmSpeed = rmTargetSpeed;
                }
            }
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                VariableStorage.ViewModel.LeftMotorSpeed = oldlmSpeed;
                VariableStorage.ViewModel.RightMotorSpeed = oldrmSpeed;
            }).GetAwaiter().GetResult();
        }

        public int GetPanValue()
        {
            return CJoyStick.PanAxis;
        }

        public int GetTiltValue()
        {
            return CJoyStick.TiltAxis;
        }

        public async Task ConnectToAJoystick()
        {
            if (VariableStorage.DeviceFormFactor == DeviceFormFactorType.Desktop)
            {
                if (BluetoothDevices != null && BluetoothDevices.Count > 1)
                {
                    Debug.WriteLine("We got some bluetooth devices");
                    CJoyStick = new PS4JoyStick();
                }
                else
                {
                    var dialog = new Windows.UI.Popups.MessageDialog("Could not find a PS4 controller. Are you just testing?");

                    dialog.Commands.Add(new Windows.UI.Popups.UICommand("No. Connect to PS4 contoller") { Id = 1 });
                    dialog.Commands.Add(new Windows.UI.Popups.UICommand("I'm testing") { Id = 0 });

                    var result = await dialog.ShowAsync();
                    if ((int) result.Id == 0)
                    {
                        VariableStorage.ViewModel.ControllerStatus = "Connected to keyboard joystick";
                        CJoyStick = new KeyboardJoystick();
                    }
                    else
                    {
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
                            VariableStorage.JoyStick.StopPollingController();
                            VariableStorage.BattleBotCommunication.StopCommunication();
                            Application.Current.Exit();
                        }

                    }
                }
            }
            else if (VariableStorage.DeviceFormFactor == DeviceFormFactorType.IoT)
            {
                CJoyStick = new PiJoystick();
            }
        }
    }
}
