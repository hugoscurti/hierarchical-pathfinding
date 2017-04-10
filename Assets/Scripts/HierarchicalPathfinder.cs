using System;
using System.Collections;
using System.Collections.Generic;

public class HierarchicalPathfinder
{

    public static LinkedList<Edge> FindPath(Graph graph, GridTile start, GridTile dest)
    {
        Node nStart, nDest;

        //1. Insert nodes
        graph.InsertNodes(start, dest, out nStart, out nDest);

        LinkedList<Edge> path;
        //2. search for path in the highest level
        path = Pathfinder.FindPath(nStart, nDest);


        //3. Remove all created nodes from the graph
        graph.RemoveNodes(nStart, nDest);

        return path;
    }
}