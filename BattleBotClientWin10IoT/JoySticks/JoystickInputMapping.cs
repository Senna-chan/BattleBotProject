using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleBotClientWin10IoT.JoySticks
{
    public class JoystickInputMapping
    {
        public int SpeedAxis { get; set; }
        public int TurnAxis { get; set; }
        public int PanAxis { get; set; }
        public int TiltAxis { get; set; }
        public int SpeedGearUpButton { get; set; }
        public int SpeedGearDownButton { get; set; }
        public int TurnGearUpButton { get; set; }
        public int TurnGearDownButton { get; set; }
        public int SchootButton { get; set; }
        public int ServoLockButton { get; set; }
        public int ServoStabalizeButton { get; set; }
    }
}
