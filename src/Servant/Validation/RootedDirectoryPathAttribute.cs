using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Servant.Validation
{
    public class RootedDirectoryPathAttribute : RootedPathAttribute
    {
        protected override ValidationResult CheckPathExistance(string path)
        {
            return Directory.Exists(path)
                ? ValidationResult.Success
                : new ValidationResult("The directory is not exists.");
        }
    }
}
