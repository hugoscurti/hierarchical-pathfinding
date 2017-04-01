using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Graph
{
    int depth;
    //List of clusters for every level of abstraction
    List<Cluster>[] C;

    Map map;

    /// <summary>
    /// Construct a graph from the map
    /// </summary>
    public Graph(Map map, int MaxLevel, int clusterSize)
    {
        this.map = map;

        C = new List<Cluster>[MaxLevel];
        
        for (int i = 0; i < MaxLevel; ++i)
        {
            C[i] = BuildClusters(i, clusterSize);
        }
    }

    private void BuildGraph()
    {

    }

    /// <summary>
    /// Build Clusters of a certain level, given the size of a cluster
    /// </summary>
    /// <param name="level"></param>
    /// <param name="clusterSize">The size of a cluster edge</param>
    /// <returns></returns>
    private List<Cluster> BuildClusters(int level, int clusterSize)
    {
        List<Cluster> clusters = new List<Cluster>();

        Cluster c;
        int ClusterHeight = Mathf.CeilToInt((float)map.Height / clusterSize);
        int ClusterWidth = Mathf.CeilToInt((float)map.Width / clusterSize);

        int i, j;
        if (level == 0)
        {
            //Create clusters of this level
            for (i = 0; i < ClusterHeight; ++i)
                for (j = 0; j < ClusterWidth; ++j)
                {
                    c = new Cluster()
                    {
                        TopLeft = new GridTile(j * clusterSize, i * clusterSize),

                    };
                    c.BottomRight = new GridTile(
                        Mathf.Min(c.TopLeft.x + clusterSize - 1, map.Width - 1),
                        Mathf.Min(c.TopLeft.y + clusterSize - 1, map.Height - 1));
                    c.Width = c.BottomRight.x - c.TopLeft.x + 1;
                    c.Height = c.BottomRight.y - c.TopLeft.y + 1;

                    clusters.Add(c);
                }

            Cluster c1; Cluster c2;
            //Add border nodes for every clusters
            for (i = 0; i < clusters.Count; ++i)
                for (j = i + 1; j < clusters.Count; ++j)
                {
                    c1 = clusters[i];
                    c2 = clusters[j];

                    //Check if both clusters are adjacent
                    if (c1.TopLeft.x == c2.TopLeft.x) {
                        if (c1.BottomRight.y + 1 == c2.TopLeft.y)
                            CreateBorderNodes(c1, c2, false);
                        else if (c2.BottomRight.y + 1 == c1.TopLeft.y)
                            CreateBorderNodes(c2, c1, false);

                    } else if (c1.TopLeft.y == c2.TopLeft.y) {
                        if (c1.BottomRight.x + 1 == c2.TopLeft.x)
                            CreateBorderNodes(c1, c2, true);
                        else if (c2.BottomRight.x + 1 == c1.TopLeft.x)
                            CreateBorderNodes(c2, c1, true);
                    }
                }

            //TODO: Add Intra edges for every border nodes and pathfind between them
            //TODO: Consider edges from higher level clusters
            for (i = 0; i < clusters.Count; ++i)
            {

            }

        } else
        {
            //TODO: Add clusters for arbitrary levels bigger than 1
        }

        return clusters;
    }

    /// <summary>
    /// Create border nodes and attach them together.
    /// We always pass the lower cluster first (in c1).
    /// Adjacent index : if x == true, then c1.BottomRight.x else c1.BottomRight.y
    /// </summary>
    private void CreateBorderNodes(Cluster c1, Cluster c2, bool x)
    {
        int i, iMin, iMax;
        if (x)
        {
            iMin = c1.TopLeft.y;
            iMax = c1.Height;
        } else
        {
            iMin = c1.TopLeft.x;
            iMax = c1.Width;
        }

        int lineSize = 0;
        for (i = iMin; i < iMax; ++i)
        {
            if ((x && (!map.Obstacles[c1.BottomRight.x][i] && !map.Obstacles[c2.TopLeft.x][i]))
                || (!map.Obstacles[i][c1.BottomRight.y] && !map.Obstacles[i][c2.TopLeft.y]))
            {
                lineSize++;
            } else {

                CreateInterEdges(c1, c2, x, ref lineSize, i);
            }
        }

        //If line size > 0 after looping, then we have another line to fill in
        CreateInterEdges(c1, c2, x, ref lineSize, i);
    }

    //i is the index at which we stopped (either its an obstacle or the end of the cluster
    private void CreateInterEdges(Cluster c1, Cluster c2, bool x, ref int lineSize, int i)
    {
        if (lineSize > 0)
        {
            if (lineSize <= 5)
            {
                //Line is too small, create 1 inter edges
                CreateInterEdge(c1, c2, x, i - (i / 2 + 1));
            }
            else
            {
                //Create 2 inter edges
                CreateInterEdge(c1, c2, x, i - lineSize);
                CreateInterEdge(c1, c2, x, i - 1);
            }

            lineSize = 0;
        }
    }

    //Inter edge are edges that crosses clusters
    private void CreateInterEdge(Cluster c1, Cluster c2, bool x, int i)
    {
        GridTile g1, g2;
        Node n1, n2;
        if (x)
        {
            g1 = new GridTile(c1.BottomRight.x, i);
            g2 = new GridTile(c2.TopLeft.x, i);
        }
        else
        {
            g1 = new GridTile(i, c1.BottomRight.y);
            g2 = new GridTile(i, c2.TopLeft.y);
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
        
}