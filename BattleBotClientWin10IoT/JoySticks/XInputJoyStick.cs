using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Gaming.Input;
using BattleBotClientWin10IoT.Helpers;
using BattleBotClientWin10IoT.Interfaces;

namespace BattleBotClientWin10IoT.JoySticks
{
    public class XInputJoyStick : IJoyStickInterface
    {
        private Gamepad gamepad;
        public JoyStickTypes JoystickType => JoyStickTypes.XInput;
        public int SpeedAxis { get; set; }
        public int TurnAxis { get; set; }
        public int PanAxis { get; set; }
        public int TiltAxis { get; set; }

        private GamepadButtons _oldButtons;
        private GamepadButtons _currentButtons;

        public XInputJoyStick(Gamepad gamepad)
        {
            this.gamepad = gamepad;
            _oldButtons = gamepad.GetCurrentReading().Buttons;
        }

        public void GetControllerData()
        {
            var readings = gamepad.GetCurrentReading();
            _currentButtons = readings.Buttons;
            SpeedAxis = Convert.ToInt32(Math.Floor(readings.LeftThumbstickY * 100));
            TurnAxis = Convert.ToInt32(Math.Floor(readings.LeftThumbstickX * 100));
            PanAxis = Convert.ToInt32(Math.Floor(readings.RightThumbstickX * 100));
            TiltAxis = Convert.ToInt32(Math.Floor(readings.RightThumbstickY * 100));
        }

        public int GetSpeedAxisPosition()
        {
            return SpeedAxis;
        }

        public int GetTurnAxisPosition()
        {
            return TurnAxis;
        }

        public int GetPanAxisPosition()
        {
            return PanAxis;
        }

        public int GetTiltAxisPosition()
        {
            return TiltAxis;
        }

        public bool GetShootButtonState()
        {
            return (_currentButtons & GamepadButtons.A) == 0 && (_oldButtons & GamepadButtons.A) != 0;
        }

        public bool GetSpeedDownGearButtonState()
        {
            return (_currentButtons & GamepadButtons.DPadDown) == 0 && (_oldButtons & GamepadButtons.DPadDown) != 0;
        }

        public bool GetSpeedUpGearButtonState()
        {
            return (_currentButtons & GamepadButtons.DPadUp) == 0 && (_oldButtons & GamepadButtons.DPadUp) != 0;
        }

        public bool GetTurnSharperGearButtonState()
        {
            return (_currentButtons & GamepadButtons.DPadRight) == 0 && (_oldButtons & GamepadButtons.DPadRight) != 0;
        }

        public bool GetTurnWeakerGearButtonState()
        {
            return (_currentButtons & GamepadButtons.DPadLeft) == 0 && (_oldButtons & GamepadButtons.DPadLeft) != 0;
        }

        public void PopulateOldButtons()
        {
            _oldButtons = _currentButtons;
        }
    }
}
