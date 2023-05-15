using System;
using GraphClassLibrary;

namespace GraphConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Graph g = new Graph(new int[,] { 
            //    { 0, 0, 1, 1, 0 },
            //    { 1, 0, 0, 0, 0 },
            //    { 0, 1, 0, 0, 0 },
            //    { 0, 0, 0, 0, 1 },
            //    { 0, 0, 0, 0, 0 },
            //});
            
            Graph g = new Graph(6);
            // КР
            //g.AddEdge(0, 1, 1, true);
            //g.AddEdge(0, 2, 1, true);
            //g.AddEdge(0, 3, 1, true);
            //g.AddEdge(0, 4, 1, true);
            //g.AddEdge(0, 6, 1, true);
            //g.AddEdge(0, 7, 1, true);
            //g.AddEdge(1, 2, 1, true);
            //g.AddEdge(3, 1, 1, true);
            //g.AddEdge(4, 6, 1, true);
            //g.AddEdge(5, 3, 1, true);
            //g.AddEdge(6, 7, 1, true);
            //g.AddEdge(7, 5, 1, true);


            // Negative
            //g.AddEdge(0, 1, -2, true);
            //g.AddEdge(0, 2, 7, true);
            //g.AddEdge(0, 3, 5, true);
            //g.AddEdge(1, 2, 8, true);
            //g.AddEdge(1, 3, 6, true);
            //g.AddEdge(2, 1, 3, true);
            //g.AddEdge(2, 3, -4, true);
            //g.AddEdge(3, 0, -1, true);

            // Example 9
            //g.AddEdge(0, 1, 4, false);
            //g.AddEdge(0, 7, 8, false);
            //g.AddEdge(1, 2, 8, false);
            //g.AddEdge(1, 7, 11, false);
            //g.AddEdge(2, 3, 7, false);
            //g.AddEdge(2, 5, 4, false);
            //g.AddEdge(2, 8, 2, false);
            //g.AddEdge(3, 4, 9, false);
            //g.AddEdge(3, 5, 11, false);
            //g.AddEdge(4, 5, 10, false);
            //g.AddEdge(5, 6, 2, false);
            //g.AddEdge(6, 7, 1, false);
            //g.AddEdge(6, 8, 6, false);
            //g.AddEdge(7, 8, 7, false);

            // 6 Euler
            g.AddEdge(0, 1, 1, false);
            g.AddEdge(0, 2, 1, false);
            g.AddEdge(1, 2, 1, false);
            g.AddEdge(1, 3, 1, false);
            g.AddEdge(1, 4, 1, false);
            g.AddEdge(2, 4, 1, false);
            g.AddEdge(2, 5, 1, false);
            g.AddEdge(3, 4, 1, false);
            g.AddEdge(4, 5, 1, false);

            //g.AddEdge(0, 1, 1, false);
            //g.AddEdge(0, 2, 1, false);
            //g.AddEdge(0, 3, 1, false);
            //g.AddEdge(1, 2, 1, false);
            //g.AddEdge(1, 4, 1, false);
            //g.AddEdge(1, 5, 1, false);
            //g.AddEdge(2, 3, 1, false);
            //g.AddEdge(3, 4, 1, false);
            //g.AddEdge(4, 5, 1, false);

            // 7 SCC
            //g.AddEdge(0, 1, 1, true);
            //g.AddEdge(1, 3, 1, true);
            //g.AddEdge(3, 2, 1, true);
            //g.AddEdge(2, 0, 1, true);
            //g.AddEdge(4, 2, 1, true);
            //g.AddEdge(4, 3, 1, true);
            //g.AddEdge(4, 5, 1, true);
            //g.AddEdge(5, 6, 1, true);
            //g.AddEdge(6, 4, 1, true);

            //Console.WriteLine(g.GetAdjListString());
            //Console.WriteLine(g.GetAdjMatrixString());
            //Console.WriteLine(g.N);
            //Console.WriteLine(g.E);
            //Console.WriteLine(g.IsDirected());
            //Console.WriteLine(g.IsUndirected());
            //Console.WriteLine(g.GetVertexPower().Item4);
            //Console.WriteLine(g.BFS(0, true));
            //Console.WriteLine(g.BFS_R(0, true));
            //Console.WriteLine(g.DFS_R(0, true));
            //Console.WriteLine(g.DFS(0, true));
            //Console.WriteLine(g.SCC().Item2);
            //Console.WriteLine(g.EulerCicle(0).Item2);
            //Console.WriteLine(g.EulerCicleFleury(0).Item2);
            //Console.WriteLine(g.EulerCicle2(0).Item2);

            //Console.WriteLine(g.FS_Cycles(0).Item2);

            //Console.WriteLine(g.FindHameltonianСyclesDefault(0).Item2);
            //Console.WriteLine(g.FindHameltonianСyclesArithmetic(0).Item2);
            //Console.WriteLine(g.FindHameltonianСyclesRobertsFlores(0).Item2);
            //Console.WriteLine(g.MST_Kruskal().Item2);
            //Console.WriteLine(g.PrimMST(0).Item2);

            //Console.WriteLine(g.BFS(0));
            //Console.WriteLine(g.BFSAdjMatrix(0));

            //Console.WriteLine(g.DFS(0));
            //Console.WriteLine(g.DFS_R(0));
            //Console.WriteLine(g.DFSAdjMatrix(0));

            //Console.WriteLine(g.DijkstraAdjacencyList(0).Item3);
            //Console.WriteLine(g.BellmanFord(0).Item3);
            //Console.WriteLine(g.FloydWarshall().Item3);
            //Console.WriteLine(g.Levit(0).Item3);
            //Console.WriteLine(g.Johnson().DijkstraAdjacencyList(0).Item3);

            //foreach (var item in g.RoadsFromLength(0, 2))
            //    Console.WriteLine(item);

            //Console.WriteLine(g.numberofPaths(0, 7));

            //LinkedList<int> list = new LinkedList<int>();
            //list.Add(1);
            //list.Add(4);
            //list.Add(7);
            //list.Add(5);
            //list.RemoveAt(0);
            //list.AddAt(99, 1);
            //foreach (var item in list)
            //{
            //    Console.WriteLine(item);
            //}
            //for (int i = 0; i < list.Count; i++)
            //{
            //    Console.WriteLine(list[i]);
            //}

            //BinaryHeap<int> bh = new BinaryHeap<int>();
            //bh.Add(new Node<int>(15));
            //bh.Add(new Node<int>(10));
            //bh.Add(new Node<int>(4));
            //bh.Add(new Node<int>(17));
            //bh.Add(new Node<int>(6));
            //while (bh.count != 0)
            //    Console.WriteLine(bh.Remove());
        }
    }
}