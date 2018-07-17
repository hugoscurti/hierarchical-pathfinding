using System.Collections.Generic;


public class Node
{
    public GridTile pos;
    public List<Edge> edges;
    public Node child;

    public Node(GridTile value)
    {
        this.pos = value;
        edges = new List<Edge>();
    }
}


