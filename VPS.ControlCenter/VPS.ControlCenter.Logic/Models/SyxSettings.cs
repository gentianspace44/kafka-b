namespace VPS.ControlCenter.Logic.Models
{
    public class SyxSettings
    {
        public string SyxEndPoint { get; set; }
        public string SyXUsername { get; set; }
        public string SyXPassword { get; set; }
        public int TokenKeepAliveTimerInMinutes { get; set; }
    }
}
