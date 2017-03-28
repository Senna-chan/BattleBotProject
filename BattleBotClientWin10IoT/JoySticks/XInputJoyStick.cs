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
            _oldButtons = gamepad.GetCurrentReading().Buttons; // Populating old buttons. If not then there could be a crash.
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
            return ButtonPressed(GamepadButtons.A);
        }

        public bool GetSpeedDownGearButtonState()
        {
            return ButtonPressed(GamepadButtons.DPadDown);
        }

        public bool GetSpeedUpGearButtonState()
        {
            return ButtonPressed(GamepadButtons.DPadUp);
        }

        public bool GetTurnSharperGearButtonState()
        {
            return ButtonPressed(GamepadButtons.DPadRight);
        }

        public bool GetTurnWeakerGearButtonState()
        {
            return ButtonPressed(GamepadButtons.DPadLeft);
        }

        public void PopulateOldButtons()
        {
            _oldButtons = _currentButtons;
        }

        public bool GetServoHardLockButtonState()
        {
            return ButtonPressed(GamepadButtons.RightThumbstick);
        }

        public bool GetServoStabalizeButtonState()
        {
            return ButtonPressed(GamepadButtons.RightShoulder);
        }

        private bool ButtonPressed(GamepadButtons button)
        {
            return (_currentButtons & button) == 0 && (_oldButtons & button) != 0;
        }
    }
}
