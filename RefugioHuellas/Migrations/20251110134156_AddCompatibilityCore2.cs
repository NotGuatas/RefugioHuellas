using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefugioHuellas.Migrations
{
    /// <inheritdoc />
    public partial class AddCompatibilityCore2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Prompt",
                table: "PersonalityTraits",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prompt",
                table: "PersonalityTraits");
        }
    }
}
