using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.Command.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class addcustomer_id_to_outbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Outboxes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Outboxes");
        }
    }
}
