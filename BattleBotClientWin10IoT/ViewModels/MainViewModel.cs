using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;
using BattleBotClientWin10IoT.Helpers;

namespace BattleBotClientWin10IoT.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _spiStatus = "Not connected";
        private string _espStatus = "Not connected";

        public string SpiStatus
        {
            get { return _spiStatus; }
            set
            {
                if (_spiStatus != value)
                {
                    _spiStatus = value;
                    RaisePropertyChanged("spiStatus");
                }
            }
        }

        public string EspStatus
        {
            get { return _espStatus; }
            set
            {
                if (_espStatus != value)
                {
                    _espStatus = value;
                    RaisePropertyChanged("EspStatus");
                }
            }
        }


        private string _controllerStatus = "Controller not initialized";

        public string ControllerStatus
        {
            get { return _controllerStatus; }
            set
            {
                if (_controllerStatus != value)
                {
                    _controllerStatus = value;
                    RaisePropertyChanged("ControllerStatus");
                }
            }
        }

        private string _cameraStatus = "Camera not initialized";

        public string CameraStatus
        {
            get { return _cameraStatus; }
            set
            {
                if (_cameraStatus != value)
                {
                    _cameraStatus = value;
                    RaisePropertyChanged("CameraStatus");
                }
            }
        }

        private int _speedGear = 1;

        public int SpeedGear
        {
            get { return _speedGear; }
            set
            {
                if (_speedGear != value)
                {
                    _speedGear = value;
                    RaisePropertyChanged("SpeedGear");
                }
            }
        }

        private int _turnGear = 1;

        public int TurnGear
        {
            get { return _turnGear; }
            set
            {
                if (_turnGear != value)
                {
                    _turnGear = value;
                    RaisePropertyChanged("TurnGear");
                }
            }
        }

        private int _LeftMotorSpeed = 100;

        public int LeftMotorSpeed
        {
            get { return _LeftMotorSpeed; }
            set
            {
                if (_LeftMotorSpeed != value)
                {
                    _LeftMotorSpeed = value;
                    RaisePropertyChanged("LeftMotorSpeed");
                }
            }
        }

        private int _rightMotorSpeed = 100;

        public int RightMotorSpeed
        {
            get { return _rightMotorSpeed; }
            set
            {
                if (_rightMotorSpeed != value)
                {
                    _rightMotorSpeed = value;
                    RaisePropertyChanged("RightMotorSpeed");
                }
            }
        }

        private bool _shoot = false;

        public bool Shoot
        {
            get { return _shoot; }
            set
            {
                if (_shoot != value)
                {
                    _shoot = value;
                    RaisePropertyChanged("Shoot");
                }
            }
        }

        private int _TurretPosition = 0;

        public int TurretPosition
        {
            get { return _TurretPosition; }
            set
            {
                if (_TurretPosition != value)
                {
                    _TurretPosition = value;
                    RaisePropertyChanged("TurretPosition");
                }
            }
        }

        public int WaitTime
        {
            get { return Settings.GetIntSetting("waittime"); }
            set
            {
                if (Settings.GetIntSetting("waittime") != value)
                {
                    Settings.SaveSetting("waittime", value);
                    RaisePropertyChanged("WaitTime");
                }
            }
        }

        public bool RealEngineHandling
        {
            get { return Settings.GetBoolSetting("realenginehandling"); }
            set
            {
                if (Settings.GetBoolSetting("realenginehandling") != value)
                {
                    Settings.SaveSetting("realenginehandling", value);
                    RaisePropertyChanged("RealEngineHandling");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
                handler(this, args);
            }
        }
    }
}