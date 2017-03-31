using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;

public class MapLoader : MonoBehaviour {

    //Variables to draw on gui
    private static Texture2D RectTexture;
    private static GUIStyle RectStyle;

    private string BaseMapDirectory;

    //Tile colors
    private Color Green = Color.green;
    private Color Black = Color.black;
    private Color White = Color.white;

    private Map map;
    private char[][] MapTiles;

    //Tile size for drawing the map on screen
    float TileUnit;

    private void Awake()
    {
        BaseMapDirectory = Path.Combine(Application.dataPath, "../Maps/map");

        map = LoadMap("arena.map");
    }


    /// <summary>
    /// Loads a map from the base map directory
    /// </summary>
    /// <param name="MapName">Name of the map.</param>
    public Map LoadMap(string MapName)
    {
        FileInfo f = new FileInfo(Path.Combine(BaseMapDirectory, MapName));

        return ReadMap(f);
    }

    /// <summary>
    /// Reads map and returns a map object 
    /// </summary>
    Map ReadMap(FileInfo file)
    {
        string line;
        string[] blocks;

        Map map = new Map();

        using (FileStream fs = file.OpenRead())
        using (StreamReader sr = new StreamReader(fs))
        {

            //Line 1 : type octile
            line = sr.ReadLine();
            if (line != "type octile") throw new System.Exception("Invalid format");

            //Line 2 : height
            line = sr.ReadLine();
            blocks = line.Split(' ');
            if (blocks[0] != "height") throw new Exception("Invalid format");
            map.Height = int.Parse(blocks[1]);

            //Line 3 : width
            line = sr.ReadLine();
            blocks = line.Split(' ');
            if (blocks[0] != "width") throw new Exception("Invalid format");
            map.Width = int.Parse(blocks[1]);

            //Line 4 to end : map
            line = sr.ReadLine();
            if (line != "map") throw new Exception("Invalid format");

            map.Obstacles = new bool[map.Height][];
            map.FreeTiles = 0;

            //If we draw the map, instantiate the MapTiles Array
            MapTiles = new char[map.Height][];

            //Read tiles section
            ReadTiles(sr, map);

            return map;
        }
    }

    /// <summary>
    /// Read tiles from the map file, adding tiles and filling obstacles in the array
    /// </summary>
    void ReadTiles(StreamReader sr, Map map)
    {
        char c;
        string line;

        for (int i = 0; i < map.Height; ++i) {
            line = sr.ReadLine();
            map.Obstacles[i] = new bool[map.Width];
            MapTiles[i] = new char[map.Width];

            for (int j = 0; j < map.Width; ++j) {
                c = line[j];
                MapTiles[i][j] = c;

                switch (c)
                {
                    case '@':
                    case 'O':
                        map.Obstacles[i][j] = true;
                        break;
                    case 'T':
                        map.Obstacles[i][j] = true;
                        break;
                    case '.':
                    case 'G':
                        map.Obstacles[i][j] = false;
                        map.FreeTiles++;
                        break;
                    default:
                        throw new Exception("Character not recognized");
                }
            }
        }
    }

    private void OnGUI()
    {
        //TODO: Set size of a tile with respect to the map's size and the screen's ratio
        if (map.Width > map.Height)
            TileUnit = Screen.width / map.Width;
        else
            TileUnit = Screen.height / map.Height;

        Rect tile = new Rect();
        tile.width = TileUnit;
        tile.height = TileUnit;

        //TODO: Set position properly with float values
        int startx = Mathf.FloorToInt((float)Screen.width / 2 - ((float)map.Width / 2) * TileUnit);
        int starty = Mathf.FloorToInt((float)Screen.height / 2 - ((float)map.Height / 2) * TileUnit);

        for (int i =0; i < MapTiles.Length; ++i) {
            for(int j = 0; j < MapTiles[i].Length; ++j) {
                tile.x = j * TileUnit + startx;
                tile.y = i * TileUnit + starty;

                switch (MapTiles[i][j])
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

    //TODO: put code to draw map here
    public static void GUIDrawMap(char[][] MapTiles)
    {

    }

    // Note that this function is meant to be called from OnGUI() functions.
    public static void GUIDrawRect(Rect position, Color color)
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
