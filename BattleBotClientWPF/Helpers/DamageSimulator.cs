using System;
using System.Net;
using System.Windows;
using BattleBotClientWPF.Helpers;

namespace BattleBotClientWPF
{
    class DamageSimulator
    {
        private static int _damageLeftWheels, _damageRightWheels, _damageWebCam, _damageControllUnit;

        public enum DamageTypes
        {
            LeftWheels,
            RightWheels,
            WebCam,
            ControllUnit,
            Repair
        };
        public void SimulateDamage(DamageTypes damageType, int damage)
        {
            switch (damageType)
            {
                case DamageTypes.LeftWheels:
                    _damageLeftWheels += damage;
                    break;
                case DamageTypes.RightWheels:
                    _damageRightWheels += damage;
                    break;
                case DamageTypes.WebCam:
                    _damageWebCam += damage;
                    DoWebCamDamage();
                    break;
                case DamageTypes.ControllUnit:
                    _damageControllUnit += damage;
                    CalculateSleepTime();
                    break;
                case DamageTypes.Repair:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null);
            }
        }

        private void DoWebCamDamage()
        {
            if (_damageWebCam > 70)
            {
                VariableStorage.ViewModel.isWebCrackVisible = true;
            }
            var url = VariableStorage.ViewModel.WebAddress+"cmd_pipe.php?cmd=px%201090%20678%2060%200%202592%201944";
            WebClient client = new WebClient();
            client.DownloadStringAsync(new Uri(url));
            client.Dispose();
        }
        private static void CalculateSleepTime()
        {

        }

        public static string GetDamageReport()
        {
            return string.Format("DMG:{0}|{1}|{2}|{3}",_damageLeftWheels, _damageLeftWheels, _damageWebCam, _damageControllUnit);
        }

        public void Repair()
        {
            _damageWebCam = 0;
            _damageLeftWheels = 0;
            _damageRightWheels = 0;
            _damageControllUnit = 0;

            var url = VariableStorage.ViewModel.WebAddress + "cmd_pipe.php?cmd=px%201090%20678%2060%200%202592%201944";
            WebClient client = new WebClient();
            client.DownloadStringAsync(new Uri(url));
            client.Dispose();
            VariableStorage.ViewModel.isWebCrackVisible = false;
        }
    }
}
