using System;
using System.Collections.Generic;
using System.Text;

namespace SFM;

public class DialogueGraph
{
    public Guid Id { get; set; }
    public Guid NpcId { get; set; }
    public Guid? StartNodeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Node> Nodes { get; set; } = new();
    public List<Connection> Connections { get; set; } = new();
}
