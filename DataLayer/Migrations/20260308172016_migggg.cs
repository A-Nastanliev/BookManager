using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class migggg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookRequests_Users_SenderId",
                table: "BookRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_UsersBook_Users_UserId",
                table: "UsersBook");

            migrationBuilder.AddForeignKey(
                name: "FK_BookRequests_Users_SenderId",
                table: "BookRequests",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UsersBook_Users_UserId",
                table: "UsersBook",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookRequests_Users_SenderId",
                table: "BookRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_UsersBook_Users_UserId",
                table: "UsersBook");

            migrationBuilder.AddForeignKey(
                name: "FK_BookRequests_Users_SenderId",
                table: "BookRequests",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersBook_Users_UserId",
                table: "UsersBook",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
