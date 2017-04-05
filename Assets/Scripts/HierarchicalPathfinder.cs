using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchicalPathfinder : MonoBehaviour
{

    float Depth;
    Map map;

    // Use this for initialization
    void Start()
    {
        //map = Map.LoadMap("arena.map");

        //Graph g = new Graph(map, 1, 10);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public LinkedList<GridTile> FindPath(Graph graph, GridTile start, GridTile dest, Map map)
    {
        //1. Add node start to graph (for each level)

        //2. Add node dest to graph (for each level)

        //3. search for path (from highest to lowest level)

        throw new NotImplementedException("Function not yet implemented");
    }
}