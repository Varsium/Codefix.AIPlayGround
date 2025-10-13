namespace Codefix.AIPlayGround.Models
{
    public class WorkflowConnection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FromNodeId { get; set; } = "";
        public string ToNodeId { get; set; } = "";
        public string FromPort { get; set; } = "";
        public string ToPort { get; set; } = "";
        public string Label { get; set; } = "";
        public string Type { get; set; } = "default"; // default, conditional, parallel
        public Dictionary<string, object> Properties { get; set; } = new();
        public bool IsSelected { get; set; }
        
        // Compatibility properties for the simple Home.razor version
        public string From { get; set; } = "";
        public string To { get; set; } = "";
    }
}
