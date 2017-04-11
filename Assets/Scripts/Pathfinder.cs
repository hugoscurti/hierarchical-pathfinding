using Priority_Queue;
using System.Collections.Generic;
using UnityEngine;


public class Pathfinder {
    private static float SQRT2 = Mathf.Sqrt(2f);

    //Offset used to get neighbours of a cell (in manhattan setup)
    static KeyValuePair<GridTile, float>[] neighbours = {
        new KeyValuePair<GridTile, float>(new GridTile(-1, 0), 1f),
        new KeyValuePair<GridTile, float>(new GridTile(1, 0), 1f),
        new KeyValuePair<GridTile, float>(new GridTile(0, -1), 1f),
        new KeyValuePair<GridTile, float>(new GridTile(0, 1), 1f),
        //Diagonal movements
        new KeyValuePair<GridTile, float>(new GridTile(-1,-1), SQRT2),
        new KeyValuePair<GridTile, float>(new GridTile(-1, 1), SQRT2),
        new KeyValuePair<GridTile, float>(new GridTile( 1, 1), SQRT2),
        new KeyValuePair<GridTile, float>(new GridTile( 1,-1), SQRT2)
    };

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

    public static LinkedList<Edge> FindPath(GridTile start, GridTile dest, Boundaries boundaries, bool[][] obstacles)
    {
        Dictionary<GridTile, bool> Visited = new Dictionary<GridTile, bool>();
        Dictionary<GridTile, GridTile> Parent = new Dictionary<GridTile, GridTile>();
        Dictionary<GridTile, float> gScore = new Dictionary<GridTile, float>();

        //Simple priority queue, from this repo https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
        SimplePriorityQueue<GridTile, float> pq = new SimplePriorityQueue<GridTile, float>();

        float temp_gCost, prev_gCost;

        gScore[start] = 0;
        pq.Enqueue(start, EuclidianDistance(start, dest));
        GridTile current, neighbour = new GridTile();

        while (pq.Count > 0)
        {
            current = pq.Dequeue();
            if (current.Equals(dest))
                //Rebuild path
                return RebuildPath(Parent, gScore, dest);

            Visited[current] = true;

            //Visit all neighbours of current
            foreach (KeyValuePair<GridTile,float> offset in neighbours)
            {
                neighbour.x = current.x + offset.Key.x;
                neighbour.y = current.y + offset.Key.y;

                //Check if neighbour is an Obstacles
                //Check if neighbour is outside of the boundaries specified
                if ( IsOutOfGrid(neighbour, boundaries) || obstacles[neighbour.y][neighbour.x] )
                    continue;

                //Already visited
                if (Visited.ContainsKey(neighbour))
                    continue;

                temp_gCost = gScore[current] + offset.Value;

                //If new value is not better then do nothing
                if (gScore.TryGetValue(neighbour, out prev_gCost) && temp_gCost >= prev_gCost)
                    continue;

                Parent[neighbour] = current;
                gScore[neighbour] = temp_gCost;

                pq.Enqueue(neighbour, temp_gCost + EuclidianDistance(neighbour, dest));
                //Loose the reference to this neighbour, we don't want to modify it in the queue
                neighbour = new GridTile();
            }
        }

        return new LinkedList<Edge>();
    }

    private static bool IsOutOfGrid(GridTile pos, Boundaries boundaries)
    {
        return (pos.x < boundaries.Min.x || pos.x > boundaries.Max.x) ||
               (pos.y < boundaries.Min.y || pos.y > boundaries.Max.y);
    }

    //Rebuild path with edges
    private static LinkedList<Edge> RebuildPath(Dictionary<GridTile, GridTile> Parent, Dictionary<GridTile, float> cost, GridTile dest)
    {
        LinkedList<Edge> res = new LinkedList<Edge>();
        float currentCost = cost[dest];
        float edgeCost;
        Edge e;
        Node currNode = new Node(dest);
        Node prevNode;
        GridTile previous = Parent[dest];

        do {
            prevNode = new Node(previous);
            edgeCost = currentCost - cost[previous];

            e = new Edge() { start = prevNode, end = currNode, type = EdgeType.INTER, weight = edgeCost };
            prevNode.edges.Add(e); //This might not be necessary. Just for consistency

            res.AddFirst(e);

            //Update the current value
            currNode = prevNode;
            currentCost -= edgeCost;
            
        } while (Parent.TryGetValue(previous, out previous));

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
