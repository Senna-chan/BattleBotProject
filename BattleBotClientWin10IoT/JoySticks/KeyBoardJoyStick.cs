using System.Collections.Generic;
using System.Linq;
using Windows.System;
using BattleBotClientWin10IoT.Helpers;
using BattleBotClientWin10IoT.Interfaces;
using BattleBotClientWin10IoT.Models;

namespace BattleBotClientWin10IoT.JoySticks
{
    internal class KeyboardJoystick : IJoyStickInterface
    {
        private readonly List<KeyHelperModel> oldKeys = new List<KeyHelperModel>
        {
            new KeyHelperModel {Key = VirtualKey.I, KeyDown = false},
            new KeyHelperModel {Key = VirtualKey.J, KeyDown = false},
            new KeyHelperModel {Key = VirtualKey.K, KeyDown = false},
            new KeyHelperModel {Key = VirtualKey.L, KeyDown = false},
            new KeyHelperModel {Key = VirtualKey.Space, KeyDown = false},
            new KeyHelperModel {Key = VirtualKey.CapitalLock, KeyDown = false},
            new KeyHelperModel {Key = VirtualKey.Tab, KeyDown = false}
        };
        public JoyStickTypes JoystickType => JoyStickTypes.Keyboard;

        public int SpeedAxis { get; set; }
        public int TurnAxis { get; set; }
        public int PanAxis { get; set; }
        public int TiltAxis { get; set; }

        public void GetControllerData()
        {
            if (KeyHelper.IsKeyDown(VirtualKey.W))
            {
                SpeedAxis = 1;
            }
            else if (KeyHelper.IsKeyDown(VirtualKey.S))
            {
                SpeedAxis = -1;
            }
            else
            {
                SpeedAxis = 0;
            }
            if (KeyHelper.IsKeyDown(VirtualKey.A))
            {
                TurnAxis = -1;
            }
            else if (KeyHelper.IsKeyDown(VirtualKey.D))
            {
                TurnAxis = 1;
            }
            else
            {
                TurnAxis = 0;
            }
            if (KeyHelper.IsKeyDown(VirtualKey.Up))
            {
                if (TiltAxis != 100) TiltAxis += 1;
            }
            else if (KeyHelper.IsKeyDown(VirtualKey.Down))
            {
                if (TiltAxis != 0) TiltAxis -= 1;
            }
            if (KeyHelper.IsKeyDown(VirtualKey.Left))
            {
                if (PanAxis != 0) PanAxis -= 1;
            }
            else if (KeyHelper.IsKeyDown(VirtualKey.Right))
            {
                if (PanAxis != 100) PanAxis += 1;
            }
        }

        public int GetSpeedAxisPosition()
        {
            return GeneralHelpers.MapIntToValue(SpeedAxis, -1, 1, -100, 100);
        }

        public int GetTurnAxisPosition()
        {
            return GeneralHelpers.MapIntToValue(TurnAxis, -1, 1, -100, 100);
        }

        public int GetPanAxisPosition()
        {
            return PanAxis;
        }

        public int GetTiltAxisPosition()
        {
            return TiltAxis;
        }

        public bool GetShootButtonState()
        {
            return ButtonPressed(VirtualKey.Space);
        }

        public bool GetSpeedDownGearButtonState()
        {
            return ButtonPressed(VirtualKey.K);
        }

        public bool GetSpeedUpGearButtonState()
        {
            return ButtonPressed(VirtualKey.I);
        }

        public bool GetTurnSharperGearButtonState()
        {
            return ButtonPressed(VirtualKey.L);
        }

        public bool GetTurnWeakerGearButtonState()
        {
            return ButtonPressed(VirtualKey.J);
        }

        public void PopulateOldButtons()
        {
            foreach (var oldKey in oldKeys)
                oldKey.KeyDown = KeyHelper.IsKeyDown(oldKey.Key);
        }

        public bool GetServoLockButtonState()
        {
            return ButtonPressed(VirtualKey.CapitalLock);
        }

        public bool GetServoStabalizeButtonState()
        {
            return ButtonPressed(VirtualKey.Tab);
        }

        private bool ButtonPressed(VirtualKey key)
        {
            return oldKeys.First(k => k.Key == key).KeyDown && KeyHelper.IsKeyDown(key);
        }

        public bool IsConnected { get { return true; } set { IsConnected = true; }}
        public bool IsWireless { get { return false; } set { IsWireless = false; } }
        public int? GetBatteryStatus()
        {
            return null;
        }
    }
}