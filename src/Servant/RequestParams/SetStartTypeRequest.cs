using System.ComponentModel.DataAnnotations;
using Servant.Services.WinService;

namespace Servant.RequestParams
{
    public class SetStartTypeRequest
    {
        [Required]
        public ServiceStartType StartType { get; set; }
    }
}
