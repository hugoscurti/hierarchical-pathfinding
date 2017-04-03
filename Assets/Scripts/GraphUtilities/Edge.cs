
using System.Collections.Generic;

public class Edge
{
    public Node start;
    public Node end;
    public EdgeType type;
    public float weight;

    //TODO: Think about how to represent higher level edges with higher level GridTiles ?
    public LinkedList<GridTile> UnderlyingPath;
}


public enum EdgeType
{
    INTRA,
    INTER
}
