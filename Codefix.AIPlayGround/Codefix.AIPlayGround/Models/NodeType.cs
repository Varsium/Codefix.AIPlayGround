namespace Codefix.AIPlayGround.Models
{
    public class NodeType
    {
        public string Type { get; set; } = "";
        public string Label { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Description { get; set; } = "";
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}
