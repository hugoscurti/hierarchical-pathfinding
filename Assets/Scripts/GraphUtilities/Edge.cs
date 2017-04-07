
using System.Collections.Generic;

public class Edge
{
    public Node start;
    public Node end;
    public EdgeType type;
    public float weight;

    public LinkedList<Edge> UnderlyingPath;
}


public enum EdgeType
{
    INTRA,
    INTER
}
