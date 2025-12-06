using FluentValidation;
using Maliev.SupplierService.Api.DTOs.Requests;

namespace Maliev.SupplierService.Api.Validators;

/// <summary>
/// Validator for <see cref="CreateEvaluationRequest"/> objects.
/// </summary>
public class CreateEvaluationRequestValidator : AbstractValidator<CreateEvaluationRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateEvaluationRequestValidator"/> class.
    /// Defines validation rules for properties of <see cref="CreateEvaluationRequest"/>.
    /// </summary>
    public CreateEvaluationRequestValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid performance category");

        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5).WithMessage("Score must be between 1 and 5");

        RuleFor(x => x.EvaluationDate)
            .NotEmpty().WithMessage("Evaluation date is required")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Evaluation date cannot be in the future");

        RuleFor(x => x.Comments)
            .MaximumLength(2000).WithMessage("Comments must not exceed 2000 characters")
            .When(x => x.Comments is not null);
    }
}
