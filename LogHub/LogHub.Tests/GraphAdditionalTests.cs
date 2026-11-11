using LogHub.Graphs;

namespace LogHub.Tests;

public class GraphAdditionalTests
{
    [Fact]
    public void ManualIterator_Reset_RestartsTraversal()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        a.Dependencies.Add(b);

        var graph = new Graph();
        using var iterator = graph.ManualDepthFirst(a);

        Assert.True(iterator.MoveNext());
        Assert.Equal("A", iterator.Current.Name);

        iterator.Reset();

        Assert.True(iterator.MoveNext());
        Assert.Equal("A", iterator.Current.Name);
    }

    [Fact]
    public void ManualIterator_ReturnsFalseAfterLastNode()
    {
        var node = new Node { Name = "A" };
        var graph = new Graph();

        using var iterator = graph.ManualDepthFirst(node);

        Assert.True(iterator.MoveNext());
        Assert.False(iterator.MoveNext());
    }

    [Fact]
    public void ManualIterator_Dispose_ClearsTraversal()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        a.Dependencies.Add(b);

        var graph = new Graph();
        var iterator = graph.ManualDepthFirst(a);

        Assert.True(iterator.MoveNext());

        iterator.Dispose();

        Assert.False(iterator.MoveNext());
    }

    [Fact]
    public void DepthFirst_DoesNotRepeatSharedDependency()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        var c = new Node { Name = "C" };
        var d = new Node { Name = "D" };

        a.Dependencies.AddRange(new[] { b, c });
        b.Dependencies.Add(d);
        c.Dependencies.Add(d);

        var graph = new Graph();

        var result = graph.DepthFirst(a)
            .Select(node => node.Name)
            .ToList();

        Assert.Equal(4, result.Count);
        Assert.Equal(1, result.Count(name => name == "D"));
    }

    [Fact]
    public void BreadthFirst_DoesNotRepeatSharedDependency()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        var c = new Node { Name = "C" };
        var d = new Node { Name = "D" };

        a.Dependencies.AddRange(new[] { b, c });
        b.Dependencies.Add(d);
        c.Dependencies.Add(d);

        var graph = new Graph();

        var result = graph.BreadthFirst(a)
            .Select(node => node.Name)
            .ToList();

        Assert.Equal(4, result.Count);
        Assert.Equal(1, result.Count(name => name == "D"));
    }

    [Fact]
    public void CycleDetection_DetectsSelfDependency()
    {
        var a = new Node { Name = "A" };
        a.Dependencies.Add(a);

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

        Assert.Contains("A -> A", writer.ToString());
    }

    [Fact]
    public void CycleDetection_DoesNotPrintCycleForAcyclicGraph()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B" };
        var c = new Node { Name = "C" };

        a.Dependencies.Add(b);
        b.Dependencies.Add(c);

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

        Assert.DoesNotContain("Обнаружен цикл", writer.ToString());
    }
}
