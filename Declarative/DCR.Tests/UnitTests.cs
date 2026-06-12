using DCR.Enums;
using LogImport.Models;
using Xunit;

namespace DCR.Tests;

public class ConstraintsTests
{
    public class AtMostOneTests
    {
        [Fact]
        public void AtMostOne_AllActivitiesOccurOnce_ReturnsAllActivities()
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
        public void AtMostOne_SomeRepeatingActivities_ReturnsOnlyNonRepeatingActivities()
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
        public void AtMostOne_ActivityRepeatedInOneTrace_ReturnNonRepeatingActivities()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity A", "Activity A", "Activity B" }
            };

            var result = Constraints.AtMostOne(log);

            Assert.Equal(new HashSet<(string, string)>
            {
                ("Activity B", "Activity B")
            }, result);
        }

        [Fact]
        public void AtMostOne_ActivityRepeatedInOneTraceHalfThreshold_ReturnsAllActivities()
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
        public void AtMostOne_ActivityRepeatedZeroThreshold_ReturnsAllActivities()
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
        public void AtMostOne_EmptyLog_ReturnsEmptyResult()
        {
            var log = new List<List<string>>();

            var result = Constraints.AtMostOne(log);

            Assert.Empty(result);
        }

        [Fact]
        public void AtMostOne_EmptyLogWithTwoTraces_ReturnsEmptyResult()
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
        public void AtMostOne_ActivityRepeatsInOneOfThreeTracesTwoThirdThreshold_ReturnsActivity()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A" },
                new() { "Activity A", "Activity A" },
                new()
            };

            var result = Constraints.AtMostOne(log, 2.0 / 3.0);

            Assert.Equal(new HashSet<(string, string)>
            {
                ("Activity A", "Activity A")
            }, result);
        }

        [Fact]
        public void AtMostOne_MultipleTracesOneActivityRepeatsInOneTrace_ReturnsNonRepeatingActivities()
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
        public void Precedence_ABeforeB_ReturnsCorrectResult()
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
        public void Precedence_BBeforeA_ReturnsCorrectResult()
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
        public void Precedence_MultipleTracesABeforeBInAllTraces_ReturnsOnlyPrecedenceAB()
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
        public void Precedence_TwoTracesWithTwoActivitiesSwitchedOrderInEachTrace_DoesNotReturnAnyPrecedence()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity B", "Activity A" }
            };

            var result = Constraints.Precedence(log);

            Assert.DoesNotContain(("Activity A", "Activity B"), result);
            Assert.DoesNotContain(("Activity B", "Activity A"), result);
        }

        [Fact]
        public void Precedence_TwoTracesWithTwoActivitiesSwitchedOrderInEachTraceHalfThreshold_ReturnsBothPossiblePrecedence()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity B", "Activity A" }
            };

            var result = Constraints.Precedence(log, 0.5);

            Assert.Contains(("Activity A", "Activity B"), result);
            Assert.Contains(("Activity B", "Activity A"), result);
        }

        [Fact]
        public void Precedence_ABeforeBInOneTraceBMissingInSecondTrace_ReturnsPrecedenceAB()
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
        public void Precedence_OneTrace_ReturnsAllPossiblePrecedences()
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
        public void Precedence_ThresholdAboveOne_ReturnsEmpty()
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
        public void Response_BAfterAInOneTraceBMissingInSecondTrace_ReturnsResponseAB()
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
        public void Response_BNotPresentInLog_DoesNotContainBInResult()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity G" },
                new() { "Activity A", "Activity H", "Activity I" }
            };

            var result = Constraints.Response(log);

            Assert.DoesNotContain(("Activity A", "Activity B"), result);
            Assert.DoesNotContain(("Activity G", "Activity B"), result);
            Assert.DoesNotContain(("Activity H", "Activity B"), result);
            Assert.DoesNotContain(("Activity I", "Activity B"), result);
        }

        [Fact]
        public void Response_BFollowsActivityInTwoTraces_ReturnsAllTracesWithBFollowing()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" },
                new() { "Activity G", "Activity B" },
                new() { "Activity B", "Activity H" }
            };

            var result = Constraints.Response(log);

            Assert.Contains(("Activity A", "Activity B"), result);
            Assert.Contains(("Activity G", "Activity B"), result);
        }

        [Fact]
        public void Response_BTwoTimesInTheSameTrace_ReturnsCorrectResponses()
        {
            var log = new List<List<string>>
            {
                new() { "Activity B", "Activity A", "Activity G", "Activity B" }
            };

            var result = Constraints.Response(log);

            Assert.Contains(("Activity A", "Activity B"), result);
            Assert.Contains(("Activity G", "Activity B"), result);
        }

        [Fact]
        public void Response_BMissingFromOneTraceThresholdThreeQuarters_ReturnsResponseAB()
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
        public void Response_Response_BMissingFromOneTraceThresholdLowerThanHalf_ReturnsResponseAB()
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
        public void Response_ARepeatsInTrace_ReturnsCorrectResponses()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity G", "Activity A", "Activity H", "Activity B" }
            };

            var result = Constraints.Response(log);

            Assert.Contains(("Activity A", "Activity B"), result);
            Assert.Contains(("Activity A", "Activity H"), result);
            Assert.Contains(("Activity H", "Activity B"), result);
        }
        
    }

    public class ChainPrecedenceTests
    {
        [Fact]
        public void ChainPrecedence_DifferentImmediatePredecessors_DoesNotReturnChainPrecedenceAB()
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
        public void ChainPrecedence_MultipleDifferentBPrecessors_DoesNotReturnChainPrecedenceWithAnyPredecessor()
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
        public void ChainPrecedence_ReversedOrderOfABInEachTrace_ReturnsEmptyResult()
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
        public void ChainPrecedence()
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
        public void ChainPrecedence_TwoDifferentSuccessors_ReturnsEmpty()
        {
            var log = new List<List<string>>
            {
                new() { "Activity B", "Activity A" },
                new() { "Activity B", "Activity C" }
            };

            var result = Constraints.ChainPrecedence(log);

            Assert.Empty(result);
        }
        
        [Fact]
        public void ChainPrecedence_AImmediatelyPrecedesBTwoTimes_ReturnsChainPrecedenceAB()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B", "Activity A", "Activity B" }
            };

            var result = Constraints.ChainPrecedence(log);

            Assert.Contains(("Activity A", "Activity B"), result);
        }
    }

    public class NotChainSuccessionTests
    {
        [Fact]
        public void NotChainSuccession_ActivitiesFollowEachOther_DoesNotReturnActivityPairsFollowingEachOther()
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
        public void NotChainSuccession_ActivitiesInOneTrace_ReturnsOnlyActivitiesNotImmediatelyFollowing()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity G", "Activity B" }
            };

            var result = Constraints.NotChainSuccession(log);

            Assert.Contains(("Activity A", "Activity B"), result);
            Assert.DoesNotContain(("Activity A", "Activity G"), result);
            Assert.DoesNotContain(("Activity G", "Activity B"), result);
        }

        [Fact]
        public void NotChainSuccession_TwoTracesWithDifferentActivitiesAndDifferentOrder_ReturnsActivitiesFromBothTraces()
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
        public void NotChainSuccession_TwoTracesSameStartingActivity_DoesNotContainStartingActivityInAnyChainSuccession()
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
        public void NotChainSuccession_SameActivityFollowsItself_DoesNotContainSelfNotChainSuccession()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity A" }
            };

            var result = Constraints.NotChainSuccession(log);

            Assert.DoesNotContain(("Activity A", "Activity A"), result);
        }

        [Fact]
        public void NotChainSuccession_OneTraceWithTwoActivities_ReturnsResultInCorrectOrder()
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
        public void AlternatePrecedence_EmptyLog_ReturnsEmptyResult()
        {
            var log = new List<List<string>>();
            var result = Constraints.AlternatePrecedence(log);
            Assert.Empty(result);
        }

        [Fact]
        public void AlternatePrecedence_ActivitiesNeverOccurTogether_ReturnsEmptyResult()
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
        public void AlternatePrecedence_ThreeActivitiesInOneTrace_ReturnsResultWithOnlyImmediatelyFollowingActivities()
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
        public void AlternatePrecedence_TwoTracesWithTheTwoActivitiesAndSameOrder_ReturnsOneAlternatePrecedence()
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
        public void AlternatePrecedence_TwoTracesWithSomeDIfferentActivitiesButBCFollowEachOtherInBoth_ReturnsAlternatePrecedenceBC()
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
        public void AlternatePrecedence_ImmediatePairsPresentInOneTraceMissingInOther_ReturnsBothPairs()
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
        public void DeterminePredecessorSuccessor_SingleTrace_ReturnsCorrectPredecessorsAndSuccessorsForEachActivity()
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
        public void DeterminePredecessorSuccessor_TwoTraces_ReturnsCorrectPredecessorsAndSuccessorsForEachActivity()
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
        public void DeterminePredecessorSuccessor_ActivityRepeatedInTrace_ReturnsCorrectPredecessorsAndSuccessorsForEachActivity()
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
        public void AreCoOccurring_EmtpyLog_ReturnsFalse()
        {
            var log = new List<List<string>>();
            Assert.False(Constraints.AreCoOccurring(log, "Activity A", "Activity B"));
        }

        [Fact]
        public void AreCoOccurring_LogWithDifferentActivitiesThanChecked_ReturnsFalse()
        {
            var log = new List<List<string>>
            {
                new() { "Activity G", "Activity H" }
            };

            Assert.False(Constraints.AreCoOccurring(log, "Activity A", "Activity B"));
        }

        [Fact]
        public void AreCoOccurring_LogWithSameActivitiesAsChecked_ReturnsTrue()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A", "Activity B" }
            };

            Assert.True(Constraints.AreCoOccurring(log, "Activity A", "Activity B"));
        }
        
        [Fact]
        public void AreCoOccurring_ActivitiesInDifferentTraces_ReturnsFalse()
        {
            var log = new List<List<string>>
            {
                new() { "Activity A" },
                new() { "Activity B" }
            };

            Assert.False(Constraints.AreCoOccurring(log, "Activity A", "Activity B"));
        }
    }

    public class GetAllActivitiesTests
    {
        [Fact]
        public void GetAllActivities_EmptyLog_ReturnsEmptyList()
        {
            var log = new List<List<string>>();
            var result = Constraints.GetAllActivities(log);
            Assert.Empty(result);
        }

        [Fact]
        public void GetAllActivities_LogWithActivitiesAndTwoTraces_ReturnsCorrectResult()
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
        public void RemoveTransitiveResponses_ContainsRedundantRelation_RemovesCorrectlyOnlyRedundantRelation()
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
        public void RemoveTransitiveResponses_DoesNotContainRedundantRelation_LeavesRelationsAsIs()
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
        public void RemoveTransitiveResponses_MoreResponseRelationsContainsRedundantRelations_RemovesCorrectlyOnlyRedundantRelation()
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
        public void RemoveTransitiveResponses_TwoTracesWithoutAnySharedActivities_LeavesRelationsAsIs()
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
        public void RemoveTransitiveResponses_MoreTracesWithoutAnySharedActivities_LeavesRelationsAsIs()
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
    public void RemoveTransitiveConditions_ContainsRedundantRelation_RemovesCorrectlyOnlyRedundantRelation()
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
    public void RemoveTransitiveConditions_DoesNotContainRedundantRelation_LeavesRelationsAsIs()
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
    public void RemoveTransitiveConditions_MoreConditionRelationsContainsRedundantRelations_RemovesCorrectlyOnlyRedundantRelation()
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
    public void RemoveTransitiveConditions_TwoTracesWithoutAnySharedActivities_LeavesRelationsAsIs()
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
    public void RemoveTransitiveConditions_MoreTracesWithoutAnySharedActivities_LeavesRelationsAsIs()
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
    public void RemoveTransitiveExcludes_AlternatePrecedenceExists_RemoveTransitiveExclude()
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
    public void RemoveTransitiveExcludes_AlternatePrecedenceDoesNotExist_KeepAllExcludes()
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
    public void RemoveTransitiveExcludes_MultipleExcludes_RemoveOnlyRedundantOnes()
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
    public void RemoveTransitiveExcludes_TransitiveSelfExclude_RemovesSelfExclude()
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
        public void RemoveTransitiveExcludes_UnrelatedExcludesAndAlternatePrecedence_KeepsAllExcludes()
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
    public void Build_LogWithTwoTraces_CorrectlyInitializeGraph()
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
    public void Build_TwoGraphDifferentThresholds_InitializedGraphsDifferInRelations()
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
    public void Build_AppliesActivityFilter_KeepsOnlyFilteredActivitiesAndTheirRelations()
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
    public void Build_AppliesRelationFilter_KeepsOnlyFilteredRelations()
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
    public void Build_SimpleLog_GraphDoesNotContainsIncludes()
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
    public void Build_SimpleLog_InitializesGraphWithCorrectSelfExcludes()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B" }
        };

        var graph = DcrGraphBuilder.Build(log);
        
        Assert.Contains(("Activity A", "Activity A"), graph.Excludes);
        Assert.Contains(("Activity B", "Activity B"), graph.Excludes);
    }

    [Fact]
    public void Build_SimpleLogWithThreeActivities_InitializesGraphWithRemovedTransitiveCondition()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B", "Activity C" }
        };

        var graph = DcrGraphBuilder.Build(log);

        Assert.DoesNotContain(("Activity A", "Activity C"), graph.Conditions);
        Assert.Contains(("Activity A", "Activity B"), graph.Conditions);
        Assert.Contains(("Activity B", "Activity C"), graph.Conditions);
    }

    [Fact]
    public void Build_SimpleLogWithThreeActivities_InitializesGraphWithRemovedTransitiveResponse()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B", "Activity C" }
        };

        var graph = DcrGraphBuilder.Build(log);

        Assert.DoesNotContain(("Activity A", "Activity C"), graph.Responses);
        Assert.Contains(("Activity A", "Activity B"), graph.Responses);
        Assert.Contains(("Activity B", "Activity C"), graph.Responses);
    }

    [Fact]
    public void Build_TwoTracesWithOneActivity_CreatesMutualExcludes()
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
    public void Build_TwoTracesWithOneConsistentStartingActivity_DoesNotAddIncorrectExcludes()
    {
        var log = new List<List<string>>
        {
            new() { "Activity A", "Activity B" },
            new() { "Activity A", "Activity C" }
        };

        var graph = DcrGraphBuilder.Build(log);

        Assert.DoesNotContain(("Activity A", "Activity C"), graph.Excludes);
        Assert.DoesNotContain(("Activity A", "Activity B"), graph.Excludes);
    }
}

public class DcrConformanceCheckerTests
{
    public class CheckTrace
    {
        [Fact]
        public void CheckTrace_SimpleGraphAndSimpleLogWithSameActivities_ReturnsTrue()
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
        public void CheckTrace_LogContainsUknownActivities_ReturnsTrue()
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
        public void CheckTrace_ConditionViolated_ReturnsFalse()
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
        public void CheckTrace_ConditionSatisfied_ReturnsTrue()
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
        public void CheckTrace_ResponseViolated_ReturnsFalse()
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
        public void CheckTrace_ResponseSatisfied_ReturnsTrue()
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
        public void CheckTrace_ExcludeViolated_ReturnsFalse()
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
        public void CheckTrace_IncludeSatisfied_ReturnsTrue()
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
        public void CheckTrace_MultipleActivitiesReponsesViolated_ReturnsFalse()
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
        public void CheckTrace_UnknownActivityExclude_ReturnsTrue()
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
        public void CheckTrace_GraphWithMultipleRelationsResponseViolated_ReturnsFalse()
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
        public void ParseToTraces_ValidImportedLog_ReturnsValidParsedSortedLog()
        {
            var imported = new ImportedEventLog(
                new List<string[]>
                {
                    new[] { "1", "Activity B", "08/03/2025 19:35:20" },
                    new[] { "2", "Activity D", "08/03/2025 19:35:10" },
                    new[] { "1", "Activity A", "08/03/2025 19:35:15" },
                    new[] { "2", "Activity F", "08/03/2025 19:35:30" },
                    new[] { "1", "Activity C", "08/03/2025 19:35:25" },
                    new[] { "2", "Activity E", "08/03/2025 19:35:20" }
                },
                new[] { "CaseId", "Activity", "Timestamp" }
            );
            
            var result = LogParser.ParseToTraces(imported, "CaseId", "Activity", "Timestamp");


            Assert.Equal(2, result.Count);

            List<string> trace1 = result.Single(t => t.Contains("Activity A"));
            List<string> trace2 = result.Single(t => t.Contains("Activity D"));

            Assert.Equal(new[] { "Activity A", "Activity B", "Activity C" }, trace1);
            Assert.Equal(new[] { "Activity D", "Activity E", "Activity F" }, trace2);
        }
        
        [Fact]
        public void ParseToTraces_IncorrectIdColumn_ThrowsException()
        {
            var imported = new ImportedEventLog(
                new List<string[]>
                {
                    new[] { "1", "Activity A", "08/03/2025 19:35:20" }
                },
                new[] { "CaseId", "Activity", "Timestamp" }
            );

            Assert.Throws<Exception>(() => LogParser.ParseToTraces(imported, "Incorrect", "Activity", "Timestamp"));
        }
        
        [Fact]
        public void ParseToTraces_IncorrectActivityColumn_ThrowsException()
        {
            var imported = new ImportedEventLog(
                new List<string[]>
                {
                    new[] { "1", "Activity A", "08/03/2025 19:35:20" }
                },
                new[] { "CaseId", "Activity", "Timestamp" }
            );

            Assert.Throws<Exception>(() => LogParser.ParseToTraces(imported, "CaseId", "Incorrect", "Timestamp"));
        }
        
        [Fact]
        public void ParseToTraces_IncorrectTimestampColumn_ThrowsException()
        {
            var imported = new ImportedEventLog(
                new List<string[]>
                {
                    new[] { "1", "Activity A", "08/03/2025 19:35:20" }
                },
                new[] { "CaseId", "Activity", "Timestamp" }
            );

            Assert.Throws<Exception>(() => LogParser.ParseToTraces(imported, "CaseId", "Activity", "Incorrect"));
        }
    }
}
