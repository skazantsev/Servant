using System.ComponentModel.DataAnnotations;
using Servant.Validation;

namespace Servant.RequestParams
{
    public class GetFileRequest
    {
        [Required]
        [RootedPath]
        public string Path { get; set; }
    }
}
