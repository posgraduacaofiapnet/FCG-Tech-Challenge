using System;
using FCG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260504120000_AddEntityBaseIdToLibraryItems")]
    public partial class AddEntityBaseIdToLibraryItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGames",
                table: "UserGames");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "UserGames",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGames",
                table: "UserGames",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_UserId_GameId",
                table: "UserGames",
                columns: new[] { "UserId", "GameId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGames",
                table: "UserGames");

            migrationBuilder.DropIndex(
                name: "IX_UserGames_UserId_GameId",
                table: "UserGames");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserGames");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGames",
                table: "UserGames",
                columns: new[] { "UserId", "GameId" });
        }
    }
}
