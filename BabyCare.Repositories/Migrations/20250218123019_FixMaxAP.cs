using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BabyCare.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class FixMaxAP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FetalGrowthRecords_GrowthCharts_GrowthChartId",
                table: "FetalGrowthRecords");

            migrationBuilder.DropIndex(
                name: "IX_FetalGrowthRecords_GrowthChartId",
                table: "FetalGrowthRecords");

            migrationBuilder.DropColumn(
                name: "GrowthChartId",
                table: "FetalGrowthRecords");

            migrationBuilder.AddColumn<int>(
                name: "AppointmentBookingCount",
                table: "UserMemberships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxAppointmentCanBooking",
                table: "MembershipPackages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppointmentBookingCount",
                table: "UserMemberships");

            migrationBuilder.DropColumn(
                name: "MaxAppointmentCanBooking",
                table: "MembershipPackages");

            migrationBuilder.AddColumn<int>(
                name: "GrowthChartId",
                table: "FetalGrowthRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FetalGrowthRecords_GrowthChartId",
                table: "FetalGrowthRecords",
                column: "GrowthChartId");

            migrationBuilder.AddForeignKey(
                name: "FK_FetalGrowthRecords_GrowthCharts_GrowthChartId",
                table: "FetalGrowthRecords",
                column: "GrowthChartId",
                principalTable: "GrowthCharts",
                principalColumn: "Id");
        }
    }
}
