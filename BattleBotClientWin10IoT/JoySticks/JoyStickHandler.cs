using BattleBotClientWin10IoT.Helpers;
using BattleBotClientWin10IoT.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Gaming.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using BattleBotClientWin10IoT.Views;

namespace BattleBotClientWin10IoT.JoySticks
{
    class JoyStickHandler
    {
        private IJoyStickInterface CJoyStick;
        private CancellationTokenSource CancelPolling = new CancellationTokenSource();

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
                if (CJoyStick.GetServoLockButtonState() && VariableStorage.ViewModel.PanTiltLocking != 1)
                {
                    VariableStorage.ViewModel.PanTiltLocking = 1;
                    VariableStorage.BattleBotCommunication.Send("SM:1");
                }
                else if(CJoyStick.GetServoLockButtonState() && VariableStorage.ViewModel.PanTiltLocking == 1)
                {
                    VariableStorage.ViewModel.PanTiltLocking = 0;
                    VariableStorage.BattleBotCommunication.Send("SM:0");
                }
                if (CJoyStick.GetServoStabalizeButtonState() && VariableStorage.ViewModel.PanTiltLocking != 2)
                {
                    VariableStorage.ViewModel.PanTiltLocking = 2;
                    VariableStorage.BattleBotCommunication.Send("SM:2");
                }
                else if(CJoyStick.GetServoStabalizeButtonState() && VariableStorage.ViewModel.PanTiltLocking == 2)
                {
                    VariableStorage.ViewModel.PanTiltLocking = 0;
                    VariableStorage.BattleBotCommunication.Send("SM:0");
                }
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
            lmSpeed = GeneralHelpers.MapIntToValue(lmSpeed, -100, 100, 0, 200);
            rmSpeed = GeneralHelpers.MapIntToValue(rmSpeed, -100, 100, 0, 200);
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                VariableStorage.ViewModel.LeftMotorSpeed = lmSpeed;
                VariableStorage.ViewModel.RightMotorSpeed = rmSpeed;
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

        public async Task<int?> ConnectToAJoystick()
        {
            await Task.Delay(5000);
            if (Gamepad.Gamepads.Count != 0)
            {
                CJoyStick = new XInputJoyStick(Gamepad.Gamepads[0]);
                VariableStorage.ViewModel.ControllerStatus = "Connected to XInput controller";
            }
            else if(RawGameController.RawGameControllers.Count == 1)
            {
                // TODO: Make a box to prompt for the joystick on the desktop side
                var gameControllerConfigs = Directory.GetFiles(VariableStorage.AssetDirectory + "\\JoyStickMappings");
                var hidString = RawGameController.RawGameControllers[0].HardwareVendorId + "-" + RawGameController.RawGameControllers[0].HardwareProductId;
                if (gameControllerConfigs != null && gameControllerConfigs.Contains(hidString))
                {
                    CJoyStick = new GenericJoyStick(RawGameController.RawGameControllers[0]);
                    VariableStorage.ViewModel.ControllerStatus = "Connected to Gemeric controller";
                }
                else
                {// Here we need to initialize the controller setup but because of async that needs to be done in InitializePage
                    return 1;
                }
            }
            else 
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Could not find a controller. Do you want to use a keyboard as joystick?");

                dialog.Commands.Add(new Windows.UI.Popups.UICommand("No, connect to contoller") { Id = 1 });
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes, use my keyboard") { Id = 0 });

                var result = await dialog.ShowAsync();
                if ((int)result.Id == 0)
                {
                    VariableStorage.ViewModel.ControllerStatus = "Connected to keyboard";
                    CJoyStick = new KeyboardJoystick();
                }
                else
                {
                    if (Gamepad.Gamepads.Count != 0)
                    {
                        CJoyStick = new XInputJoyStick(Gamepad.Gamepads[0]);
                        VariableStorage.ViewModel.ControllerStatus = "Connected to XInput controller";
                    }
                    else
                    {
                        dialog = new Windows.UI.Popups.MessageDialog("Still no controller. Bye");
                        dialog.Commands.Add(new Windows.UI.Popups.UICommand("Ok"));
                        await dialog.ShowAsync();
                        //VariableStorage.JoyStick.StopPollingController();
                        //VariableStorage.BattleBotCommunication.StopCommunication();
                        Application.Current.Exit();
                    }

                }
            }
            return null;
        }
    }
}
