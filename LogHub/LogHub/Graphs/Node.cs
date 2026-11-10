namespace LogHub.Graphs;

public class Node
{
    public string Name { get; set; } = string.Empty;

    public List<Node> Dependencies { get; set; } = new();
}
