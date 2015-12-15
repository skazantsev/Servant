using System.ComponentModel.DataAnnotations;

namespace Servant.RequestParams
{
    public class DeleteFsPathRequest
    {
        [Required]
        public string FileSystemPath { get; set; }
    }
}
