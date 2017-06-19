using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    public int depth;
    //List of clusters for every level of abstraction
    public List<Cluster>[] C;

    //Keep a representation of the map by low level nodes
    Dictionary<GridTile, Node> nodes;

    int width;
    int height;

    //We keep track of added nodes to remove them afterwards
    List<Node> AddedNodes;

    /// <summary>
    /// Construct a graph from the map
    /// </summary>
    public Graph(Map map, int MaxLevel, int clusterSize)
    {
        depth = MaxLevel;
        AddedNodes = new List<Node>();

        nodes = CreateMapRepresentation(map);
        width = map.Width;
        height = map.Height;

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


    /// <summary>
    /// Create the node-based representation of the map
    /// </summary>
    private Dictionary<GridTile, Node> CreateMapRepresentation(Map map)
    {
        var mapnodes = new Dictionary<GridTile, Node>(map.FreeTiles);
        int i, j;
        GridTile gridTile;

        //1. Create all nodes necessary
        for (i = 0; i < map.Width; ++i)
            for (j = 0; j < map.Height; ++j)
            {
                if (!map.Obstacles[j][i])
                {
                    gridTile = new GridTile(i, j);
                    mapnodes.Add(gridTile, new Node(gridTile));
                }
            }

        //2. Create all possible edges
        foreach (Node n in mapnodes.Values)
        {
            //Look for straight edges
            for(i = -1; i < 2; i += 2)
            {
                SearchMapEdge(map, mapnodes, n, n.pos.x + i, n.pos.y, 1f);

                SearchMapEdge(map, mapnodes, n, n.pos.x, n.pos.y + i, 1f);
            }

            //Look for diagonal edges
            for(i = -1; i< 2; i += 2)
                for(j = -1; j < 2; j += 2)
                {
                    SearchMapEdge(map, mapnodes, n, n.pos.x + i, n.pos.y + j, Pathfinder.SQRT2);
                }
        }

        return mapnodes;
    }

    /// <summary>
    /// Add the edge to the node if it's a valid map edge
    /// </summary>
    private void SearchMapEdge(Map map, Dictionary<GridTile, Node> mapNodes, Node n, int x, int y, float weight)
    {
        GridTile gridTile = new GridTile(x, y);
        if (map.IsFreeTile(gridTile))
        {
            n.edges.Add(new Edge()
            {
                start = n,
                end = mapNodes[gridTile],
                type = EdgeType.INTER,
                weight = weight
            });
        }
    }

    /// <summary>
    /// Insert start and dest nodes in graph in all layers
    /// </summary>
    public void InsertNodes(GridTile start, GridTile dest, out Node nStart, out Node nDest)
    {
        Cluster cStart, cDest;
        Node newStart, newDest;
        nStart = nodes[start];
        nDest = nodes[dest];
        bool isConnected;
        AddedNodes.Clear();

        for (int i = 0; i < depth; ++i)
        {
            cStart = null;
            cDest = null;
            isConnected = false;

            foreach (Cluster c in C[i])
            {
                if (c.Contains(start))
                    cStart = c;

                if (c.Contains(dest))
                    cDest = c;

                if (cStart != null && cDest != null)
                    break;
            }

            //This is the right cluster
            if (cStart == cDest)
            {
                newStart = new Node(start) { child = nStart };
                newDest = new Node(dest) { child = nDest };

                isConnected = ConnectNodes(newStart, newDest, cStart);

                if (isConnected)
                {
                    //If they are reachable then we set them as the nodes
                    //Otherwise we might be able to reach them from an upper layer
                    nStart = newStart;
                    nDest = newDest;
                }
            }

            if (!isConnected)
            {
                nStart = ConnectToBorder(start, cStart, nStart);
                nDest = ConnectToBorder(dest, cDest, nDest);
            }
        }
    }

    /// <summary>
    /// Remove nodes from the graph, including all underlying edges
    /// </summary>
    public void RemoveAddedNodes()
    {
        foreach(Node n in AddedNodes)
            foreach(Edge e in n.edges)
                //Find an edge in current.end that points to this node
                e.end.edges.RemoveAll((ee)=> ee.end == n);
    }

    /// <summary>
    /// Connect the grid tile to borders by creating a new node
    /// </summary>
    /// <returns>The node created</returns>
    private Node ConnectToBorder(GridTile pos, Cluster c, Node child)
    {
        Node newNode;

        //If the position is an actual border node, then return it
        if (c.Nodes.TryGetValue(pos, out newNode))
            return newNode;

        //Otherwise create a node and pathfind through border nodes
        newNode = new Node(pos) { child = child };

        foreach (KeyValuePair<GridTile, Node> n in c.Nodes)
        {
            ConnectNodes(newNode, n.Value, c);
        }

        //Since this node is not part of the graph, we keep track of it to remove it later
        AddedNodes.Add(newNode);

        return newNode;
    }

    /// <summary>
    /// Connect two nodes by pathfinding between them. 
    /// </summary>
    /// <remarks>We assume they are different nodes. If the path returned is 0, then there is no path that connects them.</remarks>
    private bool ConnectNodes(Node n1, Node n2, Cluster c)
    {
        LinkedList<Edge> path;
        LinkedListNode<Edge> iter;
        Edge e1, e2;

        float weight = 0f;

        path = Pathfinder.FindPath(n1.child, n2.child, c.Boundaries);

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

            return true;
        } else
        {
            //No path, return false
            return false;
        }
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

        Cluster clst;

        int i, j;

        //Create clusters of this level
        for (i = 0; i < ClusterHeight; ++i)
            for (j = 0; j < ClusterWidth; ++j)
            {
                clst = new Cluster();
                clst.Boundaries.Min = new GridTile(j * ClusterSize, i * ClusterSize);
                clst.Boundaries.Max = new GridTile(
                    Mathf.Min(clst.Boundaries.Min.x + ClusterSize - 1, width - 1),
                    Mathf.Min(clst.Boundaries.Min.y + ClusterSize - 1, height - 1));

                //Adjust size of cluster based on boundaries
                clst.Width = clst.Boundaries.Max.x - clst.Boundaries.Min.x + 1;
                clst.Height = clst.Boundaries.Max.y - clst.Boundaries.Min.y + 1;

                if (level > 0)
                {
                    //Since we're abstract, we will have lower level clusters
                    clst.Clusters = new List<Cluster>();

                    //Add lower level clusters in newly created clusters
                    foreach (Cluster c in C[level - 1])
                        if (clst.Contains(c))
                            clst.Clusters.Add(c);
                }

                clusters.Add(clst);
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
            GenerateIntraEdges(clusters[i]);

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
            if (x && (nodes.ContainsKey(new GridTile(c1.Boundaries.Max.x, i)) && nodes.ContainsKey(new GridTile(c2.Boundaries.Min.x, i)))
                || !x && (nodes.ContainsKey(new GridTile(i, c1.Boundaries.Max.y)) && nodes.ContainsKey(new GridTile(i, c2.Boundaries.Min.y))))
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
            n1.child = nodes[g1];
        }

        if (!c2.Nodes.TryGetValue(g2, out n2))
        {
            n2 = new Node(g2);
            c2.Nodes.Add(g2, n2);
            n2.child = nodes[g2];
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