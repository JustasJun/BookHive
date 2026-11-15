using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHive.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FollowedAt",
                table: "Follows",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowedAt",
                table: "Follows");
        }
    }
}
