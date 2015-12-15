using System.ComponentModel.DataAnnotations;

namespace Servant.RequestParams
{
    public class CopyFSPathRequest
    {
        [Required]
        public string SourcePath { get; set; }

        [Required]
        public string DestPath { get; set; }

        public bool Overwrite { get; set; }
    }
}
