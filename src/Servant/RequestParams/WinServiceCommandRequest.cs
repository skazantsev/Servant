using System.ComponentModel.DataAnnotations;

namespace Servant.RequestParams
{
    public class WinServiceCommandRequest
    {
        [Required]
        public string Action { get; set; }

        public string Value { get; set; }
    }
}
