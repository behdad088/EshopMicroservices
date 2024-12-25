using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Command.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addoutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeleteDate",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Outboxes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AggregateId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DispatchDateTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionId = table.Column<int>(type: "int", nullable: false),
                    IsDispatched = table.Column<bool>(type: "bit", nullable: false),
                    NumberOfDispatchTry = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outboxes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Outboxes_AggregateId_VersionId",
                table: "Outboxes",
                columns: new[] { "AggregateId", "VersionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Outboxes");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "Orders");
        }
    }
}
