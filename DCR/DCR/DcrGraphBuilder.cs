using LogImport.Models;

namespace DCR;

using System.Collections.Generic;

public static class DcrGraphBuilder
{
    /// <summary>
    ///     Parses imported log into a list of traces and subsequently builds DcrGraph
    /// </summary>
    /// <param name="imported">Imported log</param>
    /// <param name="id_column">Name of a column with ids in imported .csv file</param>
    /// <param name="activity_column">Name of a column with activities in imported .csv file</param>
    /// <returns>DcrGraph built from given imported log</returns>
    public static DcrGraph BuildFromImportedLog(
        ImportedEventLog imported,
        string id_column,
        string activity_column,
        HashSet<string>? activityFilter = null,
        HashSet<string>? relationFilter = null,
        double threshold = 1.0
    )
    {
        List<List<string>> traces = LogParser.ParseToTraces(imported, id_column, activity_column);
        return Build(traces, activityFilter, relationFilter, threshold);
    }
    
    /// <summary>
    ///     Builds DcrGraph from a list of traces
    /// </summary>
    /// <param name="log">List of traces from a log</param>
    /// <returns>DCR graph built from given traces</returns>
    public static DcrGraph Build(
        List<List<string>> log,
        HashSet<string>? activityFilter = null,
        HashSet<string>? relationFilter = null,
        double threshold = 1.0
    )
    {
        (DcrGraph graph, Dictionary<int, string> eventLabeling) = LogParser.InitializeFromLog(log);

        HashSet<(string, string)> atMostOne = Constraints.AtMostOne(log, threshold);
        foreach ((string a, string b) in atMostOne)
            graph.Excludes.Add((a, b));

        HashSet<(string, string)> precedence = Constraints.Precedence(log, threshold);
        foreach ((string a, string b) in precedence)
            graph.Conditions.Add((a, b));

        HashSet<(string, string)> responses = Constraints.Response(log, threshold);
        foreach ((string a, string b) in responses)
            graph.Responses.Add((a, b));

        HashSet<(string, string)> chainPrecedence = Constraints.ChainPrecedence(log);
        foreach ((string a, string b) in chainPrecedence)
        {
            if (a != b)
                graph.Includes.Add((a, b));
        }

        foreach ((string a, string b) in chainPrecedence)
        {
            if (a != b)
                graph.Excludes.Add((b, b));
        }

        HashSet<(string, string)> mutualExcludes = Constraints.MutuallyExclusive(log);
        foreach ((string a, string b) in mutualExcludes)
            graph.Excludes.Add((a, b));

        HashSet<(string, string)> notChainSuccession = Constraints.NotChainSuccession(log);
        foreach ((string a, string b) in notChainSuccession)
        {
            if (!Constraints.AreCoOccurring(log, a, b))
            {
                graph.Excludes.Add((a, b));
            }
        }

        HashSet<(string, string)> altPrec = Constraints.AlternatePrecedence(log);
        
        DcrGraphOptimizer.RemoveTransitiveExcludes(graph, altPrec);
        DcrGraphOptimizer.RemoveTransitiveConditions(graph);
        DcrGraphOptimizer.RemoveTransitiveResponses(graph);

        HashSet<(string, string)> inferred = Constraints.InferredConditions(log, threshold);
        foreach ((string a, string b) in inferred)
            graph.Conditions.Add((a, b));
        
        if (activityFilter != null && activityFilter.Count > 0)
        {
            graph.Activities = graph.Activities.Where(activityFilter.Contains).ToHashSet();

            graph.Conditions = graph.Conditions
                .Where(r => activityFilter.Contains(r.Item1) && activityFilter.Contains(r.Item2)).ToHashSet();

            graph.Responses = graph.Responses
                .Where(r => activityFilter.Contains(r.Item1) && activityFilter.Contains(r.Item2)).ToHashSet();

            graph.Excludes = graph.Excludes
                .Where(r => activityFilter.Contains(r.Item1) && activityFilter.Contains(r.Item2)).ToHashSet();

            graph.Includes = graph.Includes
                .Where(r => activityFilter.Contains(r.Item1) && activityFilter.Contains(r.Item2)).ToHashSet();
        }

        if (relationFilter != null && relationFilter.Count > 0)
        {
            if (!relationFilter.Contains("Conditions"))
            {
                graph.Conditions = new HashSet<(string, string)>();
            };

            if (!relationFilter.Contains("Responses"))
            {
                graph.Responses = new HashSet<(string, string)>();
            };
            
            if (!relationFilter.Contains("Excludes"))
            {
                graph.Excludes = new HashSet<(string, string)>();
            };
            
            if (!relationFilter.Contains("Includes"))
            {
                graph.Includes = new HashSet<(string, string)>();
            };
        }

        return graph;
    }
}