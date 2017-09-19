using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MPServer.Migrations
{
    public partial class UpdateOpenIddict2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ciphertext",
                table: "OpenIddictTokens",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreationDate",
                table: "OpenIddictTokens",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpirationDate",
                table: "OpenIddictTokens",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "OpenIddictTokens",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OpenIddictTokens",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OpenIddictAuthorizations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_Hash",
                table: "OpenIddictTokens",
                column: "Hash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropIndex(
                name: "IX_OpenIddictTokens_Hash",
                table: "OpenIddictTokens");

            migrationBuilder.DropColumn(
                name: "Ciphertext",
                table: "OpenIddictTokens");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "OpenIddictTokens");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "OpenIddictTokens");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "OpenIddictTokens");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OpenIddictTokens");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OpenIddictAuthorizations");
        }
    }
}
