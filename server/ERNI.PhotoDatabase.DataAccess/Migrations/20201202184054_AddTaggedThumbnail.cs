using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERNI.PhotoDatabase.DataAccess.Migrations
{
    public partial class AddTaggedThumbnail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TaggedThumbnailImageId",
                table: "Photos",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaggedThumbnailImageId",
                table: "Photos");
        }
    }
}
