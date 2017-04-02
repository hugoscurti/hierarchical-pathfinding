
public class Edge
{
    public Node start;
    public Node end;
    public EdgeType type;
    public int weight;
}


public enum EdgeType
{
    INTRA,
    INTER
}
