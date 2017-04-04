using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMapDisplay : MonoBehaviour {

    //Tile colors
    public Color Green = Color.green;
    public Color Black = Color.black;
    public Color White = Color.white;

    //Camera
    public Camera cam;

    private Map map;
    private Graph graph;

    //Variables to draw in scene
    public GameObject Tile2d;
    GameObject clone;
    new SpriteRenderer renderer;

    //GameObject to store Cluster related objects
    GameObject MapGameObj;
    GameObject Clusters;
    GameObject Nodes;
    GameObject Edges;

    void Awake ()
    {
        map = Map.LoadMap("arena.map");
        graph = new Graph(map, 1, 10);
    }

    // Use this for initialization
    void Start () {
        //Adjust camera with respect to the map's size
        cam.transform.position = new Vector3(map.Width / 2f, -map.Height / 2f, -map.Width);
        cam.orthographicSize = map.Height/2f + 3;

        //Instantiate Empty Containes for objects
        MapGameObj = new GameObject("Map");
        MapGameObj.transform.SetParent(transform, false);
        Clusters = new GameObject("Clusters");
        Clusters.transform.SetParent(transform, false);
        Nodes = new GameObject("Nodes");
        Nodes.transform.SetParent(transform, false);
        Edges = new GameObject("Edges");
        Edges.transform.SetParent(transform, false);

        //Instantiate huge sprite for background (including out of maps nodes @)
        DrawSprite(
            new Vector3(map.Width / 2f, map.Height / 2f, 0),
            new Vector3(map.Width, map.Height, 1),
            Black, 0, Quaternion.identity,
            MapGameObj);

		//Instantiate prefabs for gridtile
        //i is the y coordinate
        for(int i = 0; i < map.Tiles.Length; ++i)
        {
            //j is the x coordinate
            for(int j = 0; j <map.Tiles[i].Length; ++j)
            {
                switch (map.Tiles[i][j])
                {
                    case 'T':
                        DrawTile(j, i, Green);
                        break;
                    case '.':
                    case 'G':
                        DrawTile(j, i, White);
                        break;
                }
            }
        }

        DrawClusters(0);

    }

    private void DrawClusters(int layer)
    {
        HashSet<GridTile> Visited = new HashSet<GridTile>();

        foreach (Cluster c in graph.C[layer])
        {
            //1. Draw borders
            DrawBorder(c);

            //2. Draw edges
            foreach (KeyValuePair<GridTile, Node> node in c.Nodes)
            {
                //Draw node
                DrawNode(node.Key);

                Visited.Add(node.Key);

                //Draw Edges
                foreach (Edge e in node.Value.edges)
                {
                    if (!Visited.Contains(e.end.value))
                    {
                        //Draw the edge
                        DrawEdge(e.start.value, e.end.value);
                    }
                }
            }
        }
    }

    private void DrawBorder(Cluster c)
    {
        Vector3 pos = new Vector3() { z = 1 };
        Vector3 scale = new Vector3() { z = 1 };
        Quaternion rot = Quaternion.identity;

        //Min vertical line
        scale.x = 0.5f;
        scale.y = scale.x + c.Height;   //To put some padding around
        pos.x = c.Boundaries.Min.x;
        pos.y = c.Boundaries.Min.y + c.Height / 2f;
        DrawSprite(pos, scale, Black, 2, rot, Clusters);

        //Draw Max Vertical only if border is at the right boundary
        if (c.Boundaries.Max.x == map.Boundaries.Max.x)
        {
            pos.x = c.Boundaries.Max.x + 1;
            DrawSprite(pos, scale, Black, 2, rot, Clusters);
        }

        //Min horizontal line
        scale.y = 0.5f;
        scale.x = scale.y + c.Width;
        pos.x = c.Boundaries.Min.x + c.Width / 2f;
        pos.y = c.Boundaries.Min.y;
        DrawSprite(pos, scale, Black, 2, rot, Clusters);

        //Draw Max horizontal only if cluster is at boundary
        if(c.Boundaries.Max.y == map.Boundaries.Max.y)
        {
            pos.y = c.Boundaries.Max.y + 1;
            DrawSprite(pos, scale, Black, 2, rot, Clusters);
        }
    }

    private void DrawSprite(Vector3 pos, Vector3 scale, Color color, int sortOrder, Quaternion rot, GameObject parent)
    {
        clone = Instantiate(Tile2d, parent.transform);
        clone.transform.localRotation = rot;
        clone.transform.localPosition = pos;
        clone.transform.localScale = scale;
        renderer = clone.GetComponent<SpriteRenderer>();
        renderer.color = color;
        renderer.sortingOrder = sortOrder;
    }

    private void DrawNode(GridTile pos)
    {
        DrawSprite(new Vector3(pos.x + 0.5f, pos.y + 0.5f, 2),
                new Vector3(0.2f, 0.2f, 1),
                Black,
                2,
                Quaternion.identity,
                Nodes);
    }

    private void DrawEdge(GridTile start, GridTile end)
    {
        Vector3 pos = new Vector3(start.x, start.y, 3);
        Vector3 vEdge = new Vector3(end.x - start.x, end.y - start.y, 0);
        Vector3 scale = new Vector3(vEdge.magnitude, 0.1f, 1);

        pos = pos + vEdge / 2;
        pos.x += 0.5f;
        pos.y += 0.5f;

        float angle = Vector3.Angle(Vector3.right, vEdge);
        //Since Vector3.agle doesn't consider direction, we check for direction with cross product
        Vector3 cross = Vector3.Cross(Vector3.right, vEdge);
        if (cross.z < 0) angle = 360 - angle;

        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

        DrawSprite(pos, scale, Black, 2, rot, Edges);
    }

    private void DrawTile(int x, int y, Color color)
    {
        //x and y represents the location in the map. 
        //The tile we instantiate are centered, so we add up half a unit in both x and y direction
        DrawSprite(new Vector3(x + 0.5f, y + 0.5f, 0), Vector3.one, color, 1, Quaternion.identity, MapGameObj);
    }

	
	// Update is called once per frame
	void Update () {
		
	}
}
