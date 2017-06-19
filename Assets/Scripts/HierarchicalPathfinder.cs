using System;
using System.Collections;
using System.Collections.Generic;

public class HierarchicalPathfinder
{

    public static LinkedList<Edge> FindHierarchicalPath(Graph graph, GridTile start, GridTile dest)
    {
        Node nStart, nDest;

        //1. Insert nodes
        graph.InsertNodes(start, dest, out nStart, out nDest);

        LinkedList<Edge> path;
        //2. search for path in the highest level
        path = Pathfinder.FindPath(nStart, nDest);


        //3. Remove all created nodes from the graph
        graph.RemoveAddedNodes();

        return path;
    }

    public static LinkedList<Edge> FindLowlevelPath(Graph graph, GridTile start, GridTile dest)
    {
        Node nStart = graph.nodes[start],
            nDest = graph.nodes[dest];

        return Pathfinder.FindPath(nStart, nDest);
    }
}