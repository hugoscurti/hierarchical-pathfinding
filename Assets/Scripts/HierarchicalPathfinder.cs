using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HierarchicalPathfinder : MonoBehaviour
{
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

    public LinkedList<Edge> FindPath(Graph graph, GridTile start, GridTile dest)
    {
        //1. Add node start to graph (for each level)
        Node[] nStart = graph.InsertNode(start, graph.depth);

        //2. Add node dest to graph (for each level)
        Node[] nDest = graph.InsertNode(dest, graph.depth);

        //3. search for path (from highest to lowest level)
        LinkedList<Edge> path = Pathfinder.FindPath(nStart[0], nDest[0]);
        return path;
    }
}