using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


public class MapLoader : MapGraphContainer {

    public bool showClusters;
    private bool lastClusterActiveState;
    
    [Range(10, 50)]
    public int clusterSize;
    [Range(1, 5)]
    public int layers;

    [HideInInspector]
    public int selectedMap;
    [HideInInspector]
    public string mapName;

    #region Sibling components
    
    private AgentController agentCtrl = null;

    #endregion


    public void LoadMap()
    {
        map = Map.LoadMap(mapName);

        graph = new Graph(map, layers, clusterSize);

        // TODO: update camera default position
        SceneMapDisplay.SetMap(map);
        SceneMapDisplay.DrawClusters(map, graph.C.LastOrDefault());

        SceneMapDisplay.ToggleClusters(showClusters);
    }

    public LinkedList<Edge> GetPath(GridTile start, GridTile dest, bool useHPA)
    {
        if (useHPA)
        {
            var path = HierarchicalPathfinder.FindHierarchicalPath(graph, start, dest);
            return HierarchicalPathfinder.GetLayerPathFromHPA(path, layers);
        }
        else
            return HierarchicalPathfinder.FindLowlevelPath(graph, start, dest);
    }


    public void Awake()
    {
        agentCtrl = GetComponent<AgentController>();

        lastClusterActiveState = showClusters;

        // Load Map on awake
        Clear();
        LoadMap();
    }

    private void Update()
    {
        HandleClusterStateChange();
        HandleSelectPos();
    }

    void HandleClusterStateChange()
    {
        if (showClusters != lastClusterActiveState)
        {
            SceneMapDisplay.ToggleClusters(showClusters);
            lastClusterActiveState = showClusters;
        }
    }

    void HandleSelectPos()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var pos = SelectGridPos();
            if (pos != null)
                agentCtrl.SetNewPosition(pos);
        }
    }

    GridTile SelectGridPos()
    {
        GridTile pos = null;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit && hit.collider.tag == "GridTile")
        {
            Vector3 localHitPoint = transform.worldToLocalMatrix.MultiplyPoint(hit.point);
            pos = new GridTile(localHitPoint);

            if (map.Obstacles[pos.y][pos.x])
                return null;
        }

        return pos;
    }
}
