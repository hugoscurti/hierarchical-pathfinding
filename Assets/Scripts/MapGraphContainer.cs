using System;
using UnityEngine;

public abstract class MapGraphContainer : MonoBehaviour
{

    protected Map map;
    protected Graph graph;

    // Singleton instance for smooth access even in editor mode
    private SceneMapDisplay _sceneMapDisplay = null;
    protected SceneMapDisplay SceneMapDisplay {
        get {
            if (_sceneMapDisplay == null)
                _sceneMapDisplay = GetComponent<SceneMapDisplay>();
            return _sceneMapDisplay;
        }
    }


    protected void Clear()
    {
        graph = null;
        map = null;

        SceneMapDisplay.ClearMap();

        GC.Collect();
    }
}

