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

        DcrGraph graph = DcrGraphBuilder.BuildFromImportedLog(imported, "UserId", "Action");

        Console.WriteLine("Activities:");
        foreach (var a in graph.Activities)
            Console.WriteLine(a);

        Console.WriteLine("\nExcludes:");
        foreach (var (a, b) in graph.Excludes)
            Console.WriteLine($"{a} disables {b}");

        Console.WriteLine("\nIncludes:");
        foreach (var (a, b) in graph.Includes)
            Console.WriteLine($"{a} is directly followed by {b}");

        Console.WriteLine("\nConditions:");
        foreach (var (a, b) in graph.Conditions)
            Console.WriteLine($"{a} always happens before {b}");

        Console.WriteLine("\nResponses:");
        foreach (var (a, b) in graph.Responses)
            Console.WriteLine($"If {a} happens, {b} happens too");
        
        List<List<string>> traces = new List<List<string>>
        {
            new List<string> { "ReservationCreated", "CheckIn", "CarOpen", "IgnitionStart", "Drive", "Drive", "Drive", "Checkout" },
            new List<string> { "ReservationCreated", "CheckIn", "CarOpen", "UserScan", "IgnitionStart", "Drive", "Drive", "Drive", "CheckOut" },
            new List<string> { "ReservationCreated", "CheckIn", "CarOpen", "UserScan", "IgnitionStart", "Drive", "Drive", "Parked", "Payment", "Drive", "Checkout" },
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

        Console.WriteLine($"\nConformance Rate: {conformanceRate:F2}%");
        
        
        string outputPath = "..//..//..//dcr_graph.dot";
        await DcrGraphVisualiser.ExportToDotAsync(graph, outputPath);

    }
}
