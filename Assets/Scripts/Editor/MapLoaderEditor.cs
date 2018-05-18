using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapLoader))]
public class MapLoaderEditor : Editor {

    private string[] mapnames;
    private string[] fullnames;

    private void OnEnable()
    {
        var Maps = Map.GetMaps();
        mapnames = Maps.Select(m => m.Name).ToArray();
        fullnames = Maps.Select(m => m.FullName).ToArray();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapLoader mapLoader = (MapLoader)target;

        if (mapnames == null) return;

        mapLoader.selectedMap = EditorGUILayout.Popup(mapLoader.selectedMap, mapnames, EditorStyles.popup);
        mapLoader.mapName = fullnames[mapLoader.selectedMap];
    }

}
