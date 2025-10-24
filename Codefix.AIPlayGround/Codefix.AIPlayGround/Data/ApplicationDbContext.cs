using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Codefix.AIPlayGround.Models;

namespace Codefix.AIPlayGround.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<WorkflowEntity> Workflows { get; set; }
    public DbSet<WorkflowNodeEntity> WorkflowNodes { get; set; }
    public DbSet<WorkflowConnectionEntity> WorkflowConnections { get; set; }
    public DbSet<WorkflowMetadataEntity> WorkflowMetadata { get; set; }
    public DbSet<WorkflowSettingsEntity> WorkflowSettings { get; set; }
    
    // Agent Framework Entities
    public DbSet<AgentEntity> Agents { get; set; }
    public DbSet<AgentEntity> MicrosoftAgentFrameworkAgents { get; set; }
    public DbSet<AgentExecutionEntity> AgentExecutions { get; set; }
    
    // Workflow Execution Entities
    public DbSet<WorkflowExecutionEntity> WorkflowExecutions { get; set; }
    public DbSet<ExecutionStepEntity> ExecutionSteps { get; set; }
    public DbSet<ExecutionErrorEntity> ExecutionErrors { get; set; }
    
    // PeerLLM Entities
    public DbSet<PeerLLMAgentEntity> PeerLLMAgents { get; set; }
    public DbSet<PeerLLMConversationEntity> PeerLLMConversations { get; set; }
    public DbSet<PeerLLMMessageEntity> PeerLLMMessages { get; set; }
    public DbSet<PeerLLMUsageStatsEntity> PeerLLMUsageStats { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure WorkflowEntity
        builder.Entity<WorkflowEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Status);
        });

        // Configure WorkflowNodeEntity
        builder.Entity<WorkflowNodeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WorkflowId);
            entity.HasIndex(e => e.Type);
            
            entity.HasOne(e => e.Workflow)
                  .WithMany(w => w.Nodes)
                  .HasForeignKey(e => e.WorkflowId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkflowConnectionEntity
        builder.Entity<WorkflowConnectionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WorkflowId);
            entity.HasIndex(e => e.FromNodeId);
            entity.HasIndex(e => e.ToNodeId);
            
            entity.HasOne(e => e.Workflow)
                  .WithMany(w => w.Connections)
                  .HasForeignKey(e => e.WorkflowId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkflowMetadataEntity
        builder.Entity<WorkflowMetadataEntity>(entity =>
        {
            entity.HasKey(e => e.WorkflowId);
            
            entity.HasOne(e => e.Workflow)
                  .WithOne(w => w.Metadata)
                  .HasForeignKey<WorkflowMetadataEntity>(e => e.WorkflowId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkflowSettingsEntity
        builder.Entity<WorkflowSettingsEntity>(entity =>
        {
            entity.HasKey(e => e.WorkflowId);
            
            entity.HasOne(e => e.Workflow)
                  .WithOne(w => w.Settings)
                  .HasForeignKey<WorkflowSettingsEntity>(e => e.WorkflowId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure AgentEntity
        builder.Entity<AgentEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.AgentType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure AgentExecutionEntity
        builder.Entity<AgentExecutionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);
            
            entity.HasOne(e => e.Agent)
                  .WithMany(a => a.Executions)
                  .HasForeignKey(e => e.AgentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure WorkflowExecutionEntity
        builder.Entity<WorkflowExecutionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.WorkflowId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);
        });

        // Configure ExecutionStepEntity
        builder.Entity<ExecutionStepEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExecutionId);
            entity.HasIndex(e => e.NodeId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);
        });

        // Configure ExecutionErrorEntity
        builder.Entity<ExecutionErrorEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExecutionId);
            entity.HasIndex(e => e.NodeId);
            entity.HasIndex(e => e.OccurredAt);
        });

        // Configure PeerLLMAgentEntity
        builder.Entity<PeerLLMAgentEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.AgentType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.LastUsedAt);
        });

        // Configure PeerLLMConversationEntity
        builder.Entity<PeerLLMConversationEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.LastMessageAt);
            
            // Configure Metadata as JSON column
            entity.Property(e => e.Metadata)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                  )
                  .HasColumnType("nvarchar(max)");
        });

        // Configure PeerLLMMessageEntity
        builder.Entity<PeerLLMMessageEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.CreatedAt);
            
            // Configure Metadata as JSON column
            entity.Property(e => e.Metadata)
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                  )
                  .HasColumnType("nvarchar(max)");
        });

        // Configure PeerLLMUsageStatsEntity
        builder.Entity<PeerLLMUsageStatsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.CreatedAt);
        });

    }
}
