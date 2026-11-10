namespace LogHub.Graphs;

public class Graph
{
    public IEnumerable<Node> DepthFirst(Node root)
    {
        var stack = new Stack<Node>();
        var visited = new HashSet<Node>();

        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();

            if (!visited.Add(node))
                continue;

            yield return node;

            foreach (var dependency in Enumerable.Reverse(node.Dependencies))
            {
                if (!visited.Contains(dependency))
                    stack.Push(dependency);
            }
        }
    }

    public IEnumerable<Node> BreadthFirst(Node root)
    {
        var queue = new Queue<Node>();
        var visited = new HashSet<Node>();

        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();

            if (!visited.Add(node))
                continue;

            yield return node;

            foreach (var dependency in node.Dependencies)
            {
                if (!visited.Contains(dependency))
                    queue.Enqueue(dependency);
            }
        }
    }

    public ManualGraphEnumerator ManualDepthFirst(Node root)
    {
        return new ManualGraphEnumerator(root);
    }

    public void PrintCycles(Node root)
    {
        var visited = new HashSet<Node>();
        var active = new HashSet<Node>();
        var path = new List<Node>();

        void Visit(Node node)
        {
            if (active.Contains(node))
            {
                var cycleStart = path.IndexOf(node);
                var cycle = path.Skip(cycleStart)
                    .Append(node)
                    .Select(n => n.Name);

                Console.WriteLine(
                    $"Обнаружен цикл: {string.Join(" -> ", cycle)}");

                return;
            }

            if (!visited.Add(node))
                return;

            active.Add(node);
            path.Add(node);

            foreach (var dependency in node.Dependencies)
            {
                Visit(dependency);
            }

            path.RemoveAt(path.Count - 1);
            active.Remove(node);
        }

        Visit(root);
    }
}
