using System;
using System.Collections;
using System.Collections.Generic;

public class HierarchicalPathfinder
{

    public static LinkedList<Edge> FindPath(Graph graph, GridTile start, GridTile dest)
    {
        //TODO: find first level where start and end are not in the same clusters
        // If they are always in the same cluster, then simply pathfind through them

        //1. Add node start to graph (for each level)
        Node[] nStart = graph.InsertNode(start);

        //2. Add node dest to graph (for each level)
        Node[] nDest = graph.InsertNode(dest);

        //3. search for path in the highest level
        LinkedList<Edge> path = Pathfinder.FindPath(nStart[graph.depth - 1], nDest[graph.depth - 1]);
        return path;
    }
}