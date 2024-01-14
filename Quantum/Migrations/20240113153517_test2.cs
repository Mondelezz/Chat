using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quantum.Migrations
{
    /// <inheritdoc />
    public partial class test2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupRequestUserInfoOutput_GroupRequests_GroupRequestsGroup~",
                table: "GroupRequestUserInfoOutput");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupRequestUserInfoOutput_OpenUsers_UsersUserId",
                table: "GroupRequestUserInfoOutput");

            migrationBuilder.DropColumn(
                name: "GroupRequestId",
                table: "OpenUsers");

            migrationBuilder.RenameColumn(
                name: "UsersUserId",
                table: "GroupRequestUserInfoOutput",
                newName: "UserInfoOutputId");

            migrationBuilder.RenameColumn(
                name: "GroupRequestsGroupRequestId",
                table: "GroupRequestUserInfoOutput",
                newName: "GroupRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupRequestUserInfoOutput_UsersUserId",
                table: "GroupRequestUserInfoOutput",
                newName: "IX_GroupRequestUserInfoOutput_UserInfoOutputId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRequestUserInfoOutput_GroupRequests_GroupRequestId",
                table: "GroupRequestUserInfoOutput",
                column: "GroupRequestId",
                principalTable: "GroupRequests",
                principalColumn: "GroupRequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRequestUserInfoOutput_OpenUsers_UserInfoOutputId",
                table: "GroupRequestUserInfoOutput",
                column: "UserInfoOutputId",
                principalTable: "OpenUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupRequestUserInfoOutput_GroupRequests_GroupRequestId",
                table: "GroupRequestUserInfoOutput");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupRequestUserInfoOutput_OpenUsers_UserInfoOutputId",
                table: "GroupRequestUserInfoOutput");

            migrationBuilder.RenameColumn(
                name: "UserInfoOutputId",
                table: "GroupRequestUserInfoOutput",
                newName: "UsersUserId");

            migrationBuilder.RenameColumn(
                name: "GroupRequestId",
                table: "GroupRequestUserInfoOutput",
                newName: "GroupRequestsGroupRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupRequestUserInfoOutput_UserInfoOutputId",
                table: "GroupRequestUserInfoOutput",
                newName: "IX_GroupRequestUserInfoOutput_UsersUserId");

            migrationBuilder.AddColumn<Guid>(
                name: "GroupRequestId",
                table: "OpenUsers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRequestUserInfoOutput_GroupRequests_GroupRequestsGroup~",
                table: "GroupRequestUserInfoOutput",
                column: "GroupRequestsGroupRequestId",
                principalTable: "GroupRequests",
                principalColumn: "GroupRequestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupRequestUserInfoOutput_OpenUsers_UsersUserId",
                table: "GroupRequestUserInfoOutput",
                column: "UsersUserId",
                principalTable: "OpenUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
