namespace Maliev.SupplierService.Api.Models;

public class SupplierRatingDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public required string RatingPeriod { get; set; }
    public DateTimeOffset RatingDate { get; set; }
    public decimal QualityRating { get; set; }
    public decimal DeliveryRating { get; set; }
    public decimal ServiceRating { get; set; }
    public decimal PricingRating { get; set; }
    public decimal CommunicationRating { get; set; }
    public decimal OverallRating { get; set; }
    public int? TotalOrders { get; set; }
    public int? OnTimeDeliveries { get; set; }
    public int? QualityIssues { get; set; }
    public string? Comments { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class CreateSupplierRatingRequest
{
    public required string RatingPeriod { get; set; }
    public DateTimeOffset RatingDate { get; set; }
    public decimal QualityRating { get; set; }
    public decimal DeliveryRating { get; set; }
    public decimal ServiceRating { get; set; }
    public decimal PricingRating { get; set; }
    public decimal CommunicationRating { get; set; }
    public decimal OverallRating { get; set; }
    public int? TotalOrders { get; set; }
    public int? OnTimeDeliveries { get; set; }
    public int? QualityIssues { get; set; }
    public string? Comments { get; set; }
    public string? ReviewedBy { get; set; }
}

public class UpdateSupplierRatingRequest
{
    public required string RatingPeriod { get; set; }
    public DateTimeOffset RatingDate { get; set; }
    public decimal QualityRating { get; set; }
    public decimal DeliveryRating { get; set; }
    public decimal ServiceRating { get; set; }
    public decimal PricingRating { get; set; }
    public decimal CommunicationRating { get; set; }
    public decimal OverallRating { get; set; }
    public int? TotalOrders { get; set; }
    public int? OnTimeDeliveries { get; set; }
    public int? QualityIssues { get; set; }
    public string? Comments { get; set; }
    public string? ReviewedBy { get; set; }
}