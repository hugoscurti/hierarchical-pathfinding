using System;
using System.IO;
using UnityEngine;


public class Map
{
    public int Width { get; set; }
    public int Height { get; set; }

    public Boundaries Boundaries { get; set; }

    public int FreeTiles { get; set; }

    //Consider storing obstacles in a Hashset to save memory on large maps
    public bool[][] Obstacles { get; set; }

    //Original characters that forms the whole map
    public char[][] Tiles { get; set; }

    public Map() {}

    /// <summary>
    /// Loads a map from the base map directory
    /// </summary>
    /// <param name="MapName">File from which to load the map</param>
    public static Map LoadMap(string MapName)
    {
        string BaseMapDirectory = Path.Combine(Application.dataPath, "../Maps/map");
        FileInfo f = new FileInfo(Path.Combine(BaseMapDirectory, MapName));

        return ReadMap(f);
    }

    /// <summary>
    /// Reads map and returns a map object 
    /// </summary>
    static Map ReadMap(FileInfo file)
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
            ReadTiles(sr, map);

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
        string[] block = sr.ReadLine().Split(' ');
        if(block[0] != key) throw new Exception(
                string.Format("Invalid format. Expected: {0}, Actual: {1}", key, block[0]));

        return int.Parse(block[1]);
    }

    /// <summary>
    /// Read tiles from the map file, adding tiles and filling obstacles in the array
    /// </summary>
    static void ReadTiles(StreamReader sr, Map map)
    {
        char c;
        string line;

        for (int i = 0; i < map.Height; ++i)
        {
            line = sr.ReadLine();
            map.Obstacles[i] = new bool[map.Width];
            map.Tiles[i] = new char[map.Width];

            for (int j = 0; j < map.Width; ++j)
            {
                c = line[j];
                map.Tiles[i][j] = c;

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

}
