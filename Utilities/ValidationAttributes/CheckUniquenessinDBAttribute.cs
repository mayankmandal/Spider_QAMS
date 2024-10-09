using Spider_QAMS.Repositories.Skeleton;
using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Utilities.ValidationAttributes
{
    public class CheckUniquenessinDBAttribute : ValidationAttribute
    {
        private readonly string _field1;
        private readonly string? _field2;
        // Constructor for one field
        public CheckUniquenessinDBAttribute(string field1)
        {
            _field1 = field1;
        }
        // Constructor for two fields
        public CheckUniquenessinDBAttribute(string field1, string field2 = null)
        {
            _field1 = field1;
            _field2 = field2;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var uniquenessCheckService = (IUniquenessCheckService)validationContext.GetService(typeof(IUniquenessCheckService));

            // Get value of the first field (the one where the annotation is applied)
            var value1 = value == null ? string.Empty : value.ToString();

            // Fetch value of the second field if provided
            string? value2 = null;

            if (_field2 != null)
            {
                var field2Property = validationContext.ObjectType.GetProperty(_field2);
                if (field2Property == null)
                {
                    throw new ArgumentNullException(nameof(_field2), $"Property {_field2} not found on {validationContext.ObjectType.Name}");
                }

                var field2Value = field2Property.GetValue(validationContext.ObjectInstance, null);
                value2 = field2Value?.ToString() ?? string.Empty;
            }

            // Call the service to check uniqueness with one or two fields
            var result = uniquenessCheckService.IsUniqueAsync(_field1, value1, _field2, value2).Result;

            if (!result)
            {
                return new ValidationResult($"Combination of {_field1} '{value1}' and {_field2} '{value2}' is already taken. Please provide another unique combination.");
            }
            return ValidationResult.Success;
        }
    }
}
