using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codefix.AIPlayGround.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentFrameworkEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AgentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LLMConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToolsConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PromptTemplateJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MemoryConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckpointConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Flows",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FlowType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentFlowId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Flows_Flows_ParentFlowId",
                        column: x => x.ParentFlowId,
                        principalTable: "Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NodeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InputSchemaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutputSchemaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NodeTemplates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NodeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DefaultConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultInputSchemaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultOutputSchemaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSystemTemplate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgentExecutions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AgentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InputDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutputDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetricsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgentExecutions_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlowAgents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FlowId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AgentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowAgents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowAgents_Agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "Agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowAgents_Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlowExecutions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FlowId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InputDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutputDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetricsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StepsJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowExecutions_Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlowNodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FlowId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NodeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    X = table.Column<double>(type: "float", nullable: false),
                    Y = table.Column<double>(type: "float", nullable: false),
                    Width = table.Column<double>(type: "float", nullable: false),
                    Height = table.Column<double>(type: "float", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    FlowSpecificConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowNodes_Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowNodes_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodeConnections",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceNodeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TargetNodeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourcePort = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TargetPort = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ConnectionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NodeConnections_Nodes_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NodeConnections_Nodes_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutions_AgentId",
                table: "AgentExecutions",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutions_StartedAt",
                table: "AgentExecutions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutions_Status",
                table: "AgentExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_AgentType",
                table: "Agents",
                column: "AgentType");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_CreatedAt",
                table: "Agents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Name",
                table: "Agents",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_Status",
                table: "Agents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FlowAgents_AgentId",
                table: "FlowAgents",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowAgents_FlowId",
                table: "FlowAgents",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowAgents_FlowId_AgentId",
                table: "FlowAgents",
                columns: new[] { "FlowId", "AgentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlowExecutions_FlowId",
                table: "FlowExecutions",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowExecutions_StartedAt",
                table: "FlowExecutions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FlowExecutions_Status",
                table: "FlowExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FlowNodes_FlowId",
                table: "FlowNodes",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowNodes_FlowId_NodeId",
                table: "FlowNodes",
                columns: new[] { "FlowId", "NodeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlowNodes_NodeId",
                table: "FlowNodes",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_CreatedAt",
                table: "Flows",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_FlowType",
                table: "Flows",
                column: "FlowType");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_Name",
                table: "Flows",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_ParentFlowId",
                table: "Flows",
                column: "ParentFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_Status",
                table: "Flows",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_NodeConnections_ConnectionType",
                table: "NodeConnections",
                column: "ConnectionType");

            migrationBuilder.CreateIndex(
                name: "IX_NodeConnections_SourceNodeId",
                table: "NodeConnections",
                column: "SourceNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeConnections_TargetNodeId",
                table: "NodeConnections",
                column: "TargetNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_CreatedAt",
                table: "Nodes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_Name",
                table: "Nodes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_NodeType",
                table: "Nodes",
                column: "NodeType");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_Status",
                table: "Nodes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTemplates_CreatedAt",
                table: "NodeTemplates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTemplates_IsSystemTemplate",
                table: "NodeTemplates",
                column: "IsSystemTemplate");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTemplates_Name",
                table: "NodeTemplates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_NodeTemplates_NodeType",
                table: "NodeTemplates",
                column: "NodeType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentExecutions");

            migrationBuilder.DropTable(
                name: "FlowAgents");

            migrationBuilder.DropTable(
                name: "FlowExecutions");

            migrationBuilder.DropTable(
                name: "FlowNodes");

            migrationBuilder.DropTable(
                name: "NodeConnections");

            migrationBuilder.DropTable(
                name: "NodeTemplates");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Flows");

            migrationBuilder.DropTable(
                name: "Nodes");
        }
    }
}
