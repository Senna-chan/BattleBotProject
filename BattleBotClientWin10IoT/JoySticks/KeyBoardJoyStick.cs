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
            new KeyHelperModel {Key = VirtualKey.Space, KeyDown = false}
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
            return oldKeys.First(t => t.Key == VirtualKey.Space).KeyDown && KeyHelper.IsKeyUp(VirtualKey.Space);
        }

        public bool GetSpeedDownGearButtonState()
        {
            return oldKeys.First(t => t.Key == VirtualKey.K).KeyDown && KeyHelper.IsKeyUp(VirtualKey.K);
        }

        public bool GetSpeedUpGearButtonState()
        {
            return oldKeys.First(t => t.Key == VirtualKey.I).KeyDown && KeyHelper.IsKeyUp(VirtualKey.I);
        }

        public bool GetTurnSharperGearButtonState()
        {
            return oldKeys.First(t => t.Key == VirtualKey.L).KeyDown && KeyHelper.IsKeyUp(VirtualKey.L);
        }

        public bool GetTurnWeakerGearButtonState()
        {
            return oldKeys.First(t => t.Key == VirtualKey.J).KeyDown && KeyHelper.IsKeyUp(VirtualKey.J);
        }

        public void PopulateOldButtons()
        {
            foreach (var oldKey in oldKeys)
                oldKey.KeyDown = KeyHelper.IsKeyDown(oldKey.Key);
        }

    }
}