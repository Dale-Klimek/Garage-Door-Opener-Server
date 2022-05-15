namespace GarageDoorOpener.Server
{
    public class Configuration
    {
        public const string Config = "PinConfiguration";
        public int Delay { get; set; }
        public int RightDoorPin { get; set; }
        public int LeftDoorPin { get; set; }
    }
}
