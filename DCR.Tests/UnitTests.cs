using System.Globalization;
using DCR;
using DCR.Enums;
using DeclarativePM.Lib.Models.LogModels;
using Xunit;

namespace DCR.Tests;

public class ConstraintsTests
{
    public class AtMostOneTests
    {
        [Fact]
        public void Test1()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B", "Activity C" }
            };

            var result = Constraints.AtMostOne(log);

            Assert.Equal(new HashSet<(string, string)>
            {
                ("Activity A", "Activity A"), ("Activity B", "Activity B"), ("Activity C", "Activity C")
            }, result);
        }

        [Fact]
        public void Test2()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity A", "Activity B", "Activity C", "Activity C", "Activity C" }
            };

            var result = Constraints.AtMostOne(log);

            Assert.Equal(new HashSet<(string, string)>
            {
                ("Activity B", "Activity B")
            }, result);
        }

        [Fact]
        public void Test3()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity A", "Activity A", "Activity B" }
            };

            var result = Constraints.AtMostOne(log, 1.0);

            Assert.Equal(new HashSet<(string, string)>
            {
                ("Activity B", "Activity B")
            }, result);
        }

        [Fact]
        public void Test4()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity A", "Activity A", "Activity B" }
            };

            var result = Constraints.AtMostOne(log, 0.5);

            Assert.Equal(new HashSet<(string, string)>
            {
                ("Activity A", "Activity A"), ("Activity B", "Activity B")
            }, result);
        }

        [Fact]
        public void Test5()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity A", "Activity B" }
            };

            var result = Constraints.AtMostOne(log, 0.0);

            Assert.Equal(new HashSet<(string, string)>
            {
                ("Activity A", "Activity A"), ("Activity B", "Activity B")
            }, result);
        }

        [Fact]
        public void Test117()
        {
            var log = new List<List<string>>();

            var result = Constraints.AtMostOne(log);

            Assert.Empty(result);
        }

        [Fact]
        public void Test6()
        {
            var log = new List<List<string>>
            {
                new(),
                new()
            };

            var result = Constraints.AtMostOne(log);

            Assert.Empty(result);
        }

        [Fact]
        public void Test7()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A" },
                new() { "Activity A", "Activity A" },
                new() { }
            };

            var result = Constraints.AtMostOne(log, 2.0 / 3.0);

            Assert.Equal(new HashSet<(string, string)>
            {
                ("Activity A", "Activity A")
            }, result);
        }

        [Fact]
        public void Test8()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity A", "Activity A", "Activity C" },
                new() { "Activity B", "Activity C" }
            };

            var result = Constraints.AtMostOne(log);

            Assert.Equal(new HashSet<(string, string)>
            {
                ("Activity B", "Activity B"), ("Activity C", "Activity C")
            }, result);
        }
    }

    public class PrecedenceTests
    {
        [Fact]
        public void Test9()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" }
            };

            var result = Constraints.Precedence(log);

            Assert.Contains(("Activity A", "Activity B"), result);
            Assert.DoesNotContain(("Activity B", "Activity A"), result);
        }

        [Fact]
        public void Test10()
        {
            var log = new List<List<string>>
            {
                new() { "Activity B", "Activity A" }
            };

            var result = Constraints.Precedence(log);

            Assert.DoesNotContain(("Activity A", "Activity B"), result);
            Assert.Contains(("Activity B", "Activity A"), result);
        }

        [Fact]
        public void Test11()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity G", "Activity B" },
                new() { "Activity A", "Activity B" },
                new() { "Activity A", "Activity H", "Activity I", "Activity B" }
            };

            var result = Constraints.Precedence(log);

            Assert.Contains(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test12()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity B", "Activity A" }
            };

            var result = Constraints.Precedence(log, 1.0);

            Assert.DoesNotContain(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test13()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity B", "Activity A" }
            };

            var result = Constraints.Precedence(log, 0.5);

            Assert.Contains(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test14()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity G", "Activity B" },
                new() { "Activity A", "Activity H", "Activity I" }
            };

            var result = Constraints.Precedence(log);

            Assert.Contains(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test153()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B", "Activity C" }
            };

            var result = Constraints.Precedence(log);

            Assert.Contains(("Activity A", "Activity B"), result);
            Assert.Contains(("Activity A", "Activity C"), result);
            Assert.Contains(("Activity B", "Activity C"), result);
            Assert.DoesNotContain(("Activity C", "Activity A"), result);
        }

        [Fact]
        public void Test15()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" }
            };

            var result = Constraints.Precedence(log, 1.5);

            Assert.Empty(result);
        }
    }

    public class ResponseTests
    {
        [Fact]
        public void Test16()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity G", "Activity A", "Activity H", "Activity B" }
            };

            var result = Constraints.Response(log);

            Assert.Contains(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test17()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity G" },
                new() { "Activity A", "Activity H", "Activity I" }
            };

            var result = Constraints.Response(log);

            Assert.DoesNotContain(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test18()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity G", "Activity B" },
                new() { "Activity B", "Activity H" }
            };

            var result = Constraints.Response(log);

            Assert.Contains(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test19()
        {
            var log = new List<List<string>>
            {
                new() { "Activity B", "Activity A", "Activity G", "Activity B" }
            };

            var result = Constraints.Response(log);

            Assert.Contains(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test20()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity A", "Activity G" } 
            };

            var result = Constraints.Response(log, threshold: 0.75);

            Assert.DoesNotContain(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test21()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity A", "Activity G" } 
            };

            var result = Constraints.Response(log, threshold: 0.4);

            Assert.Contains(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test22()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity G", "Activity A", "Activity H", "Activity B" }
            };

            var result = Constraints.Response(log);

            Assert.Contains(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test23()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B", "Activity A", "Activity G" }
            };

            var result = Constraints.Response(log);

            Assert.DoesNotContain(("Activity A", "Activity B"), result);
        }
    }

    public class ChainPrecedenceTests
    {
        [Fact]
        public void Test24()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity C", "Activity A", "Activity B" }
            };

            var result = Constraints.ChainPrecedence(log);

            Assert.DoesNotContain(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test25()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity G", "Activity B" }
            };

            var result = Constraints.ChainPrecedence(log);

            Assert.DoesNotContain(("Activity A", "Activity B"), result);
            Assert.DoesNotContain(("Activity G", "Activity B"), result);
        }

        [Fact]
        public void Test26()
        {
            var log = new List<List<string>>
            {
                new() { "Activity B", "Activity A" },
                new() { "Activity A", "Activity B" }
            };

            var result = Constraints.ChainPrecedence(log);

            Assert.Empty(result);
        }

        [Fact]
        public void Test27()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B", "Activity C" },
                new() { "Activity A", "Activity B", "Activity C" }
            };

            var result = Constraints.ChainPrecedence(log);

            Assert.Empty(result);
        }

        [Fact]
        public void Test28()
        {
            var log = new List<List<string>>
            {
                new() { "Activity B", "Activity A" },
                new() { "Activity B", "Activity C" }
            };

            var result = Constraints.ChainPrecedence(log);

            Assert.DoesNotContain(result, r => r.Item2 == "Activity B");
        }

        [Fact]
        public void Test29()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity G" },
                new() { "Activity H", "Activity I" }
            };

            var result = Constraints.ChainPrecedence(log);

            Assert.DoesNotContain(result, r => r.Item1 == "Activity B" || r.Item2 == "Activity B");
        }
    }

    public class NotChainSuccessionTests
    {
        [Fact]
        public void Test30()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B", "Activity C" }
            };

            var result = Constraints.NotChainSuccession(log);

            Assert.DoesNotContain(("Activity A", "Activity B"), result);
            Assert.DoesNotContain(("Activity B", "Activity C"), result);
        }

        [Fact]
        public void Test31()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity G", "Activity B" }
            };

            var result = Constraints.NotChainSuccession(log);

            Assert.Contains(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test33()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity C", "Activity A" }
            };

            var result = Constraints.NotChainSuccession(log);

            Assert.Contains(("Activity B", "Activity A"), result);
            Assert.Contains(("Activity C", "Activity B"), result);
        }

        [Fact]
        public void Test34()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity A", "Activity C" }
            };

            var result = Constraints.NotChainSuccession(log);

            Assert.DoesNotContain(("Activity A", "Activity B"), result);
            Assert.DoesNotContain(("Activity A", "Activity C"), result);
        }

        [Fact]
        public void Test35()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity A" }
            };

            var result = Constraints.NotChainSuccession(log);

            Assert.DoesNotContain(("Activity A", "Activity A"), result);
        }

        [Fact]
        public void Test36()
        {
            var log = new List<List<string>>
            {
                new() { "Activity G", "Activity H" }
            };

            var result = Constraints.NotChainSuccession(log);

            Assert.Contains(("Activity H", "Activity G"), result);
            Assert.DoesNotContain(("Activity G", "Activity H"), result);
        }
    }

    public class AlternatePrecedenceTests
    {
        [Fact]
        public void Test37()
        {
            var log = new List<List<string>>();
            var result = Constraints.AlternatePrecedence(log);
            Assert.Empty(result);
        }

        [Fact]
        public void Test38()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A" },
                new() { "Activity B" }
            };

            var result = Constraints.AlternatePrecedence(log);
            Assert.Empty(result);
        }

        [Fact]
        public void Test39()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B", "Activity C" }
            };

            var result = Constraints.AlternatePrecedence(log);

            Assert.Contains(("Activity A", "Activity B"), result);
            Assert.Contains(("Activity B", "Activity C"), result);
            Assert.DoesNotContain(("Activity A", "Activity C"), result);
        }

        [Fact]
        public void Test40()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity A", "Activity B" }
            };

            var result = Constraints.AlternatePrecedence(log);

            Assert.Single(result);
            Assert.Contains(("Activity A", "Activity B"), result);
        }

        [Fact]
        public void Test41()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B", "Activity C" },
                new() { "Activity B", "Activity C", "Activity D" }
            };

            var result = Constraints.AlternatePrecedence(log);
            
            Assert.Contains(("Activity B", "Activity C"), result);
        }

        [Fact]
        public void Test42()
        {
            var log = new List<List<string>>
            {
                new() { "Activity G", "Activity H", "Activity I" },
                new() { "Activity G", "Activity J", "Activity I" }
            };

            var result = Constraints.AlternatePrecedence(log);

            Assert.Contains(("Activity G", "Activity H"), result);
            Assert.Contains(("Activity G", "Activity J"), result);
            Assert.DoesNotContain(("Activity H", "Activity I"), result);
            Assert.DoesNotContain(("Activity J", "Activity I"), result);
        }
    }

    public class DeterminePredecessorSuccessor
    {
        [Fact]
        public void Test87()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B", "Activity C" }
            };

            var (pred, succ) = Constraints.DeterminePredecessorSuccessor(log);

            Assert.Equal(new HashSet<string>(), pred["Activity A"]);
            Assert.Equal(new HashSet<string> { "Activity A" }, pred["Activity B"]);
            Assert.Equal(new HashSet<string> { "Activity A", "Activity B" }, pred["Activity C"]);

            Assert.Equal(new HashSet<string> { "Activity B", "Activity C" }, succ["Activity A"]);
            Assert.Equal(new HashSet<string> { "Activity C" }, succ["Activity B"]);
            Assert.Equal(new HashSet<string>(), succ["Activity C"]);
        }

        [Fact]
        public void Test86()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B", "Activity C" },
                new() { "Activity B", "Activity D" }
            };

            var (pred, succ) = Constraints.DeterminePredecessorSuccessor(log);

            Assert.Equal(new HashSet<string>(), pred["Activity A"]);
            Assert.Equal(new HashSet<string> { "Activity A" }, pred["Activity B"]);
            Assert.Equal(new HashSet<string> { "Activity A", "Activity B" }, pred["Activity C"]);
            Assert.Equal(new HashSet<string> { "Activity B" }, pred["Activity D"]);

            Assert.Equal(new HashSet<string> { "Activity B", "Activity C" }, succ["Activity A"]);
            Assert.Equal(new HashSet<string> { "Activity C", "Activity D" }, succ["Activity B"]);
            Assert.Equal(new HashSet<string>(), succ["Activity C"]);
            Assert.Equal(new HashSet<string>(), succ["Activity D"]);
        }

        [Fact]
        public void Test88()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B", "Activity A", "Activity C" }
            };

            var (pred, succ) = Constraints.DeterminePredecessorSuccessor(log);

            Assert.Equal(new HashSet<string> { "Activity A", "Activity B" }, pred["Activity A"]);
            Assert.Equal(new HashSet<string> { "Activity A" }, pred["Activity B"]);
            Assert.Equal(new HashSet<string> { "Activity A", "Activity B" }, pred["Activity C"]);

            Assert.Equal(new HashSet<string> { "Activity B", "Activity A", "Activity C" }, succ["Activity A"]);
            Assert.Equal(new HashSet<string> { "Activity A", "Activity C" }, succ["Activity B"]);
            Assert.Equal(new HashSet<string>(), succ["Activity C"]);
        }
    }

    public class AreCoOccurringTests
    {
        [Fact]
        public void Test43()
        {
            var log = new List<List<string>>();
            Assert.False(Constraints.AreCoOccurring(log, "Activity A", "Activity B"));
        }

        [Fact]
        public void Test44()
        {
            var log = new List<List<string>>
            {
                new() { "Activity G", "Activity H" }
            };

            Assert.False(Constraints.AreCoOccurring(log, "Activity A", "Activity B"));
        }

        [Fact]
        public void Test45()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" }
            };

            Assert.True(Constraints.AreCoOccurring(log, "Activity A", "Activity B"));
        }
    }

    public class GetAllActivitiesTests
    {
        [Fact]
        public void Test46()
        {
            var log = new List<List<string>>();
            var result = Constraints.GetAllActivities(log);
            Assert.Empty(result);
        }

        [Fact]
        public void Test47()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity B", "Activity C", "Activity A" }
            };

            var result = Constraints.GetAllActivities(log);

            Assert.Equal(new HashSet<string> { "Activity A", "Activity B", "Activity C" }, result);
        }
    }
}

public class DcrOptimizerTests
{
    public class RemoveTransitiveResponsesTests
    {
        [Fact]
        public void Test48()
        {
            var graph = new DcrGraph
            {
                Responses = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B"),
                    ("Activity B", "Activity C"),
                    ("Activity A", "Activity C")
                }
            };

            DcrGraphOptimizer.RemoveTransitiveResponses(graph);

            Assert.DoesNotContain(("Activity A", "Activity C"), graph.Responses);
            Assert.Contains(("Activity A", "Activity B"), graph.Responses);
            Assert.Contains(("Activity B", "Activity C"), graph.Responses);
        }

        [Fact]
        public void Test49()
        {
            var graph = new DcrGraph
            {
                Responses = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B"),
                    ("Activity B", "Activity C")
                }
            };

            DcrGraphOptimizer.RemoveTransitiveResponses(graph);

            Assert.Contains(("Activity A", "Activity B"), graph.Responses);
            Assert.Contains(("Activity B", "Activity C"), graph.Responses);
            Assert.Equal(2, graph.Responses.Count);
        }

        [Fact]
        public void Test50()
        {
            var graph = new DcrGraph
            {
                Responses = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B"),
                    ("Activity B", "Activity C"),
                    ("Activity C", "Activity D"),
                    ("Activity A", "Activity C"),
                    ("Activity A", "Activity D"),
                    ("Activity B", "Activity D")
                }
            };

            DcrGraphOptimizer.RemoveTransitiveResponses(graph);

            Assert.DoesNotContain(("Activity A", "Activity C"), graph.Responses);
            Assert.DoesNotContain(("Activity A", "Activity D"), graph.Responses);
            Assert.DoesNotContain(("Activity B", "Activity D"), graph.Responses);

            Assert.Contains(("Activity A", "Activity B"), graph.Responses);
            Assert.Contains(("Activity B", "Activity C"), graph.Responses);
            Assert.Contains(("Activity C", "Activity D"), graph.Responses);
        }

        [Fact]
        public void Test51()
        {
            var graph = new DcrGraph
            {
                Responses = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B"),
                    ("Activity G", "Activity H")
                }
            };

            DcrGraphOptimizer.RemoveTransitiveResponses(graph);

            Assert.Contains(("Activity A", "Activity B"), graph.Responses);
            Assert.Contains(("Activity G", "Activity H"), graph.Responses);
            Assert.Equal(2, graph.Responses.Count);
        }

        [Fact]
        public void Test52()
        {
            var graph = new DcrGraph
            {
                Responses = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B"),
                    ("Activity C", "Activity D"),
                    ("Activity E", "Activity F")
                }
            };

            DcrGraphOptimizer.RemoveTransitiveResponses(graph);

            Assert.Equal(3, graph.Responses.Count);
        }
    }
    public class RemoveTransitiveConditionsTests
    {
    [Fact]
    public void Test53()
    {
        var graph = new DcrGraph
        {
            Conditions = new HashSet<(string, string)>
            {
                ("Activity A", "Activity B"),
                ("Activity B", "Activity C"),
                ("Activity A", "Activity C")
            }
        };

        DcrGraphOptimizer.RemoveTransitiveConditions(graph);

        Assert.DoesNotContain(("Activity A", "Activity C"), graph.Conditions);
        Assert.Contains(("Activity A", "Activity B"), graph.Conditions);
        Assert.Contains(("Activity B", "Activity C"), graph.Conditions);
    }

    [Fact]
    public void Test54()
    {
        var graph = new DcrGraph
        {
            Conditions = new HashSet<(string, string)>
            {
                ("Activity A", "Activity B"),
                ("Activity B", "Activity C")
            }
        };

        DcrGraphOptimizer.RemoveTransitiveConditions(graph);

        Assert.Contains(("Activity A", "Activity B"), graph.Conditions);
        Assert.Contains(("Activity B", "Activity C"), graph.Conditions);
        Assert.Equal(2, graph.Conditions.Count);
    }

    [Fact]
    public void Test55()
    {
        var graph = new DcrGraph
        {
            Conditions = new HashSet<(string, string)>
            {
                ("Activity A", "Activity B"),
                ("Activity B", "Activity C"),
                ("Activity C", "Activity D"),
                ("Activity A", "Activity C"),
                ("Activity A", "Activity D"),
                ("Activity B", "Activity D")
            }
        };

        DcrGraphOptimizer.RemoveTransitiveConditions(graph);

        Assert.DoesNotContain(("Activity A", "Activity C"), graph.Conditions);
        Assert.DoesNotContain(("Activity A", "Activity D"), graph.Conditions);
        Assert.DoesNotContain(("Activity B", "Activity D"), graph.Conditions);

        Assert.Contains(("Activity A", "Activity B"), graph.Conditions);
        Assert.Contains(("Activity B", "Activity C"), graph.Conditions);
        Assert.Contains(("Activity C", "Activity D"), graph.Conditions);
    }

    [Fact]
    public void Test56()
    {
        var graph = new DcrGraph
        {
            Conditions = new HashSet<(string, string)>
            {
                ("Activity A", "Activity B"),
                ("Activity G", "Activity H")
            }
        };

        DcrGraphOptimizer.RemoveTransitiveConditions(graph);

        Assert.Contains(("Activity A", "Activity B"), graph.Conditions);
        Assert.Contains(("Activity G", "Activity H"), graph.Conditions);
        Assert.Equal(2, graph.Conditions.Count);
    }

    [Fact]
    public void Test57()
    {
        var graph = new DcrGraph
        {
            Conditions = new HashSet<(string, string)>
            {
                ("Activity A", "Activity B"),
                ("Activity C", "Activity D"),
                ("Activity E", "Activity F")
            }
        };

        DcrGraphOptimizer.RemoveTransitiveConditions(graph);

        Assert.Equal(3, graph.Conditions.Count);
    }
}
    public class RemoveTransitiveExcludesTests
    {
    [Fact]
    public void Test58()
    {
        var graph = new DcrGraph
        {
            Excludes = new HashSet<(string, string)>
            {
                ("Activity A", "Activity C"),
                ("Activity B", "Activity C")
            }
        };

        var alt = new HashSet<(string, string)>
        {
            ("Activity B", "Activity A")
        };

        DcrGraphOptimizer.RemoveTransitiveExcludes(graph, alt);

        Assert.DoesNotContain(("Activity A", "Activity C"), graph.Excludes);
        Assert.Contains(("Activity B", "Activity C"), graph.Excludes);
    }

    [Fact]
    public void Test59()
    {
        var graph = new DcrGraph
        {
            Excludes = new HashSet<(string, string)>
            {
                ("Activity A", "Activity C"),
                ("Activity B", "Activity C")
            }
        };

        var alt = new HashSet<(string, string)>();

        DcrGraphOptimizer.RemoveTransitiveExcludes(graph, alt);

        Assert.Contains(("Activity A", "Activity C"), graph.Excludes);
        Assert.Contains(("Activity B", "Activity C"), graph.Excludes);
    }

    [Fact]
    public void Test60()
    {
        var graph = new DcrGraph
        {
            Excludes = new HashSet<(string, string)>
            {
                ("Activity A", "Activity G"),
                ("Activity B", "Activity G"),
                ("Activity C", "Activity G")
            }
        };

        var alt = new HashSet<(string, string)>
        {
            ("Activity B", "Activity A"),
            ("Activity C", "Activity A")
        };

        DcrGraphOptimizer.RemoveTransitiveExcludes(graph, alt);

        Assert.DoesNotContain(("Activity A", "Activity G"), graph.Excludes);
        Assert.Contains(("Activity B", "Activity G"), graph.Excludes);
        Assert.Contains(("Activity C", "Activity G"), graph.Excludes);
    }

    [Fact]
    public void Test61()
    {
        var graph = new DcrGraph
        {
            Excludes = new HashSet<(string, string)>
            {
                ("Activity A", "Activity A"),
                ("Activity B", "Activity A")
            }
        };

        var alt = new HashSet<(string, string)>
        {
            ("Activity B", "Activity A")
        };

        DcrGraphOptimizer.RemoveTransitiveExcludes(graph, alt);

        Assert.DoesNotContain(("Activity A", "Activity A"), graph.Excludes);
        Assert.Contains(("Activity B", "Activity A"), graph.Excludes);
    }


        [Fact]
        public void Test62()
        {
        var graph = new DcrGraph
        {
            Excludes = new HashSet<(string, string)>
            {
                ("Activity A", "Activity B"),
                ("Activity C", "Activity D")
            }
        };

        var alt = new HashSet<(string, string)>
        {
            ("Activity G", "Activity H")
        };

        DcrGraphOptimizer.RemoveTransitiveExcludes(graph, alt);

        Assert.Contains(("Activity A", "Activity B"), graph.Excludes);
        Assert.Contains(("Activity C", "Activity D"), graph.Excludes);
        Assert.Equal(2, graph.Excludes.Count);
        }
    }
}

public class DcrGraphBuilderTests
{
    [Fact]
    public void Test63()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B" },
            new() { "Activity C" }
        };

        var graph = DcrGraphBuilder.Build(log);

        Assert.Equal(new HashSet<string> { "Activity A", "Activity B", "Activity C" }, graph.Activities);
        Assert.Equal(3, graph.Events.Count);
        Assert.Equal(3, graph.Labeling.Count);
    }

    [Fact]
    public void Test64()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B" },
            new() { "Activity A", "Activity C" },
            new() { "Activity A" }
        };

        var graphLow = DcrGraphBuilder.Build(log, threshold: 0.34);
        var graphHigh = DcrGraphBuilder.Build(log, threshold: 1.0);

        Assert.Contains(("Activity B", "Activity C"), graphLow.Conditions);
        Assert.DoesNotContain(("Activity B", "Activity C"), graphHigh.Conditions);
    }

    [Fact]
    public void Test65()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B" },
            new() { "Activity A", "Activity C" }
        };

        var filter = new HashSet<string> { "Activity A" };
        var graph = DcrGraphBuilder.Build(log, activityFilter: filter);

        Assert.Equal(new HashSet<string> { "Activity A" }, graph.Activities);

        Assert.Empty(graph.Conditions);
        Assert.Empty(graph.Responses);
        Assert.Empty(graph.Includes);

        Assert.Contains(("Activity A", "Activity A"), graph.Excludes);
        Assert.Single(graph.Excludes);
    }

    [Fact]
    public void Test66()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B" }
        };

        var relationFilter = new HashSet<RelationType> { RelationType.Conditions };
        var graph = DcrGraphBuilder.Build(log, relationFilter: relationFilter);

        Assert.NotEmpty(graph.Conditions);
        Assert.Empty(graph.Responses);
        Assert.Empty(graph.Includes);
        Assert.Empty(graph.Excludes);
    }
    
    [Fact]
    public void Test67()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B" }
        };

        var graph = DcrGraphBuilder.Build(log);

        Assert.DoesNotContain(("Activity A", "Activity B"), graph.Includes);
        Assert.Empty(graph.Includes);
    }

    [Fact]
    public void Test68()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B" }
        };

        var graph = DcrGraphBuilder.Build(log);

        Assert.Contains(("Activity B", "Activity B"), graph.Excludes);
    }

    [Fact]
    public void Test69()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B", "Activity C" }
        };

        var graph = DcrGraphBuilder.Build(log);

        Assert.DoesNotContain(("Activity A", "Activity C"), graph.Conditions);
    }

    [Fact]
    public void Test70()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B", "Activity C" }
        };

        var graph = DcrGraphBuilder.Build(log);

        Assert.DoesNotContain(("Activity A", "Activity C"), graph.Responses);
    }

    [Fact]
    public void Test71()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A" },
            new() { "Activity B" }
        };

        var graph = DcrGraphBuilder.Build(log);

        Assert.Contains(("Activity A", "Activity B"), graph.Excludes);
        Assert.Contains(("Activity B", "Activity A"), graph.Excludes);
    }

    [Fact]
    public void Test72()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B" },
            new() { "Activity A", "Activity C" }
        };

        var graph = DcrGraphBuilder.Build(log);

        Assert.DoesNotContain(("Activity A", "Activity C"), graph.Excludes);
    }
}

public class DcrConformanceCheckerTests
{
    public class CheckTrace
    {
        [Fact]
        public void Test73()
        {
            var graph = new DcrGraph
            {
                Activities = new HashSet<string> { "Activity A", "Activity B" }
            };

            var trace = new List<string> { "Activity A", "Activity B" };

            var result = DcrConformanceChecker.CheckTrace(graph, trace);

            Assert.True(result.IsConformant);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Test74()
        {
            var graph = new DcrGraph
            {
                Activities = new HashSet<string> { "Activity A" }
            };

            var trace = new List<string> { "Activity A", "Activity G", "Activity A" };

            var result = DcrConformanceChecker.CheckTrace(graph, trace);

            Assert.True(result.IsConformant);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Test75()
        {
            var graph = new DcrGraph
            {
                Activities = new HashSet<string> { "Activity A", "Activity B" },
                Conditions = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B")
                }
            };

            var trace = new List<string> { "Activity B", "Activity A" };

            var result = DcrConformanceChecker.CheckTrace(graph, trace);

            Assert.False(result.IsConformant);
            Assert.Contains(result.Errors, e => e.Contains("Activity A") && e.Contains("Activity B"));
        }

        [Fact]
        public void Test76()
        {
            var graph = new DcrGraph
            {
                Activities = new HashSet<string> { "Activity A", "Activity B" },
                Conditions = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B")
                }
            };

            var trace = new List<string> { "Activity A", "Activity B" };

            var result = DcrConformanceChecker.CheckTrace(graph, trace);

            Assert.True(result.IsConformant);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Test77()
        {
            var graph = new DcrGraph
            {
                Activities = new HashSet<string> { "Activity A", "Activity B" },
                Responses = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B")
                }
            };

            var trace = new List<string> { "Activity A" };

            var result = DcrConformanceChecker.CheckTrace(graph, trace);

            Assert.False(result.IsConformant);
            Assert.Contains(result.Errors, e => e.Contains("Activity B"));
        }

        [Fact]
        public void Test78()
        {
            var graph = new DcrGraph
            {
                Activities = new HashSet<string> { "Activity A", "Activity B" },
                Responses = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B")
                }
            };

            var trace = new List<string> { "Activity A", "Activity B" };

            var result = DcrConformanceChecker.CheckTrace(graph, trace);

            Assert.True(result.IsConformant);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Test79()
        {
            var graph = new DcrGraph
            {
                Activities = new HashSet<string> { "Activity A", "Activity B" },
                Excludes = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B")
                }
            };

            var trace = new List<string> { "Activity A", "Activity B" };

            var result = DcrConformanceChecker.CheckTrace(graph, trace);

            Assert.False(result.IsConformant);
            Assert.Contains(result.Errors, e => e.Contains("excluded"));
        }

        [Fact]
        public void Test80()
        {
            var graph = new DcrGraph
            {
                Activities = new HashSet<string> { "Activity A", "Activity B" },
                Includes = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B")
                }
            };

            var trace = new List<string> { "Activity A", "Activity B" };

            var result = DcrConformanceChecker.CheckTrace(graph, trace);

            Assert.True(result.IsConformant);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Test81()
        {
            var graph = new DcrGraph
            {
                Activities = new HashSet<string> { "Activity A", "Activity B", "Activity C" },
                Responses = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B"),
                    ("Activity A", "Activity C")
                }
            };

            var trace = new List<string> { "Activity A", "Activity B" };

            var result = DcrConformanceChecker.CheckTrace(graph, trace);

            Assert.False(result.IsConformant);
            Assert.Contains(result.Errors, e => e.Contains("Activity C"));
        }

        [Fact]
        public void Test82()
        {
            var graph = new DcrGraph
            {
                Activities = new HashSet<string> { "Activity A", "Activity B" },
                Excludes = new HashSet<(string, string)>
                {
                    ("Activity G", "Activity B")
                },
                Includes = new HashSet<(string, string)>
                {
                    ("Activity A", "Activity B")
                }
            };

            var trace = new List<string> { "Activity A", "Activity B" };

            var result = DcrConformanceChecker.CheckTrace(graph, trace);

            Assert.True(result.IsConformant);
        }

        [Fact]
        public void Test83()
        {
            var graph = new DcrGraph
            {
                Activities = new HashSet<string> { "Activity A", "Activity B" },
                Conditions = new HashSet<(string, string)> { ("Activity A", "Activity B") },
                Responses = new HashSet<(string, string)> { ("Activity B", "Activity A") }
            };

            var trace = new List<string> { "Activity A", "Activity B" };

            var result = DcrConformanceChecker.CheckTrace(graph, trace);

            Assert.False(result.IsConformant);
            Assert.Equal(1, result.FitnessErrorCount);
        }
    }
}

public class LogParse
{
    public class ParseToTraces
    {
        [Fact]
        public void Test84()
        {
            var headers = new List<string> { "CaseId", "Activity", "Timestamp" };
            var rows = new List<List<string>>
            {
                new() { "1", "Activity B", "08/03/2025 19:35:20" },
                new() { "2", "Activity D", "08/03/2025 19:35:10" },
                new() { "1", "Activity A", "08/03/2025 19:35:15" },
                new() { "2", "Activity F", "08/03/2025 19:35:30" },
                new() { "1", "Activity C", "08/03/2025 19:35:25" },
                new() { "2", "Activity E", "08/03/2025 19:35:20" }
            };

            int caseIdIndex = headers.FindIndex(h => h.Equals("CaseId", StringComparison.OrdinalIgnoreCase));
            int activityIndex = headers.FindIndex(h => h.Equals("Activity", StringComparison.OrdinalIgnoreCase));
            int timestampIndex = headers.FindIndex(h => h.Equals("Timestamp", StringComparison.OrdinalIgnoreCase));

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

            Assert.Equal(2, grouped.Count);

            List<string> trace1 = grouped.Single(t => t.Contains("Activity A"));
            List<string> trace2 = grouped.Single(t => t.Contains("Activity D"));

            Assert.Equal(new[] { "Activity A", "Activity B", "Activity C" }, trace1);
            Assert.Equal(new[] { "Activity D", "Activity E", "Activity F" }, trace2);
        }
    }
}
