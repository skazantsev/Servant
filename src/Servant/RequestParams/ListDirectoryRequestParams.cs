using Servant.Validation;
using System.ComponentModel.DataAnnotations;

namespace Servant.RequestParams
{
    public class ListDirectoryRequestParams
    {
        [Required]
        [RootedDirectoryPath]
        public string Path { get; set; }
    }
}
