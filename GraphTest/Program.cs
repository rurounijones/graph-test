using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Graphviz;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

namespace GraphTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var graph = new UndirectedGraph<TaxiPoint, TaggedEdge<TaxiPoint, string>>();

            // Krymsk Airfield
            TaxiPoint runwayFour = new TaxiPoint("Runway 4");
            TaxiPoint adJunction = new TaxiPoint("Alpha / Delta Junction");
            TaxiPoint acJunction = new TaxiPoint("Alpha / Charlie Junction");
            TaxiPoint bdJunction = new TaxiPoint("Bravo / Delta Junction");
            TaxiPoint padsFourteenThroughTwenty = new TaxiPoint("Pads 14 to 20");

            graph.AddVertex(runwayFour);
            graph.AddVertex(adJunction);
            graph.AddVertex(acJunction);
            graph.AddVertex(bdJunction);
            graph.AddVertex(padsFourteenThroughTwenty);

            TaggedEdge<TaxiPoint, string> runwayFourToAdJunction = new TaggedEdge<TaxiPoint, string>(runwayFour, adJunction, "Alpha");
            TaggedEdge<TaxiPoint, string> adJunctionToAcJunction = new TaggedEdge<TaxiPoint, string>(adJunction, acJunction, "Alpha");
            TaggedEdge<TaxiPoint, string> adJunctionToBdJunction = new TaggedEdge<TaxiPoint, string>(adJunction, bdJunction, "Delta");
            TaggedEdge<TaxiPoint, string> acJunctionToPadsFourteenThroughTwenty = new TaggedEdge<TaxiPoint, string>(acJunction, padsFourteenThroughTwenty, "Charlie");

            graph.AddEdge(runwayFourToAdJunction);
            graph.AddEdge(adJunctionToAcJunction);
            graph.AddEdge(adJunctionToBdJunction);
            graph.AddEdge(acJunctionToPadsFourteenThroughTwenty);

            Dictionary<TaggedEdge<TaxiPoint, string>, double> edgeCostDictionary = new Dictionary<TaggedEdge<TaxiPoint, string>, double>(graph.EdgeCount)
            {
                { runwayFourToAdJunction, 1 },
                { adJunctionToAcJunction, 1 },
                { adJunctionToBdJunction, 1 },
                { acJunctionToPadsFourteenThroughTwenty, 1}
            };

            string dotGraph = graph.ToGraphviz(algorithm =>
            {
                // Custom init example
                algorithm.FormatVertex += (sender, vertexArgs) =>
                {
                    vertexArgs.VertexFormat.Label = $"{vertexArgs.Vertex.Name}";
                };
                algorithm.FormatEdge += (sender, edgeArgs) =>
                {
                    var label = new QuikGraph.Graphviz.Dot.GraphvizEdgeLabel
                    {
                        Value = $"Taxiway {edgeArgs.Edge.Tag} : Cost {edgeCostDictionary[edgeArgs.Edge]}"
                    };
                    edgeArgs.EdgeFormat.Label = label;
                };
            });

            Console.WriteLine("Graph Definition");
            Console.WriteLine("-------------------------------------");
            Console.WriteLine(dotGraph);
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Shortest Path Test");

            TaxiPoint root = padsFourteenThroughTwenty;

            Func<TaggedEdge<TaxiPoint, string>, double> edgeCost = AlgorithmExtensions.GetIndexer(edgeCostDictionary);

            TryFunc<TaxiPoint, IEnumerable<TaggedEdge<TaxiPoint, string>>> tryGetPaths = graph.ShortestPathsDijkstra(edgeCost, root);

            // Query path for given vertices
            TaxiPoint target = runwayFour;

            Console.WriteLine("Getting Path");

            if (tryGetPaths(target, out IEnumerable<TaggedEdge<TaxiPoint, string>> path))
            {
                List<string> taxiways = new List<string>();
                foreach (TaggedEdge<TaxiPoint, string> edge in path)
                {
                    taxiways.Add(edge.Tag);
                }

                Console.WriteLine("{0} to {1} via taxiways {2}", root.Name, target.Name, string.Join(", ", RemoveRepeating(taxiways)));
            } else
            {
                Console.WriteLine("Path was null");
            }

            Console.WriteLine("Press Enter to close");
            Console.ReadLine();
        }

        static List<string> RemoveRepeating(List<string> taxiways)
        {
            List<string> dedupedTaxiways = new List<string>();

            for (int i = 0; i < taxiways.Count; i++)
            {
                if (i == 0)
                {
                    dedupedTaxiways.Add(taxiways[0]);
                } else if (taxiways[i] != taxiways[i - 1])
                {
                    dedupedTaxiways.Add(taxiways[i]);
                }
            }

            return dedupedTaxiways;
        }

        class TaxiPoint
        {
            public string Name;

            public TaxiPoint(string name)
            {
                Name = name;
            }
        }
    }
}
