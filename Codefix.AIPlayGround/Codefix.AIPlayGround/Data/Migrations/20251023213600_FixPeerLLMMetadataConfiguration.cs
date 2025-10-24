using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codefix.AIPlayGround.Migrations
{
    /// <inheritdoc />
    public partial class FixPeerLLMMetadataConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeerLLMAgents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HostEndpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AgentType = table.Column<int>(type: "int", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapabilitiesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalRequests = table.Column<int>(type: "int", nullable: false),
                    SuccessfulRequests = table.Column<int>(type: "int", nullable: false),
                    FailedRequests = table.Column<int>(type: "int", nullable: false),
                    AverageResponseTimeMs = table.Column<double>(type: "float", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerLLMAgents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeerLLMConversations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AgentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MessagesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageCount = table.Column<int>(type: "int", nullable: false),
                    TotalTokens = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerLLMConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeerLLMMessages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConversationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AgentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenCount = table.Column<int>(type: "int", nullable: false),
                    ProcessingTimeMs = table.Column<double>(type: "float", nullable: false),
                    IsStreaming = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerLLMMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PeerLLMUsageStats",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AgentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalRequests = table.Column<int>(type: "int", nullable: false),
                    SuccessfulRequests = table.Column<int>(type: "int", nullable: false),
                    FailedRequests = table.Column<int>(type: "int", nullable: false),
                    TotalTokens = table.Column<int>(type: "int", nullable: false),
                    TotalCost = table.Column<double>(type: "float", nullable: false),
                    AverageResponseTimeMs = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeerLLMUsageStats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMAgents_AgentType",
                table: "PeerLLMAgents",
                column: "AgentType");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMAgents_CreatedAt",
                table: "PeerLLMAgents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMAgents_LastUsedAt",
                table: "PeerLLMAgents",
                column: "LastUsedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMAgents_Name",
                table: "PeerLLMAgents",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMAgents_Status",
                table: "PeerLLMAgents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMConversations_AgentId",
                table: "PeerLLMConversations",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMConversations_LastMessageAt",
                table: "PeerLLMConversations",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMConversations_StartedAt",
                table: "PeerLLMConversations",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMConversations_Status",
                table: "PeerLLMConversations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMConversations_UserId",
                table: "PeerLLMConversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMMessages_AgentId",
                table: "PeerLLMMessages",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMMessages_ConversationId",
                table: "PeerLLMMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMMessages_CreatedAt",
                table: "PeerLLMMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMMessages_Role",
                table: "PeerLLMMessages",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMUsageStats_AgentId",
                table: "PeerLLMUsageStats",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMUsageStats_CreatedAt",
                table: "PeerLLMUsageStats",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMUsageStats_Date",
                table: "PeerLLMUsageStats",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_PeerLLMUsageStats_UserId",
                table: "PeerLLMUsageStats",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeerLLMAgents");

            migrationBuilder.DropTable(
                name: "PeerLLMConversations");

            migrationBuilder.DropTable(
                name: "PeerLLMMessages");

            migrationBuilder.DropTable(
                name: "PeerLLMUsageStats");
        }
    }
}
