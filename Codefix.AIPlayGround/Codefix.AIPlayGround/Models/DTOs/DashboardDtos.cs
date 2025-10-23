namespace Codefix.AIPlayGround.Models.DTOs;

public class DashboardStatsResponse
{
    public int TotalAgents { get; set; }
    public int ActiveAgents { get; set; }
    public int TotalExecutions { get; set; }
    public double SuccessRate { get; set; }
    public double AgentGrowth { get; set; }
    public double ActiveGrowth { get; set; }
    public double ExecutionGrowth { get; set; }
}

public class DashboardActivityResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string IconColor { get; set; } = "primary";
}

public class AgentDistributionResponse
{
    public string AgentType { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ExecutionTrendResponse
{
    public DateTime Date { get; set; }
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public int FailedExecutions { get; set; }
}
