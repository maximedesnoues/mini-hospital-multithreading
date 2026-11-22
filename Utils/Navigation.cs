using MiniHospital.Config;
using System.Numerics;

namespace MiniHospital.Utils
{
    public class Navigation
    {
        private static readonly Dictionary<(NavNode, NavNode), Vector2[]> paths = new Dictionary<(NavNode, NavNode), Vector2[]>();

        public static Vector2[] FindPath(NavNode from, NavNode to)
        {
            return paths[(from, to)];
        }

        public static void InitFullGraph()
        {
            paths.Clear();

            Dictionary<NavNode, List<(NavNode To, float Cost)>> graph = new Dictionary<NavNode, List<(NavNode To, float Cost)>>();
            foreach (var node in HospitalLayout.NavPoints.Keys)
            {
                graph.Add(node, new List<(NavNode To, float Cost)>());
            }

            // Connexions bidirectionnelles
            foreach ((NavNode from, NavNode to) in HospitalLayout.NavLinks)
            {
                float cost = Vector2.Distance(
                    HospitalLayout.NavPoints[from],
                    HospitalLayout.NavPoints[to]
                );

                graph[from].Add((to, cost));
                graph[to].Add((from, cost));
            }

            // Un Dijkstra complet par point de départ
            foreach (NavNode start in graph.Keys)
                ComputeAllPaths(start, graph);
        }

        private static void ComputeAllPaths(NavNode start, Dictionary<NavNode, List<(NavNode To, float Cost)>> graph)
        {
            Dictionary<NavNode, float> distances = new Dictionary<NavNode, float>();
            foreach (NavNode node in graph.Keys)
            {
                distances.Add(node, float.PositiveInfinity);
            }

            var previous = new Dictionary<NavNode, NavNode>();
            var queue = new PriorityQueue<NavNode, float>();

            distances[start] = 0f;
            queue.Enqueue(start, 0f);

            // Dijkstra : calcul des distances et prédécesseurs (condition d'arrêt = plus de noeud à parcourir)
            while (queue.TryDequeue(out NavNode current, out float priority))
            {
                if (priority > distances[current])
                    continue;

                foreach (var node in graph[current])
                {
                    float distance = distances[current] + node.Cost;

                    if (distance < distances[node.To])
                    {
                        distances[node.To] = distance;
                        previous[node.To] = current;
                        queue.Enqueue(node.To, distance);
                    }
                }
            }

            // Reconstruction de tous les chemins depuis start
            foreach (NavNode destination in graph.Keys)
            {
                if (float.IsPositiveInfinity(distances[destination]))
                    throw new Exception(string.Format("Graph de navigation incomplet: aucun chemin entre {0} et {1}", start, destination));

                var route = new List<Vector2>();
                NavNode node = destination;

                while (node != start)
                {
                    route.Add(HospitalLayout.NavPoints[node]);
                    node = previous[node];
                }

                route.Add(HospitalLayout.NavPoints[start]);
                route.Reverse();

                paths[(start, destination)] = route.ToArray();
            }
        }
    }
}
