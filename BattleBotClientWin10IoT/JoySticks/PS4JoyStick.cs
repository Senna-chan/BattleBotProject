using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleBotClientWin10IoT.Helpers.JoySticks
{
    class PS4JoyStick : IJoyStickInterface
    {
        public int SpeedAxis { get; set; }
        public int TurnAxis { get; set; }

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
    }
}
