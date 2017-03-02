namespace BattleBotClientWin10IoT.Interfaces
{
    public enum JoyStickTypes// Add custom joystick types here and add them to the class that uses this interface for custom handling of certain things
    {
        Keyboard,
        HIDDevice,
        PiHardware,
        PSController
    }
    interface IJoyStickInterface
    {
        JoyStickTypes JoystickType { get; }
        int SpeedAxis { get; set; }
        int TurnAxis { get; set; }
        int PanAxis { get; set; }
        int TiltAxis { get; set; }
        void GetControllerData();
        int GetSpeedAxisPosition();
        int GetTurnAxisPosition();
        int GetPanAxisPosition();
        int GetTiltAxisPosition();
        bool GetShootButtonState();
        bool GetSpeedDownGearButtonState();
        bool GetSpeedUpGearButtonState();
        bool GetTurnSharperGearButtonState();
        bool GetTurnWeakerGearButtonState();
        void PopulateOldButtons();
    }
}
