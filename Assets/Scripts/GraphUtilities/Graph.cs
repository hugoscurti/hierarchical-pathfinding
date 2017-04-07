using System;
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

        C = new List<Cluster>[MaxLevel];
        
        for (int i = 0; i < MaxLevel; ++i)
        {
            C[i] = BuildClusters(i, clusterSize);
        }
    }

    public Node[] InsertNode(GridTile pos, int MaxLevel)
    {
        Node[] layerNodes = new Node[MaxLevel];
        for(int i = 0; i < MaxLevel; ++i)
        {
            //Determine in which cluster should we add it
            //TODO: Potentially find a better way to find the right cluster
            foreach (Cluster c in C[i])
            {
                if (c.Boundaries.Min.x <= pos.x && c.Boundaries.Min.y <= pos.y &&
                    c.Boundaries.Max.x >= pos.x && c.Boundaries.Max.y >= pos.y)
                {
                    //This is the right cluster
                    layerNodes[i] = ConnectToBorder(pos, c, i);
                    //TODO: Add the node in the cluster?
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
    private Node ConnectToBorder(GridTile pos, Cluster c, int level)
    {
        Node newNode;

        //If the position is an actual border node, then return it
        if (c.Nodes.TryGetValue(pos, out newNode))
            return newNode;

        //Otherwise create a node and pathfind through border nodes
        newNode = new Node(pos);
        foreach (KeyValuePair<GridTile, Node> n in c.Nodes)
        {
            ConnectNodes(newNode, n.Value, c);
        }

        return newNode;
    }

    /// <summary>
    /// Connect two nodes by pathfinding between them. 
    /// </summary>
    /// <remarks>We assume they are different nodes. If the path returned is 0, then there is no path that connects them.</remarks>
    private void ConnectNodes(Node n1, Node n2, Cluster c)
    {
        LinkedList<Edge> path;
        LinkedListNode<Edge> iter;
        Edge e1, e2;

        float weight = 0f;

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


    /// <summary>
    /// Build Clusters of a certain level, given the size of a cluster
    /// </summary>
    private List<Cluster> BuildClusters(int level, int clusterSize)
    {
        List<Cluster> clusters = new List<Cluster>();

        Cluster c1; Cluster c2;
        int ClusterHeight = Mathf.CeilToInt((float)map.Height / clusterSize);
        int ClusterWidth = Mathf.CeilToInt((float)map.Width / clusterSize);

        int i, j;
        if (level == 0)
        {
            //Create clusters of this level
            for (i = 0; i < ClusterHeight; ++i)
                for (j = 0; j < ClusterWidth; ++j)
                {
                    c1 = new Cluster();
                    c1.Boundaries.Min = new GridTile(j * clusterSize, i * clusterSize);
                    c1.Boundaries.Max = new GridTile(
                        Mathf.Min(c1.Boundaries.Min.x + clusterSize - 1, map.Width - 1),
                        Mathf.Min(c1.Boundaries.Min.y + clusterSize - 1, map.Height - 1));

                    //Adjust size of cluster based on boundaries
                    c1.Width = c1.Boundaries.Max.x - c1.Boundaries.Min.x + 1;
                    c1.Height = c1.Boundaries.Max.y - c1.Boundaries.Min.y + 1;

                    clusters.Add(c1);
                }

            //Add border nodes for every adjacent pair of clusters
            for (i = 0; i < clusters.Count; ++i)
            {
                c1 = clusters[i];

                for (j = i + 1; j < clusters.Count; ++j)
                {
                    c2 = clusters[j];

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
            }

            //Add Intra edges for every border nodes and pathfind between them
            for (i = 0; i < clusters.Count; ++i)
            {
                GenerateIntraEdges(clusters[i]);
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
                CreateInterEdge(c1, c2, x, i - (lineSize / 2 + 1));
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

    //Inter edges are edges that crosses clusters
    private void CreateInterEdge(Cluster c1, Cluster c2, bool x, int i)
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
     
    //Intra edges are edges that lives inside clusters
    private void GenerateIntraEdges(Cluster c)
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

                ConnectNodes(n1, n2, c);
            }
        }
    }
}