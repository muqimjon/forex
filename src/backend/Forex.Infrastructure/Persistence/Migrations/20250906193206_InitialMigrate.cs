#nullable disable

namespace Forex.Infrastructure.Persistence.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

/// <inheritdoc />
public partial class InitialMigrate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Currencies",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false),
                NormalizedName = table.Column<string>(type: "text", nullable: false),
                Symbol = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Currencies", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Invoices",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CostPrice = table.Column<decimal>(type: "numeric", nullable: false),
                CostDelivery = table.Column<decimal>(type: "numeric", nullable: false),
                TransferFee = table.Column<decimal>(type: "numeric", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Invoices", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Manufactories",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false),
                NormalizedName = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Manufactories", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false),
                NormalizedName = table.Column<string>(type: "text", nullable: false),
                Code = table.Column<int>(type: "integer", nullable: false),
                Measure = table.Column<string>(type: "text", nullable: false),
                PhotoPath = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SemiProducts",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: true),
                NormalizedName = table.Column<string>(type: "text", nullable: false),
                Code = table.Column<int>(type: "integer", nullable: false),
                Measure = table.Column<string>(type: "text", nullable: false),
                PhotoPath = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SemiProducts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Shops",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false),
                NormalizedName = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Shops", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false),
                NormalizedName = table.Column<string>(type: "text", nullable: false),
                Phone = table.Column<string>(type: "text", nullable: false),
                Email = table.Column<string>(type: "text", nullable: true),
                NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                Role = table.Column<int>(type: "integer", nullable: false),
                Address = table.Column<string>(type: "text", nullable: true),
                Description = table.Column<string>(type: "text", nullable: true),
                PasswordHash = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ProductItems",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProductId = table.Column<long>(type: "bigint", nullable: false),
                SemiProductId = table.Column<long>(type: "bigint", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductItems_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductItems_SemiProducts_SemiProductId",
                    column: x => x.SemiProductId,
                    principalTable: "SemiProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SemiProductEntries",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SemiProductId = table.Column<long>(type: "bigint", nullable: false),
                InvoceId = table.Column<long>(type: "bigint", nullable: false),
                ManufactoryId = table.Column<long>(type: "bigint", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                CostPrice = table.Column<decimal>(type: "numeric", nullable: false),
                CostDelivery = table.Column<decimal>(type: "numeric", nullable: false),
                TransferFee = table.Column<decimal>(type: "numeric", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SemiProductEntries", x => x.Id);
                table.ForeignKey(
                    name: "FK_SemiProductEntries_Invoices_InvoceId",
                    column: x => x.InvoceId,
                    principalTable: "Invoices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SemiProductEntries_Manufactories_ManufactoryId",
                    column: x => x.ManufactoryId,
                    principalTable: "Manufactories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SemiProductEntries_SemiProducts_SemiProductId",
                    column: x => x.SemiProductId,
                    principalTable: "SemiProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SemiProductResidues",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SemiProductId = table.Column<long>(type: "bigint", nullable: false),
                ManufactoryId = table.Column<long>(type: "bigint", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SemiProductResidues", x => x.Id);
                table.ForeignKey(
                    name: "FK_SemiProductResidues_Manufactories_ManufactoryId",
                    column: x => x.ManufactoryId,
                    principalTable: "Manufactories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SemiProductResidues_SemiProducts_SemiProductId",
                    column: x => x.SemiProductId,
                    principalTable: "SemiProducts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ResidueShops",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProductId = table.Column<long>(type: "bigint", nullable: false),
                ShopId = table.Column<long>(type: "bigint", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ResidueShops", x => x.Id);
                table.ForeignKey(
                    name: "FK_ResidueShops_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ResidueShops_Shops_ShopId",
                    column: x => x.ShopId,
                    principalTable: "Shops",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ShopCashes",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ShopId = table.Column<long>(type: "bigint", nullable: false),
                CurrencyId = table.Column<long>(type: "bigint", nullable: false),
                Balance = table.Column<decimal>(type: "numeric", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShopCashes", x => x.Id);
                table.ForeignKey(
                    name: "FK_ShopCashes_Currencies_CurrencyId",
                    column: x => x.CurrencyId,
                    principalTable: "Currencies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ShopCashes_Shops_ShopId",
                    column: x => x.ShopId,
                    principalTable: "Shops",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Accounts",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                BeginSumm = table.Column<decimal>(type: "numeric", nullable: false),
                Discount = table.Column<decimal>(type: "numeric", nullable: false),
                Balance = table.Column<decimal>(type: "numeric", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Accounts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Accounts_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ContainerEntries",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SenderId = table.Column<long>(type: "bigint", nullable: false),
                InvoceId = table.Column<long>(type: "bigint", nullable: false),
                Count = table.Column<long>(type: "bigint", nullable: false),
                Price = table.Column<decimal>(type: "numeric", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContainerEntries", x => x.Id);
                table.ForeignKey(
                    name: "FK_ContainerEntries_Invoices_InvoceId",
                    column: x => x.InvoceId,
                    principalTable: "Invoices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ContainerEntries_Users_SenderId",
                    column: x => x.SenderId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ProductEntries",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ProductId = table.Column<long>(type: "bigint", nullable: false),
                ShopId = table.Column<long>(type: "bigint", nullable: false),
                EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                CostPrice = table.Column<decimal>(type: "numeric", nullable: false),
                CostPreparation = table.Column<decimal>(type: "numeric", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductEntries", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductEntries_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductEntries_Shops_ShopId",
                    column: x => x.ShopId,
                    principalTable: "Shops",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProductEntries_Users_EmployeeId",
                    column: x => x.EmployeeId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Sales",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                CostPrice = table.Column<decimal>(type: "numeric", nullable: false),
                BenifitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                TotalCount = table.Column<int>(type: "integer", nullable: false),
                TotalSum = table.Column<decimal>(type: "numeric", nullable: false),
                DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Note = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Sales", x => x.Id);
                table.ForeignKey(
                    name: "FK_Sales_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ShopId = table.Column<long>(type: "bigint", nullable: false),
                UserId = table.Column<long>(type: "bigint", nullable: false),
                Amount = table.Column<decimal>(type: "numeric", nullable: false),
                CurrencyId = table.Column<long>(type: "bigint", nullable: false),
                PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                IsIncome = table.Column<bool>(type: "boolean", nullable: false),
                Description = table.Column<string>(type: "text", nullable: true),
                Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Transactions", x => x.Id);
                table.ForeignKey(
                    name: "FK_Transactions_Currencies_CurrencyId",
                    column: x => x.CurrencyId,
                    principalTable: "Currencies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Transactions_Shops_ShopId",
                    column: x => x.ShopId,
                    principalTable: "Shops",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Transactions_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SaleItems",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SaleId = table.Column<long>(type: "bigint", nullable: false),
                ProductId = table.Column<long>(type: "bigint", nullable: false),
                Count = table.Column<int>(type: "integer", nullable: false),
                CostPrice = table.Column<decimal>(type: "numeric", nullable: false),
                Benifit = table.Column<decimal>(type: "numeric", nullable: false),
                TotalSum = table.Column<decimal>(type: "numeric", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SaleItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_SaleItems_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SaleItems_Sales_SaleId",
                    column: x => x.SaleId,
                    principalTable: "Sales",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Accounts_UserId",
            table: "Accounts",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ContainerEntries_InvoceId",
            table: "ContainerEntries",
            column: "InvoceId");

        migrationBuilder.CreateIndex(
            name: "IX_ContainerEntries_SenderId",
            table: "ContainerEntries",
            column: "SenderId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductEntries_EmployeeId",
            table: "ProductEntries",
            column: "EmployeeId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductEntries_ProductId",
            table: "ProductEntries",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductEntries_ShopId",
            table: "ProductEntries",
            column: "ShopId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductItems_ProductId",
            table: "ProductItems",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ProductItems_SemiProductId",
            table: "ProductItems",
            column: "SemiProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ResidueShops_ProductId",
            table: "ResidueShops",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ResidueShops_ShopId",
            table: "ResidueShops",
            column: "ShopId");

        migrationBuilder.CreateIndex(
            name: "IX_SaleItems_ProductId",
            table: "SaleItems",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_SaleItems_SaleId",
            table: "SaleItems",
            column: "SaleId");

        migrationBuilder.CreateIndex(
            name: "IX_Sales_UserId",
            table: "Sales",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_SemiProductEntries_InvoceId",
            table: "SemiProductEntries",
            column: "InvoceId");

        migrationBuilder.CreateIndex(
            name: "IX_SemiProductEntries_ManufactoryId",
            table: "SemiProductEntries",
            column: "ManufactoryId");

        migrationBuilder.CreateIndex(
            name: "IX_SemiProductEntries_SemiProductId",
            table: "SemiProductEntries",
            column: "SemiProductId");

        migrationBuilder.CreateIndex(
            name: "IX_SemiProductResidues_ManufactoryId",
            table: "SemiProductResidues",
            column: "ManufactoryId");

        migrationBuilder.CreateIndex(
            name: "IX_SemiProductResidues_SemiProductId",
            table: "SemiProductResidues",
            column: "SemiProductId");

        migrationBuilder.CreateIndex(
            name: "IX_ShopCashes_CurrencyId",
            table: "ShopCashes",
            column: "CurrencyId");

        migrationBuilder.CreateIndex(
            name: "IX_ShopCashes_ShopId",
            table: "ShopCashes",
            column: "ShopId");

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_CurrencyId",
            table: "Transactions",
            column: "CurrencyId");

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_ShopId",
            table: "Transactions",
            column: "ShopId");

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_UserId",
            table: "Transactions",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Accounts");

        migrationBuilder.DropTable(
            name: "ContainerEntries");

        migrationBuilder.DropTable(
            name: "ProductEntries");

        migrationBuilder.DropTable(
            name: "ProductItems");

        migrationBuilder.DropTable(
            name: "ResidueShops");

        migrationBuilder.DropTable(
            name: "SaleItems");

        migrationBuilder.DropTable(
            name: "SemiProductEntries");

        migrationBuilder.DropTable(
            name: "SemiProductResidues");

        migrationBuilder.DropTable(
            name: "ShopCashes");

        migrationBuilder.DropTable(
            name: "Transactions");

        migrationBuilder.DropTable(
            name: "Products");

        migrationBuilder.DropTable(
            name: "Sales");

        migrationBuilder.DropTable(
            name: "Invoices");

        migrationBuilder.DropTable(
            name: "Manufactories");

        migrationBuilder.DropTable(
            name: "SemiProducts");

        migrationBuilder.DropTable(
            name: "Currencies");

        migrationBuilder.DropTable(
            name: "Shops");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
