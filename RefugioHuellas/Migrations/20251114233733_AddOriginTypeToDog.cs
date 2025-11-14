using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefugioHuellas.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginTypeToDog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "AdoptionApplications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "AdoptionApplications");
        }
    }
}
