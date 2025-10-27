namespace DCR;

public static class DcrGraphOptimizer
{
    /// <summary>
    ///     Removes irrelevant transitive responses from a given graph
    /// </summary>
    /// <param name="graph">DcrGraph with potential transitive responses</param>
    public static void RemoveTransitiveResponses(DcrGraph graph)
    {
        HashSet<(string, string)> toRemove = new HashSet<(string, string)>();

        foreach ((string a, string b) in graph.Responses)
        {
            foreach ((string x, string c) in graph.Responses)
            {
                if (x == b && graph.Responses.Contains((a, c)))
                    toRemove.Add((a, c));
            }
        }

        foreach ((string, string) r in toRemove)
            graph.Responses.Remove(r);
    }
    
    /// <summary>
    ///     Removes irrelevant transitive conditions from a given graph
    /// </summary>
    /// <param name="graph">DcrGraph with potential transitive conditions</param>
    public static void RemoveTransitiveConditions(DcrGraph graph)
    {
        HashSet<(string, string)> toRemove = new HashSet<(string, string)>();

        foreach ((string a, string b) in graph.Conditions)
        {
            foreach ((string x, string c) in graph.Conditions)
            {
                if (x == b && graph.Conditions.Contains((a, c)))
                    toRemove.Add((a, c));
            }
        }

        foreach ((string, string) r in toRemove)
            graph.Conditions.Remove(r);
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

        foreach ((string s, string t) in graph.Excludes)
        {
            foreach ((string u, string x) in graph.Excludes)
            {
                if (x != t || s == u) continue;
                
                if (alternatePrecedence.Contains((u, s)))
                {
                    toRemove.Add((s, t));
                    break;
                }
            }
        }

        foreach ((string, string) r in toRemove)
            graph.Excludes.Remove(r);
    }
}