using Priority_Queue;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder {

    private static float EuclidianDistance(Node node1, Node node2)
    {
        return EuclidianDistance(node1.pos, node2.pos);
    }


    private static float EuclidianDistance(GridTile tile1, GridTile tile2)
    {
        return Mathf.Sqrt(Mathf.Pow(tile2.x - tile1.x, 2) + Mathf.Pow(tile2.y - tile1.y, 2));
    }

    public static LinkedList<Edge> FindPath(Node start, Node dest, Boundaries boundaries = null)
    {
        HashSet<GridTile> Visited = new HashSet<GridTile>();
        Dictionary<GridTile, Edge> Parent = new Dictionary<GridTile, Edge>();
        Dictionary<GridTile, float> gScore = new Dictionary<GridTile, float>();

        SimplePriorityQueue<Node, float> pq = new SimplePriorityQueue<Node, float>();

        float temp_gCost, prev_gCost;

        gScore[start.pos] = 0;
        pq.Enqueue(start, EuclidianDistance(start, dest));
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
                //If we defined boundaries, check if it crosses it
                if (boundaries != null && IsOutOfGrid(e.end.pos, boundaries))
                    continue;

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

                pq.Enqueue(e.end, temp_gCost + EuclidianDistance(e.end, dest));
            }
        }
        
        return new LinkedList<Edge>();
    }

    private static bool IsOutOfGrid(GridTile pos, Boundaries boundaries)
    {
        return (pos.x < boundaries.Min.x || pos.x > boundaries.Max.x) ||
               (pos.y < boundaries.Min.y || pos.y > boundaries.Max.y);
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
