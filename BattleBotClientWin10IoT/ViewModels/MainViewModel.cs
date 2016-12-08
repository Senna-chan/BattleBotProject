using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;

namespace BattleBotClientWin10IoT.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {

        private BitmapImage _mJpegStreamImage;
        public BitmapImage mJpegStreamImage
        {
            get { return _mJpegStreamImage; }
            set
            {
                if (_mJpegStreamImage != value)
                {
                    _mJpegStreamImage = value;
                    RaisePropertyChanged("mJpegStreamImage");
                }
            }
        }



        private string _spiStatus = "Not connected";

        public string spiStatus
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

        private string _mdnsStatus = "mDNS not initialized";

        public string mdnsStatus
        {
            get { return _mdnsStatus; }
            set
            {
                if (_mdnsStatus != value)
                {
                    _mdnsStatus = value;
                    RaisePropertyChanged("mdnsStatus");
                }
            }
        }


        private int _Speed = 0;

        public int Speed
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


        private int _Wheelpos = 0;

        public int Wheelpos
        {
            get { return _Wheelpos; }
            set
            {
                if (_Wheelpos != value)
                {
                    _Wheelpos = value;
                    RaisePropertyChanged("Wheelpos");
                }
            }
        }

        private int _LeftMotorSpeed = 0;

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

        private int _RightMotorSpeed = 0;

        public int RightMotorSpeed
        {
            get { return _RightMotorSpeed; }
            set
            {
                if (_RightMotorSpeed != value)
                {
                    _RightMotorSpeed = value;
                    RaisePropertyChanged("RightMotorSpeed");
                }
            }
        }

        private bool _Shoot = false;

        public bool Shoot
        {
            get { return _Shoot; }
            set
            {
                if (_Shoot != value)
                {
                    _Shoot = value;
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