using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Weather_App.Server.Migrations
{
    public partial class SwitchToUnixSeconds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Timestamp",
                table: "WeatherLogs");

            migrationBuilder.AddColumn<long>(
                name: "UnixTimeSeconds",
                table: "WeatherLogs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnixTimeSeconds",
                table: "WeatherLogs");

            migrationBuilder.AddColumn<DateTime>(
                name: "Timestamp",
                table: "WeatherLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
