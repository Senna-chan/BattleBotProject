using System;
using System.Collections.Generic;
using OpenTK.Input;

namespace BattleBotClientWPF.Helpers
{
    public class ControllerHandler
    {
        const float ConversionFactor = 1.0f / (short.MaxValue /*+ 0.5f*/);
        float _speed = 0, _wheelPos = 0;
        byte _controllMode, _gear = 1, _speedgear = 1;
        //JoystickState oldState;
        float _xl, _yl, _xr, _yr, _r2, _l2;
        ButtonState _cross, _circle, _triangle, _square, _start, _selectB, _l1, _r1, _l3;
        JoystickHatState _hatSwitch;
        Dictionary<string, ButtonState> _psButtons = null;
        float _wheelPos1, _wheelPos2, servoPosX, servoPosY;
        int _freq;

        public ControllerHandler(Dictionary<string, ButtonState> oldPsButtons, int freq, string mode, byte controllmode = 0)
        {
            _freq = freq;
            if (controllmode != 0 && controllmode <= 2) _controllMode = controllmode;
            else _controllMode = 1;
            if (Joystick.GetState(0).IsConnected)
            {
                JoystickState joyStick = Joystick.GetState(0);
                _xl = joyStick.GetAxisRaw(JoystickAxis.Axis0); // Left  stick sideways
                _yl = joyStick.GetAxisRaw(JoystickAxis.Axis1) * -1; // Left  stick updown
                _xr = joyStick.GetAxisRaw(JoystickAxis.Axis3) * -1; // Right stick sideways
                _yr = joyStick.GetAxisRaw(JoystickAxis.Axis4); // Right stick updown
                _cross = joyStick.GetButton(JoystickButton.Button0); // Cross
                _circle = joyStick.GetButton(JoystickButton.Button1); // Circle
                _triangle = joyStick.GetButton(JoystickButton.Button3); // Triangle
                _square = joyStick.GetButton(JoystickButton.Button2); // Square
                _start = joyStick.GetButton(JoystickButton.Button7); // Start
                _selectB = joyStick.GetButton(JoystickButton.Button6); // Select
                _l1 = joyStick.GetButton(JoystickButton.Button4);    // L1
                _r1 = joyStick.GetButton(JoystickButton.Button5);    // R1
                _l3 = joyStick.GetButton(JoystickButton.Button8);   // L3
                _hatSwitch = joyStick.GetHat(JoystickHat.Hat0); // hatSwitch

//                Console.WriteLine("" +
//                                  "x left: {0}\n" +
//                                  "y left: {1}\n" +
//                                  "x right: {2}\n" +
//                                  "y right: {3}\n" +
//                                  "Speed: {4}\n" +
//                                  "Wheels: {5}\n" +
//                                  "ControllMode: {6}\n"
//                    , _xl, _yl, _xr, _yr, _speed * 2.55, _wheelPos, _controllMode);
                _psButtons = new Dictionary<string, ButtonState>
                {
                    {"cross", _cross},
                    {"circle", _circle},
                    {"triangle", _triangle},
                    {"square", _square},
                    {"start", _start},
                    {"select", _selectB},
                    {"l1", _l1 },
                    {"r1", _r1 },
                    {"l3", _l3 }
                };
//                foreach (var button in _psButtons)
//                {
//                    Console.WriteLine(button.Key + ": " + button.Value);
//                }
//                Console.WriteLine();

                if (oldPsButtons != null)
                {
                    #region Speed controll code

                    #region Left stick code

                    if (_controllMode == 1)
                    {
                        // Backwards
                        if (_yl > 500) _speed = _yl * ConversionFactor * 100;
                        else if (_yl < 500 && _speed < 1) _speed += 5f;
                        // Forwards
                        if (_yl < -500 && _yl != -32510)
                            _speed = _yl * ConversionFactor * 100; // gives an value between 0 and 100
                        else if (_yl == -32510 && _speed > 1) _speed = 100;
                        else if (_yl > -500 && _speed > 1) _speed -= 5f;
                        if (_speed >= -5 && _speed <= 5) _speed = 0; // Fix for absolute stop of engines
                    }

                    #endregion

                    #region cross+circle

                    if (_controllMode == 2)
                    {
                        if (_cross == ButtonState.Pressed && _hatSwitch.Position == HatPosition.Centered &&
                            !(_speed == 70 || _speed > 70))
                        {
                            _speed += 10;
                        }
                        else if (_cross == ButtonState.Pressed && _hatSwitch.IsUp && _speed != 100)
                        {
                            _speed += 10;
                        }
                        else if (_cross == ButtonState.Pressed && _hatSwitch.Position == HatPosition.Centered && _speed > 70)
                        {
                            _speed -= 10;
                        }
                        else if (_cross == ButtonState.Released && _speed > 0)
                        {
                            _speed = 0;
                        }
                        else if (_circle == ButtonState.Pressed && !(_speed == -70 || _speed < -70))
                        {
                            _speed -= 10;
                        }
                        else if (_circle == ButtonState.Pressed && _hatSwitch.IsDown && _speed != -100)
                        {
                            _speed -= 10;
                        }

                        else if (_circle == ButtonState.Pressed && _hatSwitch.Position == HatPosition.Centered &&
                                    _speed < -70)
                        {
                            _speed += 10;
                        }
                        else if (_circle == ButtonState.Released && _speed < 0)
                        {
                            _speed = 0;
                        }
                    }

                    #endregion

                    #endregion
                    #region steering controll code
                    #region Tank
                    if (mode == "Tank")
                    {
                        

                        #region leftstick

                        if (_controllMode == 1)
                        {
                            // Backwards
                            if (_xl > 500) _wheelPos1 = _xl * ConversionFactor * 100;
                            else if (_xl < 500 && _wheelPos1 > 1) _wheelPos1 = 0;
                            // Forwards
                            if (_xl < -500 && _xl != 32510) _wheelPos2 = _xl * ConversionFactor * 100 * -1;
                            else if (_xl == 32510 && _wheelPos2 > 1) _wheelPos2 = -100;
                            else if (_xl > -500 && _wheelPos2 < -1) _wheelPos2 = 0;
                            if (_xl == 0)
                            {
                                _wheelPos1 = 0;
                                _wheelPos2 = 0;
                            }
                        }

                        #endregion

                        #region cross+circle

                        if (_controllMode == 2)
                        {
                            if (_hatSwitch.IsRight && _wheelPos1 < 50) _wheelPos1 += 10;
                            else if (_hatSwitch.Position == HatPosition.Centered && _wheelPos1 > 0) _wheelPos1 = 0;
                            if (_hatSwitch.IsLeft && _wheelPos2 < 50) _wheelPos2 += 10;
                            else if (_hatSwitch.Position == HatPosition.Centered && _wheelPos2 > 0) _wheelPos2 = 0;
                        }

                        #endregion

                        
                    }
                    #endregion
                    #region Normal
                    if (mode == "Normal")
                    {
                        #region leftstick
                        if (_controllMode == 1)
                        {
                            // Backwards
                            if (_xl > 500) _wheelPos = _xl * ConversionFactor * 100;
                            else if (_xl < 500 && _wheelPos > 1) _wheelPos = 0;
                            // Forwards
                            if (_xl < -500 && _xl != 32510) _wheelPos = _xl * ConversionFactor * 100;
                            else if (_xl == 32510 && _wheelPos > 1) _wheelPos = -100;
                            else if (_xl > -500 && _wheelPos < -1) _wheelPos = 0;
                            if (_xl == 0)
                            {
                                _wheelPos = 0;
                            }
                        }
                        #endregion
                        #region cross+circle

                        if (_controllMode == 2)
                        {
                            if (_hatSwitch.IsRight) _wheelPos = 50;
                            else if (_hatSwitch.Position == HatPosition.Centered && _wheelPos > 0) _wheelPos = 0;
                            if (_hatSwitch.IsLeft) _wheelPos = -50;
                            else if (_hatSwitch.Position == HatPosition.Centered && _wheelPos < 0) _wheelPos = 0;
                        }

                        #endregion
                    }
                    #endregion
                    #endregion

                    #region pan_tilt
                    #region servoPosY
                    if (_yr > 500) servoPosY = _yr * ConversionFactor * 100;
                    else if (_yr < 500 && servoPosY > 1) servoPosY = 0;
                    // Forwards
                    if (_yr < -500 && _yr != 32510) servoPosY = _yr * ConversionFactor * 100;
                    else if (_yr == 32510 && servoPosY > 1) servoPosY = -100;
                    else if (_yr > -500 && servoPosY < -1) servoPosY = 0;
                    if (_yr == 0)
                    {
                        servoPosY = 0;
                    }
                    #endregion
                    #region servoPosX
                    // Backwards
                    if (_xr > 500) servoPosX = _xr * ConversionFactor * 100;
                    else if (_xr < 500 && servoPosX > 1) servoPosX = 0;
                    // Forwards
                    if (_xr < -500 && _xr != 32510) servoPosX = _xr * ConversionFactor * 100;
                    else if (_xr == 32510 && servoPosX > 1) servoPosX = -100;
                    else if (_xr > -500 && servoPosX < -1) servoPosX = 0;
                    if (_xr == 0)
                    {
                        servoPosX = 0;
                    }
                    #endregion
                    #endregion
                    #region Button checks

                    // Gets all the buttons out of the oldpsbuttons to see if it was pressed
                    ButtonState selectOld, startOld, triangleOld, squareOld, l1Old, r1Old, l3Old;
                    oldPsButtons.TryGetValue("select", out selectOld);
                    oldPsButtons.TryGetValue("start", out startOld);
                    oldPsButtons.TryGetValue("triangle", out triangleOld);
                    oldPsButtons.TryGetValue("square", out squareOld);
                    oldPsButtons.TryGetValue("l1", out l1Old);
                    oldPsButtons.TryGetValue("r1", out r1Old);
                    oldPsButtons.TryGetValue("l1", out l3Old);


                    if (_square == ButtonState.Released && squareOld == ButtonState.Pressed) _freq += 50;       // Freqency tuning in case its needed
                    if (_triangle == ButtonState.Released && triangleOld == ButtonState.Pressed) _freq -= 50;   // Freqency tuning in case its needed

                    if (_l1 == ButtonState.Released && l1Old == ButtonState.Pressed)
                    {
                        if (_gear != 1) _gear--;
                    }
                    if (_r1 == ButtonState.Released && r1Old == ButtonState.Pressed)
                    {
                        if (_gear != 3) _gear++;
                    }

                    if (_l3 == ButtonState.Released && l3Old == ButtonState.Pressed) // Softreset of the speed and wheelpos in case of a glitch in the controls
                    {
                        _speed = 0;
                        _wheelPos = 0;
                    }

                    if (_selectB == ButtonState.Released && selectOld == ButtonState.Pressed)
                    {
                        if (_controllMode < 2) _controllMode += 1;
                        else _controllMode = 1;

                        _speed = 0;
                        _wheelPos = 0;
                    }

                    //                    switch (gear)
                    //                    {
                    //                        case 1:
                    //                            if (speed < 50 && speed > -50) speed = 0;
                    //                            break;
                    //                        case 2:
                    //                            if (speed < 35 && speed > -35) speed = 0;
                    //                            break;
                    //                        case 3:
                    //                            if (speed < 20 && speed > -20) speed = 0;
                    //                            break;
                    //                        default:
                    //                            break;
                    //                    }

                    #endregion
                }
            }
        }

        public int GetSpeed()
        {
            return Convert.ToInt32(Math.Floor(_speed));
        }

        public int GetWheelPos1()
        {
            return Convert.ToInt32(Math.Floor(_wheelPos1));
        }

        public int GetWheelPos2()
        {
            return Convert.ToInt32(Math.Floor(_wheelPos2));
        }

        public int GetWheelPos()
        {
            return Convert.ToInt32(Math.Floor(_wheelPos));
        }

        public int GetServoX()
        {
            return Convert.ToInt32(Math.Floor(servoPosX));
        }

        public int GetServoY()
        {
            return Convert.ToInt32(Math.Floor(servoPosY));
        }

        public int GetFrequency()
        {
            return _freq;
        }

        public byte[] GetVars()
        {
            return new[] { _controllMode, _gear, _speedgear };
        }

        public Dictionary<string, ButtonState> GetPsButtons()
        {
            return _psButtons;
        }
    }
}
