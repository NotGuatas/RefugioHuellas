using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefugioHuellas.Migrations
{
    /// <inheritdoc />
    public partial class AddAdoptionApplicationAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdoptionApplicationAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdoptionApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    TraitId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdoptionApplicationAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdoptionApplicationAnswers_AdoptionApplications_AdoptionApplicationId",
                        column: x => x.AdoptionApplicationId,
                        principalTable: "AdoptionApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdoptionApplicationAnswers_PersonalityTraits_TraitId",
                        column: x => x.TraitId,
                        principalTable: "PersonalityTraits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionApplicationAnswers_AdoptionApplicationId",
                table: "AdoptionApplicationAnswers",
                column: "AdoptionApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_AdoptionApplicationAnswers_TraitId",
                table: "AdoptionApplicationAnswers",
                column: "TraitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdoptionApplicationAnswers");
        }
    }
}
