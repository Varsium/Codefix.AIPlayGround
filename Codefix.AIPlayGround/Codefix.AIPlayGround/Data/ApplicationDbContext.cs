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
    public DbSet<AgentExecutionEntity> AgentExecutions { get; set; }
    public DbSet<FlowEntity> Flows { get; set; }
    public DbSet<FlowAgentEntity> FlowAgents { get; set; }
    public DbSet<FlowExecutionEntity> FlowExecutions { get; set; }
    public DbSet<NodeEntity> Nodes { get; set; }
    public DbSet<FlowNodeEntity> FlowNodes { get; set; }
    public DbSet<NodeConnectionEntity> NodeConnections { get; set; }
    public DbSet<NodeTemplateEntity> NodeTemplates { get; set; }

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

        // Configure FlowEntity
        builder.Entity<FlowEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.FlowType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ParentFlowId);
            
            entity.HasOne(e => e.ParentFlow)
                  .WithMany(f => f.SubFlows)
                  .HasForeignKey(e => e.ParentFlowId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure FlowAgentEntity
        builder.Entity<FlowAgentEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FlowId);
            entity.HasIndex(e => e.AgentId);
            entity.HasIndex(new[] { "FlowId", "AgentId" }).IsUnique();
            
            entity.HasOne(e => e.Flow)
                  .WithMany(f => f.FlowAgents)
                  .HasForeignKey(e => e.FlowId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Agent)
                  .WithMany(a => a.FlowAgents)
                  .HasForeignKey(e => e.AgentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure FlowExecutionEntity
        builder.Entity<FlowExecutionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FlowId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);
            
            entity.HasOne(e => e.Flow)
                  .WithMany(f => f.Executions)
                  .HasForeignKey(e => e.FlowId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure NodeEntity
        builder.Entity<NodeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.NodeType);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure FlowNodeEntity
        builder.Entity<FlowNodeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FlowId);
            entity.HasIndex(e => e.NodeId);
            entity.HasIndex(new[] { "FlowId", "NodeId" }).IsUnique();
            
            entity.HasOne(e => e.Flow)
                  .WithMany(f => f.FlowNodes)
                  .HasForeignKey(e => e.FlowId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Node)
                  .WithMany(n => n.FlowNodes)
                  .HasForeignKey(e => e.NodeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure NodeConnectionEntity
        builder.Entity<NodeConnectionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SourceNodeId);
            entity.HasIndex(e => e.TargetNodeId);
            entity.HasIndex(e => e.ConnectionType);
            
            entity.HasOne(e => e.SourceNode)
                  .WithMany(n => n.SourceConnections)
                  .HasForeignKey(e => e.SourceNodeId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.TargetNode)
                  .WithMany(n => n.TargetConnections)
                  .HasForeignKey(e => e.TargetNodeId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure NodeTemplateEntity
        builder.Entity<NodeTemplateEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.NodeType);
            entity.HasIndex(e => e.IsSystemTemplate);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
