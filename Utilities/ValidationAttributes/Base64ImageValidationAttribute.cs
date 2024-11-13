using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Spider_QAMS.Utilities.ValidationAttributes
{
    public class Base64ImageValidationAttribute: ValidationAttribute
    {
        private readonly int _maxFileSize;
        private readonly string[] _allowedExtensions;

        public Base64ImageValidationAttribute(int maxFileSize, string allowedExtensions)
        {
            _maxFileSize = maxFileSize;
            _allowedExtensions = allowedExtensions.ToLower().Split(',');
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;

            }

            string base64Data = value.ToString();

            // Regex to extract base64 data and determine mine type
            var match = Regex.Match(base64Data, @"^data:image\/([a-zA-Z]+);base64,(.*)$");

            if (!match.Success)
            {
                return new ValidationResult("Invalid base64 image format.");
            }

            string fileExtension = match.Groups[1].Value.ToLower();
            if(!_allowedExtensions.Contains(fileExtension))
            {
                return new ValidationResult($"Only {string.Join(",",_allowedExtensions)} images are allowed.");
            }

            // Check the file size (base64-encoded data will be larger than the original file size)
            // Estimate base64 size by multiplying by 1.37 (base64 encoding overhead)

            int estimateFileSize = (int)(base64Data.Length * 0.75);

            if(estimateFileSize > _maxFileSize)
            {
                return new ValidationResult($"File size cannot exceed {_maxFileSize/1024} KB");
            }

            return ValidationResult.Success;
        }
    }
}
