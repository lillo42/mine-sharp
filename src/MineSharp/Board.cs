namespace MineSharp;

public class Board
{
    public Board(int rows, int columns, int mines)
    {
        var rand = new Random();
        var sample = new HashSet<int>();
        
        for (var i = 0; i < mines; i++)
        {
            while (!sample.Add(rand.Next(0, rows * columns)))
            {
                
            }
        }

        var tiles = Enumerable.Range(0, rows)
            .SelectMany(row => Enumerable.Range(0, columns).Select(column => (row, column)))
            .Select((point, index) =>
            {
                // compute the tiles adjacent to the one being constructed
                var adjacentTiles = Sweep.Adjacent(point.row, point.column, rows, columns)
                    .ToHashSet();

                // sum the number of adjacent tiles that are in the randomly generated mines set
                var adjacentMines = adjacentTiles
                    .Sum(x => sample.Contains(x) ? 1 : 0);

                return new Tile
                {
                    AdjacentTiles = adjacentTiles,
                    AdjacentMines = adjacentMines,
                    Mine = sample.Contains(index),
                    Exposed = false,
                    Flagged = false
                };
            })
            .ToList();

        Rows = rows;
        Columns = columns;
        Mines = mines;
        FlaggedCells = 0;
        CorrectlyFlaggedMines = 0;
        Tiles = tiles;
    }

    public List<Tile> Tiles { get; }

    // number of rows on the board
    public int Rows { get; }

    // number of columns on the board
    public int Columns { get; }

    public int Mines { get; }
    public int FlaggedCells { get; set; }

    // the total number of correctly flagged mines, allows checking a win in O(1)
    public int CorrectlyFlaggedMines { get; set; }

    // the exposed tiles
    public HashSet<int> Seen { get; } = new();

    private int IndexFromCoord(int row, int column) => Sweep.IndexFromCoord(row, column, Columns);

    public int AvailableFlags => Mines - FlaggedCells;

    public bool Won
    {
        get
        {
            var exposedOrCorrectlyFlagged = Seen.Count + CorrectlyFlaggedMines;
            var ntitles = Rows * Columns;
            return ntitles == exposedOrCorrectlyFlagged;
        }
    }

    public bool Expose(int row, int column)
    {
        var tile = Tile(row, column);
        if (tile.Mine)
        {
            tile.Exposed = true;
            return true;
        }

        var queue = new Queue<(int row, int column)>();
        queue.Enqueue((row, column));
        while (queue.TryDequeue(out var coordinate))
        {
            if (Seen.Add(IndexFromCoord(coordinate.row, coordinate.column)))
            {
                tile = Tile(coordinate.row, coordinate.column);
                tile.Exposed = !(tile.Mine || tile.Flagged);
                if (tile.AdjacentMines == 0)
                {
                    foreach (var index in tile.AdjacentTiles)
                    {
                        queue.Enqueue(Sweep.CoordFromIndex(index, Columns));
                    }
                }
            }
        }

        return false;
    }

    public Tile Tile(int row, int column) => Tiles[IndexFromCoord(row, column)];

    public void ExposeAll()
    {
        for (var i = 0; i < Tiles.Count; i++)
        {
            var (row, column) = Sweep.CoordFromIndex(i, Columns);
            Expose(row, column);
        }
    }

    public void Flag(int row, int column)
    {
        var nFlagged = FlaggedCells;
        var tile = Tile(row, column);
        var wasFlagged = tile.Flagged;
        var flagged = !wasFlagged;
        CorrectlyFlaggedMines += flagged && tile.Mine ? 1 : 0;

        if (wasFlagged)
        {
            FlaggedCells = Math.Max(FlaggedCells - 1, 0);
            tile.Flagged = flagged;
        }
        else if (nFlagged < Mines && !tile.Exposed)
        {
            tile.Flagged = flagged;
            FlaggedCells++;
        }
    }
}
