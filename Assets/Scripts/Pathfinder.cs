using Priority_Queue;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder {

    //Offset used to get neighbours of a cell (in manhattan setup)
    static GridTile[] neighbours = {
        new GridTile(-1, 0),
        new GridTile(1, 0),
        new GridTile(0, -1),
        new GridTile(0, 1)
    };

    private static int EuclidianDistanceSquared(Node node1, Node node2)
    {
        return EuclidianDistanceSquared(node1.pos, node2.pos);
    }


    private static int EuclidianDistanceSquared(GridTile tile1, GridTile tile2)
    {
        return (int) (
            Mathf.Pow(tile2.x - tile1.x, 2) + Mathf.Pow(tile2.y - tile1.y, 2)
        );
    }

    public static LinkedList<Edge> FindPath(Node start, Node dest)
    {
        HashSet<GridTile> Visited = new HashSet<GridTile>();
        Dictionary<GridTile, Edge> Parent = new Dictionary<GridTile, Edge>();
        Dictionary<GridTile, float> gScore = new Dictionary<GridTile, float>();

        SimplePriorityQueue<Node, float> pq = new SimplePriorityQueue<Node, float>();

        float temp_gCost, prev_gCost;

        gScore[start.pos] = 0;
        pq.Enqueue(start, EuclidianDistanceSquared(start, dest));
        Node current;

        while(pq.Count > 0)
        {
            current = pq.Dequeue();
            if (current.pos.Equals(dest.pos))
                //Rebuild path and return it
                return RebuildPath(Parent, current);


            Visited.Add(current.pos);

            //Visit all neighbours through edges going out of node
            foreach (Edge e in current.edges)
            {
                //Check if we visited the outer end of the edge
                if (Visited.Contains(e.end.pos))
                    continue;

                temp_gCost = gScore[current.pos] + e.weight;
                
                //If new value is not better then do nothing
                if (gScore.TryGetValue(e.end.pos, out prev_gCost) && temp_gCost >= prev_gCost)
                    continue;

                //Otherwise store the new value and add the destination into the queue
                Parent[e.end.pos] = e;
                gScore[e.end.pos] = temp_gCost;

                pq.Enqueue(e.end, temp_gCost + EuclidianDistanceSquared(e.end, dest));
            }
        }
        
        //If we go through here that means we didn't find a path
        Debug.Log("Can't reach the node specified");
        return new LinkedList<Edge>();
    }

    //TODO: Handle diagonal movements?
    public static LinkedList<GridTile> FindPath(GridTile start, GridTile dest, Boundaries boundaries, bool[][] obstacles)
    {
        Dictionary<GridTile, bool> Visited = new Dictionary<GridTile, bool>();
        Dictionary<GridTile, GridTile> Parent = new Dictionary<GridTile, GridTile>();
        Dictionary<GridTile, float> gScore = new Dictionary<GridTile, float>();

        //Simple priority queue, from this repo https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
        SimplePriorityQueue<GridTile, float> pq = new SimplePriorityQueue<GridTile, float>();

        float temp_gCost, prev_gCost;

        gScore[start] = 0;
        pq.Enqueue(start, EuclidianDistanceSquared(start, dest));
        GridTile current, neighbour = new GridTile();

        while (pq.Count > 0)
        {
            current = pq.Dequeue();
            if (current.Equals(dest))
                //Rebuild path
                return RebuildPath(Parent, dest);

            Visited[current] = true;

            //Visit all neighbours of current
            foreach (GridTile offsetNeighbour in neighbours)
            {
                neighbour.x = current.x + offsetNeighbour.x;
                neighbour.y = current.y + offsetNeighbour.y;

                //Check if neighbour is an Obstacles
                //Check if neighbour is outside of the boundaries specified
                if ( IsOutOfGrid(neighbour, boundaries) || obstacles[neighbour.y][neighbour.x] )
                    continue;

                //Already visited
                if (Visited.ContainsKey(neighbour))
                    continue;

                temp_gCost = gScore[current] + 1;

                //If new value is not better then do nothing
                if (gScore.TryGetValue(neighbour, out prev_gCost) && temp_gCost >= prev_gCost)
                    continue;

                Parent[neighbour] = current;
                gScore[neighbour] = temp_gCost;

                pq.Enqueue(neighbour, temp_gCost + EuclidianDistanceSquared(neighbour, dest));
                //Loose the reference to this neighbour, we don't want to modify it in the queue
                neighbour = new GridTile();
            }
        }

        Debug.Log("Can't reach the node specified");
        return new LinkedList<GridTile>();
    }

    private static bool IsOutOfGrid(GridTile pos, Boundaries boundaries)
    {
        return (pos.x < boundaries.Min.x || pos.x > boundaries.Max.x) ||
               (pos.y < boundaries.Min.y || pos.y > boundaries.Max.y);
    }

    //Rebuild of grid tiles
    private static LinkedList<GridTile> RebuildPath(Dictionary<GridTile, GridTile> Parent, GridTile dest)
    {
        LinkedList<GridTile> res = new LinkedList<GridTile>();

        GridTile current = dest;
        do {
            res.AddFirst(current);
        } while (Parent.TryGetValue(current, out current));

        return res;
    }


    //Rebuild edges
    private static LinkedList<Edge> RebuildPath(Dictionary<GridTile, Edge> Parent, Node dest)
    {
        LinkedList<Edge> res = new LinkedList<Edge>();
        GridTile current = dest.pos;
        Edge e = null;

        while(Parent.TryGetValue(current, out e))
        {
            res.AddFirst(e);
            current = e.start.pos;
        }

        return res;
    }
}
