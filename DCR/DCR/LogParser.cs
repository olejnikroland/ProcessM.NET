using System.Globalization;

namespace DCR;
using System;
using System.Collections.Generic;
using System.Linq;
using LogImport.Models;

public static class LogParser
{
    /// <summary>
    ///     Parses imported log into traces
    /// </summary>
    /// <param name="log">Imported log</param>
    /// <param name="id_column">Name of a id column in an imported .csv file</param>
    /// <param name="activity_column">Name of an activity column in an imported .csv file</param>
    /// <returns>List of traces</returns>
    public static List<List<string>> ParseToTraces(
        ImportedEventLog log,
        string id_column,
        string activity_column,
        string timestamp_column
    )
    {
        var headers = log.Headers.ToList();
        var rows = log.Rows;

        int caseIdIndex = headers.FindIndex(h => h.Equals(id_column, StringComparison.OrdinalIgnoreCase));
        int activityIndex = headers.FindIndex(h => h.Equals(activity_column, StringComparison.OrdinalIgnoreCase));
        int timestampIndex = headers.FindIndex(h => h.Equals(timestamp_column, StringComparison.OrdinalIgnoreCase));

        if (caseIdIndex == -1 || activityIndex == -1 || timestampIndex == -1)
            throw new Exception("Missing id, activity or timestamp columns");

        const string tsFormat = "dd/MM/yyyy HH:mm:ss";

        var grouped = rows
            .GroupBy(row => row[caseIdIndex])
            .Select(g => g
                .OrderBy(row => DateTime.ParseExact(row[timestampIndex], tsFormat, CultureInfo.InvariantCulture))
                .Select(row => row[activityIndex])
                .ToList()
            )
            .ToList();

        return grouped;
    }
    
    /// <summary>
    ///     Initializes DcrGraph from parsed traces
    /// </summary>
    /// <param name="traces">Parsed traces</param>
    /// <returns>Initialized DcrGraph and a dictionary of labeled events present in the graph</returns>
    public static (DcrGraph Graph, Dictionary<int, string> EventLabeling) InitializeFromLog(List<List<string>> traces)
    {
        HashSet<int> events = new HashSet<int>();
        HashSet<string> activities = new HashSet<string>();
        Dictionary<int, string> labeling = new Dictionary<int, string>();

        int id = 1;
        foreach (List<string> trace in traces)
        {
            foreach (string act in trace)
            {
                activities.Add(act);
                labeling[id] = act;
                events.Add(id++);
            }
        }

        (HashSet<int> Executed, HashSet<int> Pending, HashSet<int> Included) marking =
        (new HashSet<int>(), new HashSet<int>(), new HashSet<int>(events));

        return (new DcrGraph
        {
            Events = events,
            Activities = activities,
            Conditions = new(),
            Responses = new(),
            Includes = new(),
            Excludes = new(),
            Marking = marking,
            Labeling = labeling
        }, labeling);
    }
}

