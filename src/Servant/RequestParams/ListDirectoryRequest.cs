using Servant.Validation;
using System.ComponentModel.DataAnnotations;

namespace Servant.RequestParams
{
    public class ListDirectoryRequest
    {
        [Required]
        [RootedPath]
        public string Path { get; set; }
    }
}
