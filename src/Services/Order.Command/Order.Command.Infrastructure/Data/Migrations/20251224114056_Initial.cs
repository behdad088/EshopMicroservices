using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Command.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DeleteDate = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<int>(type: "integer", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    BillingAddress_AddressLine = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    BillingAddress_Country = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_EmailAddress = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    BillingAddress_FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BillingAddress_LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BillingAddress_State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BillingAddress_ZipCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Payment_CVV = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Payment_CardName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Payment_CardNumber = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Payment_Expiration = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Payment_PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    ShippingAddress_AddressLine = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ShippingAddress_Country = table.Column<string>(type: "text", nullable: false),
                    ShippingAddress_EmailAddress = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    ShippingAddress_FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ShippingAddress_LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ShippingAddress_State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ShippingAddress_ZipCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: false),
                    ProductId = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderId1 = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId1",
                        column: x => x.OrderId1,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Outboxes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AggregateId = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateType = table.Column<string>(type: "text", nullable: false),
                    DispatchDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VersionId = table.Column<int>(type: "integer", nullable: false),
                    IsDispatched = table.Column<bool>(type: "boolean", nullable: false),
                    NumberOfDispatchTry = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outboxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Outboxes_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId1",
                table: "OrderItems",
                column: "OrderId1");

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_AggregateId_VersionId",
                table: "Outboxes",
                columns: new[] { "AggregateId", "VersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_OrderId",
                table: "Outboxes",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Outboxes");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
