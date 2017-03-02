using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BattleBotClientWin10IoT.Helpers
{
    public static class Settings
    {
        public static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public static void SaveSetting(string settingName, object settingValue)
        {
            localSettings.Values[settingName] = settingValue;
        }

        public static int GetIntSetting(string settingName)
        {
            var setting = localSettings.Values[settingName];
            if (setting == null) return 0;
            return (int) setting;
        }

        public static string GetStringSetting(string settingName)
        {
            var setting = localSettings.Values[settingName];
            if (setting == null) return string.Empty;
            return (string) setting;
        }

        public static bool GetBoolSetting(string settingName)
        {
            var setting = localSettings.Values[settingName];
            if (setting == null) return false;
            return (bool) setting;
        }
    }
}
