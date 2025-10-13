namespace Codefix.AIPlayGround.Models;

public class AgentTypeDefinition
{
    public AgentType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}
