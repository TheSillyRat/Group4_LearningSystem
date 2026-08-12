using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleIdToQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                table: "Quizzes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_ModuleId",
                table: "Quizzes",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_Module_ModuleId",
                table: "Quizzes",
                column: "ModuleId",
                principalTable: "Module",
                principalColumn: "ModuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_Module_ModuleId",
                table: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_ModuleId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "Quizzes");
        }
    }
}
