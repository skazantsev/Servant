namespace Servant.Models
{
    public class WinService
    {
        private WinService()
        { }

        public class SimpleInfo
        {
            public string ServiceName { get; set; }
        }

        public class FullInfo
        {
            public string ServiceName { get; set; }

            public string DisplayName { get; set; }

            public string Description { get; set; }

            public string State { get; set; }

            public string StartMode { get; set; }

            public string Account { get; set; }

            public string PathName { get; set; }
        }
    }
}
