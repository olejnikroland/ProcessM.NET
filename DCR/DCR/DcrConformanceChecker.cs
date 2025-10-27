namespace DCR;

public static class DcrConformanceChecker
{
    public class ConformanceResult
    {
        public List<string> Trace { get; set; } = new();
        public bool IsConformant { get; set; }
        public List<string> Errors { get; set; } = new();
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
            if (!included.Contains(activity))
            {
                errors.Add($"Activity '{activity}' is not included.");
                return new ConformanceResult { Trace = trace, IsConformant = false, Errors = errors };
            }

            foreach (var (a, b) in graph.Conditions)
            {
                if (b == activity && !executed.Contains(a))
                {
                    errors.Add($"Condition failed: '{a}' must precede '{b}'.");
                    return new ConformanceResult { Trace = trace, IsConformant = false, Errors = errors };
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

        if (pending.Count > 0)
        {
            foreach (var p in pending)
                errors.Add($"Pending response '{p}' was not executed.");

            return new ConformanceResult { Trace = trace, IsConformant = false, Errors = errors };
        }

        return new ConformanceResult { Trace = trace, IsConformant = true, Errors = new List<string>() };

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
        return (results, conformanceRate);
    }
}
