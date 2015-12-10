using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Servant.Validation;

namespace Servant.RequestParams
{
    [DataContract]
    public class GetFileRequest
    {
        [Required]
        [RootedFilePath]
        public string Path { get; set; }
    }
}
