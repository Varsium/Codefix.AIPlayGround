namespace Codefix.AIPlayGround.Models
{
    public class WorkflowNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 150;
        public double Height { get; set; } = 80;
        public Dictionary<string, object> Properties { get; set; } = new();
        public List<string> InputPorts { get; set; } = new();
        public List<string> OutputPorts { get; set; } = new();
        public bool IsSelected { get; set; }
        public string Status { get; set; } = "idle"; // idle, running, completed, error
    }
}
