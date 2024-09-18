using Spider_QAMS.Repositories.Skeleton;
using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Utilities.ValidationAttributes
{
    public class CheckUniquenessinDBAttribute : ValidationAttribute
    {
        private readonly string _field;
        public CheckUniquenessinDBAttribute(string field)
        {
            _field = field;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var uniquenessCheckService = (IUniquenessCheckService)validationContext.GetService(typeof(IUniquenessCheckService));
            var val = value == null ? string.Empty : value.ToString();
            if (_field != null)
            {
                var result = uniquenessCheckService.IsUniqueAsync(_field, val).Result;
                if (!result)
                {
                    return new ValidationResult($"'{val}' is already taken. Please provide another {_field}.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
