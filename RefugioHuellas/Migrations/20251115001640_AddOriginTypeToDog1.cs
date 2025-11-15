using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefugioHuellas.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginTypeToDog1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginTypeId",
                table: "Dogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OriginTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OriginTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_OriginTypeId",
                table: "Dogs",
                column: "OriginTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dogs_OriginTypes_OriginTypeId",
                table: "Dogs",
                column: "OriginTypeId",
                principalTable: "OriginTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dogs_OriginTypes_OriginTypeId",
                table: "Dogs");

            migrationBuilder.DropTable(
                name: "OriginTypes");

            migrationBuilder.DropIndex(
                name: "IX_Dogs_OriginTypeId",
                table: "Dogs");

            migrationBuilder.DropColumn(
                name: "OriginTypeId",
                table: "Dogs");
        }
    }
}
