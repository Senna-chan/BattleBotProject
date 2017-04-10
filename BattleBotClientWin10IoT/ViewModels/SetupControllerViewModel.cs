using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BattleBotClientWin10IoT.Annotations;
using BattleBotClientWin10IoT.JoySticks;

namespace BattleBotClientWin10IoT.ViewModels
{
    public class SetupControllerViewModel : INotifyPropertyChanged
    {
        private JoystickInputMapping _mapping = new JoystickInputMapping();

        public int SpeedAxis
        {
            get { return _mapping.SpeedAxis; }
            set {
                if (_mapping.SpeedAxis != value)
                {
                    _mapping.SpeedAxis = value;
                    OnPropertyChanged("SpeedAxis");
                }
            }
        }

        public int TurnAxis
        {
            get { return _mapping.TurnAxis; }
            set
            {
                if (_mapping.TurnAxis != value)
                {
                    _mapping.TurnAxis = value;
                    OnPropertyChanged("TurnAxis");
                }
            }
        }

        public int PanAxis
        {
            get { return _mapping.PanAxis; }
            set
            {
                if (_mapping.PanAxis != value)
                {
                    _mapping.PanAxis = value;
                    OnPropertyChanged("PanAxis");
                }
            }
        }

        public int TiltAxis
        {
            get { return _mapping.TiltAxis; }
            set
            {
                if (_mapping.TiltAxis != value)
                {
                    _mapping.TiltAxis = value;
                    OnPropertyChanged("TiltAxis");
                }
            }
        }

        public int SpeedGearUpButton
        {
            get { return _mapping.SpeedGearUpButton; }
            set
            {
                if (_mapping.SpeedGearUpButton != value)
                {
                    _mapping.SpeedGearUpButton = value;
                    OnPropertyChanged("SpeedGearUpButton");
                }
            }
        }

        public int SpeedGearDownButton
        {
            get { return _mapping.SpeedGearDownButton; }
            set
            {
                if (_mapping.SpeedGearDownButton != value)
                {
                    _mapping.SpeedGearDownButton = value;
                    OnPropertyChanged("SpeedGearDownButton");
                }
            }
        }

        public int TurnGearUpButton
        {
            get { return _mapping.TurnGearUpButton; }
            set
            {
                if (_mapping.TurnGearUpButton != value)
                {
                    _mapping.TurnGearUpButton = value;
                    OnPropertyChanged("TurnGearUpButton");
                }
            }
        }

        public int TurnGearDownButton
        {
            get { return _mapping.TurnGearDownButton; }
            set
            {
                if (_mapping.TurnGearDownButton != value)
                {
                    _mapping.TurnGearDownButton = value;
                    OnPropertyChanged("TurnGearDownButton");
                }
            }
        }

        public int SchootButton
        {
            get { return _mapping.SchootButton; }
            set
            {
                if (_mapping.SchootButton != value)
                {
                    _mapping.SchootButton = value;
                    OnPropertyChanged("SchootButton");
                }
            }
        }

        public int ServoLockButton
        {
            get { return _mapping.ServoLockButton; }
            set
            {
                if (_mapping.ServoLockButton != value)
                {
                    _mapping.ServoLockButton = value;
                    OnPropertyChanged("ServoLockButton");
                }
            }
        }

        public int ServoStabalizeButton
        {
            get { return _mapping.ServoStabalizeButton; }
            set
            {
                if (_mapping.ServoStabalizeButton != value)
                {
                    _mapping.ServoStabalizeButton = value;
                    OnPropertyChanged("ServoStabalizeButton");
                }
            }
        }
        



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
