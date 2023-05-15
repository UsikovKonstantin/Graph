using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GraphClassLibrary
{
    public class Graph
    {
        #region Поля
        public const int INF = 1_000_000_000;  // бесконечность
        public int N;  // количество вершин 
        public int E;  // количество ребер 
        public int[,] adjMatrix;   // матрица смежности
        public Dictionary<int, List<Tuple<int, int>>> adjList;  // список смежности (u, v, len)

        public bool[] Visited;  // посещена ли вершина
        Color[] Colors;  // цвет вершины
        int[] Dist;  // количество ребер до вершины

        int time;  // счётчик времени 
        int[] entryTime;  // время захода в вершину
        int[] exitTime;  // время выхода из вершины
        
        string res;

        StackRef<int> St;  // для поиска сильно связных компонент
        ListRef Cycle;  // хранит Эйлеров цикл или Гамильтонов цикл

        List<ListRef> CyclesFS;  // множество фундаментальных циклов или Гамильтоновых
        int[] S;  // стек для поиска фундаментальных циклов
        int ind = 0;  // индекс стека для поиска фундаментальных циклов
        int count;  // счётчик для поиска фундаментальных циклов
        int[] Mark;  // отметка вершин для поиска фундаментальных циклов

        int[] parent;  // массив предков для алгоритма Крускала
        int[] rank;  // для алгоритма Крускала

        List<string> roads = new List<string>();  // пути
        #endregion

        #region Конструкторы
        public Graph()
        {
            N = 0;
            E = 0;
            adjMatrix = new int[0, 0];
            adjList = new Dictionary<int, List<Tuple<int, int>>>();
        }

        public Graph(int N)
        {
            this.N = N;
            E = 0;
            adjMatrix = new int[N, N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    adjMatrix[i, j] = INF;
            adjList = new Dictionary<int, List<Tuple<int, int>>>();
        }

        public Graph(int[,] adjMatrix)
        {
            N = adjMatrix.GetLength(0);
            this.adjMatrix = adjMatrix;
            adjList = new Dictionary<int, List<Tuple<int, int>>>();

            E = 0;
            for (int i = 0; i < N; i++)
            {
                List<Tuple<int, int>> curList = new List<Tuple<int, int>>();
                for (int j = 0; j < N; j++)
                    if (adjMatrix[i, j] == 0 || adjMatrix[i, j] == INF)
                    {
                        adjMatrix[i, j] = INF;
                    }
                    else
                    {
                        curList.Add(new Tuple<int, int>(j, adjMatrix[i, j]));
                        E++;
                    }
                adjList.Add(i, curList);
            }
        }

        public Graph(int N, Dictionary<int, List<Tuple<int, int>>> adjList)
        {
            this.N = N;
            this.adjList = adjList;

            adjMatrix = new int[N, N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    adjMatrix[i, j] = INF;

            E = 0;
            for (int i = 0; i < N; i++)
                if (adjList.ContainsKey(i))
                    foreach (Tuple<int, int> tup in adjList[i])
                    {
                        adjMatrix[i, tup.Item1] = tup.Item2;
                        E++;
                    }
        }
        #endregion

        #region Добавление/удаление вершин и ребер
        public void AddVertex()
        {
            N++;
            int[,] adjMatrixNew = new int[N, N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    adjMatrixNew[i, j] = INF;
            for (int i = 0; i < N - 1; i++)
                for (int j = 0; j < N - 1; j++)
                    adjMatrixNew[i, j] = adjMatrix[i, j];
            adjMatrix = adjMatrixNew;
        }

        public void DeleteVertex(int v)
        {
            N--;
            for (int i = 0; i < N + 1; i++)
                if (adjMatrix[i, v] != INF)
                    E--;
            for (int i = 0; i < N + 1; i++)
                if (adjMatrix[v, i] != INF)
                    E--;
            if (adjMatrix[v, v] != INF)
                E++;

            int[,] adjMatrixNew = new int[N, N];
            for (int i = 0, j = 0; i < N + 1; i++)
            {
                if (i == v)
                    continue;
                for (int k = 0, u = 0; k < N + 1; k++)
                {
                    if (k == v)
                        continue;
                    adjMatrixNew[j, u] = adjMatrix[i, k];
                    u++;
                }
                j++;
            }
            adjMatrix = adjMatrixNew;

            for (int i = N; i >= 0; i--)
                if (adjList.ContainsKey(i))
                    if (i == v)
                        adjList.Remove(i);
                    else
                        for (int j = adjList[i].Count - 1; j >= 0; j--)
                            if (adjList[i][j].Item1 == v)
                            {
                                adjList[i].Remove(adjList[i][j]);
                                if (adjList[i].Count == 0)
                                    adjList.Remove(i);
                            }

            for (int i = v + 1; i < N + 1; i++)
                if (adjList.ContainsKey(i))
                    Extensions.RenameKey(adjList, i, i - 1);

            for (int i = 0; i < N + 1; i++)
                if (adjList.ContainsKey(i))
                    for (int j = adjList[i].Count - 1; j >= 0; j--)
                        if (adjList[i][j].Item1 > v)
                            adjList[i][j] = new Tuple<int, int>(adjList[i][j].Item1 - 1, adjList[i][j].Item2);
        }

        public void AddEdge(int u, int v, int len, bool isDirected = true)
        {
            if (adjMatrix[u, v] == INF)
            {
                E++;
                adjMatrix[u, v] = len;
                if (adjList.ContainsKey(u))
                    adjList[u].Add(new Tuple<int, int>(v, len));
                else
                    adjList[u] = new List<Tuple<int, int>>() { new Tuple<int, int>(v, len) };
            }
            else
            {
                adjList[u].Remove(new Tuple<int, int>(v, adjMatrix[u, v]));
                adjList[u].Add(new Tuple<int, int>(v, len));
                adjMatrix[u, v] = len;
            }
            if (!isDirected)
                AddEdge(v, u, len, true);
        }

        public void DeleteEdge(int u, int v, bool isDirected = true)
        {
            if (adjMatrix[u, v] != INF)
            {
                E--;
                adjList[u].Remove(new Tuple<int, int>(v, adjMatrix[u, v]));
                if (adjList[u].Count == 0)
                    adjList.Remove(u);
                adjMatrix[u, v] = INF;
            }
            if (!isDirected)
                DeleteEdge(v, u, true);
        }
        #endregion

        #region Матрица смежности
        public string GetAdjMatrixString()
        {
            string res = "Матрица смежности:\n";
            const int margin = -3;

            res += $"{"",margin}";
            for (int i = 0; i < N; ++i)
                res += $"{i,margin}";
            res += '\n';
            for (int i = 0; i < N; i++)
            {
                for (int j = -1; j < N; j++)
                    if (j == -1)
                        res += $"{i,margin}";
                    else if (adjMatrix[i, j] == INF)
                        res += $"{'-',margin}";
                    else
                        res += $"{adjMatrix[i, j],margin}";
                res += '\n';
            }
            return res + "\n";
        }
        #endregion

        #region Список смежности
        public string GetAdjListString()
        {
            string res = "Список смежности:\n";
            for (int i = 0; i <= N; i++)
                if (adjList.ContainsKey(i))
                {
                    res += i + ": ";
                    foreach (var tup in adjList[i])
                        res += $"{tup.Item1}({tup.Item2}), ";
                    res = res.Substring(0, res.Length - 2);
                    res += '\n';
                }
            return res + "\n";
        }
        #endregion

        #region Направленный/ненаправленный граф
        public bool IsDirected()
        {
            for (int i = 0; i < N; i++)
                for (int j = i + 1; j < N; j++)
                    if (adjMatrix[i, j] == adjMatrix[j, i] && adjMatrix[i, j] != INF)
                        return false;
            return true;
        }

        public bool IsUndirected()
        {
            for (int i = 0; i < N; i++)
                for (int j = i + 1; j < N; j++)
                    if (adjMatrix[i, j] != adjMatrix[j, i])
                        return false;
            return true;
        }
        #endregion

        #region Матрица инцидентности
        public Tuple<int[,], string> IncidenceMatrix()
        {
            bool isDir = IsDirected();
            bool isUndir = IsUndirected();
            if (!isDir && !isUndir)
                return new Tuple<int[,], string>(null, "Граф не подходит");

            // Количество столбцов в матрице инцидентности 
            int M = 0;
            if (isDir)
            {
                M = E;
            }
            else if (isUndir)
            {
                int c = 0;
                for (int i = 0; i < N; i++)
                    if (adjMatrix[i, i] != INF)
                        c++;
                M = (E - c) / 2 + c;
            }

            // Список ребер
            string result = "Список ребер:\n";
            int[,] incMatrix = new int[N, M];
            int ind = 0;
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    if (adjMatrix[i, j] != INF)
                        if (isDir)
                        {
                            result += $"{Convert.ToChar('a' + ind)}: {i} -> {j}\n";
                            incMatrix[i, ind] = 1;
                            incMatrix[j, ind] = -1;
                            if (i == j)
                                incMatrix[j, ind] = 2;
                            ind++;
                        }
                        else if (isUndir && i <= j)
                        {
                            result += $"{Convert.ToChar('a' + ind)}: {i} - {j}\n";
                            incMatrix[i, ind] = 1;
                            incMatrix[j, ind] = 1;
                            ind++;
                        }

            // Матрца инцидентности
            result += "Матрица инцидентности:\n";
            const int margin = -3;
            result += $"{"",margin}";
            for (int i = 0; i < M; ++i)
                result += $"{Convert.ToChar('a' + i),margin}";
            result += '\n';
            for (int i = 0; i < N; i++)
            {
                for (int j = -1; j < M; j++)
                    if (j == -1)
                        result += $"{i,margin}";
                    else
                        result += $"{incMatrix[i, j],margin}";
                result += '\n';
            }
            return E == 0 ? new Tuple<int[,], string>(null, "Граф не подходит") : new Tuple<int[,], string>(incMatrix, result + "\n");
        }
        #endregion

        #region Таблица степеней вершин
        public Tuple<int[], int[], int[], string> GetVertexPower()
        {
            const int margin = -8;
            int[] Sum = new int[N];
            int[] Out = new int[N];
            int[] In = new int[N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    if (adjMatrix[i, j] != INF)
                    {
                        Sum[i]++;
                        Sum[j]++;
                        Out[i]++;
                        In[j]++;
                    }

            string result = "Таблица степеней вершин:\n";
            result += $"{"",margin}";
            result += $"{"Всего",margin}";
            result += $"{"Выходит",margin}";
            result += $"{"Входит",margin}\n";

            for (int i = 0; i < N; i++)
            {
                result += $"{$"{i}:",margin}";
                result += $"{Sum[i],margin}";
                result += $"{Out[i],margin}";
                result += $"{In[i],margin}\n";
            }
                
            return new Tuple<int[], int[], int[], string>(Sum, Out, In, result + "\n");
        }
        #endregion

        #region Вспомогательные методы
        public string PathsWithMinEdges(int[] dist, int v)
        {
            string res = "";
            for (int i = 0; i < N; ++i)
                if (dist[i] > 0 && dist[i] < INF)
                {
                    int T = i;
                    string R = "";
                    while (T != v)
                        for (int j = 0; j < N; ++j)
                            if (adjMatrix[j, T] != INF && dist[j] == dist[T] - 1)
                            {
                                T = j;
                                R = T + " => " + R;
                                break;
                            }
                    res += R;
                    res += i + "\n";
                }
            return res;
        }

        public string GetArrayString(int[] D)
        {
            string res = "";
            const int margin = -3;
            res += $"{"",margin}";
            for (int i = 0; i < N; ++i)
                res += $"{i,margin}";
            res += "\n";

            res += $"{"",margin}";
            for (int i = 0; i < N; i++)
                res += $"{(D[i] == INF ? "-" : D[i].ToString()),margin}";
            return res;
        }
        #endregion

        #region Поиск в ширину (нерекурсивный)
        public string BFS(int v, bool checkFull = false)
        {
            // Инициализация
            Visited = new bool[N];
            Dist = new int[N];
            Colors = new Color[N];
            for (int i = 0; i < N; i++)
            {
                Visited[i] = false;
                Dist[i] = INF;
                Colors[i] = Color.White;
            }

            // Первый проход
            string result = "Поиск в ширину:\n";
            result += "1 компонента связности: ";
            var res = BFSPartial(v);
            for (int i = 0; i < res.Item1.Count; i++)
                result += res.Item1[i] + " ";
            result += "\n";
            result += res.Item2;
            result += "\n";

            // Массив расстояний и минимальные пути
            string minDistance = GetArrayString(Dist);
            string pathsWithMinEdges = PathsWithMinEdges(Dist, v);

            // Если остались непосещённые вершины
            int c = 1;
            if (checkFull)
                for (int i = 0; i < Visited.Length; i++)
                    if (!Visited[i])
                    {
                        c++;
                        result += $"{c} компонента связности: ";
                        res = BFSPartial(i);
                        for (int j = 0; j < res.Item1.Count; j++)
                        {
                            result += res.Item1[j] + " ";
                            result += "\n";
                            result += res.Item2;
                            result += "\n";
                        }
                    }

            // Информация
            result += "\nМинимум переходов:\n";
            result += minDistance + "\n";
            result += "\nПути с минимальным числом рёбер:\n";
            result += pathsWithMinEdges + "\n\n";
            return result;
        }

        private Tuple<ListRef, string> BFSPartial(int u)
        {
            string traversal = "";
            ListRef component = new ListRef();

            QueueRef<int> Queue = new QueueRef<int>();
            Dist[u] = 0;
            Visited[u] = true;
            Colors[u] = Color.Gray;
            Queue.Enqueue(u);
            traversal += $"+{u} ";
            component.Add(u);

            while (!Queue.IsEmpty)
            {
                u = Queue.Dequeue();
                Colors[u] = Color.Black;
                traversal += $"-{u} ";
                
                if (adjList.ContainsKey(u))
                    foreach (var tup in adjList[u])
                    {
                        int v = tup.Item1;
                        if (Colors[v] == Color.White)
                        {
                            Dist[v] = Dist[u] + 1;
                            Visited[v] = true;
                            Colors[v] = Color.Gray;
                            Queue.Enqueue(v);
                            traversal += $"+{v} ";
                            component.Add(v);
                        }
                    }
            }
            return new Tuple<ListRef, string>(component, traversal);
        }
        #endregion

        #region Поиск в ширину (рекурсивный)
        public string BFS_R(int u, bool checkFull = false)
        {
            // Инициализация
            Visited = new bool[N];
            Dist = new int[N];
            Colors = new Color[N];
            for (int i = 0; i < N; i++)
            {
                Visited[i] = false;
                Dist[i] = INF;
                Colors[i] = Color.White;
            }

            // Первый проход
            string result = "Поиск в ширину:\n";
            result += "1 компонента связности: ";
            QueueRef<int> Q = new QueueRef<int>();
            Q.Enqueue(u);
            var res = BFSPartial_R(-1, Q, "", new ListRef());
            for (int i = 0; i < res.Item1.Count; i++)
                result += res.Item1[i] + " ";
            result += "\n";
            result += res.Item2;
            result += "\n";

            // Массив расстояний и минимальные пути
            string minDistance = GetArrayString(Dist);
            string pathsWithMinEdges = PathsWithMinEdges(Dist, u);

            // Если остались непосещённые вершины
            int c = 1;
            if (checkFull)
                for (int i = 0; i < Visited.Length; i++)
                    if (!Visited[i])
                    {
                        c++;
                        result += $"{c} компонента связности: ";
                        Q = new QueueRef<int>();
                        Q.Enqueue(i);
                        res = BFSPartial_R(-1, Q, "", new ListRef());
                        for (int j = 0; j < res.Item1.Count; j++)
                        {
                            result += res.Item1[j] + " ";
                            result += "\n";
                            result += res.Item2;
                            result += "\n";
                        }
                    }

            // Информация
            result += "\nМинимум переходов:\n";
            result += minDistance + "\n";
            result += "\nПути с минимальным числом рёбер:\n";
            result += pathsWithMinEdges + "\n\n";
            return result;
        }

        private Tuple<ListRef, string> BFSPartial_R(int u, QueueRef<int> Queue, string traversal, ListRef component)
        {
            if (Queue.IsEmpty)
                return new Tuple<ListRef, string>(component, traversal);
            
            int v = Queue.Dequeue();
            if (u == -1)
            {
                Dist[v] = 0;
                traversal += $"+{v} ";
                Visited[v] = true;
            }
            
            Colors[v] = Color.Black;
            traversal += $"-{v} ";
            component.Add(v);

            if (adjList.ContainsKey(v))
                foreach (var tup in adjList[v])
                {
                    u = tup.Item1;
                    if (Colors[u] == Color.White)
                    {
                        Colors[u] = Color.Gray;
                        Queue.Enqueue(u);
                        Dist[u] = Dist[v] + 1;
                        traversal += $"+{u} ";
                        Visited[u] = true;
                    }
                }
            return BFSPartial_R(v, Queue, traversal, component);
        }
        #endregion

        #region Поиск в ширину (нерекурсивный, через матрицу смежности)
        public string BFSAdjMatrix(int v, bool checkFull = false)
        {
            // Инициализация
            Visited = new bool[N];
            Dist = new int[N];
            Colors = new Color[N];
            for (int i = 0; i < N; i++)
            {
                Visited[i] = false;
                Dist[i] = INF;
                Colors[i] = Color.White;
            }

            // Первый проход
            string result = "Поиск в ширину:\n";
            result += "1 компонента связности: ";
            var res = BFSPartialAdjMatrix(v);
            for (int i = 0; i < res.Item1.Count; i++)
                result += res.Item1[i] + " ";
            result += "\n";
            result += res.Item2;
            result += "\n";

            // Массив расстояний и минимальные пути
            string minDistance = GetArrayString(Dist);
            string pathsWithMinEdges = PathsWithMinEdges(Dist, v);

            // Если остались непосещённые вершины
            int c = 1;
            if (checkFull)
                for (int i = 0; i < Visited.Length; i++)
                    if (!Visited[i])
                    {
                        c++;
                        result += $"{c} компонента связности: ";
                        res = BFSPartialAdjMatrix(i);
                        for (int j = 0; j < res.Item1.Count; j++)
                        {
                            result += res.Item1[j] + " ";
                            result += "\n";
                            result += res.Item2;
                            result += "\n";
                        }
                    }

            // Информация
            result += "\nМинимум переходов:\n";
            result += minDistance + "\n";
            result += "\nПути с минимальным числом рёбер:\n";
            result += pathsWithMinEdges + "\n\n";
            return result;
        }

        private Tuple<ListRef, string> BFSPartialAdjMatrix(int u)
        {
            string traversal = "";
            ListRef component = new ListRef();

            QueueRef<int> Queue = new QueueRef<int>();
            Dist[u] = 0;
            Visited[u] = true;
            Colors[u] = Color.Gray;
            Queue.Enqueue(u);
            traversal += $"+{u} ";
            component.Add(u);

            while (!Queue.IsEmpty)
            {
                u = Queue.Dequeue();
                Colors[u] = Color.Black;
                traversal += $"-{u} ";

                for (int v = 0; v < N; v++)
                    if (adjMatrix[u, v] != INF)
                        if (Colors[v] == Color.White)
                        {
                            Dist[v] = Dist[u] + 1;
                            Visited[v] = true;
                            Colors[v] = Color.Gray;
                            Queue.Enqueue(v);
                            traversal += $"+{v} ";
                            component.Add(v);
                        }
            }
            return new Tuple<ListRef, string>(component, traversal);
        }
        #endregion

        #region Поиск в глубину (нерекурсивный)
        public string DFS(int v, bool checkFull = false)
        {
            // Инициализация
            time = 0;
            Colors = new Color[N];
            entryTime = new int[N];
            exitTime = new int[N];
            Visited = new bool[N];
            St = new StackRef<int>();
            for (int i = 0; i < N; i++)
            {
                Colors[i] = Color.White;
                entryTime[i] = 0;
                exitTime[i] = 0;
            }

            // Первый проход
            string result = "Поиск в глубину:\n";
            result += "1 компонента связности: ";
            res = "";
            var rslt = DFSPartial(v);
            for (int i = 0; i < rslt.Item1.Count; i++)
                result += rslt.Item1[i] + " ";
            result += "\n";
            result += rslt.Item2;
            result += "\n";

            // Если остались непосещённые вершины
            int c = 1;
            if (checkFull)
                for (int i = 0; i < N; i++)
                    if (Colors[i] == Color.White)
                    {
                        c++;
                        result += $"{c} компонента связности: ";
                        res = "";
                        rslt = DFSPartial(i);
                        for (int j = 0; j < rslt.Item1.Count; j++)
                            result += rslt.Item1[j] + " ";
                        result += "\n";
                        result += rslt.Item2;
                        result += "\n";
                    }

            // Информация
            result += "\nВремя захода в вершину:\n";
            result += GetArrayString(entryTime) + "\n";
            result += "\nВремя выхода из вершины:\n";
            result += GetArrayString(exitTime) + "\n\n";
            return result;
        }

        public Tuple<ListRef, string> DFSPartial(int u)
        {
            string traversal = "";
            ListRef component = new ListRef();

            StackRef<int> GrayPath = new StackRef<int>();
            GrayPath.Push(u);
            while (!GrayPath.IsEmpty)
            {
                int V = GrayPath.Peek();
                if (Colors[V] == Color.White)
                {
                    Colors[V] = Color.Gray;
                    traversal += $"({V} ";
                    Visited[V] = true;
                    entryTime[V] = time++;
                    component.Add(V);
                }

                bool FoundWhite = false;

                if (adjList.ContainsKey(V))
                    foreach (var tup in adjList[V])
                    {
                        int v = tup.Item1;
                        if (Colors[v] == Color.White)
                        {
                            FoundWhite = true;
                            GrayPath.Push(v);
                            break;
                        }
                    }

                if (!FoundWhite)
                {
                    traversal += $" {V})";
                    exitTime[V] = time++;
                    Colors[V] = Color.Black;
                    GrayPath.Pop();
                    St.Push(V);
                }
            }

            return new Tuple<ListRef, string>(component, traversal);
        }
        #endregion

        #region Поиск в глубину (рекурсивный)
        public string DFS_R(int v, bool checkFull = false)
        {
            // Инициализация
            time = 0;
            Colors = new Color[N];
            entryTime = new int[N];
            exitTime = new int[N];
            for (int i = 0; i < N; i++)
            {
                Colors[i] = Color.White;
                entryTime[i] = 0;
                exitTime[i] = 0;
            }

            // Первый проход
            string result = "Поиск в глубину:\n";
            result += "1 компонента связности: ";
            res = "";
            var rslt = DFS_Vizit_R(v, new ListRef());
            for (int i = 0; i < rslt.Item1.Count; i++)
                result += rslt.Item1[i] + " ";
            result += "\n";
            result += rslt.Item2;
            result += "\n";

            // Если остались непосещённые вершины
            int c = 1;
            if (checkFull)
                for (int i = 0; i < N; i++)
                    if (Colors[i] == Color.White)
                    {
                        c++;
                        result += $"{c} компонента связности: ";
                        res = "";
                        rslt = DFS_Vizit_R(i, new ListRef());
                        for (int j = 0; j < rslt.Item1.Count; j++)
                            result += rslt.Item1[j] + " ";
                        result += "\n";
                        result += rslt.Item2;
                        result += "\n";
                    }

            // Информация
            result += "\nВремя захода в вершину:\n";
            result += GetArrayString(entryTime) + "\n";
            result += "\nВремя выхода из вершины:\n";
            result += GetArrayString(exitTime) + "\n\n";
            return result;
        }

        private Tuple<ListRef, string> DFS_Vizit_R(int v, ListRef component)
        {
            Colors[v] = Color.Gray;
            entryTime[v] = time++;
            res += $"({v} ";
            component.Add(v);
          
            if (adjList.ContainsKey(v))
                foreach (var tup in adjList[v])
                {
                    int u = tup.Item1;
                    if (Colors[u] == Color.White)
                        DFS_Vizit_R(u, component);
                }

            Colors[v] = Color.Black;
            res += $" {v})";
            exitTime[v] = time++;
            return new Tuple<ListRef, string>(component, res);
        }
        #endregion

        #region Поиск в глубину (нерекурсивный, через матрицу смежности)
        public string DFSAdjMatrix(int v, bool checkFull = false)
        {
            // Инициализация
            time = 0;
            Colors = new Color[N];
            entryTime = new int[N];
            exitTime = new int[N];
            Visited = new bool[N];
            St = new StackRef<int>();
            for (int i = 0; i < N; i++)
            {
                Colors[i] = Color.White;
                entryTime[i] = 0;
                exitTime[i] = 0;
            }

            // Первый проход
            string result = "Поиск в глубину:\n";
            result += "1 компонента связности: ";
            res = "";
            var rslt = DFSPartialAdjMatrix(v);
            for (int i = 0; i < rslt.Item1.Count; i++)
                result += rslt.Item1[i] + " ";
            result += "\n";
            result += rslt.Item2;
            result += "\n";

            // Если остались непосещённые вершины
            int c = 1;
            if (checkFull)
                for (int i = 0; i < N; i++)
                    if (Colors[i] == Color.White)
                    {
                        c++;
                        result += $"{c} компонента связности: ";
                        res = "";
                        rslt = DFSPartialAdjMatrix(i);
                        for (int j = 0; j < rslt.Item1.Count; j++)
                            result += rslt.Item1[j] + " ";
                        result += "\n";
                        result += rslt.Item2;
                        result += "\n";
                    }

            // Информация
            result += "\nВремя захода в вершину:\n";
            result += GetArrayString(entryTime) + "\n";
            result += "\nВремя выхода из вершины:\n";
            result += GetArrayString(exitTime) + "\n\n";
            return result;
        }

        public Tuple<ListRef, string> DFSPartialAdjMatrix(int u)
        {
            string traversal = "";
            ListRef component = new ListRef();

            StackRef<int> GrayPath = new StackRef<int>();
            GrayPath.Push(u);
            while (!GrayPath.IsEmpty)
            {
                int V = GrayPath.Peek();
                if (Colors[V] == Color.White)
                {
                    Colors[V] = Color.Gray;
                    traversal += $"({V} ";
                    Visited[V] = true;
                    entryTime[V] = time++;
                    component.Add(V);
                }

                bool FoundWhite = false;

                for (int v = 0; v < N; v++)
                    if (adjMatrix[V, v] != INF)
                        if (Colors[v] == Color.White)
                        {
                            FoundWhite = true;
                            GrayPath.Push(v);
                            break;
                        }

                if (!FoundWhite)
                {
                    traversal += $" {V})";
                    exitTime[V] = time++;
                    Colors[V] = Color.Black;
                    GrayPath.Pop();
                    St.Push(V);
                }
            }

            return new Tuple<ListRef, string>(component, traversal);
        }
        #endregion

        #region Обратный граф
        public Graph GetReverse()
        {
            int[,] adjMatrixNew = new int[N, N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    adjMatrixNew[i, j] = adjMatrix[j, i];
            return new Graph(adjMatrixNew);
        }
        #endregion

        #region Поиск сильно связанных компонент
        public Tuple<List<ListRef>, string> SCC()
        {
            // Инициализация
            St = new StackRef<int>();
            Visited = new bool[N];
            Colors = new Color[N];
            entryTime = new int[N];
            exitTime = new int[N];
            for (int i = 0; i < N; i++)
                Colors[i] = Color.White;
            // Поиск в глубину
            for (int i = 0; i < N; i++)
                if (!Visited[i])
                    DFSPartial(i);

            // Получить обратный граф
            Graph g = GetReverse();
            // Инициализация
            g.Visited = new bool[N];
            g.Colors = new Color[N];
            for (int i = 0; i < N; i++)
                g.Colors[i] = Color.White;
            g.entryTime = new int[N];
            g.exitTime = new int[N];
            g.St = new StackRef<int>();

            // Результат
            string result = "Сильно связные компоненты:\n";
            List<ListRef> components = new List<ListRef>();
            while (!St.IsEmpty)
            {
                int v = St.Pop();
                if (!g.Visited[v])
                {
                    var x = g.DFSPartial(v);
                    result += x.Item1 + "\n";
                    components.Add(x.Item1);
                }
            }
            return new Tuple<List<ListRef>, string>(components, result);
        }
        #endregion

        #region Является ли граф Эйлеровым
        private bool isEulerGraph()
        {
            for (int i = 0; i < N; i++)
            {
                int c = 0;
                for (int j = 0; j < N; j++)
                    if (adjMatrix[i, j] != INF)
                        c++;
                if (c % 2 != 0)
                    return false;
            }
            return true;
        }
        #endregion

        #region Поиск Эйлеровых циклов (рекурсивный)
        public Tuple<ListRef, string> EulerCicle(int v)
        {
            if (!isEulerGraph())
                return new Tuple<ListRef, string>(null, "Граф не является Эйлеровым\n");

            int[,] adjMatrixNew = new int[N, N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    adjMatrixNew[i, j] = adjMatrix[i, j];
            Graph g = new Graph(adjMatrixNew);

            g.res = "";
            g.Cycle = new ListRef();
            g.EulerCicle_Main(v);

            string result = "Эйлеров цикл:\n";
            result += g.res + "\n";
            return new Tuple<ListRef, string>(g.Cycle, result);
        }

        public void EulerCicle_Main(int v)
        {
            for (int i = 0; i < N; i++)
                if (adjMatrix[v, i] != INF)
                {
                    adjMatrix[v, i] = INF;
                    adjMatrix[i, v] = INF;
                    EulerCicle_Main(i);
                }
            Cycle.Add(v);
            res += v + " ";
        }
        #endregion

        #region Поиск Эйлеровых циклов (нерекурсивный)
        public Tuple<ListRef, string> EulerCicle2(int v)
        {
            if (!isEulerGraph())
                return new Tuple<ListRef, string>(null, "Граф не является Эйлеровым\n");

            string res = "";
            ListRef Cycle = new ListRef();
            St = new StackRef<int>();
            St.Push(v);
            while (!St.IsEmpty)
            {
                int vv = St.Peek();
                if (!adjList.ContainsKey(vv))
                {
                    int u = St.Pop();
                    res += u + " ";
                    Cycle.Add(u);
                }
                else
                {
                    int u = adjList[vv][0].Item1;
                    St.Push(u);
                    DeleteEdge(vv, u, false);
                }
            }

            string result = "Эйлеров цикл:\n";
            result += res + "\n";
            return new Tuple<ListRef, string>(Cycle, result);
        }
        #endregion

        #region Эйлеров цикл (алгоритм Флёри)
        public Tuple<ListRef, string> EulerCicleFleury(int u)
        {
            if (!isEulerGraph())
                return new Tuple<ListRef, string>(null, "Граф не является Эйлеровым\n");

            int[,] adjMatrixNew = new int[N, N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    adjMatrixNew[i, j] = adjMatrix[i, j];
            Graph g = new Graph(adjMatrixNew);
            g.res = $"{u} ";
            g.Cycle = new ListRef();
            g.Cycle.Add(u);
            g.EulerUtil(u);

            string result = "Эйлеров цикл:\n";
            result += g.res + "\n";
            return new Tuple<ListRef, string>(g.Cycle, result);
        }

        private void EulerUtil(int u)
        {
            if (!adjList.ContainsKey(u))
                return;

            for (int i = adjList[u].Count - 1; i >= 0; i--)
            {
                if (!adjList.ContainsKey(u))
                    return;
                int v = adjList[u][i].Item1;

                if (isValidNextEdge(u, v))
                {
                    res += v + " ";
                    Cycle.Add(v);

                    DeleteEdge(u, v, false);
                    EulerUtil(v);
                }
            }
        }

        private bool isValidNextEdge(int u, int v)
        {
            if (adjList[u].Count == 1)
                return true;

            // Инициализация
            time = 0;
            Colors = new Color[N];
            entryTime = new int[N];
            exitTime = new int[N];
            Visited = new bool[N];
            St = new StackRef<int>();
            for (int i = 0; i < N; i++)
            {
                Colors[i] = Color.White;
                entryTime[i] = 0;
                exitTime[i] = 0;
            }
            int count1 = DFSPartial(u).Item1.Count;

            int len = adjMatrix[u, v];
            DeleteEdge(u, v, false);
            // Инициализация
            time = 0;
            Colors = new Color[N];
            entryTime = new int[N];
            exitTime = new int[N];
            Visited = new bool[N];
            St = new StackRef<int>();
            for (int i = 0; i < N; i++)
            {
                Colors[i] = Color.White;
                entryTime[i] = 0;
                exitTime[i] = 0;
            }
            int count2 = DFSPartial(u).Item1.Count;

            AddEdge(u, v, len, false);
            return (count1 > count2) ? false : true;
        }
        #endregion

        #region Множество фундаментальных циклов
        public Tuple<List<ListRef>, string> FS_Cycles(int v)
        {
            CyclesFS = new List<ListRef>();
            Mark = new int[N];
            S = new int[N + 1];
            res = "Множество фундаментальных циклов:\n";
            FS_Cycles_Main(v, 0);
            return new Tuple<List<ListRef>, string>(CyclesFS, res);
        }

        public void FS_Cycles_Main(int x, int y = 0)
        {
            count++;
            Mark[x] = count;
            for (int v = 0; v < N; v++)
                if (adjMatrix[x, v] != INF)
                {
                    S[ind++] = v;
                    if (Mark[v] == 0)
                        FS_Cycles_Main(v, x);
                    else if (Mark[v] < Mark[x] && v != y)
                    {
                        ListRef cycle = new ListRef();
                        int indNew = ind - 1;
                        do
                        {
                            res += S[indNew] + " ";
                            cycle.Add(S[indNew]);
                            indNew--;
                        } while (indNew >= 0 && S[indNew] != v);
                        res += "\n";
                        CyclesFS.Add(cycle);
                    }
                    ind--;
                }
        }
        #endregion

        #region Гамильтоновы циклы (стандартный метод)
        public Tuple<List<ListRef>, string> FindHameltonianСyclesDefault(int v)
        {
            S = new int[N];
            Visited = new bool[N];
            S[0] = v;
            Visited[v] = true;
            res = "Гамильтоновы циклы:\n";
            CyclesFS = new List<ListRef>();
            G_Circle(1, v);
            return new Tuple<List<ListRef>, string>(CyclesFS, res + "\n");
        }

        public void G_Circle(int u, int k)
        {
            int v = S[u - 1];
            for (int j = 0; j < N; j++)
                if (adjMatrix[v, j] != INF)
                    if (u == N   && j == k)
                    {
                        Cycle = new ListRef();
                        for (int i = 0; i < N; i++)
                        {
                            res += S[i] + " ";
                            Cycle.Add(S[i]);
                        }
                        res += S[0] + "\n";
                        Cycle.Add(S[0]);
                        CyclesFS.Add(Cycle);
                    }
                    else if (!Visited[j])
                    {
                        S[u] = j;
                        Visited[j] = true;
                        G_Circle(u + 1, k);
                        Visited[j] = false;
                    }
        }
        #endregion

        #region Гамильтоновы циклы (арифметический метод)
        public Tuple<List<string>, string> FindHameltonianСyclesArithmetic(int v)
        {
            int n = adjMatrix.GetLength(0);
            string[,] A = new string[n, n];
            List<string>[,] B = new List<string>[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (adjMatrix[i, j] == INF)
                    {
                        A[i, j] = "0";
                        B[i, j] = new List<string>();
                    }
                    else
                    {
                        A[i, j] = "1";
                        B[i, j] = new List<string>() { j.ToString() + " " };
                    }

            List<string>[,] P2Prime = new List<string>[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    P2Prime[i, j] = new List<string>();
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < n; k++)
                        if (A[k, j] == "1" && B[i, k].Count != 0)
                            P2Prime[i, j].Add(B[i, k][0]);

            for (int i = 0; i < n - 2; i++)
            {
                F(P2Prime);
                P2Prime = MulStrMatrices(B, P2Prime);
            }

            List<string> res = P2Prime[v, v];
            
            string result = "Гамильтоновы циклы:\n";
            for (int i = 0; i < res.Count; i++)
                result += v + " " + res[i] + v + "\n";
            result += "\n";

            return new Tuple<List<string>, string>(res, result);
        }

        private List<string>[,] MulStrMatrices(List<string>[,] a, List<string>[,] b)
        {
            int n = a.GetLength(0);
            List<string>[,] res = new List<string>[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    res[i, j] = new List<string>();

            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < n; k++)
                        res[i, j].AddRange(MulElements(a[i, k], b[k, j]));

            return res;
        }

        private List<string> MulElements(List<string> a, List<string> b)
        {
            if (a.Count == 0 || b.Count == 0)
                return new List<string>();

            List<string> res = new List<string>();
            for (int i = 0; i < a.Count; i++)
                for (int j = 0; j < b.Count; j++)
                    res.Add(a[i] + b[j]);
            return res;
        }

        private void F(List<string>[,] a)
        {
            int n = a.GetLength(0);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    if (i == j)
                        a[i, j] = new List<string>();
                    else
                        for (int k = a[i, j].Count - 1; k >= 0; k--)
                            if (a[i, j][k].Contains(i.ToString())
                              || a[i, j][k].Contains(j.ToString()))
                                a[i, j].RemoveAt(k);
        }
        #endregion  

        #region Гамельтоновы циклы (Робертс - Флорес)
        public Tuple<List<List<int>>, string> FindHameltonianСyclesRobertsFlores(int v)
        {
            int[,] M = new int[0, 0];
            FillM(ref M);

            string res = "Гамильтоновы циклы:\n";
            List<List<int>> cycles = new List<List<int>>();
            List<int> path = new List<int>();
            path.Add(v);
            // Массив глубины для каждой вершины
            int[] depth = new int[N];
            for (int i = 0; i < N; i++)
                depth[i] = -1;
            while (path.Count > 0)
            {
                // vertex = последний элемент в текущем пути
                int vertex = path[path.Count - 1];
                // Увеличиваем глубину
                depth[vertex]++;
                // Если не достигли дна - выбираем следующую вершину, иначе присваеваем INF 
                int next_node = INF;
                if (depth[vertex] < M.GetLength(0))
                    next_node = M[depth[vertex], vertex];
                // Если следуюящая вершина найдена и она не находится в текущем пути
                if (next_node != INF && !path.Contains(next_node))
                {
                    path.Add(next_node);
                    depth[next_node] = -1;
                    continue;
                }
                else if (path.Contains(next_node))
                    continue;
                // Вершина не подходит - производим возврат на шаг назад
                else
                {
                    // Гамельтонов путь построен - выводим
                    if (path.Count == N && adjMatrix[vertex, v] != INF)
                    {
                        List<int> cycle = new List<int>();
                        for (int i = 0; i < N; i++)
                        {
                            res += path[i] + " ";
                            cycle.Add(path[i]);
                        }
                        res += path[0] + "\n";
                        cycle.Add(path[0]);
                        cycles.Add(cycle);
                    }
                    // Удаляем последний элемент в пути
                    path.RemoveAt(path.Count - 1);
                }
            }
            return new Tuple<List<List<int>>, string>(cycles, res);
        }

        private void FillM(ref int[,] M)
        {
            int maxC = 0;
            for (int i = 0; i < N; i++)
            {
                int c = 0;
                for (int j = 0; j < N; j++)
                    if (adjMatrix[i, j] != INF)
                    {
                        c++;
                        if (c > maxC)
                            maxC = c;
                    }
            }

            M = new int[maxC, N];
            for (int i = 0; i < N; i++)
            {
                int ind = 0;
                for (int j = 0; j < N; j++)
                    if (adjMatrix[i, j] != INF)
                    {
                        M[ind, i] = j;
                        ind++;
                    }
                for (int j = ind; j < maxC; j++)
                    M[j, i] = INF;
            }
        }
        #endregion

        #region Построение минимального остовного дерева (Крускал) - выбираем минимальные рёбра, не образующие циклов
        public Tuple<Graph, string> MST_Kruskal()
        {
            parent = new int[N];
            rank = new int[N];
            for (int i = 0; i < N; i++)
                MakeSet(i);
            List<int[]> E = new List<int[]>();
            for (int i = 0; i < N; i++)
                for (int j = i; j < N; j++)
                    if (adjMatrix[i, j] != INF)
                    {
                        int[] e = new int[] { i, j, adjMatrix[i, j] };
                        E.Add(e);
                    }

            int[,] newMatrix = new int[N, N];
            E = E.OrderBy(x => x[2]).ToList();
            int totalWeight = 0;
            int c = 0;
            string result = "Добавление ребер:\n";
            for (int i = 0; i < E.Count; i++)
            {
                int u = E[i][0];
                int v = E[i][1];
                if (Find_Set(u) != Find_Set(v))
                {
                    c++;
                    totalWeight += E[i][2];
                    Union(u, v);
                    newMatrix[u, v] = adjMatrix[u, v];
                    newMatrix[v, u] = adjMatrix[v, u];
                    result += $"{u}-{v} ({E[i][2]})\n";
                }
            }
            if (c != N - 1)
                return new Tuple<Graph, string>(null, "Граф несвязный");
            result += "Вес минимального остовного дерева: " + totalWeight + "\n";
            Graph newG = new Graph(newMatrix);
            result += newG.GetAdjMatrixString() + "\n";
            return new Tuple<Graph, string>(newG, result);
        }

        private void MakeSet(int v)
        {
            parent[v] = v;
        }

        private int Find_Set(int v)
        {
            if (v == parent[v])
                return v;
            return parent[v] = Find_Set(parent[v]);
        }

        private void Union(int u, int v)
        {
            u = Find_Set(u);
            v = Find_Set(v);
            if (rank[u] < rank[v])
                parent[u] = v;
            else
            {
                parent[v] = u;
                if (rank[u] == rank[v])
                    ++rank[u];
            }
        }
        #endregion

        #region Построение минимального остовного дерева (Прим) - выбираем случайную вершину и от неё строим дерево
        public Tuple<Graph, string> PrimMST(int v)
        {
            int[] parent = new int[N];
            int[] key = new int[N];
            bool[] mstSet = new bool[N];
            List<int> added = new List<int>() { v };

            for (int i = 0; i < N; i++)
            {
                key[i] = int.MaxValue;
                mstSet[i] = false;
            }

            key[v] = 0;
            parent[v] = -1;

            int c = 0;
            int totalWeight = 0;
            int[,] newMatrix = new int[N, N];
            string result = "";
            for (int count = 0; count < N; count++)
            {
                int u = minKey(key, mstSet);
                mstSet[u] = true;
                for (int j = 0; j < N; j++)
                    if (adjMatrix[u, j] != INF && mstSet[j] == false && adjMatrix[u, j] < key[j])
                    {
                        parent[j] = u;
                        key[j] = adjMatrix[u, j];
                    }
                
                if (parent[u] != -1)
                {
                    result += $"Добавили ребро: {parent[u]}-{u} ({adjMatrix[parent[u], u]})\n\n";
                    c++;
                    totalWeight += adjMatrix[parent[u], u];
                    newMatrix[parent[u], u] = adjMatrix[parent[u], u];
                    newMatrix[u, parent[u]] = adjMatrix[u, parent[u]];
                }

                // Вывод разреза
                added.Add(u);
                if (count != N - 1)
                    result += "Разрез:\n";
                for (int i = 0; i < N; i++)
                    if (adjList.ContainsKey(i))
                        for (int j = 0; j < adjList[i].Count; j++)
                        {
                            int k = adjList[i][j].Item1;
                            if (i <= k && (added.Contains(i) && !added.Contains(k) || !added.Contains(i) && added.Contains(k)))
                                result += $"{i}-{k} ({adjList[i][j].Item2})\n";
                        }
            }
            if (c != N - 1)
                return new Tuple<Graph, string>(null, "Граф несвязный");
            result += "Вес минимального остовного дерева: " + totalWeight + "\n";
            Graph newG = new Graph(newMatrix);
            result += newG.GetAdjMatrixString() + "\n";
            return new Tuple<Graph, string>(newG, result);
        }

        private int minKey(int[] key, bool[] mstSet)
        {
            int min = int.MaxValue;
            int min_index = -1;
            for (int v = 0; v < N; v++)
                if (mstSet[v] == false && key[v] < min)
                {
                    min = key[v];  // длина дороги
                    min_index = v;  // индекс вершины
                }
            return min_index;
        }
        #endregion

        #region Пути от одной вершины до всех остальных через массив предков
        public string GetPath(int v, int[] parent)
        {
            if (parent[v] == -1)
                return "";
            string res = "";
            int cur = v;
            while (cur != -1)
            {
                if (res == "")
                    res = cur.ToString();
                else
                    res = cur + "-" + res;
                cur = parent[cur];
            }
            return res;
        }

        public List<string> GetPaths(int[] parent)
        {
            List<string> res = new List<string>();
            for (int i = 0; i < N; i++)
                res.Add(GetPath(i, parent));
            return res;
        }

        public List<string> GetPathsFull(int u, int[] distance, int[] parent)
        {
            List<string> res = GetPaths(parent);
            for (int i = 0; i < N; i++)
                if (i != u)
                    if (distance[i] == INF)
                        res[i] = $"{u}->{i}: -";
                    else
                        res[i] = $"{u}->{i}: {res[i]} ({distance[i]})";
            return res;
        }

        public string GetPathsFullString(int u, int[] distances, int[] parents)
        {
            string res = "";
            res += $"Пути и расстояния от вершины {u} до всех остальных:\n";
            List<string> paths = GetPathsFull(u, distances, parents);
            foreach (string path in paths)
                if (path != "")
                    res += path + "\n";
            return res;
        }
        #endregion

        #region Пути от всех вершин до всех остальных через массив предков
        public string GetPath(int[,] parents, int i, int j)
        {
            res = "";
            string path = GetPathMain(parents, i, j);
            return i + "-" + path + j;
        }

        public string GetPathMain(int[,] parents, int i, int j)
        {
            int k = parents[i, j];
            if (k == -1)
                return res;
            GetPathMain(parents, i, k);
            res += k + "-";
            GetPathMain(parents, k, j);
            return res;
        }

        public List<string> GetPaths(int[,] parent)
        {
            List<string> res = new List<string>();
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    if (i != j)
                    {
                        res.Add(GetPath(parent, i, j));
                    }

            return res;
        }

        public List<string> GetPathsFull(int[,] distance, int[,] parent)
        {
            List<string> res = GetPaths(parent);
            int ind = 0;
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                {
                    if (i != j)
                    {
                        if (distance[i, j] == INF)
                            res[ind] = $"{i}->{j}: -";
                        else
                            res[ind] = $"{i}->{j}: {res[ind]} ({distance[i, j]})";
                        ind++;
                    }
                }
            return res;
        }

        public string GetPathsFullString(int[,] distance, int[,] parent)
        {
            string res = "";
            res += $"Пути и расстояния от всех вершин до всех :\n";
            List<string> paths = GetPathsFull(distance, parent);
            foreach (string path in paths)
                res += path + "\n";
            return res;
        }
        #endregion

        #region Алгоритм Дейкстры O(V^2) - список смежности (dist, parent, string)
        public Tuple<int[], int[], string> DijkstraAdjacencyList(int v)
        {
            int[] distance = new int[N];
            bool[] visited = new bool[N];
            int[] parent = new int[N];
            for (int i = 0; i < N; i++)
            {
                distance[i] = INF;
                visited[i] = false;
                parent[i] = -1;
            }

            distance[v] = 0;
            for (int i = 0; i < N - 1; i++)
            {
                int minDistance = INF;
                int U = -1;
                for (int j = 0; j < N; j++)
                    if (!visited[j] && distance[j] <= minDistance)
                    {
                        minDistance = distance[j];
                        U = j;
                    }
                if (adjList.ContainsKey(U))
                    foreach (var vertexTo_Len in adjList[U])
                    {
                        int V = vertexTo_Len.Item1;
                        int len = vertexTo_Len.Item2;
                        if (distance[U] + len < distance[V])
                        {
                            distance[V] = distance[U] + len;
                            parent[V] = U;
                        }
                    }
                visited[U] = true;
            }
            return new Tuple<int[], int[], string> (distance, parent, GetPathsFullString(v, distance, parent));
        }
        #endregion

        #region Беллман - Форд O(E * V)
        public Tuple<int[], int[], string> BellmanFord(int v)
        {
            int[] distance = new int[N];
            int[] parent = new int[N];
            for (int i = 0; i < N; i++)
            {
                distance[i] = INF;
                parent[i] = -1;
            }

            distance[v] = 0;
            for (int i = 0; i < N - 1; i++)
            {
                bool flag = true;
                foreach (var startVertex_adj in adjList)
                {
                    int U = startVertex_adj.Key;
                    foreach (var vertexTo_Len in startVertex_adj.Value)
                    {
                        int V = vertexTo_Len.Item1;
                        int len = vertexTo_Len.Item2;
                        if (distance[U] != INF && distance[U] + len < distance[V])
                        {
                            distance[V] = distance[U] + len;
                            parent[V] = U;
                            flag = false;
                        }
                    }
                }
                if (flag)
                    break;
            }

            foreach (var startVertex_adj in adjList)
            {
                int U = startVertex_adj.Key;
                foreach (var vertexTo_Len in startVertex_adj.Value)
                {
                    int V = vertexTo_Len.Item1;
                    int len = vertexTo_Len.Item2;
                    if (distance[U] != INF && distance[U] + len < distance[V])
                        return new Tuple<int[], int[], string>(null, null, null);
                }
            }
            return new Tuple<int[], int[], string>(distance, parent, GetPathsFullString(v, distance, parent));
        }
        #endregion

        #region Флойд - Уоршал O(V^3) 
        public Tuple<int[,], int[,], string> FloydWarshall()
        {
            int[,] distance = new int[N, N];
            int[,] parent = new int[N, N];

            for (int i = 0; i < N; ++i)
                for (int j = 0; j < N; ++j)
                {
                    distance[i, j] = adjMatrix[i, j];
                    parent[i, j] = -1;
                }
            for (int i = 0; i < N; i++)
                distance[i, i] = 0;

            for (int k = 0; k < N; k++)
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < N; j++)
                        if (distance[i, k] != INF && distance[k, j] != INF && distance[i, k] + distance[k, j] < distance[i, j])
                        {
                            distance[i, j] = distance[i, k] + distance[k, j];
                            parent[i, j] = k;
                        }

            for (int i = 0; i < N; i++)
                if (distance[i, i] < 0)
                    return new Tuple<int[,], int[,], string>(null, null, null);
            return new Tuple<int[,], int[,], string>(distance, parent, GetPathsFullString(distance, parent));
        }
        #endregion

        #region Алгоритм Левита
        public Tuple<int[], int[], string> Levit(int v)
        {
            int[] distance = new int[N];
            bool[] inQueue = new bool[N];
            int[] parent = new int[N];
            for (int i = 0; i < N; i++)
            {
                distance[i] = INF;
                inQueue[i] = false;
                parent[i] = -1;
            }

            distance[v] = 0;
            inQueue[v] = true;
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(v);
            while (queue.Count != 0)
            {
                int U = queue.Dequeue();
                inQueue[U] = false;
                //visited[U] = true;
                if (adjList.ContainsKey(U))
                    foreach (var vertexTo_Len in adjList[U])
                    {
                        int V = vertexTo_Len.Item1;
                        int len = vertexTo_Len.Item2;
                        if (distance[U] + len < distance[V])
                        {
                            distance[V] = distance[U] + len;
                            parent[V] = U;
                            if (!inQueue[V])
                            {
                                queue.Enqueue(V);
                                inQueue[V] = true;
                            }
                        }
                    }
            }
            return new Tuple<int[], int[], string>(distance, parent, GetPathsFullString(v, distance, parent));
        }
        #endregion

        #region Алгоритм Джонсона
        public Graph Johnson()
        {
            int[,] adjMatrixNew = new int[N, N];
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    adjMatrixNew[i, j] = adjMatrix[i, j];
            Graph g = new Graph(adjMatrixNew);
            g.AddVertex();
            for (int i = 0; i < N; i++)
                g.AddEdge(N, i, 0, true);
            
            int[] distance = g.BellmanFord(N).Item1;
            Graph g2 = new Graph(N);
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    if (adjMatrix[i, j] != INF)
                        g2.AddEdge(i, j, adjMatrix[i, j] + distance[i] - distance[j], true);
            return g2;
        }
        #endregion

        #region Пути
        #region Задача о числе путей из u в v в ациклическом графе (без рекурсии)
        public List<int> topological_sorting(int n)
        {
            int[] fre = GetVertexPower().Item3;
            Queue<int> q = new Queue<int>();

            // insert all vertices which
            // don't have any parent.
            for (int i = 0; i < n; i++)
                if (fre[i] == 0)
                    q.Enqueue(i);

            List<int> l = new List<int>();

            // using kahn's algorithm
            // for topological sorting
            while (q.Count > 0)
            {
                int u = q.Peek();
                q.Dequeue();

                // insert front element of queue to list
                l.Add(u);

                // go through all its childs
                if (!adjList.ContainsKey(u))
                {
                    continue;
                }
                for (int i = 0; i < adjList[u].Count; i++)
                {
                    fre[adjList[u][i].Item1]--;

                    // whenever the frequency is zero then add
                    // this vertex to queue.
                    if (fre[adjList[u][i].Item1] == 0)
                        q.Enqueue(adjList[u][i].Item1);
                }
            }
            return l;
        }

        // Function that returns the number of paths
        public int numberofPaths(int source, int destination)
        {
            // make topological sorting
            List<int> s = topological_sorting(N);

            // to store required answer.
            int[] dp = new int[N];

            // answer from destination
            // to destination is 1.
            dp[destination] = 1;

            // traverse in reverse order
            for (int i = s.Count - 1; i >= 0; i--)
            {
                if (!adjList.ContainsKey(s[i]))
                {
                    continue;
                }
                for (int j = 0; j < adjList[s[i]].Count; j++)
                {
                    dp[s[i]] += dp[adjList[s[i]][j].Item1];
                }
            }

            return dp[source];
        }
        #endregion

        #region Сложение графов
        public static Graph SumGraphs(Graph[] graphs)
        {
            // Определить максимальную размерность
            int maxN = -1;
            for (int i = 0; i < graphs.Length; i++)
                if (graphs[i].N > maxN)
                    maxN = graphs[i].N;
            int[,] matr = new int[maxN, maxN];

            // Сложить элементы
            for (int k = 0; k < graphs.Length; k++)
                for (int i = 0; i < graphs[k].N; i++)
                    for (int j = 0; j < graphs[k].N; j++)
                        matr[i, j] += graphs[k].adjMatrix[i, j];

            // Если получилось > 0, то делаем 1
            for (int i = 0; i < maxN; i++)
                for (int j = 0; j < maxN; j++)
                    if (matr[i, j] != 0)
                        matr[i, j] = 1;
            return new Graph(matr);
        }
        
        public static Graph SumGraphs2(Graph[] graphs)
        {
            Dictionary<int, List<Tuple<int, int>>> adjL = new Dictionary<int, List<Tuple<int, int>>>();
            for (int i = 0; i < graphs.Length; i++)
            {
                foreach (var item in graphs[i].adjList)
                {
                    if (adjL.ContainsKey(item.Key))
                        adjL[item.Key].AddRange(item.Value);
                    else
                        adjL[item.Key] = item.Value;
                    adjL[item.Key] = adjL[item.Key].Distinct().ToList();
                }
            }

            int maxN = -1;
            for (int i = 0; i < graphs.Length; i++)
                if (graphs[i].N > maxN)
                    maxN = graphs[i].N;

            return new Graph(maxN, adjL);
        }
        #endregion

        #region Пути из вершины u
        public List<string> RoadsFrom(int u, List<int> including = null, List<int> excluding = null)
        {
            roads = new List<string>();
            RoadsFromMain(u, u.ToString());
            ApplyFilters(including, excluding);
            return roads;
        }

        public void RoadsFromMain(int u, string res)
        {
            for (int i = 0; i < N; i++)
                if (adjMatrix[u, i] != INF && adjMatrix[u, i] != 0)
                {
                    string newRes = res + " " + i;
                    roads.Add(newRes);
                    RoadsFromMain(i, newRes);
                }
        }
        #endregion

        #region Пути в вершину v
        public List<string> RoadsTo(int v, List<int> including = null, List<int> excluding = null)
        {
            roads = new List<string>();
            RoadsToMain(v, v.ToString());
            ApplyFilters(including, excluding);
            return roads;
        }

        public void RoadsToMain(int v, string res)
        {
            for (int i = 0; i < N; i++)
                if (adjMatrix[i, v] != INF && adjMatrix[i, v] != 0)
                {
                    string newRes = i + " " + res;
                    roads.Add(newRes);
                    RoadsToMain(i, newRes);
                }
        }
        #endregion

        #region Пути длины len
        public List<string> RoadsLength(int len, List<int> including = null, List<int> excluding = null)
        {
            roads = new List<string>();
            for (int i = 0; i < N; i++)
                RoadsLengthMain(i, len, i.ToString());
            ApplyFilters(including, excluding);
            return roads;
        }

        public void RoadsLengthMain(int u, int len, string res)
        {
            if (len > 1)
            {
                for (int i = 0; i < N; i++)
                    if (adjMatrix[u, i] != INF && adjMatrix[u, i] != 0)
                    {
                        string newRes = res + " " + i;
                        RoadsLengthMain(i, len - 1, newRes);
                    }
            }
            else
                for (int i = 0; i < N; i++)
                    if (adjMatrix[u, i] != INF && adjMatrix[u, i] != 0)
                        roads.Add(res + " " + i);
        }
        #endregion

        #region Пути из вершины u в вершину v
        public List<string> RoadsFromTo(int u, int v, List<int> including = null, List<int> excluding = null)
        {
            roads = new List<string>();
            RoadsFromToMain(u, v, u.ToString());
            ApplyFilters(including, excluding);
            return roads;
        }

        public void RoadsFromToMain(int u, int v, string res)
        {
            for (int i = 0; i < N; i++)
                if (adjMatrix[u, i] != INF && adjMatrix[u, i] != 0)
                    if (i != v)
                    {
                        string newRes = res + " " + i;
                        RoadsFromToMain(i, v, newRes);
                    }
                    else
                        roads.Add(res + " " + v.ToString());
        }
        #endregion

        #region Пути из вершины u длины len
        public List<string> RoadsFromLength(int u, int len, List<int> including = null, List<int> excluding = null)
        {
            roads = new List<string>();
            RoadsLengthMain(u, len, u.ToString());
            ApplyFilters(including, excluding);
            return roads;
        }
        #endregion

        #region Пути в вершину v длины len
        public List<string> RoadsToLength(int v, int len, List<int> including = null, List<int> excluding = null)
        {
            roads = new List<string>();
            RoadsToLengthMain(v, len, v.ToString());
            ApplyFilters(including, excluding);
            return roads;
        }

        public void RoadsToLengthMain(int v, int len, string res)
        {
            if (len > 1)
            {
                for (int i = 0; i < N; i++)
                    if (adjMatrix[i, v] != INF && adjMatrix[i, v] != 0)
                    {
                        string newRes = i + " " + res;
                        RoadsToLengthMain(i, len - 1, newRes);
                    }
            }
            else
                for (int i = 0; i < N; i++)
                    if (adjMatrix[i, v] != INF && adjMatrix[i, v] != 0)
                        roads.Add(i + " " + res);
        }
        #endregion

        #region Пути из вершины u в вершину v длины len
        public List<string> RoadsFromToLen(int u, int v, int len, List<int> including = null, List<int> excluding = null)
        {
            roads = new List<string>();
            RoadsFromToLenMain(u, v, len, u.ToString());
            ApplyFilters(including, excluding);
            return roads;
        }

        public void RoadsFromToLenMain(int u, int v, int len, string res)
        {
            if (len > 1)
            {
                for (int i = 0; i < N; i++)
                    if (adjMatrix[u, i] != INF && adjMatrix[u, i] != 0)
                    {
                        string newRes = res + " " + i;
                        RoadsFromToLenMain(i, v, len - 1, newRes);
                    }
            }
            else
                if (adjMatrix[u, v] != INF && adjMatrix[u, v] != 0)
                roads.Add(res + " " + v.ToString());
        }
        #endregion

        #region Все пути
        public List<string> AllRoads(List<int> including = null, List<int> excluding = null)
        {
            roads = new List<string>();
            for (int i = 0; i < N; i++)
                RoadsFromMain(i, i.ToString());
            ApplyFilters(including, excluding);
            return roads;
        }
        #endregion

        #region Пути, проходящие и не проходящие через определённые вершины
        private void ApplyFilters(List<int> including, List<int> excluding)
        {
            for (int i = roads.Count - 1; i >= 0; i--)
            {
                bool f = false;
                if (including != null)
                    foreach (int vertex in including)
                        if (!roads[i].Contains(vertex.ToString()))
                        {
                            roads.RemoveAt(i);
                            f = true;
                            break;
                        }
                if (f)
                    continue;
                if (excluding != null)
                    foreach (int vertex in excluding)
                        if (roads[i].Contains(vertex.ToString()))
                        {
                            roads.RemoveAt(i);
                            break;
                        }
            }
        }
        #endregion
        #endregion

        #region Топологическая сортировка
        public int[] TopologicSort()
        {
            int[] NR = new int[N];
            int[] Numzd = new int[N];
            for (int j = 0; j < N; j++)
                for (int k = 0; k < N; k++)
                    if (adjMatrix[j, k] != 0 && adjMatrix[j, k] != INF)
                        Numzd[k]++;

            Stack<int> st = new Stack<int>();
            for (int j = 0; j < N; j++)
                if (Numzd[j] == 0)
                    st.Push(j);

            int num = 0;
            while (st.Count != 0)
            {
                int u = st.Pop();
                NR[u] = num;
                num++;
                for (int j = 0; j < N; j++)
                    if (adjMatrix[u, j] != 0 && adjMatrix[u, j] != INF)
                    {
                        Numzd[j]--;
                        if (Numzd[j] == 0)
                            st.Push(j);
                    }
            }

            return NR;
        }
        #endregion
    }

    public static class Extensions
    {
        public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey fromKey, TKey toKey)
        {
            TValue value = dic[fromKey];
            dic.Remove(fromKey);
            dic[toKey] = value;
        }
    }

    public enum Color
    {
        Black, Gray, White
    }

    #region Очередь
    /// <summary>
    /// Очередь через ссылки.
    /// </summary>
    public class QueueRef<T>
    {
        private QueueItem<T> Head { get; set; }  // голова очереди
        private QueueItem<T> Tail { get; set; }  // хвост очереди

        /// <summary>
        /// Добавление элемента в хвост очереди.
        /// </summary>
        public void Enqueue(T val)
        {
            if (Head == null)
                Head = Tail = new QueueItem<T>() { Value = val };
            else
                Tail = Tail.Next = new QueueItem<T>() { Value = val };
        }

        /// <summary>
        /// Удаление элемента из головы очереди.
        /// </summary>
        public T Dequeue()
        {
            var item = Head;
            Head = item.Next;
            if (Head == null)
                Tail = null;
            return item.Value;
        }

        /// <summary>
        /// Пуста ли очередь.
        /// </summary>
        public bool IsEmpty
        {
            get { return Head == null; }
        }

        /// <summary>
        /// Ссылка.
        /// </summary>
        class QueueItem<T>
        {
            public T Value;
            public QueueItem<T> Next;
        }
    }
    #endregion

    #region Стек
    /// <summary>
    /// Стек через ссылки.
    /// </summary>
    public class StackRef<T>
    {
        private StackItem<T> Head { get; set; }  // голова стека
        private StackItem<T> Tail { get; set; }  // хвост стека

        /// <summary>
        /// Добавление значения в голову стека.
        /// </summary>
        public void Push(T val)
        {
            var item = Head;
            Head = new StackItem<T>() { Value = val };
            Head.Next = item;
        }

        /// <summary>
        /// Удаление головного элемента.
        /// </summary>
        public T Pop()
        {
            if (Head == null)
                throw new Exception("В стеке нет элементов");
            var item = Head;
            Head = item.Next;
            if (Head == null)
                Tail = null;
            return item.Value;
        }

        /// <summary>
        /// Просмотр головного элемента.
        /// </summary>
        public T Peek()
        {
            if (Head == null)
                throw new Exception("В стеке нет элементов");
            return Head.Value;
        }

        /// <summary>
        /// Пуст ли стек.
        /// </summary>
        public bool IsEmpty
        {
            get { return Head == null; }
        }

        /// <summary>
        /// Ссылка.
        /// </summary>
        class StackItem<T>
        {
            public T Value;
            public StackItem<T> Next;
        }
    }
    #endregion

    #region Список
    /// <summary>
    /// Список на ссылках.
    /// </summary>
    public class ListRef
    {
        private ListItem Head { get; set; }  // голова списка
        private ListItem Tail { get; set; }  // хвост списка
        public int Count { get; set; }  // количество элементов

        /// <summary>
        /// Добавление элемента в хвост списка.
        /// </summary>
        public void Add(int val)
        {
            if (Head == null)
                Head = Tail = new ListItem() { Value = val };
            else
                Tail = Tail.Next = new ListItem() { Value = val };
            Count++;
        }

        /// <summary>
        /// Добавление элмента по индексу.
        /// </summary>
        public void AddAt(int val, int index)
        {
            ListItem pr = new ListItem();
            var nxt = Head;
            for (int i = 0; i < index; i++)
            {
                pr = nxt;
                nxt = nxt.Next;
            }

            var saveNxt = nxt;
            pr.Next = new ListItem() { Value = val };
            pr.Next.Next = saveNxt;
            Count++;
        }

        /// <summary>
        /// Удаление первого элемента.
        /// </summary>
        public int RemoveFirst()
        {
            var item = Head;
            Head = item.Next;
            if (Head == null)
                Tail = null;
            Count--;
            return item.Value;
        }

        /// <summary>
        /// Удаление элмента по индексу.
        /// </summary>
        public int RemoveAt(int index)
        {
            if (index == 0)
                return RemoveFirst();
            ListItem pr = new ListItem();
            var nxt = Head;
            for (int i = 0; i < index; i++)
            {
                pr = nxt;
                nxt = nxt.Next;
            }
            Count--;
            var res = nxt.Value;
            pr.Next = nxt.Next;
            return res;
        }

        public bool Have(int value)
        {
            ListItem cur = Head;
            if (cur.Value == value)
            {
                return true;
            }
            for (int i = 1; i < Count; i++)
            {
                cur = cur.Next;
                if (cur.Value == value)
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            string res = "";
            ListItem cur = Head;
            res += cur.Value + " ";
            for (int i = 1; i < Count; i++)
            {
                cur = cur.Next;
                res += cur.Value + " ";
            }
            return res;
        }

        /// <summary>
        /// Пуст ли список.
        /// </summary>
        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        /// <summary>
        /// Ссылка.
        /// </summary>
        class ListItem
        {
            public int Value;
            public ListItem Next;
        }

        public int this[int index]
        {
            get
            {
                // если индекс имеется в стеке
                if (index >= 0 && index < Count)
                {
                    ListItem cur = Head;
                    for (int i = 0; i < index; i++)
                    {
                        cur = cur.Next;
                    }
                    return cur.Value; // то возвращаем объект по индексу
                }
                else
                    throw new ArgumentOutOfRangeException(); // иначе генерируем исключение
            }
            set
            {
                // если индекс есть в стеке
                if (index >= 0 && index < Count)
                {
                    ListItem cur = Head;
                    for (int i = 0; i < Count; i++)
                    {
                        cur = cur.Next;
                    }
                    cur.Value = value;  // переустанавливаем значение по индексу
                    //cur.Next
                    //AddAt(value, index);
                }
            }
        }
    }
    #endregion

    #region Очередь с приоритетом
    class PriorQueue<T>
    {
        public int count = 0;  // количество элементов
        public Prior_List<T> elements = new Prior_List<T>();  // список элементов
        public bool sorted = false;  // отсортированность

        /// <summary>
        /// Добавляет элемент в очередь.
        /// </summary>
        public void Enqueue(int priority, T value)
        {
            elements.Push(priority, value);
            count += 1;
            sorted = false;
        }

        /// <summary>
        /// Удаляет самый приоритетный элемент.
        /// </summary>
        public (int Priority, T Value) Dequeue()
        {
            if (count == 0) throw new Exception("No elements available");
            if (!sorted)
            {
                elements.Sort();
                sorted = true;
            }
            count--;
            return elements.Pop_First();
        }
    }

    /// <summary>
    /// Двусвязный список, позволяющий сортировать элементы по приоритету.
    /// </summary>
    class Prior_List<T>
    {
        public int count = 0;
        public Node first;
        public Node last;

        /// <summary>
        /// Добавить элемент.
        /// </summary>
        public void Push(int priority, T value)
        {
            if (count == 0)
            {
                first = new Node() { Value = (priority, value) };
                count = 1;
                last = first;
            }
            else
            {
                Node temp = new Node() { Value = (priority, value), Prev = last };
                last.Next = temp;
                last = temp;
                count++;
            }
        }

        /// <summary>
        /// Удаляет самый приоритетный элемент.
        /// </summary>
        public (int priority, T Value) Pop_First()
        {
            if (count == 0) throw new Exception("Nothing to Pop");
            if (count == 1)
            {
                var temp = first.Value;
                first = null;
                count--;
                return temp;
            }
            else
            {
                var temp = first.Value;
                first = first.Next;
                count--;
                return temp;
            }
        }

        /// <summary>
        /// Сортировка слиянием.
        /// </summary>
        private Node MergeSort(Node h)
        {
            if (h == null || h.Next == null)
            {
                return h;
            }
            Node middle = GetMiddle(h);
            Node NextOfMiddle = middle.Next;
            middle.Next = null;
            Node left = MergeSort(h);
            Node right = MergeSort(NextOfMiddle);
            Node sortedlist = sortedMerge(left, right);
            return sortedlist;
        }

        /// <summary>
        /// Получить средний элемент.
        /// </summary>
        private Node GetMiddle(Node h)
        {
            if (h == null)
                return h;
            Node fastptr = h.Next;
            Node slowptr = h;
            while (fastptr != null)
            {
                fastptr = fastptr.Next;
                if (fastptr != null)
                {
                    slowptr = slowptr.Next;
                    fastptr = fastptr.Next;
                }
            }
            return slowptr;
        }

        /// <summary>
        /// Слияние двух списков.
        /// </summary>
        Node sortedMerge(Node a, Node b)
        {
            Node result;
            if (a == null)
                return b;
            if (b == null)
                return a;
            if (a.Value.priority <= b.Value.priority)
            {
                result = a;
                result.Next = sortedMerge(a.Next, b);
            }
            else
            {
                result = b;
                result.Next = sortedMerge(a, b.Next);
            }
            return result;
        }

        /// <summary>
        /// Перестроить список.
        /// </summary>
        private void Rebuild_Nodes()
        {
            Node cur = first;
            while (cur.Next != null)
            {
                cur.Next.Prev = cur;
                cur = cur.Next;
            }
            last = cur;
        }

        /// <summary>
        /// Отсортировать список.
        /// </summary>
        public void Sort()
        {
            first = MergeSort(first);
            Rebuild_Nodes();
        }

        /// <summary>
        /// Класс "Запись"
        /// </summary>
        public class Node
        {
            public (int priority, T Value) Value;
            public Node Next;
            public Node Prev;
        }
    }
    #endregion

    #region Двоичная куча
    public class BinaryHeap<T> where T : IComparable<T>
    {
        public Node<T> root;
        public Node<T> pointer;
        public int count;

        public BinaryHeap(Node<T>[] nodes)
        {
            count = 0;
            for (int i = 0; i < nodes.Length; i++)
                Add(nodes[i]);
        }
        public BinaryHeap()
        {
            count = 0;
        }

        public void Add(Node<T> node)
        {
            if (root == null)
            {
                root = node;
                count++;
            }
            else
            {
                pointer = root;
                string bitCount = Convert.ToString(count + 1, 2);
                for (int i = 1; i < bitCount.Length; i++)
                    if (bitCount[i] == '0')
                    {
                        if (pointer.left == null)
                            pointer.left = new Node<T>(node.value, pointer);
                        pointer = pointer.left;
                    }
                    else
                    {
                        if (pointer.right == null)
                            pointer.right = new Node<T>(node.value, pointer);
                        pointer = pointer.right;
                    }
                while (true)
                {
                    if (pointer == root)
                    {
                        break;
                    }
                    if (pointer.value.CompareTo(pointer.parent.value) == -1)
                    {
                        T temp = pointer.value;
                        pointer.value = pointer.parent.value;
                        pointer.parent.value = temp;
                        pointer = pointer.parent;
                    }
                    else
                        break;
                }
                count++;
            }
        }

        public T Remove()
        {
            T output = root.value;
            pointer = root;
            string bitCount = Convert.ToString(count, 2);
            for (int i = 1; i < bitCount.Length; i++)
                if (bitCount[i] == '0')
                    pointer = pointer.left;
                else
                    pointer = pointer.right;
            root.value = pointer.value;

            if (count == 1)
            {
                root = null;
            }
            else
            {
                if (pointer.parent.left == pointer)
                    pointer.parent.left = null;
                else
                    pointer.parent.right = null;
                Heapify();
            }
            count--;
            return output;
        }

        private void Heapify()
        {
            Node<T> compare;
            pointer = root;
            while (pointer.left != null)
            {
                if (pointer.right == null)
                    compare = pointer.left;
                else if (pointer.left.value.CompareTo(pointer.right.value) < 1)
                    compare = pointer.left;
                else
                    compare = pointer.right;
                if (pointer.value.CompareTo(compare.value) == 1)
                {
                    T temp = pointer.value;
                    pointer.value = compare.value;
                    compare.value = temp;
                    pointer = compare;
                }
                else
                    break;
            }
        }
    }
    public class Node<T> where T : IComparable<T>
    {
        public Node<T> parent;
        public Node<T> left;
        public Node<T> right;
        public T value;

        public Node(T value, Node<T> parent = null, Node<T> left = null, Node<T> right = null)
        {
            this.value = value;
            this.parent = parent;
            this.left = left;
            this.right = right;
        }
    }
    #endregion

    #region Дек
    class DequeRef<T>
    {
        private DequeItem<T> Left { get; set; }  // левый конец дека
        private DequeItem<T> Right { get; set; }  // правый конец дека

        /// <summary>
        /// Добавить элемент слева.
        /// </summary>
        public void EnqueueLeft(T val)
        {
            if (Left == null)
            {
                Left = Right = new DequeItem<T>() { Value = val };
                return;
            }

            var item = Left;
            Left = new DequeItem<T>() { Value = val };
            Left.Next = item;

            if (Right == null)
                Right = Left;
        }

        /// <summary>
        /// Добавить элемент справа.
        /// </summary>
        public void EnqueueRight(T val)
        {
            if (Right == null)
            {
                Left = Right = new DequeItem<T>() { Value = val };
                return;
            }

            Right.Next = new DequeItem<T>() { Value = val };
            Right = Right.Next;

            if (Left == null)
                Left = Right;
        }

        /// <summary>
        /// Удалить элемент слева.
        /// </summary>
        public T DequeueLeft()
        {
            if (Left == null)
                throw new Exception("В деке нет элементов");
            var item = Left;
            Left = item.Next;
            if (Left == null)
                Right = null;
            return item.Value;
        }

        /// <summary>
        /// Удалить элемент справа.
        /// </summary>
        public T DequeueRight()
        {
            if (Right == null)
                throw new Exception("В деке нет элементов");
            DequeItem<T> nxt = Left;
            DequeItem<T> prv = null;
            while (nxt.Next != null)
            {
                prv = nxt;
                nxt = nxt.Next;
            }
            var item = nxt.Value;
            if (prv == null)
            {
                Right = Left = null;
            }
            else
            {
                prv.Next = null;
                Right = prv;
            }
            return item;
        }

        /// <summary>
        /// Пуст ли дек.
        /// </summary>
        public bool IsEmpty
        {
            get { return Left == null; }
        }

        /// <summary>
        /// Ссылка.
        /// </summary>
        class DequeItem<T>
        {
            public T Value;
            public DequeItem<T> Next;
        }
    }
    #endregion

    #region Дек (работает)
    public class Deque<T> : IEnumerable<T>  // двусвязный список
    {
        DoublyNode<T> head; // головной/первый элемент
        DoublyNode<T> tail; // последний/хвостовой элемент
        int count;  // количество элементов в списке

        // добавление элемента
        public void AddLast(T data)
        {
            DoublyNode<T> node = new DoublyNode<T>(data);

            if (head == null)
                head = node;
            else
            {
                tail.Next = node;
                node.Previous = tail;
            }
            tail = node;
            count++;
        }
        public void AddFirst(T data)
        {
            DoublyNode<T> node = new DoublyNode<T>(data);
            DoublyNode<T> temp = head;
            node.Next = temp;
            head = node;
            if (count == 0)
                tail = head;
            else
                temp.Previous = node;
            count++;
        }
        public T RemoveFirst()
        {
            if (count == 0)
                throw new InvalidOperationException();
            T output = head.Data;
            if (count == 1)
            {
                head = tail = null;
            }
            else
            {
                head = head.Next;
                head.Previous = null;
            }
            count--;
            return output;
        }
        public T RemoveLast()
        {
            if (count == 0)
                throw new InvalidOperationException();
            T output = tail.Data;
            if (count == 1)
            {
                head = tail = null;
            }
            else
            {
                tail = tail.Previous;
                tail.Next = null;
            }
            count--;
            return output;
        }
        public T First
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException();
                return head.Data;
            }
        }
        public T Last
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException();
                return tail.Data;
            }
        }

        public int Count { get { return count; } }
        public bool IsEmpty { get { return count == 0; } }

        public void Clear()
        {
            head = null;
            tail = null;
            count = 0;
        }

        public bool Contains(T data)
        {
            DoublyNode<T> current = head;
            while (current != null)
            {
                if (current.Data.Equals(data))
                    return true;
                current = current.Next;
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this).GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            DoublyNode<T> current = head;
            while (current != null)
            {
                yield return current.Data;
                current = current.Next;
            }
        }
    }

    public class DoublyNode<T>
    {
        public DoublyNode(T data)
        {
            Data = data;
        }
        public T Data { get; set; }
        public DoublyNode<T> Previous { get; set; }
        public DoublyNode<T> Next { get; set; }
    }
    #endregion
}