using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// <summary>
/// Domain-independent, rectangular clusters
/// </summary>
public class Cluster
{
    //Boundaries of the cluster (with respect to the original map)
    public Boundaries Boundaries;
    public Dictionary<GridTile, Node> Nodes;

    public int Width;
    public int Height;

    public Cluster()
    {
        Boundaries = new Boundaries();
        Nodes = new Dictionary<GridTile, Node>();
    }

}

