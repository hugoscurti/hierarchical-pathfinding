using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapController : MonoBehaviour {

    public EventSystem eventSys;

    public MessageBox MsgBox;

    private List<FileInfo> maps;

    private SceneMapDisplay mapDisplay;
    private UIController uiCtrl;

    private Map map;
    private Graph graph;

    private TestResult displayedResult;

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
        displayedResult = null; //Reset the displayed result

        FileInfo current = maps[uiCtrl.DdlMaps.value];
        int ClusterSize = uiCtrl.Cluster.GetValue();
        int LayerDepth = uiCtrl.Layers.GetValue();

        map = Map.LoadMap(current.FullName);

        float deltaT;
        graph = RunGenerateGraph(LayerDepth, ClusterSize, out deltaT);
        uiCtrl.ClusterTime.text = string.Format("{0} s", deltaT);

        uiCtrl.FillLayers(graph.depth);

        mapDisplay.SetMap(map, graph);

        //Automatically draw the last layer when we load a new map
        if (graph.depth == 0)
            mapDisplay.DrawClusters(map, null);
        else
            mapDisplay.DrawClusters(map, graph.C[graph.depth - 1]);

    }


    private Graph RunGenerateGraph(int LayerDepth, int ClusterSize, out float deltatime) {
        float before, after;

        before = Time.realtimeSinceStartup;
        graph = new Graph(map, LayerDepth, ClusterSize);
        after = Time.realtimeSinceStartup;

        deltatime = after - before;
        return graph;
    }

    private TestResult RunPathfind(GridTile start, GridTile dest)
    {
        float before, after;
        TestResult result = new TestResult();

        PathfindResult res = new PathfindResult();
        before = Time.realtimeSinceStartup;
        res.Path = HierarchicalPathfinder.FindPath(graph, start, dest);
        after = Time.realtimeSinceStartup;

        res.RunningTime = after - before;
        res.CalculatePathLength();
        result.HPAStarResult = res;

        res = new PathfindResult();
        before = Time.realtimeSinceStartup;
        res.Path = Pathfinder.FindPath(start, dest, map.Boundaries, map.Obstacles);
        after = Time.realtimeSinceStartup;

        res.RunningTime = after - before;
        res.CalculatePathLength();
        result.AStarResult = res;

        return result;
    }

    public void RunBenchmark()
    {
        FileInfo current = maps[uiCtrl.DdlMaps.value];

        TestResults results = new TestResults()
        {
            MapName = current.Name,
            ClusterSize = uiCtrl.Cluster.GetValue(),
            Layers = uiCtrl.Layers.GetValue()
        };

        map = Map.LoadMap(current.FullName);
        graph = RunGenerateGraph(results.Layers, results.ClusterSize, out results.GenerateClusterTime);

        List<TestCase> testcases = Benchmark.LoadTestCases(current.Name);
        
        TestResult res;
        foreach (TestCase testcase in testcases)
        {
            res = RunPathfind(testcase.Start, testcase.destination);
            res.GroupingNumber = testcase.GroupingNumber;
            results.results.Add(res);
        }

        //Write results in file
        Benchmark.WriteResults(results);
    }

    /// <summary>
    /// Function called when the button find path is clicked. 
    /// Do pathfind for both implementation (A* and HPA*), given the nodes selected
    /// </summary>
    public void FindPath()
    {
        GridTile start = uiCtrl.Source.GetPositionField();
        GridTile dest = uiCtrl.Destination.GetPositionField();

        displayedResult = RunPathfind(start, dest);

        uiCtrl.HPAStarTime.text = string.Format("{0} s", displayedResult.HPAStarResult.RunningTime);
        uiCtrl.HPAStarLength.text = displayedResult.HPAStarResult.PathLength.ToString();
        
        uiCtrl.AStarTime.text = string.Format("{0} s", displayedResult.AStarResult.RunningTime);
        uiCtrl.AStarLength.text = displayedResult.AStarResult.PathLength.ToString();

        //Display the result
        mapDisplay.DrawNormalPath(displayedResult.AStarResult.Path);
        mapDisplay.DrawHpaPath(displayedResult.HPAStarResult.Path, graph.depth - uiCtrl.DdlLayers.value);
    }

    /// <summary>
    /// Function called when the layer dropdown value has changed.
    /// Typically we want to show clusters related to the layer selected
    /// as well as resulting hierarchical paths.
    /// </summary>
    public void OnLayerChange()
    {
        int layer = uiCtrl.DdlLayers.value;

        if (layer == 0)
            mapDisplay.DrawClusters(map, null); //No clusters at the lower level
        else
            mapDisplay.DrawClusters(map, graph.C[layer - 1]);

        if (displayedResult != null)
            mapDisplay.DrawHpaPath(displayedResult.HPAStarResult.Path, graph.depth - layer);
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

    /// <summary>
    /// Select the grid position clicked on.
    /// </summary>
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
                    MsgBox.Show("You cannot select a tile marked as an obstacle.", null);
                    return;
                }

                uiCtrl.SetPosition(pos);
            }
        }
    }
}
