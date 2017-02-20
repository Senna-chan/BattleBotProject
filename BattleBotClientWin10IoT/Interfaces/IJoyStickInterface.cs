namespace BattleBotClientWin10IoT.Interfaces
{
    interface IJoyStickInterface
    {
        int SpeedAxis { get; set; }
        int TurnAxis { get; set; }
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
    }
}
