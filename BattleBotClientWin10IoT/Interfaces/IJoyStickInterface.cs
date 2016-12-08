using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;

namespace BattleBotClientWin10IoT.Helpers.JoySticks
{
    interface IJoyStickInterface
    {
        int SpeedAxis { get; set; }
        int TurnAxis { get; set; }
        void GetControllerData();
        int GetSpeedAxisPosition();
        int GetTurnAxisPosition();
        int GetPanAxisPosition();
        int GetTiltAxisPosition();
        bool GetShootButtonState();
        bool GetSpeedDownGearButtonState();
        bool GetSpeedUpGearButtonState();
        bool GetTurnSharperGearButtonState();
        bool GetTurnWeakerGearButtonState();
    }
}
