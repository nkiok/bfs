using NUnit.Framework;
using Shouldly;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ShortestPathTests
{
    internal class Graph<TNode>
    {
        public Dictionary<TNode, TNode[]> Edges = new Dictionary<TNode, TNode[]>();

        public TNode[] Neighbors(TNode id)
        {
            return Edges[id];
        }
    };

    static class BreadthFirstSearch
    {
        public static IEnumerable<string> Search(Graph<string> graph, string start, string find)
        {
            var frontier = new Queue<string>();

            frontier.Enqueue(start);

            var visited = new HashSet<string> {start};

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                foreach (var next in graph.Neighbors(current))
                {
                    if (visited.Contains(next)) continue;

                    frontier.Enqueue(next);

                    visited.Add(next);
                }
            }

            return Enumerable.Empty<string>();
        }
    }

    public class Tests
    {
        private class UserDistance
        {
            public string Source { get; set; }

            public string Target { get; set; }

            public int Distance { get; set; }
        }

        private Dictionary<string, List<UserDistance>> _userDistances;

        private readonly Graph<string> _network = new Graph<string>
            {
                Edges = new Dictionary<string, string[]>()
                {
                    {"Min", new[] {"William", "Jayden", "Omar"}},
                    {"William", new[] {"Min", "Noam"}},
                    {"Jayden", new[] {"Min", "Amelia", "Ren", "Noam"}},
                    {"Ren", new[] {"Jayden", "Omar"}},
                    {"Amelia", new[] {"Jayden", "Adam", "Miguel"}},
                    {"Adam", new[] {"Amelia", "Miguel", "Sofia", "Lucas"}},
                    {"Miguel", new[] {"Amelia", "Adam", "Liam", "Nathan"}},
                    {"Noam", new[] {"Nathan", "Jayden", "William"}},
                    {"Omar", new[] {"Ren", "Min", "Scott"}},
                    {"Sofia", new[] {"Lucas", "Miguel", "Lucas"}},
                    {"Lucas", new[] {"Sofia", "Min", "Scott"}},
                    {"Liam", new[] {"Miguel"}},
                    {"Nathan", new[] {"Scott"}},
                    {"Scott", new[] {"Nathan"}}
                }
        };

        [SetUp]
        public void Setup()
        {
            _userDistances = _network.Edges
                .SelectMany(n => n.Value.Select((u, i) =>
                    new UserDistance()
                    {
                        Distance = i + 1,
                        Source = n.Key,
                        Target = u
                    }))
                .GroupBy(g => g.Source)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        [TestCase("Jayden", "Adam")]
        public void Test(string source, string target)
        {
        }

        [TestCase("Jayden", "Adam",  new[] {"Jayden", "Amelia", "Adam"})]
        public void CalculatedPathMatchesExpectedPath(string source, string target, string[] expectedPath)
        {
            var result = BreadthFirstSearch.Search(_network, source, target);

            var nodes = new HashSet<string>();

            nodes.UnionWith(EveryOneThatKnows(source));
            nodes.UnionWith(EveryOneThatKnows(target));

            var route = new List<string> {source};

            foreach (var node in nodes)
            {
                var sourceKnownFrom = SpecificOneThatKnows(node, source);

                var targetKnownFrom = SpecificOneThatKnows(node, target);

                if (sourceKnownFrom.Equals(targetKnownFrom))
                {
                    route.Add(node);

                    route.Add(target);

                    break;
                }
            }

            var firstNotSecond = route.Except(expectedPath).ToList();
            var secondNotFirst = expectedPath.Except(route).ToList();

            firstNotSecond.ShouldBeEmpty();
            secondNotFirst.ShouldBeEmpty();
        }

        private IEnumerable<string> EveryOneThatKnows(string who)
        {
            return _network.Edges.Where(n => n.Value.Contains(who)).Select(n => n.Key);
        }

        private bool SpecificOneThatKnows(string specificOne, string who)
        {
            return _network.Edges[specificOne].Contains(who);
        }

        private static IEnumerable<T> BreadthFirstTopDownTraversal<T>(T root, Func<T, IEnumerable<T>> children)
        {
            var q = new Queue<T>();

            q.Enqueue(root);

            while (q.Count > 0)
            {
                var current = q.Dequeue();

                yield return current;

                foreach (var child in children(current))
                    q.Enqueue(child);
            }
        }
    }
}