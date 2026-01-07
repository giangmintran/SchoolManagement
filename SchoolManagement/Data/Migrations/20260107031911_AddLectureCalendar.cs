using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLectureCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LectureCalendars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Week = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LectureCalendars", x => x.Id);
                    table.CheckConstraint("CK_LectureCalendar_Week", "[Week] >= 1 AND [Week] <= 52");
                });

            migrationBuilder.CreateTable(
                name: "LectureCalendarDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LectureCalendarId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false),
                    Class = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Lesson = table.Column<int>(type: "int", nullable: false),
                    LessonTitle = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LectureCalendarDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LectureCalendarDetails_LectureCalendars_LectureCalendarId",
                        column: x => x.LectureCalendarId,
                        principalTable: "LectureCalendars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LectureCalendarDetails_LectureCalendarId",
                table: "LectureCalendarDetails",
                column: "LectureCalendarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LectureCalendarDetails");

            migrationBuilder.DropTable(
                name: "LectureCalendars");
        }
    }
}
