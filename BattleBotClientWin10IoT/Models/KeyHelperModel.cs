using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;

namespace BattleBotClientWin10IoT.Models
{
    public class KeyHelperModel
    {
        public VirtualKey Key { get; set; }
        public bool KeyDown { get; set; }
    }
}
