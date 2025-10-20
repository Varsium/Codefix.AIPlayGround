using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codefix.AIPlayGround.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowNodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    X = table.Column<double>(type: "float", nullable: false),
                    Y = table.Column<double>(type: "float", nullable: false),
                    Width = table.Column<double>(type: "float", nullable: false),
                    Height = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    PropertiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InputPortsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutputPortsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkflowId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowNodes_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowConnections",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FromNodeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ToNodeId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FromPort = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ToPort = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    From = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    To = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PropertiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkflowId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowConnections_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowMetadata",
                columns: table => new
                {
                    WorkflowId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    License = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TagsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomPropertiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowMetadata", x => x.WorkflowId);
                    table.ForeignKey(
                        name: "FK_WorkflowMetadata_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowSettings",
                columns: table => new
                {
                    WorkflowId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EnableCheckpoints = table.Column<bool>(type: "bit", nullable: false),
                    EnableLogging = table.Column<bool>(type: "bit", nullable: false),
                    EnableMetrics = table.Column<bool>(type: "bit", nullable: false),
                    MaxExecutionTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxRetryAttempts = table.Column<int>(type: "int", nullable: false),
                    ExecutionMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EnvironmentVariablesJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowSettings", x => x.WorkflowId);
                    table.ForeignKey(
                        name: "FK_WorkflowSettings_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_CreatedAt",
                table: "Workflows",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_Name",
                table: "Workflows",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_Status",
                table: "Workflows",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowConnections_FromNodeId",
                table: "WorkflowConnections",
                column: "FromNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowConnections_ToNodeId",
                table: "WorkflowConnections",
                column: "ToNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowConnections_WorkflowId",
                table: "WorkflowConnections",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodes_Type",
                table: "WorkflowNodes",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowNodes_WorkflowId",
                table: "WorkflowNodes",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowConnections");

            migrationBuilder.DropTable(
                name: "WorkflowMetadata");

            migrationBuilder.DropTable(
                name: "WorkflowNodes");

            migrationBuilder.DropTable(
                name: "WorkflowSettings");

            migrationBuilder.DropTable(
                name: "Workflows");
        }
    }
}
