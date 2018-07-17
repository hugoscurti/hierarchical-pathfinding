using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Map
{
    public int Width { get; set; }
    public int Height { get; set; }

    public Boundaries Boundaries { get; set; }

    public int FreeTiles { get; set; }

    //Consider storing obstacles in a Hashset to save memory on large maps
    //Obstacles are stores with the y value in the first array and the x value in the second array
    public bool[][] Obstacles { get; set; }

    //Original characters that forms the whole map
    //Tiles are stored with the y value in the first array and the x value in the second array
    public char[][] Tiles { get; set; }


    private Map() {}


    //Returns whether the tile is a valid free tile in the map or not
    public bool IsFreeTile(GridTile tile)
    {
        return tile.x >= 0 && tile.x < Width &&
            tile.y >= 0 && tile.y < Height &&
            !Obstacles[tile.y][tile.x];
    }


    public static List<FileInfo> GetMaps()
    {
        string BaseMapDirectory = GetBaseMapDirectory();
        DirectoryInfo d = new DirectoryInfo(BaseMapDirectory);
        return new List<FileInfo>(d.GetFiles("*.map"));
    }

    /// <summary>
    /// Loads a map from the base map directory
    /// </summary>
    /// <param name="MapName">File from which to load the map</param>
    public static Map LoadMap(string MapName)
    {
        string BaseMapDirectory = GetBaseMapDirectory();
        FileInfo f = new FileInfo(Path.Combine(BaseMapDirectory, MapName));

        return ReadMap(f);
    }

    /// <summary>
    /// Gets the base map directory
    /// </summary>
    private static string GetBaseMapDirectory()
    {
        return Path.Combine(Application.dataPath, "../Maps/map");
    }

    /// <summary>
    /// Reads map and returns a map object 
    /// </summary>
    private static Map ReadMap(FileInfo file)
    {
        Map map = new Map();

        using (FileStream fs = file.OpenRead())
        using (StreamReader sr = new StreamReader(fs))
        {

            //Line 1 : type octile
            ReadLine(sr, "type octile");

            //Line 2 : height
            map.Height = ReadIntegerValue(sr, "height");

            //Line 3 : width
            map.Width = ReadIntegerValue(sr, "width");

            //Set boundaries according to width and height
            map.Boundaries = new Boundaries
            {
                Min = new GridTile(0, 0),
                Max = new GridTile(map.Width - 1, map.Height - 1)
            };

            //Line 4 to end : map
            ReadLine(sr, "map");

            map.Obstacles = new bool[map.Height][];
            map.FreeTiles = 0;

            //Store the array of characters that makes up the map
            map.Tiles = new char[map.Height][];

            //Read tiles section
            map.ReadTiles(sr);

            return map;
        }
    }

    /// <summary>
    /// Read a line and expect the line to be the value passed in arguments
    /// </summary>
    private static void ReadLine(StreamReader sr, string value)
    {
        string line = sr.ReadLine();
        if (line != value) throw new Exception(
                string.Format("Invalid format. Expected: {0}, Actual: {1}", value, line));
    }

    /// <summary>
    /// Returns an integer value from the streamreader that comes
    /// right after a key separated by a space.
    /// I.E. width 5
    /// </summary>
    private static int ReadIntegerValue(StreamReader sr, string key)
    {
        string[] block = sr.ReadLine().Split(null);
        if(block[0] != key) throw new Exception(
                string.Format("Invalid format. Expected: {0}, Actual: {1}", key, block[0]));

        return int.Parse(block[1]);
    }

    /// <summary>
    /// Read tiles from the map file, adding tiles and filling obstacles in the array
    /// </summary>
    private void ReadTiles(StreamReader sr)
    {
        char c;
        string line;

        for (int i = 0; i < Height; ++i)
        {
            line = sr.ReadLine();
            Obstacles[i] = new bool[Width];
            Tiles[i] = new char[Width];

            for (int j = 0; j < Width; ++j)
            {
                c = line[j];
                Tiles[i][j] = c;

                switch (c)
                {
                    case '@':
                    case 'O':
                    case 'T':
                    case 'W':
                        Obstacles[i][j] = true;
                        break;
                    case '.':
                    case 'G':
                    case 'S':
                        Obstacles[i][j] = false;
                        FreeTiles++;
                        break;
                    default:
                        throw new Exception("Character not recognized");
                }
            }
        }
    }


    public void GetColor(int i, int j, out Color color)
    {
        switch (Tiles[i][j])
        {
            case 'T':
                color = Color.green;
                break;
            case '.':
            case 'G':
                color = Color.white;
                break;
            case '@':
            case 'O':
                color = Color.black;
                break;
            case 'S':   //Swamp
                color = Color.magenta;
                break;
            case 'W':   //Water
                color = Color.blue;
                break;
            default:
                throw new Exception("Character not recognized");
        }
    }

}
