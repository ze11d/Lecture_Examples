namespace LogHub.Graphs;

public class ManualGraphEnumerator : IEnumerator<Node>
{
    private readonly Node _root;
    private readonly Stack<Node> _stack = new();
    private readonly HashSet<Node> _visited = new();

    public ManualGraphEnumerator(Node root)
    {
        _root = root;
        Reset();
    }

    public Node Current { get; private set; } = null!;

    object System.Collections.IEnumerator.Current => Current;

    public bool MoveNext()
    {
        while (_stack.Count > 0)
        {
            var node = _stack.Pop();

            if (!_visited.Add(node))
                continue;

            Current = node;

            foreach (var dependency in Enumerable.Reverse(node.Dependencies))
            {
                if (!_visited.Contains(dependency))
                    _stack.Push(dependency);
            }

            return true;
        }

        return false;
    }

    public void Reset()
    {
        _stack.Clear();
        _visited.Clear();
        _stack.Push(_root);
        Current = null!;
    }

    public void Dispose()
    {
        _stack.Clear();
        _visited.Clear();
    }
}
