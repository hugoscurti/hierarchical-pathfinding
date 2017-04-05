using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MapSelector : MonoBehaviour {


    public Dropdown MapDdl;
    public InputField ClusterTxt;
    public InputField LayerTxt;

    private List<FileInfo> maps;

    private SceneMapDisplay display;

	// Use this for initialization
	void Start () {
        display = GetComponent<SceneMapDisplay>();

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
        display.SetMap(map, ClusterSize, LayerDepth);
    }
	
	// Update is called once per frame
	void Update () {
	}
}
