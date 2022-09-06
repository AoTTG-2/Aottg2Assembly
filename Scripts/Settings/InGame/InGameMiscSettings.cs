namespace Settings
{
    class InGameMiscSettings : BaseSettingsContainer
    {
        public IntSetting PVP = new IntSetting(0);
        public BoolSetting EndlessRespawnEnabled = new BoolSetting(false);
        public IntSetting EndlessRespawnTime = new IntSetting(0, minValue: 5);
        public BoolSetting AllowBlades = new BoolSetting(true);
        public BoolSetting AllowGuns = new BoolSetting(true);
        public BoolSetting AllowThunderspears = new BoolSetting(true);
        public BoolSetting AllowPlayerTitans = new BoolSetting(true);
        public BoolSetting AllowShifterSpecials = new BoolSetting(true);
        public BoolSetting AllowShifters = new BoolSetting(false);
        public BoolSetting Horses = new BoolSetting(false);
        public BoolSetting GunsAirReload = new BoolSetting(true);
        public BoolSetting PreserveKDR = new BoolSetting(false);
        public BoolSetting ClearKDROnRestart = new BoolSetting(false);
        public BoolSetting GlobalMinimapDisable = new BoolSetting(false);
        public StringSetting Motd = new StringSetting(string.Empty, maxLength: 1000);
    }

    public enum PVPMode
    {
        Off,
        FFA,
        Team
    }
}