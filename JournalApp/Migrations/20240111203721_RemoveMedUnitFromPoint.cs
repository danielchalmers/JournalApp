using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JournalApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMedUnitFromPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicationUnit",
                table: "Points");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MedicationUnit",
                table: "Points",
                type: "TEXT",
                nullable: true);
        }
    }
}
