using DCR.Enums;
using LogImport.Models;

namespace DCR;

using System.Collections.Generic;

public static class DcrGraphBuilder
{
    /// <summary>
    ///     Parses the imported event log into a list of traces and subsequently builds DcrGraph.
    /// </summary>
    /// <param name="imported">Imported event log</param>
    /// <param name="idColumn">Name of a column with ids in the imported .csv file.</param>
    /// <param name="activityColumn">Name of a column with activities in the imported .csv file.</param>
    /// <param name="timestampColumn">Name of a column with timestamp in the imported .csv file.</param>
    /// <param name="activityFilter">List of activity names to be filtered in the graph.</param>
    /// <param name="relationFilter">List of enums representing relations to be filtered in the graph.</param>
    /// <param name="threshold">Minimum proportion of traces in which a relation must hold to be discovered,
    /// expressed as a value between 0.0 and 1.0.</param>
    /// <returns>DcrGraph built from given imported log.</returns>
    public static DcrGraph BuildFromImportedLog(
        ImportedEventLog imported,
        string idColumn,
        string activityColumn,
        string timestampColumn,
        HashSet<string>? activityFilter = null,
        HashSet<RelationType>? relationFilter = null,
        double threshold = 1.0
    )
    {
        List<List<string>> traces = LogParser.ParseToTraces(imported, idColumn, activityColumn, timestampColumn);
        return Build(traces, activityFilter, relationFilter, threshold);
    }

    /// <summary>
    ///     Builds DcrGraph from a list of traces.
    /// </summary>
    /// <param name="log">List of traces from a log.</param>
    /// <param name="activityFilter">List of activity names to be filtered in the graph.</param>
    /// <param name="relationFilter">List of enums representing relations to be filtered in the graph.</param>
    /// <param name="threshold">Minimum proportion of traces in which a relation must hold to be discovered,
    /// expressed as a value between 0.0 and 1.0.</param>
    /// <returns>A DCR graph built from given traces.</returns>
    public static DcrGraph Build(
        List<List<string>> log,
        HashSet<string>? activityFilter = null,
        HashSet<RelationType>? relationFilter = null,
        double threshold = 1.0
    )
    {
        (DcrGraph graph, _) = LogParser.InitializeFromLog(log);

        HashSet<(string, string)> atMostOne = Constraints.AtMostOne(log);
        foreach ((string a, string b) in atMostOne)
            graph.Excludes.Add((a, b));
        
        HashSet<(string, string)> responses = Constraints.Response(log, threshold);
        foreach ((string a, string b) in responses)
            graph.Responses.Add((a, b));

        HashSet<(string, string)> chainPrecedence = Constraints.ChainPrecedence(log);
        
        var atMostOnceActivities = atMostOne.Select(x => x.Item1).ToHashSet();

        foreach ((string a, string b) in chainPrecedence)
        {
            if (a == b) continue;
            
            if (!atMostOnceActivities.Contains(b))
                graph.Includes.Add((a, b));
            
            graph.Excludes.Add((b, b));
        }
        
        HashSet<(string, string)> precedence = Constraints.Precedence(log, threshold);
        foreach ((string a, string b) in precedence)
            graph.Conditions.Add((a, b));
        
        var (pred, succ) = Constraints.DeterminePredecessorSuccessor(log);
        
        
        foreach (var activity in graph.Activities)
        {
            HashSet<string> coexisting = new HashSet<string>(pred[activity].Union(succ[activity]));
            var notCoexisting = graph.Activities.Except(coexisting).ToHashSet();
            notCoexisting.Remove(activity);

            foreach (string act in notCoexisting)
                graph.Excludes.Add((activity, act));
        }
        
        foreach (string activity in graph.Activities)
        {
            var precedesButNeverSucceeds = pred[activity].Except(succ[activity]);
            foreach (string sAct in precedesButNeverSucceeds)
            {
                if (!graph.Excludes.Contains((sAct, sAct)))
                    graph.Excludes.Add((activity, sAct));
            }
        }
        
        var precedencePairs = Constraints.Precedence(log);
        var precedencePerActivity = graph.Activities.ToDictionary(
            activity => activity,
            activity => precedencePairs
                .Where(p => p.Item2 == activity)
                .Select(p => p.Item1)
                .ToHashSet()
        );

        foreach (var activity in graph.Activities)
        {
            foreach (var prec in precedencePerActivity[activity])
            {
                foreach (var act in graph.Activities)
                {
                    if (graph.Excludes.Contains((prec, act)))
                        graph.Excludes.Remove((activity, act));
                }
            }
        }
        
        HashSet<(string, string)> altPrec = Constraints.AlternatePrecedence(log);

        DcrGraphOptimizer.RemoveTransitiveExcludes(graph, altPrec);
        DcrGraphOptimizer.RemoveTransitiveConditions(graph);
        DcrGraphOptimizer.RemoveTransitiveResponses(graph);
        
        
        if (activityFilter is { Count: > 0 })
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

        if (relationFilter is not { Count: > 0 })
            return graph;

        if (!relationFilter.Contains(RelationType.Conditions))
            graph.Conditions = new HashSet<(string, string)>();

        if (!relationFilter.Contains(RelationType.Responses))
            graph.Responses = new HashSet<(string, string)>();

        if (!relationFilter.Contains(RelationType.Excludes))
            graph.Excludes = new HashSet<(string, string)>();

        if (!relationFilter.Contains(RelationType.Includes))
            graph.Includes = new HashSet<(string, string)>();

        return graph;
    }
}