using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLectureCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "LectureCalendars",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LectureCalendars_UserId",
                table: "LectureCalendars",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LectureCalendars_AspNetUsers_UserId",
                table: "LectureCalendars",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LectureCalendars_AspNetUsers_UserId",
                table: "LectureCalendars");

            migrationBuilder.DropIndex(
                name: "IX_LectureCalendars_UserId",
                table: "LectureCalendars");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "LectureCalendars");
        }
    }
}
