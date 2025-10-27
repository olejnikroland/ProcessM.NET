using System.Collections.Generic;

public static class Constraints
{
    /// <summary>
    ///     Discovers AtMostOne constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered AtMostOne relation</returns>
    public static HashSet<(string, string)> AtMostOne(List<List<string>> log)
    {
        HashSet<(string, string)> result = new HashSet<(string, string)>();
        Dictionary<string, int> activityCounts = new Dictionary<string, int>();

        foreach (List<string> trace in log)
        {
            HashSet<string> seen = new HashSet<string>();

            foreach (string activity in trace)
            {
                if (seen.Contains(activity))
                {
                    if (!activityCounts.ContainsKey(activity))
                        activityCounts[activity] = 0;

                    activityCounts[activity]++;
                }
                else
                {
                    seen.Add(activity);
                }
            }
        }

        foreach (string activity in GetAllActivities(log))
        {
            if (!activityCounts.ContainsKey(activity))
                result.Add((activity, activity));
        }

        return result;
    }
    
    /// <summary>
    ///     Discovers Precedence constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered Precedence relation</returns>
    public static HashSet<(string, string)> Precedence(List<List<string>> log)
    {
        HashSet<(string, string)> result = new HashSet<(string, string)>();
        List<string> allActivities = GetAllActivities(log).ToList();

        foreach (string a in allActivities)
        {
            foreach (string b in allActivities)
            {
                if (a == b) continue;

                bool valid = true;

                foreach (List<string> trace in log)
                {
                    if (trace.Contains(b))
                    {
                        int indexB = trace.IndexOf(b);
                        bool aBeforeB = trace.Take(indexB).Contains(a);

                        if (!aBeforeB)
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (valid)
                {
                    result.Add((a, b));
                }
            }
        }

        return result;
    }
    
    /// <summary>
    ///     Discovers Response constraints from given log
    /// </summary>
    /// <param name="log">Event log</param>
    /// <returns>Set of activities with discovered Response relation</returns>
    public static HashSet<(string, string)> Response(List<List<string>> log)
    {
        HashSet<(string, string)> result = new HashSet<(string, string)>();
        HashSet<string> all = GetAllActivities(log);

        foreach (string a in all)
        {
            foreach (string b in all)
            {
                if (a == b) continue;
                bool alwaysFollowed = true;

                foreach (List<string> trace in log)
                {
                    List<int> aIndexes = new List<int>();
                    bool bFoundAfter = false;

                    for (int i = 0; i < trace.Count; i++)
                    {
                        if (trace[i] == a)
                            aIndexes.Add(i);
                    }

                    foreach (int aIndex in aIndexes)
                    {
                        bFoundAfter = false;
                        for (int j = aIndex + 1; j < trace.Count; j++)
                        {
                            if (trace[j] == b)
                            {
                                bFoundAfter = true;
                                break;
                            }
                        }

                        if (!bFoundAfter)
                        {
                            alwaysFollowed = false;
                            break;
                        }
                    }

                    if (!alwaysFollowed) break;
                }

                if (alwaysFollowed)
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
    public static HashSet<(string, string)> InferredConditions(List<List<string>> log)
    {
        HashSet<(string, string)> result = new HashSet<(string, string)>();
        HashSet<string> all = GetAllActivities(log);

        foreach (string a in all)
        {
            foreach (string b in all)
            {
                if (a == b) continue;

                bool bAlwaysAfterA = true;

                foreach (List<string> trace in log)
                {
                    List<int> aIndexes = new List<int>();
                    List<int> bIndexes = new List<int>();

                    for (int i = 0; i < trace.Count; i++)
                    {
                        if (trace[i] == a) aIndexes.Add(i);
                        if (trace[i] == b) bIndexes.Add(i);
                    }

                    foreach (int bIndex in bIndexes)
                    {
                        bool hasA = aIndexes.Any(aIndex => aIndex < bIndex);
                        if (!hasA)
                        {
                            bAlwaysAfterA = false;
                            break;
                        }
                    }

                    if (!bAlwaysAfterA) break;
                }

                if (bAlwaysAfterA)
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

