using System;
using System.Collections;
using System.Collections.Generic;

public static class HierarchicalPathfinder
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

    public static LinkedList<Edge> GetLayerPathFromHPA(LinkedList<Edge> hpa, int layer)
    {
        LinkedList<Edge> res = new LinkedList<Edge>();

        //Iterate through all edges as a breadth-first-search on parent-child connections between edges
        //we start at value layers, and add children to the queue while decrementing the layer value.
        //When the layer value is 0, we display it
        Queue<ValueTuple<int, Edge>> queue = new Queue<ValueTuple<int, Edge>>();

        //Add all edges from current level
        foreach (Edge e in hpa)
            queue.Enqueue(new ValueTuple<int, Edge>(layer, e));

        ValueTuple<int, Edge> current;
        while (queue.Count > 0)
        {
            current = queue.Dequeue();

            if (current.Item1 == 0)
            {
                res.AddLast(current.Item2);
            }
            else if (current.Item2.type == EdgeType.INTER)
            {
                //No underlying path for intra edges... 
                //Add the same edge with lower layer
                queue.Enqueue(new ValueTuple<int, Edge>(current.Item1 - 1, current.Item2));
            }
            else
            {
                foreach (Edge e in current.Item2.UnderlyingPath)
                    queue.Enqueue(new ValueTuple<int, Edge>(current.Item1 - 1, e));
            }
        }

        return res;
    }
}