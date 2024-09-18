using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Utilities.ValidationAttributes
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(ErrorMessage ?? $"File size cannot exceed {_maxFileSize / 1024} KB.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
