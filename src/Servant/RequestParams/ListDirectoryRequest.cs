using Servant.Validation;
using System.ComponentModel.DataAnnotations;

namespace Servant.RequestParams
{
    public class ListDirectoryRequest
    {
        [Required]
        [RootedDirectoryPath]
        public string Path { get; set; }
    }
}
