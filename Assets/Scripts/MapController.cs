using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapController : MonoBehaviour {

    public InputField SourceX,
       SourceY,
       DestX,
       DestY;

    public EventSystem eventSys;

    //Bool that says which last gridpoint we set between source and destination
    private bool sourceSet = false;

    public Dropdown MapDdl;
    public InputField ClusterTxt;
    public InputField LayerTxt;

    private List<FileInfo> maps;

    private SceneMapDisplay display;
    private HierarchicalPathfinder hpa;

    private Graph graph;

	// Use this for initialization
	void Start () {
        display = GetComponent<SceneMapDisplay>();
        hpa = GetComponent<HierarchicalPathfinder>();

        //Populate list of maps
        maps = Map.GetMaps();

        foreach (FileInfo f in maps)
            MapDdl.options.Add(new Dropdown.OptionData(f.Name));

        //Selects first
        MapDdl.value = 0;
        MapDdl.RefreshShownValue();
	}


    public void LoadMap()
    {
        FileInfo current = maps[MapDdl.value];
        int ClusterSize = int.Parse(ClusterTxt.text);
        int LayerDepth = int.Parse(LayerTxt.text);

        Map map = Map.LoadMap(current.FullName);
        graph = display.SetMap(map, ClusterSize, LayerDepth);
    }

    public void FindPath()
    {
        GridTile start = new GridTile(int.Parse(SourceX.text), int.Parse(SourceY.text));
        GridTile dest = new GridTile(int.Parse(DestX.text), int.Parse(DestY.text));

        LinkedList<Edge> res = hpa.FindPath(graph, start, dest);

        //Display the result
        display.DrawPath(res);
    }

    // Update is called once per frame
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            display.HandleZoom();
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

                if (sourceSet)
                {
                    //Set Destination
                    DestX.text = pos.x.ToString();
                    DestY.text = pos.y.ToString();
                }
                else
                {
                    SourceX.text = pos.x.ToString();
                    SourceY.text = pos.y.ToString();
                }

                sourceSet = !sourceSet;
            }
        }
    }


}
