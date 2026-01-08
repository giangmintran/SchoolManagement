using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClassLogbook1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassLogbooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Class = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    WeekNumber = table.Column<int>(type: "int", nullable: false),
                    SchoolYear = table.Column<string>(type: "varchar(10)", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HomeroomTeacherComment = table.Column<string>(type: "nvarchar(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassLogbooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassLogbookDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassLogbookId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    PeriodIndex = table.Column<int>(type: "int", nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    CurriculumCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    AbsentStudents = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    LessonContent = table.Column<string>(type: "nvarchar(1024)", nullable: false),
                    TeacherComment = table.Column<string>(type: "nvarchar(1024)", nullable: false),
                    ScoreLearning = table.Column<int>(type: "int", nullable: false),
                    ScoreDiscipline = table.Column<int>(type: "int", nullable: false),
                    ScoreSanitation = table.Column<int>(type: "int", nullable: false),
                    ScoreDiligent = table.Column<int>(type: "int", nullable: false),
                    ConfirmedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassLogbookDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassLogbookDetails_ClassLogbooks_ClassLogbookId",
                        column: x => x.ClassLogbookId,
                        principalTable: "ClassLogbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassLogbookDetails_ClassLogbookId_DayOfWeek_PeriodIndex",
                table: "ClassLogbookDetails",
                columns: new[] { "ClassLogbookId", "DayOfWeek", "PeriodIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassLogbooks_Class_WeekNumber_SchoolYear",
                table: "ClassLogbooks",
                columns: new[] { "Class", "WeekNumber", "SchoolYear" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassLogbookDetails");

            migrationBuilder.DropTable(
                name: "ClassLogbooks");
        }
    }
}
