namespace MineSharp;

public class Tile
{
    public bool Exposed { get; set; }
    public bool Flagged { get; set; }
    public bool Mine { get; init; }
    public int AdjacentMines { get; init; }
    public HashSet<int> AdjacentTiles { get; init; } = new();
}