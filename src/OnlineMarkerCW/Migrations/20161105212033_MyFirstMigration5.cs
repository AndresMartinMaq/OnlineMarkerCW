using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OnlineMarkerCW.Migrations
{
    public partial class MyFirstMigration5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Works",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Marked",
                table: "Works",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Works");

            migrationBuilder.DropColumn(
                name: "Marked",
                table: "Works");
        }
    }
}
