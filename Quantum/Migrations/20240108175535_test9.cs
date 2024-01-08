using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quantum.Migrations
{
    /// <inheritdoc />
    public partial class test9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFriends_Friends_FriendId",
                table: "UserFriends");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Friends",
                table: "Friends");

            migrationBuilder.RenameTable(
                name: "Friends",
                newName: "OpenUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "RequestsId",
                table: "Groups",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_OpenUsers",
                table: "OpenUsers",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "GroupRequest",
                columns: table => new
                {
                    RequestsId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountRequsts = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRequest", x => x.RequestsId);
                });

            migrationBuilder.CreateTable(
                name: "GroupRequestUserInfoOutput",
                columns: table => new
                {
                    GroupRequestRequestsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsersUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRequestUserInfoOutput", x => new { x.GroupRequestRequestsId, x.UsersUserId });
                    table.ForeignKey(
                        name: "FK_GroupRequestUserInfoOutput_GroupRequest_GroupRequestRequest~",
                        column: x => x.GroupRequestRequestsId,
                        principalTable: "GroupRequest",
                        principalColumn: "RequestsId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupRequestUserInfoOutput_OpenUsers_UsersUserId",
                        column: x => x.UsersUserId,
                        principalTable: "OpenUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_RequestsId",
                table: "Groups",
                column: "RequestsId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRequestUserInfoOutput_UsersUserId",
                table: "GroupRequestUserInfoOutput",
                column: "UsersUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_GroupRequest_RequestsId",
                table: "Groups",
                column: "RequestsId",
                principalTable: "GroupRequest",
                principalColumn: "RequestsId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriends_OpenUsers_FriendId",
                table: "UserFriends",
                column: "FriendId",
                principalTable: "OpenUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_GroupRequest_RequestsId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFriends_OpenUsers_FriendId",
                table: "UserFriends");

            migrationBuilder.DropTable(
                name: "GroupRequestUserInfoOutput");

            migrationBuilder.DropTable(
                name: "GroupRequest");

            migrationBuilder.DropIndex(
                name: "IX_Groups_RequestsId",
                table: "Groups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OpenUsers",
                table: "OpenUsers");

            migrationBuilder.DropColumn(
                name: "RequestsId",
                table: "Groups");

            migrationBuilder.RenameTable(
                name: "OpenUsers",
                newName: "Friends");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Friends",
                table: "Friends",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFriends_Friends_FriendId",
                table: "UserFriends",
                column: "FriendId",
                principalTable: "Friends",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
