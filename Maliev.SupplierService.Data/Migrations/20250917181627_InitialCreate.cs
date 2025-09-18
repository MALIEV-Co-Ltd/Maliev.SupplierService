using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Maliev.SupplierService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupplierCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    CurrencyId = table.Column<int>(type: "integer", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: true),
                    MinimumOrderAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PaymentTerms = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreditLimit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    QualityRating = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    DeliveryRating = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    ServiceRating = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suppliers_SupplierCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "SupplierCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SupplierAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    Building = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AddressLine1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AddressLine2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierAddresses_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierContacts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    UploadServiceFileId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDocuments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    RatingPeriod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RatingDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    QualityRating = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    DeliveryRating = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    ServiceRating = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    PricingRating = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    CommunicationRating = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    OverallRating = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    TotalOrders = table.Column<int>(type: "integer", nullable: true),
                    OnTimeDeliveries = table.Column<int>(type: "integer", nullable: true),
                    QualityIssues = table.Column<int>(type: "integer", nullable: true),
                    Comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierRatings_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAddresses_CountryId",
                table: "SupplierAddresses",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAddresses_CreatedAt",
                table: "SupplierAddresses",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAddresses_IsActive",
                table: "SupplierAddresses",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAddresses_IsPrimary",
                table: "SupplierAddresses",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAddresses_SupplierId",
                table: "SupplierAddresses",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAddresses_SupplierId_IsPrimary",
                table: "SupplierAddresses",
                columns: new[] { "SupplierId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAddresses_SupplierId_Type",
                table: "SupplierAddresses",
                columns: new[] { "SupplierId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAddresses_Type",
                table: "SupplierAddresses",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategories_CreatedAt",
                table: "SupplierCategories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategories_IsActive",
                table: "SupplierCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCategories_Name",
                table: "SupplierCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContacts_CreatedAt",
                table: "SupplierContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContacts_Email",
                table: "SupplierContacts",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContacts_IsActive",
                table: "SupplierContacts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContacts_IsPrimary",
                table: "SupplierContacts",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContacts_Role",
                table: "SupplierContacts",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContacts_SupplierId",
                table: "SupplierContacts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierContacts_SupplierId_IsPrimary",
                table: "SupplierContacts",
                columns: new[] { "SupplierId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDocuments_CreatedAt",
                table: "SupplierDocuments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDocuments_IsActive",
                table: "SupplierDocuments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDocuments_IsRequired",
                table: "SupplierDocuments",
                column: "IsRequired");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDocuments_SupplierId",
                table: "SupplierDocuments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDocuments_SupplierId_IsRequired",
                table: "SupplierDocuments",
                columns: new[] { "SupplierId", "IsRequired" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDocuments_SupplierId_Type",
                table: "SupplierDocuments",
                columns: new[] { "SupplierId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDocuments_Type",
                table: "SupplierDocuments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDocuments_UploadServiceFileId",
                table: "SupplierDocuments",
                column: "UploadServiceFileId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDocuments_ValidFrom",
                table: "SupplierDocuments",
                column: "ValidFrom");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDocuments_ValidTo",
                table: "SupplierDocuments",
                column: "ValidTo");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRatings_CreatedAt",
                table: "SupplierRatings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRatings_OverallRating",
                table: "SupplierRatings",
                column: "OverallRating");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRatings_RatingDate",
                table: "SupplierRatings",
                column: "RatingDate");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRatings_RatingPeriod",
                table: "SupplierRatings",
                column: "RatingPeriod");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRatings_SupplierId",
                table: "SupplierRatings",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRatings_SupplierId_RatingDate",
                table: "SupplierRatings",
                columns: new[] { "SupplierId", "RatingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRatings_SupplierId_RatingPeriod",
                table: "SupplierRatings",
                columns: new[] { "SupplierId", "RatingPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CategoryId",
                table: "Suppliers",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_CreatedAt",
                table: "Suppliers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Name",
                table: "Suppliers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_RegistrationNumber",
                table: "Suppliers",
                column: "RegistrationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Status",
                table: "Suppliers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_TaxId",
                table: "Suppliers",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_UpdatedAt",
                table: "Suppliers",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierAddresses");

            migrationBuilder.DropTable(
                name: "SupplierContacts");

            migrationBuilder.DropTable(
                name: "SupplierDocuments");

            migrationBuilder.DropTable(
                name: "SupplierRatings");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "SupplierCategories");
        }
    }
}
