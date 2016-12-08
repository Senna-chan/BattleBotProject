using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace BattleBotClientWin10IoT.Helpers
{
    class KeyHelper
    {
        public static bool IsKeyUp(VirtualKey key)
        {
            return Window.Current.CoreWindow.GetKeyState(key) == CoreVirtualKeyStates.None;
        }
        public static bool IsKeyDown(VirtualKey key)
        {
            return Window.Current.CoreWindow.GetKeyState(key) == CoreVirtualKeyStates.Down;
        }
    }
}
