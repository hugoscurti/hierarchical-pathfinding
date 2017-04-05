using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MapSelector : MonoBehaviour {

    public InputField SourceX,
       SourceY,
       DestX,
       DestY;

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
    void Update () {
	}
}
