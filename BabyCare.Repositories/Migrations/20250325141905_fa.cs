using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabyCare.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class fa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Complications",
                table: "Childs");

            migrationBuilder.DropColumn(
                name: "DeliveryPlan",
                table: "Childs");

            migrationBuilder.DropColumn(
                name: "PregnancyStage",
                table: "Childs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Complications",
                table: "Childs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryPlan",
                table: "Childs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PregnancyStage",
                table: "Childs",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
