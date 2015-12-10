using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Servant.Validation
{
    public class RootedFilePathAttribute : ValidationAttribute
    {
        public RootedFilePathAttribute() : base(() => "The path is invalid.")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var str = value as string;
            if (str == null)
                return ValidationResult.Success;

            if (str.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                return new ValidationResult("The path contains invalid characters.");

            if (!File.Exists(str))
                return new ValidationResult("The file is not exists.");

            if (!Path.IsPathRooted(str))
                return new ValidationResult("The path should be a rooted path.");

            return ValidationResult.Success;
        }
    }
}
