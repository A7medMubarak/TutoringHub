using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutoringHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotaRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuotaRowId",
                table: "Attendances",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QuotaRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ClassGroupId = table.Column<int>(type: "int", nullable: false),
                    TotalSessions = table.Column<int>(type: "int", nullable: false),
                    RemainingSessions = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotaRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotaRows_ClassGroups_ClassGroupId",
                        column: x => x.ClassGroupId,
                        principalTable: "ClassGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotaRows_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_QuotaRowId",
                table: "Attendances",
                column: "QuotaRowId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotaRows_ClassGroupId",
                table: "QuotaRows",
                column: "ClassGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotaRows_StudentId",
                table: "QuotaRows",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_QuotaRows_QuotaRowId",
                table: "Attendances",
                column: "QuotaRowId",
                principalTable: "QuotaRows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_QuotaRows_QuotaRowId",
                table: "Attendances");

            migrationBuilder.DropTable(
                name: "QuotaRows");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_QuotaRowId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "QuotaRowId",
                table: "Attendances");
        }
    }
}
