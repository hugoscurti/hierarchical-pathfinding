

public class Map
{
    public int Width { get; set; }
    public int Height { get; set; }

    public int FreeTiles { get; set; }

    //Consider storing obstacles in a Hashset to save memory on large maps
    public bool[][] Obstacles { get; set; }

    public Map() {}

    public Map(int Width, int Height, bool[][] Obstacles, int FreeTiles)
    {
        this.Width = Width;
        this.Height = Height;
        this.Obstacles = Obstacles;
        this.FreeTiles = FreeTiles;
    }
}
