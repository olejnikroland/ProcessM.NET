namespace DCR;

public static class DcrGraphOptimizer
{
    /// <summary>
    ///     Removes irrelevant transitive responses from a given graph
    /// </summary>
    /// <param name="graph">DcrGraph with potential transitive responses</param>
    public static void RemoveTransitiveConditions(DcrGraph graph)
    {
        List<(string, string)> edges = graph.Conditions.ToList();

        foreach (var edge in edges)
        {
            if (!graph.Conditions.Contains(edge))
                continue;

            var (a, b) = edge;
            
            graph.Conditions.Remove(edge);
            
            if (!HasPath(graph.Conditions, a, b))
            {
                graph.Conditions.Add(edge);
            }
        }
    }
    
    /// <summary>
    ///     Removes irrelevant transitive conditions from a given graph
    /// </summary>
    /// <param name="graph">DcrGraph with potential transitive conditions</param>
    public static void RemoveTransitiveResponses(DcrGraph graph)
    {
        List<(string, string)> edges = graph.Responses.ToList();

        foreach (var edge in edges)
        {
            if (!graph.Responses.Contains(edge))
                continue;

            var (a, b) = edge;

            graph.Responses.Remove(edge);

            if (!HasPath(graph.Responses, a, b))
            {
                graph.Responses.Add(edge);
            }
        }
    }
    
    /// <summary>
    ///     Removes irrelevant transitive excludes from a given graph that provide no new information in a context of
    ///     a DcrGraph
    /// </summary>
    /// <param name="graph">DcrGraph with potential transitive excludes</param>
    /// <param name="alternatePrecedence">
    ///     Alternate precedence relations of a given graph providing additional information to identify irrelevant
    ///     excludes
    /// </param>
    public static void RemoveTransitiveExcludes(DcrGraph graph, HashSet<(string, string)> alternatePrecedence)
    {
        HashSet<(string, string)> toRemove = new HashSet<(string, string)>();

        foreach ((string a, string b) in graph.Excludes)
        {
            foreach ((string x, string y) in graph.Excludes)
            {
                if (y != b || a == x) continue;
                
                if (alternatePrecedence.Contains((x, a)))
                {
                    toRemove.Add((a, b));
                    break;
                }
            }
        }

        foreach ((string, string) r in toRemove)
            graph.Excludes.Remove(r);
    }
    
    private static bool HasPath(
        HashSet<(string, string)> edges,
        string start,
        string target
    )
    {
        if (start == target) return true;
        
        Dictionary<string, List<string>> adj = new();
        foreach (var (a, b) in edges)
        {
            if (!adj.TryGetValue(a, out var list))
            {
                list = new List<string>();
                adj[a] = list;
            }
            list.Add(b);
        }

        HashSet<string> visited = new();
        Stack<string> stack = new();
        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0)
        {
            string a = stack.Pop();
            if (!adj.TryGetValue(a, out List<string> succ)) continue;

            foreach (string successor in succ)
            {
                if (successor == target)
                    return true;

                if (visited.Add(successor))
                    stack.Push(successor);
            }
        }

        return false;
    }
}