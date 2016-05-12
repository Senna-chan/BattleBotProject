using BattleBotClientWPF.Properties;
using CefSharp.Wpf;

namespace BattleBotClientWPF.ViewModels
{
    public class MainViewModel : Common.Library.CommonBase
    {

        private IWpfWebBrowser _webBrowser;
        private volatile string _Temperature = string.Empty;
        private volatile string _Load = string.Empty;
        private volatile string _Ram = string.Empty;
        private volatile string _Speed = string.Empty;
        private volatile string _WheelSpeedLeft = string.Empty;
        private volatile string _WheelSpeedRight = string.Empty;
        private volatile int _WheelSpeedSliderLeft;
        private volatile int _WheelSpeedSliderRight;
        private volatile string _Battery = string.Empty;
        private volatile string _Gear = string.Empty;
        private volatile bool _isHealthVisible = Settings.Default.IsHealthVisible;
        private volatile bool _isArmorVisible = Settings.Default.IsArmorVisible;
        private volatile bool _isAmmoVisible = Settings.Default.IsAmmoVisible;
        private volatile bool _isCrosshairVisibile = Settings.Default.IsCrosshairVisible;
        private volatile bool _isWebCrackVisible = false;
        private volatile string _webAddress = string.Empty;
        private volatile string _HealthColor = Settings.Default.HealthColor;
        private volatile string _ArmorColor = Settings.Default.ArmorColor;
        private volatile string _CrossHairCollor = Settings.Default.CrosshairColor;
        private volatile string _ClipSize = Settings.Default.ClipSize;
        private volatile string _AmmoSize = Settings.Default.AmmoSize;
        private volatile string _LeftWheelsHealth = "100";
        private volatile string _RightWheelsHealth = "100";
        private volatile string _ControllUnitHealth = "100";
        private volatile string _CamaraHealth = "100";
        private volatile string _WeaponsPlatformHealth = "100";
        private volatile string _UltraSonic = "10";
        private volatile string _Accelerometer_X = "0";
        private volatile string _Accelerometer_Y = "0";
        private volatile string _Accelerometer_Z = "0";
        private volatile string _Laser_Detector_1 = "0";
        private volatile string _Laser_Detector_2 = "0";
        private volatile string _MotorConfig = Settings.Default.MotorConfig;
        private volatile string _ControllerConfig = Settings.Default.ControllerConfig;
        private volatile int _Frequency = Settings.Default.Frequency;


        public int Frequency
        {
            get { return _Frequency; }
            set
            {
                if (_Frequency != value)
                {
                    _Frequency = value;
                    RaisePropertyChanged("Frequency");
                }
            }
        }


        public IWpfWebBrowser WebBrowser
        {
            get { return _webBrowser; }
            set
            {
                if (_webBrowser != value)
                {
                    _webBrowser = value;
                    RaisePropertyChanged("WebBrowser");
                }
            }
        }


        /// <summary>
        /// Get/Set isHealthVisible
        /// </summary>
        public bool isHealthVisible
        {
            get { return _isHealthVisible; }
            set
            {
                if (_isHealthVisible != value)
                {
                    _isHealthVisible = value;
                    RaisePropertyChanged("isHealthVisible");
                }
            }
        }

        /// <summary>
        /// Get/Set isArmorVisible
        /// </summary>
        public bool isArmorVisible
        {
            get { return _isArmorVisible; }
            set
            {
                if (_isArmorVisible != value)
                {
                    _isArmorVisible = value;
                    RaisePropertyChanged("isArmorVisible");
                }
            }
        }

        /// <summary>
        /// Get/Set isAmmoVisible
        /// </summary>
        public bool isAmmoVisible
        {
            get { return _isAmmoVisible; }
            set
            {
                if (_isAmmoVisible != value)
                {
                    _isAmmoVisible = value;
                    RaisePropertyChanged("isAmmoVisible");
                }
            }
        }

        /// <summary>
        /// Get/Set isWebCrackVisible
        /// </summary>
        public bool isWebCrackVisible
        {
            get { return _isWebCrackVisible; }
            set
            {
                if (_isWebCrackVisible != value)
                {
                    _isWebCrackVisible = value;
                    RaisePropertyChanged("isWebCrackVisible");
                }
            }
        }

        /// <summary>
        /// Get/Set Temperature
        /// </summary>
        public string Temperature
        {
            get { return _Temperature; }
            set
            {
                if (_Temperature != value)
                {
                    _Temperature = value;
                    RaisePropertyChanged("Temperature");
                }
            }
        }

        /// <summary>
        /// Get/Set Load
        /// </summary>
        public string Load
        {
            get { return _Load; }
            set
            {
                if (_Load != value)
                {
                    _Load = value;
                    RaisePropertyChanged("Load");
                }
            }
        }

        /// <summary>
        /// Get/Set Ram
        /// </summary>
        public string Ram
        {
            get { return _Ram; }
            set
            {
                if (_Ram != value)
                {
                    _Ram = value;
                    RaisePropertyChanged("Ram");
                }
            }
        }

        /// <summary>
        /// Get/Set Speed
        /// </summary>
        public string Speed
        {
            get { return _Speed; }
            set
            {
                if (_Speed != value)
                {
                    _Speed = value;
                    RaisePropertyChanged("Speed");
                }
            }
        }

        /// <summary>
        /// Get/Set WheelSpeedLeft
        /// </summary>
        public string WheelSpeedLeft
        {
            get { return _WheelSpeedLeft; }
            set
            {
                if (_WheelSpeedLeft != value)
                {
                    _WheelSpeedLeft = value;
                    RaisePropertyChanged("WheelSpeedLeft");
                }
            }
        }

        /// <summary>
        /// Get/Set WheelSpeedRight
        /// </summary>
        public string WheelSpeedRight
        {
            get { return _WheelSpeedRight; }
            set
            {
                if (_WheelSpeedRight != value)
                {
                    _WheelSpeedRight = value;
                    RaisePropertyChanged("WheelSpeedRight");
                }
            }
        }

        /// <summary>
        /// Get/Set Battery
        /// </summary>
        public string Battery
        {
            get { return _Battery; }
            set
            {
                if (_Battery != value)
                {
                    _Battery = value;
                    RaisePropertyChanged("Battery");
                }
            }
        }

        /// <summary>
        /// Get/Set Gear
        /// </summary>
        public string Gear
        {
            get { return _Gear; }
            set
            {
                if (_Gear != value)
                {
                    _Gear = value;
                    RaisePropertyChanged("Gear");
                }
            }
        }

        public string WebAddress
        {
            get { return "http://" + _webAddress; }
            //get { return "http://" + _webAddress + "/rcam/"; }
            set
            {
                if (_webAddress != value)
                {
                    _webAddress = value;
                    RaisePropertyChanged("WebAddress");
                }
            }
        }



        /// <summary>
        /// Get/Set HealthColor
        /// </summary>
        public string HealthColor
        {
            get { return _HealthColor; }
            set
            {
                if (_HealthColor != value)
                {
                    _HealthColor = value;
                    RaisePropertyChanged("HealthColor");
                }
            }
        }

        /// <summary>
        /// Get/Set ArmorColor
        /// </summary>
        public string ArmorColor
        {
            get { return _ArmorColor; }
            set
            {
                if (_ArmorColor != value)
                {
                    _ArmorColor = value;
                    RaisePropertyChanged("ArmorColor");
                }
            }
        }

        /// <summary>
        /// Get/Set CrossHairCollor
        /// </summary>
        public string CrossHairCollor
        {
            get { return _CrossHairCollor; }
            set
            {
                if (_CrossHairCollor != value)
                {
                    _CrossHairCollor = value;
                    RaisePropertyChanged("CrossHairCollor");
                }
            }
        }

        /// <summary>
        /// Get/Set ClipSize
        /// </summary>
        public string ClipSize
        {
            get { return _ClipSize; }
            set
            {
                if (_ClipSize != value)
                {
                    _ClipSize = value;
                    RaisePropertyChanged("ClipSize");
                }
            }
        }

        /// <summary>
        /// Get/Set AmmoSize
        /// </summary>
        public string AmmoSize
        {
            get { return _AmmoSize; }
            set
            {
                if (_AmmoSize != value)
                {
                    _AmmoSize = value;
                    RaisePropertyChanged("AmmoSize");
                }
            }
        }

        public bool IsCrosshairVisible
        {
            get { return _isCrosshairVisibile; }
            set
            {
                if (_isCrosshairVisibile != value)
                {
                    _isCrosshairVisibile = value;
                    RaisePropertyChanged("AmmoSize");
                }
            }
        }
        /// <summary>
        /// Get/Set LeftWheelsHealth
        /// </summary>
        public string LeftWheelsHealth
        {
            get { return _LeftWheelsHealth + "%"; }
            set
            {
                if (_LeftWheelsHealth != value)
                {
                    _LeftWheelsHealth = value;
                    RaisePropertyChanged("LeftWheelsHealth");
                }
            }
        }

        /// <summary>
        /// Get/Set RightWheelsHealth
        /// </summary>
        public string RightWheelsHealth
        {
            get { return _RightWheelsHealth + "%"; }
            set
            {
                if (_RightWheelsHealth != value)
                {
                    _RightWheelsHealth = value;
                    RaisePropertyChanged("RightWheelsHealth");
                }
            }
        }

        /// <summary>
        /// Get/Set ControllUnitHealth
        /// </summary>
        public string ControllUnitHealth
        {
            get { return _ControllUnitHealth + "%"; }
            set
            {
                if (_ControllUnitHealth != value)
                {
                    _ControllUnitHealth = value;
                    RaisePropertyChanged("ControllUnitHealth");
                }
            }
        }

        /// <summary>
        /// Get/Set CamaraHealth
        /// </summary>
        public string CamaraHealth
        {
            get { return _CamaraHealth + "%"; }
            set
            {
                if (_CamaraHealth != value)
                {
                    _CamaraHealth = value;
                    RaisePropertyChanged("CamaraHealth");
                }
            }
        }

        /// <summary>
        /// Get/Set WeaponsPlatformHealth
        /// </summary>
        public string WeaponsPlatformHealth
        {
            get { return _WeaponsPlatformHealth + "%"; }
            set
            {
                if (_WeaponsPlatformHealth != value)
                {
                    _WeaponsPlatformHealth = value;
                    RaisePropertyChanged("WeaponsPlatformHealth");
                }
            }
        }

        /// <summary>
        /// Get/Set WheelSpeedSliderLeft
        /// </summary>
        public int WheelSpeedSliderLeft
        {
            get { return _WheelSpeedSliderLeft; }
            set
            {
                if (_WheelSpeedSliderLeft != value)
                {
                    _WheelSpeedSliderLeft = value;
                    RaisePropertyChanged("WheelSpeedSliderLeft");
                }
            }
        }

        /// <summary>
        /// Get/Set WheelSpeedSliderRight
        /// </summary>
        public int WheelSpeedSliderRight
        {
            get { return _WheelSpeedSliderRight; }
            set
            {
                if (_WheelSpeedSliderRight != value)
                {
                    _WheelSpeedSliderRight = value;
                    RaisePropertyChanged("WheelSpeedSliderRight");
                }
            }
        }

        /// <summary>
        /// Get/Set UltraSonic
        /// </summary>
        public string UltraSonic
        {
            get { return _UltraSonic; }
            set
            {
                if (_UltraSonic != value)
                {
                    _UltraSonic = value;
                    RaisePropertyChanged("UltraSonic");
                }
            }
        }

        /// <summary>
        /// Get/Set Accelerometer_X
        /// </summary>
        public string Accelerometer_X
        {
            get { return _Accelerometer_X; }
            set
            {
                if (_Accelerometer_X != value)
                {
                    _Accelerometer_X = value;
                    RaisePropertyChanged("Accelerometer_X");
                }
            }
        }

        /// <summary>
        /// Get/Set Accelerometer_Y
        /// </summary>
        public string Accelerometer_Y
        {
            get { return _Accelerometer_Y; }
            set
            {
                if (_Accelerometer_Y != value)
                {
                    _Accelerometer_Y = value;
                    RaisePropertyChanged("Accelerometer_Y");
                }
            }
        }

        /// <summary>
        /// Get/Set Accelerometer_Z
        /// </summary>
        public string Accelerometer_Z
        {
            get { return _Accelerometer_Z; }
            set
            {
                if (_Accelerometer_Z != value)
                {
                    _Accelerometer_Z = value;
                    RaisePropertyChanged("Accelerometer_Z");
                }
            }
        }

        /// <summary>
        /// Get/Set Laser_Detector_1
        /// </summary>
        public string Laser_Detector_1
        {
            get { return _Laser_Detector_1; }
            set
            {
                if (_Laser_Detector_1 != value)
                {
                    _Laser_Detector_1 = value;
                    RaisePropertyChanged("Laser_Detector_1");
                }
            }
        }

        /// <summary>
        /// Get/Set Laser_Detector_2
        /// </summary>
        public string Laser_Detector_2
        {
            get { return _Laser_Detector_2; }
            set
            {
                if (_Laser_Detector_2 != value)
                {
                    _Laser_Detector_2 = value;
                    RaisePropertyChanged("Laser_Detector_2");
                }
            }
        }

        /// <summary>
        /// Get/Set MotorConfig
        /// </summary>
        public string MotorConfig
        {
            get { return _MotorConfig; }
            set
            {
                if (_MotorConfig != value)
                {
                    _MotorConfig = value;
                    RaisePropertyChanged("MotorConfig");
                }
            }
        }
        /// <summary>
        /// Get/Set ControllerConfig
        /// </summary>
        public string ControllerConfig
        {
            get { return _ControllerConfig; }
            set
            {
                if (_ControllerConfig != value)
                {
                    _ControllerConfig = value;
                    RaisePropertyChanged("ControllerConfig");
                }
            }
        }
    }
}
