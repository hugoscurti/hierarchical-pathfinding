using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class Node
{
    public GridTile value;
    public List<Edge> edges;

    public Node(GridTile value)
    {
        this.value = value;
        edges = new List<Edge>();
    }
}


