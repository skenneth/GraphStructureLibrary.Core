﻿using System.Security;

namespace GraphLibrary
{
    public class Graph
    {
        public List<Vertex> Vertices { get; set; }
        public List<Edge> Edges { get; set; }

        public Graph()
        {
            Vertices = new List<Vertex>();
            Edges = new List<Edge>();
        }

        public Vertex AddVertex(string name, object obj)
        {
            var vertex = Vertices.FirstOrDefault(x => x.Name == name);
            if (vertex != null)
                vertex.Value = obj;
            else if (obj is Vertex vA)
            {
                Vertices.Add(vA);
                vertex = vA;
            }
            else
            {
                vertex = new Vertex(name, obj);
                Vertices.Add(vertex);
            }
            return vertex;
        }

        public Edge AddEdge(object A, object B, DirectionState Direction = DirectionState.Both)
        {
            Vertex vA;
            if (A is Vertex v1)
                vA = Vertices.FirstOrDefault(v => v.Name == v1.Name) ?? v1;
            else
                vA = new Vertex(Guid.NewGuid().ToString(), A);
            Vertex vB;
            if (B is Vertex v2)
                vB = Vertices.FirstOrDefault(v => v.Name == v2.Name) ?? v2;
            else
                vB = new Vertex(Guid.NewGuid().ToString(), B);
            Edge edge = new Edge(vA, vB, Direction);
            vA.TryAddEdge(edge);
            vB.TryAddEdge(edge);
            Edges.Add(edge);
            return edge;
        }

        public void GenerateAdjacencyLists(bool CompareValues)
        {
            Vertices.ForEach(v =>
            {
                var edges = Edges.Where(e => e.Contains(v, CompareValues));
                v.AdjacencyList.Clear();
                v.AdjacencyList.AddRange(edges);

                var vertices = (IEnumerable<Vertex>)edges.Select(e => v.GetAdjacentVertexFromEdge(e, CompareValues));
                v.AdjacentVertices.Clear();
                v.AdjacentVertices.AddRange(vertices);
            });
        }

        public bool TryRemoveVertices(object obj)
        {
            if (obj == null)
                return false;
            int count = Vertices.Count;
            IEnumerable<Vertex> verticesToRemove;
            if(obj is Vertex vertex)
                verticesToRemove = Vertices.Where(v => v.Equals(vertex, false)); //based on Vertex name
            else
                verticesToRemove = Vertices.Where(v => v.Equals(obj, true)); //based on shared object amongst Vertices
            var edgesToRemove = verticesToRemove.SelectMany(v => v.AdjacencyList);
            foreach(var edge in edgesToRemove)
                TryRemoveEdges(edge);
            Vertices = Vertices.Where(v => !verticesToRemove.Contains(v)).ToList();
            return count != Vertices.Count;
        }
        
        public bool TryRemoveEdges(object obj)
        {
            if (obj == null)
                return false;
            int count = Edges.Count;
            IEnumerable<Edge> edgesToRemove;
            if(obj is Edge edge)
                edgesToRemove = Edges.Where(e => e.Contains(edge, false)); //based on Edge name
            else
                edgesToRemove = Edges.Where(e => e.Contains(obj, true)); //based on shared object amongst Edges
            var verticesToUpdate = edgesToRemove.SelectMany(e =>
            {
                var list = new List<Vertex>();
                if (e.A != null)
                    list.Add(e.A);
                if (e.B != null)
                    list.Add(e.B);
                return list.AsEnumerable();
            });
            foreach(var vertex in verticesToUpdate)
            {
                vertex.AdjacencyList = vertex.AdjacencyList.Where(e => !edgesToRemove.Contains(e)).ToList();
            }
            Edges = Edges.Where(e => !edgesToRemove.Contains(e)).ToList();
            return count != Edges.Count;
        }

        public void PrintAdjacencyLists()
        {
            bool swapped;
            foreach(var vertex in Vertices.OrderBy(v => v.Name))
            {
                Console.WriteLine($"{vertex}:");
                foreach(var edge in vertex.AdjacencyList.OrderBy(e => e.Name))
                {
                    swapped = false;
                    if (edge.B == vertex)
                    {
                        edge.SwapVertices();
                        swapped = true;
                    }
                    Console.Write($"\t{edge.Name}");
                    if (swapped)
                        edge.SwapVertices();
                }
                Console.WriteLine();
            }
        }
    }
}