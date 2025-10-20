using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codefix.AIPlayGround.Migrations
{
    /// <inheritdoc />
    public partial class FixNodeConnectionsCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeConnections_Nodes_TargetNodeId",
                table: "NodeConnections");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeConnections_Nodes_TargetNodeId",
                table: "NodeConnections",
                column: "TargetNodeId",
                principalTable: "Nodes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NodeConnections_Nodes_TargetNodeId",
                table: "NodeConnections");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeConnections_Nodes_TargetNodeId",
                table: "NodeConnections",
                column: "TargetNodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
