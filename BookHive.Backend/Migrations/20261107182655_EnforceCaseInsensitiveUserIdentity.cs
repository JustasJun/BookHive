using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHive.Backend.Migrations
{
    /// <inheritdoc />
    public partial class EnforceCaseInsensitiveUserIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM "Users"
                        GROUP BY lower("Username")
                        HAVING count(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Cannot enforce case-insensitive usernames: duplicate usernames differ only by case.';
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM "Users"
                        GROUP BY lower("Email")
                        HAVING count(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Cannot enforce case-insensitive emails: duplicate emails differ only by case.';
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Users"
                SET "Username" = lower("Username"),
                    "Email" = lower("Email");
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Email_Lowercase",
                table: "Users",
                sql: "\"Email\" = lower(\"Email\")");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_Username_Lowercase",
                table: "Users",
                sql: "\"Username\" = lower(\"Username\")");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Email_Lowercase",
                table: "Users");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_Username_Lowercase",
                table: "Users");
        }
    }
}
