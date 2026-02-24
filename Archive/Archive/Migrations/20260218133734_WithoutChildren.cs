using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Archive.Migrations
{
    /// <inheritdoc />
    public partial class WithoutChildren : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Детей",
                table: "аккаунт_пользователя");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Детей",
                table: "аккаунт_пользователя",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
