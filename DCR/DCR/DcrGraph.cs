using System;
using System.Collections.Generic;

/// <summary>
///     Represents a DCR graph
/// </summary>
public class DcrGraph
{
    public HashSet<int> Events { get; set; }
    public HashSet<string> Activities { get; set; }
    public Dictionary<int, string> Labeling { get; set; }
    public HashSet<(string, string)> Includes { get; set; }
    public HashSet<(string, string)> Excludes { get; set; }
    public HashSet<(string, string)> Conditions { get; set; }
    public HashSet<(string, string)> Responses { get; set; }
    public (HashSet<int> Executed, HashSet<int> Pending, HashSet<int> Included) Marking { get; set; }

    public DcrGraph()
    {
        Events = new HashSet<int>();
        Activities = new HashSet<string>();
        Labeling = new Dictionary<int, string>();
        Includes = new HashSet<(string, string)>();
        Excludes = new HashSet<(string, string)>();
        Conditions = new HashSet<(string, string)>();
        Responses = new HashSet<(string, string)>();
        Marking = (new HashSet<int>(), new HashSet<int>(), new HashSet<int>());
    }
}
