namespace Maliev.SupplierService.Api.DTOs.Responses;

/// <summary>
/// Represents a standardized error response from the API, following the Problem Details for HTTP APIs (RFC 7807) specification.
/// </summary>
/// <param name="Type">A URI reference that identifies the problem type.</param>
/// <param name="Title">A short, human-readable summary of the problem type.</param>
/// <param name="Status">The HTTP status code (e.g., 400, 500).</param>
/// <param name="Detail">A human-readable explanation specific to this occurrence of the problem.</param>
/// <param name="Instance">A URI reference that identifies the specific occurrence of the problem.</param>
/// <param name="Extensions">A dictionary of extension members for the problem details.</param>
public record ErrorResponse(
    string Type,
    string Title,
    int Status,
    string? Detail = null,
    string? Instance = null,
    IDictionary<string, object?>? Extensions = null);
