using System.ComponentModel.DataAnnotations;
using Servant.Validation;

namespace Servant.RequestParams
{
    public class ListDirectoryRequest
    {
        [Required]
        [RootedPath]
        public string Path { get; set; }
    }
}
