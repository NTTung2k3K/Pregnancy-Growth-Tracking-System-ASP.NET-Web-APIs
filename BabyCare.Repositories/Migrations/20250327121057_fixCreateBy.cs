using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabyCare.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class fixCreateBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
      name: "CreatedBy",
      table: "UserMessage");
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserMessage",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedTime",
                table: "UserMessage",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "UserMessage",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedTime",
                table: "UserMessage",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "UserMessage",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdatedTime",
                table: "UserMessage",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AddColumn<DateTime>(
                name: "SendAt",
                table: "UserMessage",
                type: "datetime(6)",
                nullable: false,
              defaultValueSql: "CURRENT_TIMESTAMP(6)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserMessage");

            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "UserMessage");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "UserMessage");

            migrationBuilder.DropColumn(
                name: "DeletedTime",
                table: "UserMessage");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "UserMessage");

            migrationBuilder.DropColumn(
                name: "LastUpdatedTime",
                table: "UserMessage");

            migrationBuilder.DropColumn(
                name: "SendAt",
                table: "UserMessage");
        }
    }
}
