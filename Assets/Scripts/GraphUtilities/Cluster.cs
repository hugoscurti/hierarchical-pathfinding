using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// <summary>
/// Domain-independent, rectangular clusters
/// </summary>
class Cluster
{
    //Boundaries of the cluster (with respect to the original map)
    public GridTile TopLeft;
    public GridTile BottomRight;
    public Dictionary<GridTile, Node> Nodes;

    public int Width;
    public int Height;

    public Cluster()
    {
        Nodes = new Dictionary<GridTile, Node>();
    }

}

