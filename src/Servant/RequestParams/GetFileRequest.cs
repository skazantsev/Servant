using Servant.Validation;
using System.ComponentModel.DataAnnotations;

namespace Servant.RequestParams
{
    public class GetFileRequest
    {
        [Required]
        [RootedFilePath]
        public string Path { get; set; }
    }
}
