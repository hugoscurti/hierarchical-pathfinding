using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapController : MonoBehaviour {

    public EventSystem eventSys;

    private List<FileInfo> maps;

    private SceneMapDisplay mapDisplay;
    private UIController uiCtrl;

    private Map map;
    private Graph graph;

	// Use this for initialization
	void Start () {
        mapDisplay = GetComponent<SceneMapDisplay>();
        uiCtrl = GetComponent<UIController>();

        //Populate list of maps
        maps = Map.GetMaps();
        uiCtrl.FillMaps(maps.Select((FileInfo f) => f.Name).ToList());
	}

    public void LoadMap()
    {
        FileInfo current = maps[uiCtrl.DdlMaps.value];
        int ClusterSize = uiCtrl.Cluster.GetValue();
        int LayerDepth = uiCtrl.Layers.GetValue();
        float before, after;

        map = Map.LoadMap(current.FullName);

        before = Time.realtimeSinceStartup;
        graph = new Graph(map, LayerDepth, ClusterSize);
        after = Time.realtimeSinceStartup;
        uiCtrl.ClusterTime.text = string.Format("{0} s", after - before);

        mapDisplay.SetMap(map, graph);
    }

    public void FindPath()
    {
        GridTile start = uiCtrl.Source.GetPositionField();
        GridTile dest = uiCtrl.Destination.GetPositionField();

        float before, after;

        before = Time.realtimeSinceStartup;
        LinkedList<Edge> hpaRes = HierarchicalPathfinder.FindPath(graph, start, dest);
        after = Time.realtimeSinceStartup;
        uiCtrl.HPAStarTime.text = string.Format("{0} s", after - before);

        before = Time.realtimeSinceStartup;
        LinkedList<Edge> aStarRes = Pathfinder.FindPath(start, dest, map.Boundaries, map.Obstacles);
        after = Time.realtimeSinceStartup;
        uiCtrl.AStarTime.text = string.Format("{0} s", after - before);

        //Display the result
        mapDisplay.DrawPaths(hpaRes, aStarRes);
    }


    public void Benchmark()
    {
        throw new System.NotImplementedException("Function not yet implemented");
    }

    // Update is called once per frame
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            mapDisplay.HandleZoom();
            mapDisplay.HandleCameraMove();
            mapDisplay.HandleCameraReset();

            SelectGridPos();
        }
    }

    void SelectGridPos()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100f);

            if (hit && hit.collider.tag == "GridTile")
            {
                Vector3 localHitPoint = transform.worldToLocalMatrix.MultiplyPoint(hit.point);
                GridTile pos = new GridTile(localHitPoint);

                //Be sure that it's a valid position
                if (map.Obstacles[pos.y][pos.x])
                {
                    EditorUtility.DisplayDialog("Info", "You cannot select a tile marked as an obstacle.", "Ok");
                    return;
                }

                uiCtrl.SetPosition(pos);
            }
        }
    }
}
