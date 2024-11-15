using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Utilities.ValidationAttributes
{
    public class MaxCountAttribute: ValidationAttribute
    {
        private readonly int _maxCount;
        private readonly Type _itemType;
        public MaxCountAttribute(int maxCount, Type itemType=null)
        {
            _maxCount = maxCount;
            _itemType = itemType;
        }
        // Internal property to expose the max count value within the same assembly
        internal int MaxCount => _maxCount;
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value is IEnumerable collection)
            {
                // Validate item type if specified
                if(_itemType != null && collection.Cast<object>().Any(item => item?.GetType() != _itemType))
                {
                    return new ValidationResult($"All items must be of type {_itemType.Name}.", new[] { validationContext.MemberName });
                }

                // Validate collection count
                if(collection.Cast<object>().Count() > _maxCount)
                {
                    return new ValidationResult($"The collection cannot exceed {_maxCount} items.", new[] {validationContext.MemberName});
                }
            }
            else if(value != null)
            {
                return new ValidationResult($"{validationContext.DisplayName} must be a collection.");
            }
            return ValidationResult.Success;
        }
    }
}
