using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabyCare.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class jflad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddedRecordCount",
                table: "UserMemberships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GrowthChartShareCount",
                table: "UserMemberships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasGenerateAppointments",
                table: "MembershipPackages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasStandardDeviationAlerts",
                table: "MembershipPackages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasViewGrowthChart",
                table: "MembershipPackages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxGrowthChartShares",
                table: "MembershipPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxRecordAdded",
                table: "MembershipPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedRecordCount",
                table: "UserMemberships");

            migrationBuilder.DropColumn(
                name: "GrowthChartShareCount",
                table: "UserMemberships");

            migrationBuilder.DropColumn(
                name: "HasGenerateAppointments",
                table: "MembershipPackages");

            migrationBuilder.DropColumn(
                name: "HasStandardDeviationAlerts",
                table: "MembershipPackages");

            migrationBuilder.DropColumn(
                name: "HasViewGrowthChart",
                table: "MembershipPackages");

            migrationBuilder.DropColumn(
                name: "MaxGrowthChartShares",
                table: "MembershipPackages");

            migrationBuilder.DropColumn(
                name: "MaxRecordAdded",
                table: "MembershipPackages");
        }
    }
}
