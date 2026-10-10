using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHive.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadingGoals_UserId",
                table: "ReadingGoals");

            migrationBuilder.AddColumn<int>(
                name: "BooksRead",
                table: "ReadingGoals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "ReadingGoals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ReadingGoals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReadingGoals_UserId",
                table: "ReadingGoals",
                column: "UserId",
                unique: true,
                filter: "\"Status\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReadingGoals_UserId",
                table: "ReadingGoals");

            migrationBuilder.DropColumn(
                name: "BooksRead",
                table: "ReadingGoals");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "ReadingGoals");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ReadingGoals");

            migrationBuilder.CreateIndex(
                name: "IX_ReadingGoals_UserId",
                table: "ReadingGoals",
                column: "UserId",
                unique: true);
        }
    }
}
