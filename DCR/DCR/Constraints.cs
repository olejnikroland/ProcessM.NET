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
        HashSet<string> activities = GetAllActivities(log);
        int totalTraces = log.Count;

        foreach (string activity in activities)
        {
            int tracesOk = 0;

            foreach (List<string> trace in log)
            {
                int count = trace.Count(x => x == activity);
                if (count <= 1)
                    tracesOk++;
            }

            if (totalTraces > 0 && (double)tracesOk / totalTraces >= threshold)
                result.Add((activity, activity));
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
        HashSet<(string, string)> result = new();
        HashSet<string> allActivities = GetAllActivities(log);
        int totalTraces = log.Count;

        foreach (string a in allActivities)
        {
            foreach (string b in allActivities)
            {
                if (a == b) continue;

                int satisfied = 0;

                foreach (var trace in log)
                {
                    List<int> aIdx = Enumerable.Range(0, trace.Count).Where(i => trace[i] == a).ToList();
                    List<int> bIdx = Enumerable.Range(0, trace.Count).Where(i => trace[i] == b).ToList();

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
    ///     Discovers Response constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered Response relation</returns>
    public static HashSet<(string, string)> Response(List<List<string>> log, double threshold = 1.0)
    {
        HashSet<(string, string)> result = new();
        HashSet<string> allActivities = GetAllActivities(log);
        int totalTraces = log.Count;

        foreach (string a in allActivities)
        {
            foreach (string b in allActivities)
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
        HashSet<string> allActivities = GetAllActivities(log);
        Dictionary<string, HashSet<string>> candidates = new();

        foreach (string activity in allActivities)
            candidates[activity] = new HashSet<string>(allActivities.Where(act => act != activity));

        HashSet<string> atMostOnce = AtMostOne(log).Select(x => x.Item1).ToHashSet();

        foreach (List<string> trace in log)
        {
            string last = null;

            foreach (string activity in trace)
            {
                if (last == null)
                {
                    candidates[activity].Clear();
                }
                else
                {
                    candidates[activity].IntersectWith(new HashSet<string> { last });
                }

                last = activity;
            }
        }

        HashSet<(string, string)> result = new();

        foreach (string key in candidates.Keys)
        {
            if (atMostOnce.Contains(key))
                continue;

            foreach (string act in candidates[key])
                result.Add((act, key));
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
        HashSet<(string, string)> result = new();
        HashSet<string> allActivities = GetAllActivities(log);

        foreach (string activity in allActivities)
        {
            foreach (string act in allActivities)
            {
                if (activity == act) continue;
                bool foundDirectSuccession = false;

                foreach (List<string> trace in log)
                {
                    for (int i = 0; i < trace.Count - 1; i++)
                    {
                        if (trace[i] == activity && trace[i + 1] == act)
                        {
                            foundDirectSuccession = true;
                            break;
                        }
                    }

                    if (foundDirectSuccession)
                        break;
                }

                if (!foundDirectSuccession)
                    result.Add((activity, act));
            }
        }

        return result;
    }
    
    /// <summary>
    ///     Discovers AlternatePrecedence constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered AlternatePrecedence relation</returns>
    public static HashSet<(string, string)> AlternatePrecedence(List<List<string>> log, double threshold = 1.0)
    {
        HashSet<(string, string)> result = new();
        HashSet<string> allActivities = GetAllActivities(log);
        int totalTraces = log.Count;

        foreach (string activity in allActivities)
        {
            foreach (string act in allActivities)
            {
                if (activity == act) continue;

                int satisfied = 0;

                foreach (var trace in log)
                {
                    var bIdx = Enumerable.Range(0, trace.Count).Where(i => trace[i] == act).ToList();
                    bool holds = true;

                    foreach (var bi in bIdx)
                    {
                        if (bi == 0 || trace[bi - 1] != activity)
                        {
                            holds = false;
                            break;
                        }
                    }

                    if (holds)
                        satisfied++;
                }

                if (totalTraces > 0 && (double)satisfied / totalTraces >= threshold)
                    result.Add((activity, act));
            }
        }

        return result;
    }
    
    public static (Dictionary<string, HashSet<string>> predecessor, 
        Dictionary<string, HashSet<string>> successor) 
        DeterminePredecessorSuccessor(List<List<string>> log)
    {
        HashSet<string> allActivities = GetAllActivities(log);

        Dictionary<string,HashSet<string>> predecessor =
            allActivities.ToDictionary(x => x, x => new HashSet<string>());
        Dictionary<string,HashSet<string>> successor =
            allActivities.ToDictionary(x => x, x => new HashSet<string>());

        foreach (List<string> trace in log)
        {
            for (int i = 0; i < trace.Count; i++)
            {
                for (int j = 0; j < i; j++)
                    predecessor[trace[i]].Add(trace[j]);

                for (int j = i + 1; j < trace.Count; j++)
                    successor[trace[i]].Add(trace[j]);
            }
        }

        return (predecessor, successor);
    }
    
    /// <summary>
    ///     Checks if given activities are occuring together in any trace of a given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered AlternatePrecedence relation</returns>
    public static bool AreCoOccurring(List<List<string>> log, string act1, string act2)
    {
        foreach (List<string> trace in log)
        {
            if (trace.Contains(act1) && trace.Contains(act2))
                return true;
        }
        return false;
    }

    /// <summary>
    ///     Gets all activities from a given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities from the log</returns>
    public static HashSet<string> GetAllActivities(List<List<string>> log)
    {
        HashSet<string> result = new();
        foreach (List<string> trace in log)
            foreach (string activity in trace)
                result.Add(activity);
        return result;
    }
}

