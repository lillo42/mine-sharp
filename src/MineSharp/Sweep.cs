namespace MineSharp;

public static class Sweep
{
    private static readonly Increment[] Increments = { Increment.One, Increment.NegativeOne, Increment.Zero };

    public static IEnumerable<int> Adjacent(int row, int column, int rows, int columns)
    {
        return Increments
            .SelectMany(rowInc => Enumerable.Repeat(rowInc, Increments.Length).Zip(Increments))
            .Select(x =>
            {
                var (rowInc, colInc) = x;
                var rowOffset = rowInc.Offset(row);
                var colOffset = colInc.Offset(column);

                return (rowInc, colInc) switch
                {
                    (Increment.Zero, Increment.Zero) => int.MaxValue,
                    (_, _) when rowOffset < rows && colOffset < columns => IndexFromCoord(rowOffset, colOffset, columns),
                    _ => int.MaxValue
                };
            })
            .Where(x => x != int.MaxValue);
    }

    public static int IndexFromCoord(int row, int column, int columns)
        => row * columns + column;
    
    public static (int Row, int Column) CoordFromIndex(int index, int columns)
        => (index / columns, index % columns);
}