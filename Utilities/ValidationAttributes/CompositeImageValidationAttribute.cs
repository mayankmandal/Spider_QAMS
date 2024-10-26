using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Utilities.ValidationAttributes
{
    public class CompositeImageValidationAttribute: ValidationAttribute
    {
        private readonly string[] _allowedExtensions;
        private readonly int _maxFileSize;
        private readonly int _maxImageCount;
        public CompositeImageValidationAttribute(string[] allowedExtensions, int maxFileSize, int maxImageCount, string? errorMessage = null)
        {
            _allowedExtensions = allowedExtensions;
            _maxFileSize = maxFileSize;
            _maxImageCount = maxImageCount;
            ErrorMessage = errorMessage ?? "Invalid image upload.";

        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is List<IFormFile> files)
            {
                if (files.Count > _maxImageCount)
                {
                    return new ValidationResult($"You can upload a maximum of {_maxImageCount} images.");
                }

                foreach (var file in files)
                {
                    // Validate each file using AllowedExtensions
                    var extensionResult = new AllowedExtensionsAttribute(_allowedExtensions).GetValidationResult(file, validationContext);
                    if (extensionResult != ValidationResult.Success)
                    {
                        return extensionResult; // Return if any image fails extension validation.
                    }

                    // Validate each file using MaxFileSize
                    var sizeResult = new MaxFileSizeAttribute(_maxFileSize).GetValidationResult(file, validationContext);
                    if (sizeResult != ValidationResult.Success)
                    {
                        return sizeResult; // Return if any image exceeds the max size.
                    }
                }
                return ValidationResult.Success;
            }
            return new ValidationResult($"Uploaded wrong file types. Please recheck the uploaded images!");
        }
    }
}
