namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a standardized validation error response from the API.
/// </summary>
/// <param name="Type">A URI reference that identifies the problem type.</param>
/// <param name="Title">A short, human-readable summary of the problem type (e.g., "Validation Error").</param>
/// <param name="Status">The HTTP status code (e.g., 400 Unprocessable Entity).</param>
/// <param name="Errors">A dictionary of validation errors, where the key is the field name and the value is an array of error messages.</param>
public record ValidationErrorResponse(
    string Type,
    string Title,
    int Status,
    IDictionary<string, string[]> Errors);
