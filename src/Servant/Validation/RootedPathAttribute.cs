using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Servant.Validation
{
    public class RootedPathAttribute : ValidationAttribute
    {
        public RootedPathAttribute() : base(() => "The path is invalid.")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var path = value as string;
            if (path == null)
                return ValidationResult.Success;

            if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                return new ValidationResult("The path contains invalid characters.");

            var result = CheckPathExistance(path);
            if (result != ValidationResult.Success)
                return result;

            if (!Path.IsPathRooted(path))
                return new ValidationResult("The path should be a rooted path.");

            return ValidationResult.Success;
        }

        protected virtual ValidationResult CheckPathExistance(string path)
        {
            return ValidationResult.Success;
        }
    }
}
