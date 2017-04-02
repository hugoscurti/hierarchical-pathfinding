using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;

public class MapDisplay : MonoBehaviour {

    //Variables to draw on gui
    private Texture2D RectTexture;
    private GUIStyle RectStyle;
    
    //Tile colors
    public Color Green = Color.green;
    public Color Black = Color.black;
    public Color White = Color.white;

    public RectTransform mainPanel;

    private Map map;
    private Graph graph;

    //Tile size for drawing the map on screen
    float TileUnit;
    int offsetX, offsetY;

    private void Awake()
    {
        map = Map.LoadMap("arena.map");

        graph = new Graph(map, 1, 10);

        //TODO: Set size of a tile with respect to the map's size and the screen's ratio
        if (map.Width > map.Height)
            TileUnit = Screen.width / map.Width;
        else
            TileUnit = Screen.height / map.Height;

        offsetX = Mathf.FloorToInt((float)Screen.width / 2 - ((float)map.Width / 2) * TileUnit);
        offsetY = Mathf.FloorToInt((float)Screen.height / 2 - ((float)map.Height / 2) * TileUnit);
    }
   

    private void OnGUI()
    {
        GUIDrawMap();
        GUIDrawGraph(0);
    }

    public void GUIDrawMap()
    {       

        Rect tile = new Rect()
        {
            width = 1,
            height = 1
        };

        for (int i = 0; i < map.Tiles.Length; ++i)
        {
            for (int j = 0; j < map.Tiles[i].Length; ++j)
            {
                tile.x = j;
                tile.y = i;

                switch (map.Tiles[i][j])
                {
                    case '@':
                    case 'O':
                        GUIDrawRect(tile, Black);
                        break;
                    case 'T':
                        GUIDrawRect(tile, Green);
                        break;
                    case '.':
                    case 'G':
                        GUIDrawRect(tile, White);
                        break;
                }
            }
        }
    }

    public void GUIDrawGraph(int layer)
    {
        Rect line = new Rect();
        HashSet<GridTile> Visited = new HashSet<GridTile>();

        foreach (Cluster c in graph.C[layer])
        {
            //1. Draw borders
            //Min vertical line
            line.width = 0.5f;
            line.height = c.Height + line.width;
            line.x = c.TopLeft.x - (line.width / 2f);
            line.y = c.TopLeft.y - (line.width / 2f);
            GUIDrawRect(line, Black);

            //Max vertical line
            line.x = c.BottomRight.x + (1 - line.width / 2f);
            GUIDrawRect(line, Black);

            //Min horizontal line
            line.height = 0.5f;
            line.width = c.Width + line.height;
            line.x = c.TopLeft.x - line.height/2f;
            line.y = c.TopLeft.y - line.height/2f;
            GUIDrawRect(line, Black);

            //Max Horizontal Line
            line.y = c.BottomRight.y + (1 - line.height/2f);
            GUIDrawRect(line, Black);

            
            //2. Draw edges
            foreach (KeyValuePair<GridTile, Node> node in c.Nodes)
            {
                //Draw node
                line.width = 0.5f;
                line.height = 0.5f;
                line.x = node.Key.x + line.width / 2f;
                line.y = node.Key.y + line.height / 2f;
                GUIDrawRect(line, Black);

                Visited.Add(node.Key);

                //Draw Edges
                foreach (Edge e in node.Value.edges)
                {
                    if (!Visited.Contains(e.end.value))
                    {
                        //Draw the edge
                        line.width = e.end.value.x == node.Key.x ? 0.5f : Mathf.Abs(e.end.value.x - node.Key.x);
                        line.height = e.end.value.y == node.Key.y ? 0.5f : Mathf.Abs(e.end.value.y - node.Key.y);
                        line.x = e.end.value.x < node.Key.x ? e.end.value.x + 0.5f / 2 : node.Key.x + 0.5f / 2;
                        line.y = e.end.value.y < node.Key.y ? e.end.value.y + 0.5f / 2 : node.Key.y + 0.5f / 2;
                        GUIDrawRect(line, Black);
                    }
                }
            }
        }

        int g = 0;

    }

    // Note that this function is meant to be called from OnGUI() functions.
    public void GUIDrawRect(Rect position, Color color)
    {
        if (RectTexture == null)
            RectTexture = new Texture2D(1, 1);

        if (RectStyle == null)
            RectStyle = new GUIStyle();

        //Edit position to be scale properly
        //Width and height *tileUnit
        //x and y * tileUnit + start[x,y]
        position.width *= TileUnit;
        position.height *= TileUnit;
        position.x = position.x * TileUnit + offsetX;
        position.y = position.y * TileUnit + offsetY;

        RectTexture.SetPixel(0, 0, color);
        RectTexture.Apply();

        RectStyle.normal.background = RectTexture;
        GUI.Box(position, GUIContent.none, RectStyle);
    }
}
