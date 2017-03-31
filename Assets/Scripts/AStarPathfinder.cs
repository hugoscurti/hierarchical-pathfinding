using Priority_Queue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour {

    //Offset used to get neighbours of a cell (in manhattan setup)
    GridTile[] neighbours = {
        new GridTile(-1, 0),
        new GridTile(1, 0),
        new GridTile(0, -1),
        new GridTile(0, 1)
    };

    //Simple priority queue, from this repo https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
    SimplePriorityQueue<GridTile, float> pq;

    void Start()
    {
        //Initialize Priority Queue
        pq = new SimplePriorityQueue<GridTile, float>();
    }

    private int EuclidianDistanceSquared(GridTile tile1, GridTile tile2)
    {
        return (int) (
            Mathf.Pow(tile2.x - tile1.x, 2) + Mathf.Pow(tile2.y - tile1.y, 2)
        );
    }


    public LinkedList<GridTile> FindPath(GridTile start, GridTile dest, Map map)
    {
        Dictionary<GridTile, bool> Visited = new Dictionary<GridTile, bool>();
        Dictionary<GridTile, GridTile> Parent = new Dictionary<GridTile, GridTile>();
        Dictionary<GridTile, float> gScore = new Dictionary<GridTile, float>();
        bool found = false;

        pq.Clear();
        float temp_gCost, prev_gCost;

        gScore[start] = 0;
        pq.Enqueue(start, EuclidianDistanceSquared(start, dest));
        GridTile current, neighbour = new GridTile();

        while (pq.Count > 0)
        {
            current = pq.Dequeue();
            if (current.Equals(dest))
            {
                found = true;
                break;
            }
            Visited[current] = true;

            //Visit all neighbours of current
            foreach (GridTile offsetNeighbour in neighbours)
            {
                neighbour.x = current.x + offsetNeighbour.x;
                neighbour.y = current.y + offsetNeighbour.y;

                //Check if neighbour is an Obstacles
                //Check if neighbour is outside of grid
                if ( IsOutOfGrid(neighbour, map) || map.Obstacles[neighbour.x][neighbour.y] )
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

        if (found)
        {
            //Rebuild path
            return RebuildPath(Parent, dest);
        }
        else
        {
            Debug.Log("Can't reach the node specified");
            return new LinkedList<GridTile>();
        }
    }

    private bool IsOutOfGrid(GridTile pos, Map map)
    {
        return (pos.x < 0 || pos.x >= map.Width) ||
               (pos.y < 0 || pos.y >= map.Height);
    }

    //Rebuild of grid tiles
    private LinkedList<GridTile> RebuildPath(Dictionary<GridTile, GridTile> Parent, GridTile dest)
    {
        LinkedList<GridTile> res = new LinkedList<GridTile>();

        GridTile current = dest;
        do {
            res.AddFirst(current);
        } while (Parent.TryGetValue(current, out current));

        return res;
    }
}
