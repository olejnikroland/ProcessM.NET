using DotNetGraph.Compilation;
using DotNetGraph.Extensions;
using DotNetGraph.Core;

namespace DCR;

public static class DcrGraphVisualiser
{
    public static async Task ExportToDotAsync(DcrGraph graph, string filePath)
    {
        var dotGraph = new DotGraph().WithIdentifier("DCR_Graph").Directed();

        foreach (var activity in graph.Activities)
        {
            var node = new DotNode()
                .WithIdentifier(activity)
                .WithLabel(activity)
                .WithShape(DotNodeShape.Ellipse)
                .WithStyle(DotNodeStyle.Filled)
                .WithFillColor(DotColor.LightGray)
                .WithFontColor(DotColor.Black);

            dotGraph.Add(node);
        }

        foreach (var (a, b) in graph.Conditions)
        {
            dotGraph.Add(new DotEdge().From(a).To(b)
                .WithLabel("condition")
                .WithColor(DotColor.Blue)
                .WithStyle(DotEdgeStyle.Solid));
        }

        foreach (var (a, b) in graph.Responses)
        {
            dotGraph.Add(new DotEdge().From(a).To(b)
                .WithLabel("response")
                .WithColor(DotColor.Green)
                .WithStyle(DotEdgeStyle.Dashed));
        }

        foreach (var (a, b) in graph.Excludes)
        {
            dotGraph.Add(new DotEdge().From(a).To(b)
                .WithLabel("exclude")
                .WithColor(DotColor.Red)
                .WithStyle(DotEdgeStyle.Dotted));
        }

        foreach (var (a, b) in graph.Includes)
        {
            dotGraph.Add(new DotEdge().From(a).To(b)
                .WithLabel("include")
                .WithColor(DotColor.Black)
                .WithStyle(DotEdgeStyle.Bold));
        }

        await using var writer = new StringWriter();
        var context = new CompilationContext(writer, new CompilationOptions());
        await dotGraph.CompileAsync(context);

        var dot = writer.GetStringBuilder().ToString();
        File.WriteAllText(filePath, dot);
    }
}
