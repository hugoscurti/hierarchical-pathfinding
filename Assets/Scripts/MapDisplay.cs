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

    //Tile size for drawing the map on screen
    float TileUnit;

    private void Awake()
    {
        map = Map.LoadMap("arena.map");
    }
   

    private void OnGUI()
    {
        GUIDrawMap();
    }

    public void GUIDrawMap()
    {
        //TODO: Set size of a tile with respect to the map's size and the screen's ratio
        if (map.Width > map.Height)
            TileUnit = Screen.width / map.Width;
        else
            TileUnit = Screen.height / map.Height;

        Rect tile = new Rect()
        {
            width = TileUnit,
            height = TileUnit
        };
        int startx = Mathf.FloorToInt((float)Screen.width / 2 - ((float)map.Width / 2) * TileUnit);
        int starty = Mathf.FloorToInt((float)Screen.height / 2 - ((float)map.Height / 2) * TileUnit);

        for (int i = 0; i < map.Tiles.Length; ++i)
        {
            for (int j = 0; j < map.Tiles[i].Length; ++j)
            {
                tile.x = j * TileUnit + startx;
                tile.y = i * TileUnit + starty;

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

    // Note that this function is meant to be called from OnGUI() functions.
    public void GUIDrawRect(Rect position, Color color)
    {
        if (RectTexture == null)
            RectTexture = new Texture2D(1, 1);

        if (RectStyle == null)
            RectStyle = new GUIStyle();

        RectTexture.SetPixel(0, 0, color);
        RectTexture.Apply();

        RectStyle.normal.background = RectTexture;
        
        GUI.Box(position, GUIContent.none, RectStyle);
    }
}
