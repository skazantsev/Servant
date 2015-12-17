using Servant.Validation;
using System.ComponentModel.DataAnnotations;

namespace Servant.RequestParams
{
    public class GetFileRequest
    {
        [Required]
        [RootedPath]
        public string Path { get; set; }
    }
}
