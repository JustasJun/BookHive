using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHive.Backend.Migrations
{
    /// <inheritdoc />
    public partial class GoalPeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Period",
                table: "ReadingGoals",
                type: "integer",
                nullable: false,
                defaultValue: 4); // 4 = OneYear

            migrationBuilder.AddColumn<DateTime>(
                name: "EndsAt",
                table: "ReadingGoals",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // old yearly goals end on December 31 of their year
            migrationBuilder.Sql(@"
                UPDATE ""ReadingGoals""
                SET ""EndsAt"" = make_timestamptz(""Year"", 12, 31, 23, 59, 59, 'UTC');");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "ReadingGoals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "ReadingGoals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE ""ReadingGoals""
                SET ""Year"" = EXTRACT(YEAR FROM ""EndsAt"")::int;");

            migrationBuilder.DropColumn(
                name: "EndsAt",
                table: "ReadingGoals");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "ReadingGoals");
        }
    }
}
