﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Graph
{
    public int depth;
    //List of clusters for every level of abstraction
    public List<Cluster>[] C;

    Map map;

    /// <summary>
    /// Construct a graph from the map
    /// </summary>
    public Graph(Map map, int MaxLevel, int clusterSize)
    {
        depth = MaxLevel;
        this.map = map;

        int ClusterWidth, ClusterHeight;

        C = new List<Cluster>[MaxLevel];

        for (int i = 0; i < MaxLevel; ++i)
        {
            if (i != 0)
                //Increment cluster size for higher levels
                clusterSize = clusterSize*3;    //Scaling factor 3 is arbitrary

            //Set number of clusters in horizontal and vertical direction
            ClusterHeight = Mathf.CeilToInt((float)map.Height / clusterSize);
            ClusterWidth = Mathf.CeilToInt((float)map.Width / clusterSize);

            if (ClusterWidth <= 1 && ClusterHeight <= 1)
            {
                /**A ClusterWidth or ClusterHeight of 1 means there is only going to be 
                 * one cluster in this direction. Therefore if both are 1 then this level is useless **/
                depth = i;
                break;
            }

            C[i] = BuildClusters(i, clusterSize, ClusterWidth, ClusterHeight);
        }
    }

    public Node[] InsertNode(GridTile pos)
    {
        Node[] layerNodes = new Node[depth];
        for(int i = 0; i < depth; ++i)
        {
            //Determine in which cluster should we add it
            //TODO: Potentially find a better way to find the right cluster
            foreach (Cluster c in C[i])
            {
                if (c.Contains(pos))
                {
                    //This is the right cluster
                    if (i == 0)
                        layerNodes[i] = ConnectToBorder(pos, c, false);
                    else
                        layerNodes[i] = ConnectToBorder(pos, c, true, layerNodes[i - 1]);
                    break;
                }
            }
        }

        return layerNodes;
    }

    /// <summary>
    /// Connect the grid tile to borders by creating a new node
    /// </summary>
    /// <returns>The node created</returns>
    private Node ConnectToBorder(GridTile pos, Cluster c, bool isAbstract, Node child = null)
    {
        Node newNode;

        //If the position is an actual border node, then return it
        if (c.Nodes.TryGetValue(pos, out newNode))
            return newNode;

        //Otherwise create a node and pathfind through border nodes
        newNode = new Node(pos);
        if (isAbstract) newNode.child = child;

        foreach (KeyValuePair<GridTile, Node> n in c.Nodes)
        {
            ConnectNodes(newNode, n.Value, c, isAbstract);
        }

        return newNode;
    }

    /// <summary>
    /// Connect two nodes by pathfinding between them. 
    /// </summary>
    /// <remarks>We assume they are different nodes. If the path returned is 0, then there is no path that connects them.</remarks>
    private void ConnectNodes(Node n1, Node n2, Cluster c, bool isAbstract)
    {
        LinkedList<Edge> path;
        LinkedListNode<Edge> iter;
        Edge e1, e2;

        float weight = 0f;

        if (isAbstract)
            path = Pathfinder.FindPath(n1.child, n2.child, c.Boundaries);
        else
            path = Pathfinder.FindPath(n1.pos, n2.pos, c.Boundaries, map.Obstacles);

        if (path.Count > 0)
        {
            e1 = new Edge()
            {
                start = n1,
                end = n2,
                type = EdgeType.INTRA,
                UnderlyingPath = path
            };

            e2 = new Edge()
            {
                start = n2,
                end = n1,
                type = EdgeType.INTRA,
                UnderlyingPath = new LinkedList<Edge>()
            };

            //Store inverse path in node n2
            //Sum weights of underlying edges at the same time
            iter = e1.UnderlyingPath.Last;
            while (iter != null)
            {
                e2.UnderlyingPath.AddLast(iter.Value);
                weight += iter.Value.weight;
                iter = iter.Previous;
            }

            //Update weights
            e1.weight = weight;
            e2.weight = weight;

            n1.edges.Add(e1);
            n2.edges.Add(e2);
        }
    }

    /// <summary>
    /// Verify whether two clusters connect with each other through their inter edges
    /// </summary>
    public bool IsReachable(Cluster c1, Cluster c2) {
        throw new NotImplementedException("Not yet implemented");
    }

    private delegate void CreateBorderNodes(Cluster c1, Cluster c2, bool x);

    /// <summary>
    /// Build Clusters of a certain level, given the size of a cluster
    /// ClusterWidth is the number of clusters in the horizontal direction.
    /// ClusterHeight is the number of clusters in the vertical direction.
    /// </summary>
    private List<Cluster> BuildClusters(int level, int ClusterSize, int ClusterWidth, int ClusterHeight)
    {
        List<Cluster> clusters = new List<Cluster>();

        Cluster c1;

        int i, j;

        //Create clusters of this level
        for (i = 0; i < ClusterHeight; ++i)
            for (j = 0; j < ClusterWidth; ++j)
            {
                c1 = new Cluster();
                c1.Boundaries.Min = new GridTile(j * ClusterSize, i * ClusterSize);
                c1.Boundaries.Max = new GridTile(
                    Mathf.Min(c1.Boundaries.Min.x + ClusterSize - 1, map.Width - 1),
                    Mathf.Min(c1.Boundaries.Min.y + ClusterSize - 1, map.Height - 1));

                //Adjust size of cluster based on boundaries
                c1.Width = c1.Boundaries.Max.x - c1.Boundaries.Min.x + 1;
                c1.Height = c1.Boundaries.Max.y - c1.Boundaries.Min.y + 1;

                if (level > 0)
                {
                    //Since we're abstract, we will have lower level clusters
                    c1.Clusters = new List<Cluster>();

                    //Add lower level clusters in newly created clusters
                    foreach (Cluster c in C[level - 1])
                        if (c1.Contains(c))
                            c1.Clusters.Add(c);
                }

                clusters.Add(c1);
            }

        if (level == 0)
        {
            //Add border nodes for every adjacent pair of clusters
            for (i = 0; i < clusters.Count; ++i)
                for (j = i + 1; j < clusters.Count; ++j)
                    DetectAdjacentClusters(clusters[i], clusters[j], CreateConcreteBorderNodes);

        } else
        {
            //Add border nodes for every adjacent pair of clusters
            for (i = 0; i < clusters.Count; ++i)
                for (j = i + 1; j < clusters.Count; ++j)
                    DetectAdjacentClusters(clusters[i], clusters[j], CreateAbstractBorderNodes);
        }

        //Add Intra edges for every border nodes and pathfind between them
        for (i = 0; i < clusters.Count; ++i)
            GenerateIntraEdges(clusters[i], level > 0);

        return clusters;
    }

    private void DetectAdjacentClusters(Cluster c1, Cluster c2, CreateBorderNodes CreateBorderNodes)
    {
        //Check if both clusters are adjacent
        if (c1.Boundaries.Min.x == c2.Boundaries.Min.x)
        {
            if (c1.Boundaries.Max.y + 1 == c2.Boundaries.Min.y)
                CreateBorderNodes(c1, c2, false);
            else if (c2.Boundaries.Max.y + 1 == c1.Boundaries.Min.y)
                CreateBorderNodes(c2, c1, false);

        }
        else if (c1.Boundaries.Min.y == c2.Boundaries.Min.y)
        {
            if (c1.Boundaries.Max.x + 1 == c2.Boundaries.Min.x)
                CreateBorderNodes(c1, c2, true);
            else if (c2.Boundaries.Max.x + 1 == c1.Boundaries.Min.x)
                CreateBorderNodes(c2, c1, true);
        }
    }

    /// <summary>
    /// Create border nodes and attach them together.
    /// We always pass the lower cluster first (in c1).
    /// Adjacent index : if x == true, then c1.BottomRight.x else c1.BottomRight.y
    /// </summary>
    private void CreateConcreteBorderNodes(Cluster c1, Cluster c2, bool x)
    {
        int i, iMin, iMax;
        if (x)
        {
            iMin = c1.Boundaries.Min.y;
            iMax = iMin + c1.Height;
        } else
        {
            iMin = c1.Boundaries.Min.x;
            iMax = iMin + c1.Width;
        }

        int lineSize = 0;
        for (i = iMin; i < iMax; ++i)
        {
            if ((x && (!map.Obstacles[i][c1.Boundaries.Max.x] && !map.Obstacles[i][c2.Boundaries.Min.x]))
                || !x && (!map.Obstacles[c1.Boundaries.Max.y][i] && !map.Obstacles[c2.Boundaries.Min.y][i]))
            {
                lineSize++;
            } else {

                CreateConcreteInterEdges(c1, c2, x, ref lineSize, i);
            }
        }

        //If line size > 0 after looping, then we have another line to fill in
        CreateConcreteInterEdges(c1, c2, x, ref lineSize, i);
    }

    //i is the index at which we stopped (either its an obstacle or the end of the cluster
    private void CreateConcreteInterEdges(Cluster c1, Cluster c2, bool x, ref int lineSize, int i)
    {
        if (lineSize > 0)
        {
            if (lineSize <= 5)
            {
                //Line is too small, create 1 inter edges
                CreateConcreteInterEdge(c1, c2, x, i - (lineSize / 2 + 1));
            }
            else
            {
                //Create 2 inter edges
                CreateConcreteInterEdge(c1, c2, x, i - lineSize);
                CreateConcreteInterEdge(c1, c2, x, i - 1);
            }

            lineSize = 0;
        }
    }

    //Inter edges are edges that crosses clusters
    private void CreateConcreteInterEdge(Cluster c1, Cluster c2, bool x, int i)
    {
        GridTile g1, g2;
        Node n1, n2;
        if (x)
        {
            g1 = new GridTile(c1.Boundaries.Max.x, i);
            g2 = new GridTile(c2.Boundaries.Min.x, i);
        }
        else
        {
            g1 = new GridTile(i, c1.Boundaries.Max.y);
            g2 = new GridTile(i, c2.Boundaries.Min.y);
        }

        if (!c1.Nodes.TryGetValue(g1, out n1))
        {
            n1 = new Node(g1);
            c1.Nodes.Add(g1, n1);
        }

        if (!c2.Nodes.TryGetValue(g2, out n2))
        {
            n2 = new Node(g2);
            c2.Nodes.Add(g2, n2);
        }

        n1.edges.Add(new Edge() { start = n1, end = n2, type = EdgeType.INTER, weight = 1 });
        n2.edges.Add(new Edge() { start = n2, end = n1, type = EdgeType.INTER, weight = 1 });
    }


    private void CreateAbstractBorderNodes(Cluster p1, Cluster p2, bool x)
    {
        foreach (Cluster c1 in p1.Clusters)
            foreach(Cluster c2 in p2.Clusters)
            {
                if ((x && c1.Boundaries.Min.y == c2.Boundaries.Min.y && c1.Boundaries.Max.x + 1 == c2.Boundaries.Min.x) || 
                    (!x && c1.Boundaries.Min.x == c2.Boundaries.Min.x && c1.Boundaries.Max.y + 1 == c2.Boundaries.Min.y))
                {
                    CreateAbstractInterEdges(p1, p2, c1, c2);
                }
            }
    }

    private void CreateAbstractInterEdges(Cluster p1, Cluster p2, Cluster c1, Cluster c2)
    {
        List<Edge> edges1 = new List<Edge>(),
            edges2 = new List<Edge>();
        Node n1, n2;

        //Add edges that connects them from c1
        foreach (KeyValuePair<GridTile, Node> n in c1.Nodes)
            foreach (Edge e in n.Value.edges)
            {
                if (e.type == EdgeType.INTER && c2.Contains(e.end.pos))
                    edges1.Add(e);
            }

        foreach(KeyValuePair<GridTile, Node> n in c2.Nodes)
            foreach (Edge e in n.Value.edges)
            {
                if (e.type == EdgeType.INTER && c1.Contains(e.end.pos))
                    edges2.Add(e);
            }

        //Find every pair of twin edges and insert them in their respective parents
        foreach (Edge e1 in edges1)
            foreach (Edge e2 in edges2)
            {
                if (e1.end == e2.start)
                {
                    if (!p1.Nodes.TryGetValue(e1.start.pos, out n1))
                    {
                        n1 = new Node(e1.start.pos) { child = e1.start };
                        p1.Nodes.Add(n1.pos, n1);
                    }

                    if (!p2.Nodes.TryGetValue(e2.start.pos, out n2))
                    {
                        n2 = new Node(e2.start.pos) { child = e2.start };
                        p2.Nodes.Add(n2.pos, n2);
                    }

                    n1.edges.Add(new Edge() { start = n1, end = n2, type = EdgeType.INTER, weight = 1 });
                    n2.edges.Add(new Edge() { start = n2, end = n1, type = EdgeType.INTER, weight = 1 });

                    break;  //Break the second loop since we've found a pair
                }
            }
    }
     
    //Intra edges are edges that lives inside clusters
    private void GenerateIntraEdges(Cluster c, bool isAbstract)
    {
        int i, j;
        Node n1, n2;

        /* We do this so that we can iterate through pairs once, 
         * by keeping the second index always higher than the first */
        var nodes = new List<Node>(c.Nodes.Values);
        
        for(i =0; i < nodes.Count; ++i)
        {
            n1 = nodes[i];
            for (j = i + 1; j < nodes.Count; ++j)
            {
                n2 = nodes[j];

                ConnectNodes(n1, n2, c, isAbstract);
            }
        }
    }
}