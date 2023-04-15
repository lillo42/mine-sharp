namespace MineSharp;

public class App
{
    public Board Board { get; }
    public int GridWidth { get; }
    public int GridHeight { get; }
    public int ActiveColumn { get; private set; }
    public int ActiveRow { get; private set; }
    public bool Lost { get; set; }

    public App(Board board, int gridWidth, int gridHeight)
    {
        Board = board;
        GridWidth = gridWidth;
        GridHeight = gridHeight;
    }

    public void Up() => ActiveRow = Math.Max(ActiveRow - 1, 0);

    public void Down() => ActiveRow = Math.Min(ActiveRow + 1, Board.Rows - 1);

    public void Left() => ActiveColumn = Math.Max(ActiveColumn - 1, 0);

    public void Right() => ActiveColumn = Math.Min(ActiveColumn + 1, Board.Columns - 1);

    public void FlagActiveCells() => Board.Flag(ActiveRow, ActiveColumn);

    public bool ExposeActiveCells() => Board.Expose(ActiveRow, ActiveColumn);

    public bool Won => Board.Won; 
    public void ExposeAll() => Board.ExposeAll();

    public (int Row, int Column) Active => (ActiveRow, ActiveColumn);

    public Cell Cell(int row, int column) => new(this, row, column);
}