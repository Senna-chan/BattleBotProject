using System;
using BattleBotClientWin10IoT.Interfaces;
using Microsoft.Xna.Framework.Input;

namespace BattleBotClientWin10IoT.JoySticks
{
    class PS4JoyStick : IJoyStickInterface
    {

        public JoyStickTypes JoystickType => JoyStickTypes.PSController;
        public int SpeedAxis { get; set; }
        public int TurnAxis { get; set; }
        public int PanAxis { get; set; }
        public int TiltAxis { get; set; }
        private ButtonState[] _oldButtons;
        private JoystickHat[] _oldHat;
        private JoystickState joystick;
        public PS4JoyStick(int joystickPort)
        {
            joystick = Joystick.GetState(joystickPort);
        }
        public void GetControllerData()
        {
            var axis = joystick.Axes;
            var buttons = joystick.Buttons;
            var hat = joystick.Hats;
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
            _oldButtons = joystick.Buttons;
            _oldHat = joystick.Hats;
        }
    }
}
