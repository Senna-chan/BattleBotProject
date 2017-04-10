using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Usb;
using BattleBotClientWin10IoT.Interfaces;
using Windows.Gaming.Input;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using BattleBotClientWin10IoT.Helpers;
using BattleBotClientWin10IoT.Views;

namespace BattleBotClientWin10IoT.JoySticks
{
    class GenericJoyStick : IJoyStickInterface
    {

        public JoyStickTypes JoystickType => JoyStickTypes.Generic;
        public int SpeedAxis { get; set; }
        public int TurnAxis { get; set; }
        public int PanAxis { get; set; }
        public int TiltAxis { get; set; }
        public bool IsConnected { get; set; }
        public bool IsWireless { get; set; }
        private RawGameController _controller;
        private JoystickInputMapping Mapping;
        public GenericJoyStick(RawGameController rawGameController)
        {
            _controller = rawGameController;
            RawGameController.RawGameControllerRemoved += delegate(object sender, RawGameController controller)
            {
                if (controller == _controller)
                {
                    IsConnected = false;
                }
            };
            IsWireless = _controller.IsWireless;
            var gameControllerConfigs = Directory.GetFiles(VariableStorage.AssetDirectory + "\\JoyStickMappings");
            var hidString = _controller.HardwareVendorId + "-" + _controller.HardwareProductId;
            if (gameControllerConfigs != null && gameControllerConfigs.Contains(hidString))
            {

            }
            else
            {
                // This shouldn't happen. Ow well. Shit can happen
                var dialog = new MessageDialog("U wot m8!\r\n\r\nThere was an error. I'm closing.");

                dialog.Commands.Add(new UICommand("Ok") { Id = 0 });

                dialog.ShowAsync().GetResults();
                Application.Current.Exit();
            }
        }

        public void GetControllerData()
        {
            throw new NotImplementedException();
        }

        public int GetSpeedAxisPosition()
        {
            throw new NotImplementedException();
        }

        public int GetTurnAxisPosition()
        {
            throw new NotImplementedException();
        }

        public int GetPanAxisPosition()
        {
            throw new NotImplementedException();
        }

        public int GetTiltAxisPosition()
        {
            throw new NotImplementedException();
        }

        public bool GetShootButtonState()
        {
            throw new NotImplementedException();
        }

        public bool GetSpeedDownGearButtonState()
        {
            throw new NotImplementedException();
        }

        public bool GetSpeedUpGearButtonState()
        {
            throw new NotImplementedException();
        }

        public bool GetTurnSharperGearButtonState()
        {
            throw new NotImplementedException();
        }

        public bool GetTurnWeakerGearButtonState()
        {
            throw new NotImplementedException();
        }

        public void PopulateOldButtons()
        {
            throw new NotImplementedException();
        }

        public bool GetServoLockButtonState()
        {
            throw new NotImplementedException();
        }

        public bool GetServoStabalizeButtonState()
        {
            throw new NotImplementedException();
        }

        public int? GetBatteryStatus()
        {
            throw new NotImplementedException();
        }
    }
}
