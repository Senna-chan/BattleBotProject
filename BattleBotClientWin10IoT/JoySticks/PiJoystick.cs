using BattleBotClientWin10IoT.Helpers.JoySticks;

namespace BattleBotClientWin10IoT.JoySticks
{
    class PiJoystick : IJoyStickInterface
    {
        public int SpeedAxis { get; set; }
        public int TurnAxis { get; set; }

        public void GetControllerData()
        {
            
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
    }
}
