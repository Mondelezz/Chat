using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quantum.Migrations
{
    /// <inheritdoc />
    public partial class test8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserFriends");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserFriends",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
