namespace MineSharp;

public enum Increment
{
    One,
    NegativeOne,
    Zero
}

public static class IncrementExtensions
{
    public static int Offset(this Increment increment, int value) => increment switch
    {
        Increment.One => value + 1,
        Increment.NegativeOne => Math.Max(value - 1, 0),
        Increment.Zero => value,
        _ => throw new ArgumentOutOfRangeException(nameof(increment), increment, null)
    };
}