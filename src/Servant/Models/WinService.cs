namespace Servant.Models
{
    public interface IWinServiceInfo
    {
        string DisplayName { get; }
    }

    public class WinServiceSimpleInfo : IWinServiceInfo
    {
        public string DisplayName { get; set; }
    }

    public class WinServiceFullInfo : IWinServiceInfo
    {
        public string DisplayName { get; set; }

        public string ServiceName { get; set; }

        public string Description { get; set; }

        public string State { get; set; }

        public string StartMode { get; set; }

        public string Account { get; set; }

        public string PathName { get; set; }
    }
}
