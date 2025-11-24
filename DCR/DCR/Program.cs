using DCR.Enums;

namespace DCR;
using LogImport.CsvImport;
using LogImport.Models;

class Program
{
    static async Task Main()
    {
        string filePath = "..//..//..//carsharing.csv";
        CsvImporter importer = new CsvImporter
        {
            HasHeaders = true,
            Delimiter = ','
        };

        ImportedEventLog imported = importer.LoadLog(filePath);
        

        var activityFilter = new HashSet<string>
        {
            "ReservationCreated",
            "CheckIn",
        };
        
        var relationFilter = new HashSet<RelationType>
        {
            RelationType.Includes,
            RelationType.Conditions,
        };

        
        //DcrGraph graph = DcrGraphBuilder.BuildFromImportedLog(imported, "UserId", "Action", "Timestamp", activityFilter, relationFilter);
        DcrGraph graph = DcrGraphBuilder.BuildFromImportedLog(imported, "UserId", "Action", "Timestamp");

        Console.WriteLine("Activities:");
        foreach (var a in graph.Activities)
            Console.WriteLine(a);

        Console.WriteLine("\nExcludes:");
        foreach (var (a, b) in graph.Excludes)
            Console.WriteLine($"Exclude({a}, {b})");

        Console.WriteLine("\nIncludes:");
        foreach (var (a, b) in graph.Includes)
            Console.WriteLine($"Include({a}, {b})");

        Console.WriteLine("\nConditions:");
        foreach (var (a, b) in graph.Conditions)
            Console.WriteLine($"Condition({a}, {b})");

        Console.WriteLine("\nResponses:");
        foreach (var (a, b) in graph.Responses)
            Console.WriteLine($"Response({a}, {b})");
        
        List<List<string>> traces = new List<List<string>>
        {
            new List<string> { "ReservationCreated", "CheckIn", "CarOpen", "IgnitionStart", "Drive", "Drive", "Drive", "CheckOut" },
            new List<string> { "ReservationCreated", "CheckIn", "CarOpen", "UserScan", "IgnitionStart", "Drive", "Drive", "Drive", "CheckOut" },
            new List<string> { "ReservationCreated", "CheckIn", "CarOpen", "UserScan", "IgnitionStart", "Drive", "Drive", "Parked", "Payment", "Drive", "Checkout" },
            new List<string> { "CheckIn", "ReservationCreated" }
        };

        var (results, conformanceRate) = DcrConformanceChecker.CheckLog(graph, traces);
        
        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            string status = result.IsConformant ? "Conformant" : "Not conformant";

            Console.WriteLine($"\nTrace {i + 1}: {string.Join(", ", result.Trace)}");
            Console.WriteLine($"Status: {status}");

            if (!result.IsConformant && result.Errors.Count > 0)
            {
                Console.WriteLine($"{result.Errors[0]}");
            }
        }

        Console.WriteLine($"\nConformance Rate: {conformanceRate:F2}%\n");
        
        foreach (var result in results)
        {
            Console.WriteLine($"Trace: {string.Join(", ", result.Trace)}");
            Console.WriteLine($"Fitness: {result.Fitness:F2}");
            Console.WriteLine($"Conformant: {result.IsConformant}");
            Console.WriteLine();
        }
        
        var graph2 = DcrGraphBuilder.BuildFromImportedLog(imported, "UserId", "Action", "Timestamp", threshold: 1);
        
        Console.WriteLine("\nGraph with threshold = 1.0");
        Console.WriteLine($"Conditions: {graph2.Conditions.Count}");
        Console.WriteLine($"Responses: {graph2.Responses.Count}");
        Console.WriteLine($"Excludes: {graph2.Excludes.Count}");
        Console.WriteLine($"Includes: {graph2.Includes.Count}");

        var graph3 = DcrGraphBuilder.BuildFromImportedLog(imported, "UserId", "Action", "Timestamp", threshold: 0.5);
        
        Console.WriteLine("\nGraph with threshold = 0.5");
        Console.WriteLine($"Conditions: {graph3.Conditions.Count}");
        Console.WriteLine($"Responses: {graph3.Responses.Count}");
        Console.WriteLine($"Excludes: {graph3.Excludes.Count}");
        Console.WriteLine($"Includes: {graph3.Includes.Count}");
        
        string outputPath = "..//..//..//dcr_graph.dot";
        await DcrGraphVisualiser.ExportToDotAsync(graph, outputPath);

    }
}
