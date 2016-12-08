using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using BattleBotClientWin10IoT.Helpers;
using BattleBotClientWin10IoT.Helpers.JoySticks;

namespace BattleBotClientWin10IoT.JoySticks
{
    class KeyboardJoystick : IJoyStickInterface
    {
        public int SpeedAxis { get; set; }
        public int TurnAxis { get; set; }

        public void GetControllerData()
        {
            if (KeyHelper.IsKeyDown(VirtualKey.W))
            {
                SpeedAxis = 1;
            }
            else if (KeyHelper.IsKeyDown(VirtualKey.S))
            {
                SpeedAxis = -1;
            }
            if (KeyHelper.IsKeyDown(VirtualKey.A))
            {
                TurnAxis = 1;
            }
            else if (KeyHelper.IsKeyDown(VirtualKey.D))
            {
                TurnAxis = -1;
            }
        }

        public int GetSpeedAxisPosition()
        {
            return GeneralHelpers.MapIntToValue(SpeedAxis, -1, 1, 0, 1023);
        }

        public int GetTurnAxisPosition()
        {
            return GeneralHelpers.MapIntToValue(TurnAxis, -1, 1, 0, 1023);
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
