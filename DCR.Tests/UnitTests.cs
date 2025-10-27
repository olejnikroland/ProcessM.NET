using System;
using System.Collections.Generic;
using System.Linq;
using DCR;
using LogImport.Models;
using Xunit;

public class ConstraintsTests
{
    private static List<List<string>> SampleLog => new()
    {
        new() { "Start", "Login", "Browse", "Purchase", "End" },
        new() { "Start", "Login", "Browse", "Return", "Return", "End" },
        new() { "Start", "Browse", "Purchase", "Return", "End" },
        new() { "Start", "Login", "End" },
    };

    [Fact]
    public void AtMostOneTest()
    {
        var result = Constraints.AtMostOne(SampleLog);
        Assert.Contains(("Start", "Start"), result);
        Assert.Contains(("Login", "Login"), result);
        Assert.Contains(("Browse", "Browse"), result);
        Assert.Contains(("Purchase", "Purchase"), result);
        Assert.DoesNotContain(("Return", "Return"), result);
        Assert.Contains(("End", "End"), result);
    }

    [Fact]
    public void PrecedenceTest()
    {
        var result = Constraints.Precedence(SampleLog);
        Assert.Contains(("Start", "Login"), result);
        Assert.Contains(("Start", "End"), result);
        Assert.Contains(("Browse", "Purchase"), result);
        Assert.DoesNotContain(("Login", "Browse"), result);
    }

    [Fact]
    public void ResponseTest()
    {
        var result = Constraints.Response(SampleLog);
        Assert.Contains(("Start", "End"), result);
        Assert.Contains(("Browse", "End"), result);
        Assert.Contains(("Login", "End"), result);
        Assert.Contains(("Purchase", "End"), result);
        Assert.Contains(("Return", "End"), result);
        Assert.DoesNotContain(("Login", "Browse"), result);
    }

    [Fact]
    public void MutuallyExclusiveTest()
    {
        var log = new List<List<string>>
        {
            new() { "Start", "Purchase", "End" },
            new() { "Start", "Browse", "End" },
        };
        var result = Constraints.MutuallyExclusive(log);
        Assert.Contains(("Purchase", "Browse"), result);
        Assert.DoesNotContain(("Start", "End"), result);
    }

    [Fact]
    public void NotChainSuccessionTest()
    {
        var result = Constraints.NotChainSuccession(SampleLog);
        Assert.Contains(("Login", "Purchase"), result);
        Assert.DoesNotContain(("Browse", "Purchase"), result);
        Assert.DoesNotContain(("Browse", "Return"), result);
    }

    [Fact]
    public void InferredConditionsTest()
    {
        var result = Constraints.InferredConditions(SampleLog);
        Assert.Contains(("Start", "Login"), result);
        Assert.DoesNotContain(("Login", "Browse"), result);
        Assert.Contains(("Start", "End"), result);
        Assert.Contains(("Browse", "Purchase"), result);
    }
}

public class DcrGraphBuilderTests
{
    [Fact]
    public void BuildTest()
    {
        var log = new List<List<string>>
        {
            new() { "Start", "End" },
            new() { "Start", "Login", "End" }
        };
        var graph = DcrGraphBuilder.Build(log);

        Assert.Contains("Start", graph.Activities);
        Assert.Contains("Login", graph.Activities);
        Assert.Contains("End", graph.Activities);
        Assert.Contains(("Start", "End"), graph.Responses);
        Assert.Contains(("Start", "Start"), graph.Excludes);
        Assert.Contains(("End", "End"), graph.Excludes);
    }
}

public class DcrGraphOptimizerTests
{
    [Fact]
    public void RemoveTransitiveResponsesTest()
    {
        var graph = new DcrGraph();
        graph.Responses.Add(("Browse", "Purchase"));
        graph.Responses.Add(("Purchase", "Return"));
        graph.Responses.Add(("Browse", "Return"));

        DcrGraphOptimizer.RemoveTransitiveResponses(graph);

        Assert.DoesNotContain(("Browse", "Return"), graph.Responses);
    }

    [Fact]
    public void RemoveRedundantExcludesTest()
    {
        var graph = new DcrGraph();
        graph.Excludes.Add(("Browse", "Purchase"));
        graph.Excludes.Add(("Login", "Purchase"));
        var altPrec = new HashSet<(string, string)> { ("Login", "Browse") };

        DcrGraphOptimizer.RemoveTransitiveExcludes(graph, altPrec);

        Assert.DoesNotContain(("Browse", "Purchase"), graph.Excludes);
    }
}

public class LogParserTests
{
    [Fact]
    public void ParseToTracesTest()
    {
        var imported = new ImportedEventLog(
            new List<string[]> {
                new[] {"1", "Start"},
                new[] {"1", "End"},
                new[] {"2", "Start"},
            },
            new[] {"case_id", "activity"}
        );

        var traces = LogParser.ParseToTraces(imported, "case_id", "activity");
        Assert.Equal(2, traces.Count);
        Assert.Equal(new[] { "Start", "End" }, traces[0]);
        Assert.Equal(new[] { "Start" }, traces[1]);
    }

    [Fact]
    public void InitializeFromLogTest()
    {
        var log = new List<List<string>>
        {
            new() { "Login", "Purchase" },
            new() { "Browse" }
        };

        var (graph, labeling) = LogParser.InitializeFromLog(log);

        Assert.Equal(3, graph.Events.Count);
        Assert.Equal(3, labeling.Count);
        Assert.Contains("Login", graph.Activities);
        Assert.Contains("Purchase", graph.Activities);
        Assert.Contains("Browse", graph.Activities);
    }

}

public class ConformanceCheckingTests()
{
    [Fact]
    public void CorrectConformantTraceTest()
    {
        var graph = new DcrGraph
        {
            Activities = new HashSet<string> { "Start", "Login", "End" },
            Conditions = new HashSet<(string, string)> { ("Start", "Login") },
            Responses = new HashSet<(string, string)> { ("Login", "End") },
            Excludes = new HashSet<(string, string)>(),
            Includes = new HashSet<(string, string)>()
        };

        var trace = new List<string> { "Start", "Login", "End" };
        var result = DcrConformanceChecker.CheckTrace(graph, trace);

        Assert.True(result.IsConformant);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void MissingConditionTest()
    {
        var graph = new DcrGraph
        {
            Activities = new HashSet<string> { "Start", "End" },
            Conditions = new HashSet<(string, string)> { ("Start", "End") },
            Responses = new HashSet<(string, string)>(),
            Excludes = new HashSet<(string, string)>(),
            Includes = new HashSet<(string, string)>()
        };

        var trace = new List<string> { "End" };
        var result = DcrConformanceChecker.CheckTrace(graph, trace);

        Assert.False(result.IsConformant);
        Assert.Contains(result.Errors, e => e.Contains("Condition failed"));
    }

    [Fact]
    public void ExcludedActivityTest()
    {
        var graph = new DcrGraph
        {
            Activities = new HashSet<string> { "Start", "End" },
            Conditions = new HashSet<(string, string)>(),
            Responses = new HashSet<(string, string)>(),
            Excludes = new HashSet<(string, string)> { ("Start", "End") },
            Includes = new HashSet<(string, string)>()
        };

        var trace = new List<string> { "Start", "End" };
        var result = DcrConformanceChecker.CheckTrace(graph, trace);

        Assert.False(result.IsConformant);
        Assert.Contains(result.Errors, e => e.Contains("not included"));
    }

    [Fact]
    public void PendingResponseNotHandledTest()
    {
        var graph = new DcrGraph
        {
            Activities = new HashSet<string> { "Start", "End" },
            Conditions = new HashSet<(string, string)>(),
            Responses = new HashSet<(string, string)> { ("Start", "End") },
            Excludes = new HashSet<(string, string)>(),
            Includes = new HashSet<(string, string)>()
        };

        var trace = new List<string> { "Start" };
        var result = DcrConformanceChecker.CheckTrace(graph, trace);

        Assert.False(result.IsConformant);
        Assert.Contains(result.Errors, e => e.Contains("Pending response"));
    }
}