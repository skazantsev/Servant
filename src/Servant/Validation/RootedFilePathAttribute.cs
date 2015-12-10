﻿using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Servant.Validation
{
    public class RootedFilePathAttribute : RootedPathAttribute
    {
        protected override ValidationResult CheckPathExistance(string path)
        {
            return File.Exists(path)
                ? ValidationResult.Success
                : new ValidationResult("The file is not exists.");
        }
    }
}