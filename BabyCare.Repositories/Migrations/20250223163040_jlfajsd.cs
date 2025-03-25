using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabyCare.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class jlfajsd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MembershipPackages");

            migrationBuilder.DropColumn(
                name: "HeightEstimate",
                table: "Childs");

            migrationBuilder.DropColumn(
                name: "PregnancyWeekAtBirth",
                table: "Childs");

            migrationBuilder.DropColumn(
                name: "WeightEstimate",
                table: "Childs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MembershipPackages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "HeightEstimate",
                table: "Childs",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PregnancyWeekAtBirth",
                table: "Childs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "WeightEstimate",
                table: "Childs",
                type: "real",
                nullable: true);
        }
    }
}
