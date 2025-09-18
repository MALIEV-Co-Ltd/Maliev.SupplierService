using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Maliev.SupplierService.Data.DbContexts;
using Maliev.SupplierService.Data.Entities;

namespace Maliev.SupplierService.Tests.DbContexts;

public class SupplierDbContextTests : IDisposable
{
    private readonly SupplierDbContext _context;

    public SupplierDbContextTests()
    {
        var options = new DbContextOptionsBuilder<SupplierDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SupplierDbContext(options);
    }

    [Fact]
    public async Task SaveChanges_SetsAuditFields_OnCreate()
    {
        // Arrange
        var supplier = new Supplier
        {
            Name = "Test Supplier",
            Status = SupplierStatus.Active
        };

        // Act
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Assert
        supplier.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        supplier.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        supplier.CreatedAt.Should().Be(supplier.UpdatedAt);
    }

    [Fact]
    public async Task SaveChanges_UpdatesAuditFields_OnModify()
    {
        // Arrange
        var supplier = new Supplier
        {
            Name = "Test Supplier",
            Status = SupplierStatus.Active
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var originalUpdatedAt = supplier.UpdatedAt;

        // Wait a small amount to ensure timestamp difference
        await Task.Delay(10);

        // Act
        supplier.Name = "Updated Supplier";
        await _context.SaveChangesAsync();

        // Assert
        supplier.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        supplier.CreatedAt.Should().BeBefore(supplier.UpdatedAt);
    }

    [Fact]
    public async Task SupplierCategory_CascadeDelete_RemovesRelatedSuppliers()
    {
        // Arrange
        var category = new SupplierCategory
        {
            Name = "Test Category",
            IsActive = true
        };

        var supplier = new Supplier
        {
            Name = "Test Supplier",
            Status = SupplierStatus.Active,
            Category = category
        };

        _context.SupplierCategories.Add(category);
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act
        _context.SupplierCategories.Remove(category);
        await _context.SaveChangesAsync();

        // Assert
        var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == supplier.Id);
        supplierExists.Should().BeFalse();
    }

    [Fact]
    public async Task SupplierContacts_CascadeDelete_WithSupplier()
    {
        // Arrange
        var supplier = new Supplier
        {
            Name = "Test Supplier",
            Status = SupplierStatus.Active
        };

        var contact = new SupplierContact
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Supplier = supplier
        };

        _context.Suppliers.Add(supplier);
        _context.SupplierContacts.Add(contact);
        await _context.SaveChangesAsync();

        // Act
        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        // Assert
        var contactExists = await _context.SupplierContacts.AnyAsync(c => c.Id == contact.Id);
        contactExists.Should().BeFalse();
    }

    [Fact]
    public async Task Supplier_CanHaveMultipleContacts()
    {
        // Arrange
        var supplier = new Supplier
        {
            Name = "Test Supplier",
            Status = SupplierStatus.Active
        };

        var contacts = new[]
        {
            new SupplierContact
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Role = ContactRole.Primary,
                Supplier = supplier
            },
            new SupplierContact
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Role = ContactRole.Finance,
                Supplier = supplier
            }
        };

        // Act
        _context.Suppliers.Add(supplier);
        _context.SupplierContacts.AddRange(contacts);
        await _context.SaveChangesAsync();

        // Assert
        var supplierWithContacts = await _context.Suppliers
            .Include(s => s.Contacts)
            .FirstAsync(s => s.Id == supplier.Id);

        supplierWithContacts.Contacts.Should().HaveCount(2);
        supplierWithContacts.Contacts.Should().Contain(c => c.Role == ContactRole.Primary);
        supplierWithContacts.Contacts.Should().Contain(c => c.Role == ContactRole.Finance);
    }

    [Fact]
    public async Task SupplierRating_CalculatesCorrectly()
    {
        // Arrange
        var supplier = new Supplier
        {
            Name = "Test Supplier",
            Status = SupplierStatus.Active
        };

        var rating = new SupplierRating
        {
            RatingPeriod = "Q1 2024",
            RatingDate = DateTimeOffset.UtcNow,
            QualityRating = 4.5m,
            DeliveryRating = 4.2m,
            ServiceRating = 4.8m,
            PricingRating = 4.0m,
            CommunicationRating = 4.3m,
            OverallRating = 4.36m,
            Supplier = supplier
        };

        // Act
        _context.Suppliers.Add(supplier);
        _context.SupplierRatings.Add(rating);
        await _context.SaveChangesAsync();

        // Assert
        var savedRating = await _context.SupplierRatings.FirstAsync();
        savedRating.OverallRating.Should().Be(4.36m);
        savedRating.QualityRating.Should().Be(4.5m);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}