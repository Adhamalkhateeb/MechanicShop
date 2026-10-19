using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MechanicShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Part_RepairTask_RepairTaskId",
                table: "Part");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RepairTask",
                table: "RepairTask");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Part",
                table: "Part");

            migrationBuilder.RenameTable(
                name: "RepairTask",
                newName: "RepairTasks");

            migrationBuilder.RenameTable(
                name: "Part",
                newName: "Parts");

            migrationBuilder.RenameIndex(
                name: "IX_Part_RepairTaskId",
                table: "Parts",
                newName: "IX_Parts_RepairTaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RepairTasks",
                table: "RepairTasks",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Parts",
                table: "Parts",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_RepairTasks_RepairTaskId",
                table: "Parts",
                column: "RepairTaskId",
                principalTable: "RepairTasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_RepairTasks_RepairTaskId",
                table: "Parts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RepairTasks",
                table: "RepairTasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Parts",
                table: "Parts");

            migrationBuilder.RenameTable(
                name: "RepairTasks",
                newName: "RepairTask");

            migrationBuilder.RenameTable(
                name: "Parts",
                newName: "Part");

            migrationBuilder.RenameIndex(
                name: "IX_Parts_RepairTaskId",
                table: "Part",
                newName: "IX_Part_RepairTaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RepairTask",
                table: "RepairTask",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Part",
                table: "Part",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddForeignKey(
                name: "FK_Part_RepairTask_RepairTaskId",
                table: "Part",
                column: "RepairTaskId",
                principalTable: "RepairTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
