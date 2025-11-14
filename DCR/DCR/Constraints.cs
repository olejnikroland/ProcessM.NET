using System.Collections.Generic;

public static class Constraints
{
    /// <summary>
    ///     Discovers AtMostOne constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered AtMostOne relation</returns>
    public static HashSet<(string, string)> AtMostOne(List<List<string>> log, double threshold = 1.0)
    {
        HashSet<(string, string)> result = new();
        var activities = GetAllActivities(log);
        int totalTraces = log.Count;

        foreach (var act in activities)
        {
            int tracesOk = 0;

            foreach (var trace in log)
            {
                int count = trace.Count(x => x == act);
                if (count <= 1)
                    tracesOk++;
            }

            if (totalTraces > 0 && (double)tracesOk / totalTraces >= threshold)
                result.Add((act, act));
        }

        return result;
    }
    
    /// <summary>
    ///     Discovers Precedence constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered Precedence relation</returns>
    public static HashSet<(string, string)> Precedence(List<List<string>> log, double threshold = 1.0)
    {
        HashSet<(string, string)> result = new HashSet<(string, string)>();
        List<string> allActivities = GetAllActivities(log).ToList();
        int totalTraces = log.Count;

        foreach (string a in allActivities)
        {
            foreach (string b in allActivities)
            {
                if (a == b) continue;

                int satisfiedCount = 0;

                foreach (List<string> trace in log)
                {
                    bool holds;

                    if (!trace.Contains(b))
                    {
                        holds = true;
                    }
                    else
                    {
                        int indexB = trace.IndexOf(b);
                        bool aBeforeB = trace.Take(indexB).Contains(a);
                        holds = aBeforeB;
                    }

                    if (holds)
                        satisfiedCount++;
                }

                if (totalTraces > 0 && (double)satisfiedCount / totalTraces >= threshold)
                    result.Add((a, b));
            }
        }

        return result;
    }
    
    /// <summary>
    ///     Discovers Response constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered Response relation</returns>
    public static HashSet<(string, string)> Response(List<List<string>> log, double threshold = 1.0)
    {
        HashSet<(string, string)> result = new();
        var all = GetAllActivities(log);
        int totalTraces = log.Count;

        foreach (string a in all)
        {
            foreach (string b in all)
            {
                if (a == b) continue;

                int satisfied = 0;

                foreach (var trace in log)
                {
                    var aIdx = Enumerable.Range(0, trace.Count).Where(i => trace[i] == a).ToList();
                    bool holds = true;

                    foreach (int i in aIdx)
                    {
                        bool bAfter = trace.Skip(i + 1).Any(x => x == b);
                        if (!bAfter)
                        {
                            holds = false;
                            break;
                        }
                    }

                    if (holds)
                        satisfied++;
                }

                if (totalTraces > 0 && (double)satisfied / totalTraces >= threshold)
                    result.Add((a, b));
            }
        }

        return result;
    }
    
    /// <summary>
    ///     Discovers ChainPrecedence constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered ChainPrecedence relation</returns>
    public static HashSet<(string, string)> ChainPrecedence(List<List<string>> log)
    {
        HashSet<(string, string)> result = new HashSet<(string, string)>();
        HashSet<string> allActivities = log.SelectMany(trace => trace).ToHashSet();

        foreach (string b in allActivities)
        {
            string? requiredPredecessor = null;
            bool valid = true;

            foreach (List<string> trace in log)
            {
                for (int i = 1; i < trace.Count; i++)
                {
                    if (trace[i] == b)
                    {
                        string a = trace[i - 1];
                        if (requiredPredecessor == null)
                        {
                            requiredPredecessor = a;
                        }
                        else if (requiredPredecessor != a)
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (!valid) break;
                
                if (trace.First() == b)
                {
                    valid = false;
                    break;
                }
            }

            if (valid && requiredPredecessor != null)
            {
                result.Add((requiredPredecessor, b));
            }
        }

        return result;
    }
    
    /// <summary>
    ///     Discovers MutuallyExclusive constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered MutuallyExclusive relation</returns>
    public static HashSet<(string, string)> MutuallyExclusive(List<List<string>> log)
    {
        HashSet<(string, string)> result = new HashSet<(string, string)>();
        HashSet<string> allActivities = GetAllActivities(log);

        foreach (string a in allActivities)
        {
            foreach (string b in allActivities)
            {
                if (a == b) continue;

                bool seenTogether = false;

                foreach (List<string> trace in log)
                {
                    if (trace.Contains(a) && trace.Contains(b))
                    {
                        seenTogether = true;
                        break;
                    }
                }

                if (!seenTogether)
                    result.Add((a, b));
            }
        }

        return result;
    }
    
    /// <summary>
    ///     Discovers NotChainSuccession constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered NotChainSuccession relation</returns>
    public static HashSet<(string, string)> NotChainSuccession(List<List<string>> log)
    {
        HashSet<(string, string)> result = new HashSet<(string, string)>();
        HashSet<string> allActivities = GetAllActivities(log);

        foreach (string a in allActivities)
        {
            foreach (string b in allActivities)
            {
                if (a == b) continue;
                bool foundDirectSuccession = false;

                foreach (List<string> trace in log)
                {
                    for (int i = 0; i < trace.Count - 1; i++)
                    {
                        if (trace[i] == a && trace[i + 1] == b)
                        {
                            foundDirectSuccession = true;
                            break;
                        }
                    }

                    if (foundDirectSuccession)
                        break;
                }

                if (!foundDirectSuccession)
                    result.Add((a, b));
            }
        }

        return result;
    }
    
    /// <summary>
    ///     Discovers InferredConditions constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered InferredConditions relation</returns>
    public static HashSet<(string, string)> InferredConditions(List<List<string>> log, double threshold = 1.0)
    {
        HashSet<(string, string)> result = new();
        var all = GetAllActivities(log);
        int totalTraces = log.Count;

        foreach (var a in all)
        {
            foreach (var b in all)
            {
                if (a == b) continue;

                int satisfied = 0;

                foreach (var trace in log)
                {
                    var aIdx = Enumerable.Range(0, trace.Count).Where(i => trace[i] == a).ToList();
                    var bIdx = Enumerable.Range(0, trace.Count).Where(i => trace[i] == b).ToList();

                    bool holds = true;

                    foreach (var bi in bIdx)
                    {
                        bool hasA = aIdx.Any(ai => ai < bi);
                        if (!hasA)
                        {
                            holds = false;
                            break;
                        }
                    }

                    if (holds)
                        satisfied++;
                }

                if (totalTraces > 0 && (double)satisfied / totalTraces >= threshold)
                    result.Add((a, b));
            }
        }

        return result;
    }
    
    /// <summary>
    ///     Discovers AlternatePrecedence constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered AlternatePrecedence relation</returns>
    public static HashSet<(string, string)> AlternatePrecedence(List<List<string>> log)
    {
        HashSet<(string, string)> result = new HashSet<(string, string)>();

        foreach (List<string> trace in log)
        {

            for (int i = 0; i < trace.Count - 1; i++)
            {
                string a = trace[i];
                string b = trace[i + 1];

                result.Add((a, b));
            }
        }

        return result;
    }
    
    /// <summary>
    ///     Checks if given activities are occuring together in any trace of a given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered AlternatePrecedence relation</returns>
    public static bool AreCoOccurring(List<List<string>> log, string a, string b)
    {
        foreach (List<string> trace in log)
        {
            if (trace.Contains(a) && trace.Contains(b))
                return true;
        }
        return false;
    }

    /// <summary>
    ///     Gets all activities from a given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities from the log</returns>
    private static HashSet<string> GetAllActivities(List<List<string>> log)
    {
        HashSet<string> set = new HashSet<string>();
        foreach (List<string> trace in log)
            foreach (string activity in trace)
                set.Add(activity);
        return set;
    }
}

