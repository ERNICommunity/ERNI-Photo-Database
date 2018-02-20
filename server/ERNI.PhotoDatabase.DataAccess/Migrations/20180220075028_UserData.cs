using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace ERNI.PhotoDatabase.DataAccess.Migrations
{
    public partial class UserData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CanUpload = table.Column<bool>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    IsAdmin = table.Column<bool>(nullable: false),
                    LastName = table.Column<string>(nullable: true),
                    UniqueIdentifier = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UniqueIdentifier",
                table: "Users",
                column: "UniqueIdentifier",
                unique: true,
                filter: "[UniqueIdentifier] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
