namespace DCR;

public static class DcrConformanceChecker
{
    public class ConformanceResult
    {
        public List<string> Trace { get; set; } = new();
        public bool IsConformant { get; set; }
        public List<string> Errors { get; set; } = new();
        public double Fitness { get; set; }
        public int FitnessErrorCount { get; set; }
    }
    
    /// <summary>
    ///     Conformance checks trace against a DCR graph
    /// </summary>
    /// <param name="graph">DCR graph containing discovered relations</param>
    /// <param name="trace">Given trace to check against a DCR graph</param>
    /// <returns>
    ///     ConformanceResult containing checked trace, its conformance status and information about why the trace was
    ///     not compliant in the event that it does not pass conformance checking
    /// </returns>
    public static ConformanceResult CheckTrace(DcrGraph graph, List<string> trace)
    {
        var executed = new HashSet<string>();
        var pending = new HashSet<string>();
        var included = new HashSet<string>(graph.Activities);
        var errors = new List<string>();

        foreach (var activity in trace)
        {
            if (!graph.Activities.Contains(activity))
                continue;
            
            if (!included.Contains(activity))
            {
                errors.Add($"Activity '{activity}' was executed while excluded.");
                continue;
            }

            foreach (var (a, b) in graph.Conditions)
            {
                if (b == activity && !executed.Contains(a))
                {
                    errors.Add($"Condition failed: '{a}' must precede '{b}'.");
                }
            }

            executed.Add(activity);
            pending.Remove(activity);

            foreach (var (a, b) in graph.Responses)
            {
                if (a == activity)
                    pending.Add(b);
            }

            foreach (var (a, b) in graph.Excludes)
            {
                if (a == activity)
                    included.Remove(b);
            }

            foreach (var (a, b) in graph.Includes)
            {
                if (a == activity)
                    included.Add(b);
            }
        }

        foreach (var p in pending)
            errors.Add($"Pending response '{p}' was not executed.");

        var result = new ConformanceResult
        {
            Trace = trace,
            IsConformant = errors.Count == 0,
            Errors = errors,
            FitnessErrorCount = errors.Count()
        };

        result.Fitness = ComputeFitness(result, graph);
        return result;

    }
    
    /// <summary>
    ///     Conformance checks given log against a DCR graph
    /// </summary>
    /// <param name="graph">DCR graph containing discovered relations</param>
    /// <param name="log">Log to compare against a given DCR graph</param>
    /// <returns>
    ///     List of results for each trace and percentage of traces that passed the conformance checking
    /// </returns>
    public static (List<ConformanceResult> Results, double ConformanceRate) CheckLog(DcrGraph graph, List<List<string>> log)
    {
        var results = new List<ConformanceResult>();
        int conformantCount = 0;

        foreach (var trace in log)
        {
            var result = CheckTrace(graph, trace);
            if (result.IsConformant)
                conformantCount++;

            results.Add(result);
        }

        double conformanceRate = (double)conformantCount / log.Count * 100.0;
        
        foreach (var result in results)
        {
            result.Fitness = ComputeFitness(result, graph);
        }

        results = results.OrderBy(r => r.Fitness).ToList();
        
        return (results, conformanceRate);
    }
    
    private static double ComputeFitness(ConformanceResult result, DcrGraph graph)
    {
        int totalRules =
            graph.Conditions.Count +
            graph.Responses.Count +
            graph.Excludes.Count +
            graph.Includes.Count;

        if (totalRules == 0)
            return 1.0;

        int satisfiedRules = totalRules - result.FitnessErrorCount;

        double fitness = Math.Max(0, Math.Min(1, (double)satisfiedRules / totalRules));
        return fitness;
    }
}
