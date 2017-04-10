using BattleBotClientWin10IoT.Interfaces;

namespace BattleBotClientWin10IoT.JoySticks
{
    class PiJoystick : IJoyStickInterface
    {
        public JoyStickTypes JoystickType => JoyStickTypes.PiHardware;
        public int SpeedAxis { get; set; }
        public int TurnAxis { get; set; }
        public int PanAxis { get; set; }
        public int TiltAxis { get; set; }
        public void GetControllerData()
        {
            throw new System.NotImplementedException();
        }

        public int GetSpeedAxisPosition()
        {
            throw new System.NotImplementedException();
        }

        public int GetTurnAxisPosition()
        {
            throw new System.NotImplementedException();
        }

        public int GetPanAxisPosition()
        {
            throw new System.NotImplementedException();
        }

        public int GetTiltAxisPosition()
        {
            throw new System.NotImplementedException();
        }

        public bool GetShootButtonState()
        {
            throw new System.NotImplementedException();
        }

        public bool GetSpeedDownGearButtonState()
        {
            throw new System.NotImplementedException();
        }

        public bool GetSpeedUpGearButtonState()
        {
            throw new System.NotImplementedException();
        }

        public bool GetTurnSharperGearButtonState()
        {
            throw new System.NotImplementedException();
        }

        public bool GetTurnWeakerGearButtonState()
        {
            throw new System.NotImplementedException();
        }

        public bool GetServoLockButtonState()
        {
            throw new System.NotImplementedException();
        }

        public bool GetServoStabalizeButtonState()
        {
            throw new System.NotImplementedException();
        }

        public void PopulateOldButtons()
        {
            throw new System.NotImplementedException();
        }

        public int? GetBatteryStatus()
        {
            return null;
        }

        public bool IsConnected { get { return true; } set { IsConnected = true; } }
        public bool IsWireless { get { return false; } set { IsWireless = false; } }
    }
}
