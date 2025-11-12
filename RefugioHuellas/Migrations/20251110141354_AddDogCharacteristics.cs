using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefugioHuellas.Migrations
{
    /// <inheritdoc />
    public partial class AddDogCharacteristics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Breed",
                table: "Dogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EnergyLevel",
                table: "Dogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdealEnvironment",
                table: "Dogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Dogs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Breed",
                table: "Dogs");

            migrationBuilder.DropColumn(
                name: "EnergyLevel",
                table: "Dogs");

            migrationBuilder.DropColumn(
                name: "IdealEnvironment",
                table: "Dogs");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Dogs");
        }
    }
}
