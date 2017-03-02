using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using BattleBotClientWin10IoT.Models;

namespace BattleBotClientWin10IoT.Helpers
{
    class KeyHelper
    {
        public static List<KeyHelperModel> oldKeyPresses = new List<KeyHelperModel>();
        
        public static bool IsKeyUp(VirtualKey key)
        {
            CoreVirtualKeyStates keyState = CoreVirtualKeyStates.Down;
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                keyState = Window.Current.CoreWindow.GetKeyState(key);
            }).GetAwaiter().GetResult();
            return keyState == CoreVirtualKeyStates.None;
        }
        public static bool IsKeyDown(VirtualKey key)
        {
            CoreVirtualKeyStates keyState = CoreVirtualKeyStates.None;
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                keyState = Window.Current.CoreWindow.GetKeyState(key);
            }).GetAwaiter().GetResult();
            return keyState == CoreVirtualKeyStates.Down;
        }
    }
}
