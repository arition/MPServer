using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MPServer.Migrations
{
    public partial class UpdateOpenIddict : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictAuthorizations_AspNetUsers_UserId",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictTokens_AspNetUsers_UserId",
                table: "OpenIddictTokens");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "IX_OpenIddictTokens_UserId",
                table: "OpenIddictTokens");

            migrationBuilder.DropIndex(
                name: "IX_OpenIddictAuthorizations_UserId",
                table: "OpenIddictAuthorizations");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "OpenIddictTokens",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "OpenIddictAuthorizations",
                newName: "Subject");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationId",
                table: "OpenIddictAuthorizations",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictAuthorizations_ApplicationId",
                table: "OpenIddictAuthorizations",
                column: "ApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictAuthorizations_OpenIddictApplications_ApplicationId",
                table: "OpenIddictAuthorizations",
                column: "ApplicationId",
                principalTable: "OpenIddictApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictAuthorizations_OpenIddictApplications_ApplicationId",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropIndex(
                name: "IX_OpenIddictAuthorizations_ApplicationId",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "OpenIddictAuthorizations");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "OpenIddictTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "OpenIddictAuthorizations",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_UserId",
                table: "OpenIddictTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictAuthorizations_UserId",
                table: "OpenIddictAuthorizations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictAuthorizations_AspNetUsers_UserId",
                table: "OpenIddictAuthorizations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictTokens_AspNetUsers_UserId",
                table: "OpenIddictTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
