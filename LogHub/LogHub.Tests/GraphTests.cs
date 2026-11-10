using LogHub.Graphs;

namespace LogHub.Tests;

public class GraphTests
{
    [Fact]
    public void DepthFirst_ReturnsNodesInCorrectOrder()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        var c = new Node { Name = "C" };
        var d = new Node { Name = "D" };

        a.Dependencies.AddRange(new[] { b, c });
        b.Dependencies.Add(d);

        var graph = new Graph();

        var result = graph.DepthFirst(a)
            .Select(node => node.Name)
            .ToList();

        Assert.Equal(new[] { "A", "B", "D", "C" }, result);
    }

    [Fact]
    public void BreadthFirst_ReturnsNodesInCorrectOrder()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        var c = new Node { Name = "C" };
        var d = new Node { Name = "D" };

        a.Dependencies.AddRange(new[] { b, c });
        b.Dependencies.Add(d);

        var graph = new Graph();

        var result = graph.BreadthFirst(a)
            .Select(node => node.Name)
            .ToList();

        Assert.Equal(new[] { "A", "B", "C", "D" }, result);
    }

    [Fact]
    public void NestedForeach_ReturnsCorrectResults()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        var c = new Node { Name = "C" };

        a.Dependencies.Add(b);
        b.Dependencies.Add(c);

        var graph = new Graph();
        var result = new List<string>();

        foreach (var service in graph.DepthFirst(a))
        {
            foreach (var dependency in graph.BreadthFirst(service))
            {
                result.Add($"{service.Name} -> {dependency.Name}");
            }
        }

        Assert.Equal(
            new[]
            {
                "A -> A",
                "A -> B",
                "A -> C",
                "B -> B",
                "B -> C",
                "C -> C"
            },
            result);
    }

    [Fact]
    public void Traversal_WithCycle_DoesNotRepeatNodes()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        var c = new Node { Name = "C" };

        a.Dependencies.Add(b);
        b.Dependencies.Add(c);
        c.Dependencies.Add(a);

        var graph = new Graph();

        var dfs = graph.DepthFirst(a).ToList();
        var bfs = graph.BreadthFirst(a).ToList();

        Assert.Equal(3, dfs.Count);
        Assert.Equal(3, bfs.Count);
    }

    [Fact]
    public void SingleNode_ReturnsOnlyOneNode()
    {
        var node = new Node { Name = "A" };
        var graph = new Graph();

        Assert.Equal(new[] { "A" },
            graph.DepthFirst(node).Select(n => n.Name));

        Assert.Equal(new[] { "A" },
            graph.BreadthFirst(node).Select(n => n.Name));
    }

    [Fact]
    public void ManualIterator_AndYield_ReturnSameSequence()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        var c = new Node { Name = "C" };

        a.Dependencies.AddRange(new[] { b, c });
        b.Dependencies.Add(c);

        var graph = new Graph();

        var manual = new List<string>();

        using (var enumerator = graph.ManualDepthFirst(a))
        {
            while (enumerator.MoveNext())
            {
                manual.Add(enumerator.Current.Name);
            }
        }

        var yield = graph.DepthFirst(a)
            .Select(node => node.Name)
            .ToList();

        Assert.Equal(yield, manual);
    }

    [Fact]
    public void CycleDetection_PrintsCyclePath()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        var c = new Node { Name = "C" };

        a.Dependencies.Add(b);
        b.Dependencies.Add(c);
        c.Dependencies.Add(a);

        var graph = new Graph();

        using var writer = new StringWriter();
        var originalOutput = Console.Out;

        try
        {
            Console.SetOut(writer);
            graph.PrintCycles(a);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        Assert.Contains("A -> B -> C -> A", writer.ToString());
    }
}
