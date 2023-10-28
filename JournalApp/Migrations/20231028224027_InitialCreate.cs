using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JournalApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Group = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    ReadOnly = table.Column<bool>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicationDose = table.Column<decimal>(type: "TEXT", nullable: true),
                    MedicationUnit = table.Column<string>(type: "TEXT", nullable: true),
                    MedicationEveryDaySince = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "Days",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Days", x => x.Guid);
                });

            migrationBuilder.CreateTable(
                name: "Points",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "TEXT", nullable: false),
                    DayGuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    CategoryGuid = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Mood = table.Column<string>(type: "TEXT", nullable: true),
                    SleepHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    ScaleIndex = table.Column<int>(type: "INTEGER", nullable: true),
                    Bool = table.Column<bool>(type: "INTEGER", nullable: true),
                    Number = table.Column<double>(type: "REAL", nullable: true),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    MedicationDose = table.Column<decimal>(type: "TEXT", nullable: true),
                    MedicationUnit = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Points", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Points_Categories_CategoryGuid",
                        column: x => x.CategoryGuid,
                        principalTable: "Categories",
                        principalColumn: "Guid");
                    table.ForeignKey(
                        name: "FK_Points_Days_DayGuid",
                        column: x => x.DayGuid,
                        principalTable: "Days",
                        principalColumn: "Guid");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Points_CategoryGuid",
                table: "Points",
                column: "CategoryGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Points_DayGuid",
                table: "Points",
                column: "DayGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Points");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Days");
        }
    }
}
