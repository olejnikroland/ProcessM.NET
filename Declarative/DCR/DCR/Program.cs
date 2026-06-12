namespace DCR;

using Enums;
using LogImport.CsvImport;
using LogImport.Models;

/// <summary>
///     ProcessM.NET DisCoveR demo. Demonstrates process discovery,
///     threshold comparison, graph export, and conformance checking on a carsharing event log.
/// </summary>
internal class Program
{
    private static async Task Main()
    {
        //Set paths of input .csv file and output .dot file
        string demoMaterialsPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../DemoMaterials"));
        string csvPath = Path.Combine(demoMaterialsPath, "carsharing.csv");
        string dotPath = Path.Combine(demoMaterialsPath, "dcr_graph.dot");
        
        //Initialise importer and import of an event log from set path
        CsvImporter importer = new CsvImporter
        {
            HasHeaders = true,
            Delimiter = ','
        };

        ImportedEventLog imported = importer.LoadLog(csvPath);
        
        
        //Creation of an activity filter and a relation filter
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

        
        //Process discovery with activity and relation filter.
        //Applied filters will impact conformance checking in the next part of this demo.
        //Remove them for an unfiltered process discovery and conformance checking
        DcrGraph graph = DcrGraphBuilder.BuildFromImportedLog(imported, "UserId",
            "Action", "Timestamp", activityFilter, relationFilter);
        
        //Console write of discovered activities and relations
        Console.WriteLine("----------Process Discovery----------\n");
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
        
        //Comparison of how threshold change affects the output of process discovery
        Console.WriteLine("\n----------Process discovery threshold comparison----------");
        
        var graphFullThreshold = DcrGraphBuilder.BuildFromImportedLog(imported, "UserId", "Action",
            "Timestamp", threshold: 1);
        
        Console.WriteLine("\nGraph with threshold 1.0");
        Console.WriteLine($"Conditions: {graphFullThreshold.Conditions.Count}");
        Console.WriteLine($"Responses: {graphFullThreshold.Responses.Count}");
        Console.WriteLine($"Excludes: {graphFullThreshold.Excludes.Count}");
        Console.WriteLine($"Includes: {graphFullThreshold.Includes.Count}");

        var graphHalfThreshold = DcrGraphBuilder.BuildFromImportedLog(imported, "UserId", "Action",
            "Timestamp", threshold: 0.5);
        
        Console.WriteLine("\nGraph with threshold 0.5");
        Console.WriteLine($"Conditions: {graphHalfThreshold.Conditions.Count}");
        Console.WriteLine($"Responses: {graphHalfThreshold.Responses.Count}");
        Console.WriteLine($"Excludes: {graphHalfThreshold.Excludes.Count}");
        Console.WriteLine($"Includes: {graphHalfThreshold.Includes.Count}");
        
        //Export into a .dot file
        await DcrGraphVisualiser.ExportToDotAsync(graph, dotPath);
        
        //Creation of a parsed log to be used in conformance checking
        List<List<string>> traces = new List<List<string>>
        {
            new () { "ReservationCreated", "CheckIn", "CarOpen", "IgnitionStart", "Drive", "Drive", "Drive",
                "CheckOut" },
            new () { "ReservationCreated", "CheckIn", "CarOpen", "UserScan", "IgnitionStart", "Drive",
                "Drive", "Drive", "CheckOut" },
            new () { "ReservationCreated", "CheckIn", "CarOpen", "UserScan", "IgnitionStart", "Drive",
                "Drive", "Parked", "Payment", "Drive", "Checkout" },
            new () { "CheckIn", "ReservationCreated" }
        };
        
        //Conformance checking
        var (results, conformanceRate) = DcrConformanceChecker.CheckLog(graph, traces);
        
        //Console writes presenting the results of conformance checking. Traces are sorted by their fitness rate
        Console.WriteLine("\n----------Conformance Checking----------");
        
        foreach (var result in results)
        {
            string status = result.IsConformant ? "Conformant" : "Not conformant";

            Console.WriteLine($"\nTrace: {string.Join(", ", result.Trace)}");
            Console.WriteLine($"Status: {status}");
            Console.WriteLine($"Fitness: {result.Fitness:F2}");

            if (result is { IsConformant: false, Errors.Count: > 0 })
                Console.WriteLine($"{result.Errors[0]}");
        }

        Console.WriteLine($"\nConformance Rate: {conformanceRate:F2}%\n");

    }
}
