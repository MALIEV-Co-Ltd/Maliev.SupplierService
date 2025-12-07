using System.ComponentModel.DataAnnotations;

namespace Maliev.SupplierService.Api.Attributes;

/// <summary>
/// Validation attribute that ensures a DateOnly value is not in the future (must be today or earlier).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class NotFutureDateAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFutureDateAttribute"/> class.
    /// </summary>
    public NotFutureDateAttribute()
        : base("The {0} field must not be a future date.")
    {
    }

    /// <summary>
    /// Validates that the specified value is not a future date.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A <see cref="ValidationResult"/> indicating success or failure.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        DateOnly dateValue;

        if (value is DateOnly date)
        {
            dateValue = date;
        }
        else if (value is DateTime dateTime)
        {
            dateValue = DateOnly.FromDateTime(dateTime);
        }
        else
        {
            return new ValidationResult($"The {validationContext.DisplayName} field must be a valid date.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (dateValue > today)
        {
            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }

        return ValidationResult.Success;
    }
}
