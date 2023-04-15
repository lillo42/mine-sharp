using Boto.Styles;
using Boto.Widget;

namespace MineSharp;

public record Cell(App App, int Row, int Column)
{
    public bool IsActive => App.Active == (Row, Column);
    public bool IsExposed => App.Board.Tile(Row, Column).Exposed;
    public bool IsFlagged => App.Board.Tile(Row, Column).Flagged;
    public bool IsMine => App.Board.Tile(Row, Column).Mine;

    public Block Block(bool lost)
    {
        var foreground = Color.White;
        if (IsActive)
        {
            foreground = Color.Cyan;
        }
        else if(lost && IsMine)
        {
            foreground = Color.LightRed;
        }
        
        var style = new Style { Background = Color.Black, Foreground = foreground, AddModifier = IsActive ? Modifier.Bold : Modifier.Empty };
        return new Block()
            .AllBorders()
            .RoundedBorderType()
            .Style(style);
    }


    public Style TextStyle
    {
        get
        {
            var foreground = Color.Black;
            if (IsExposed && IsMine)
            {
                foreground = Color.LightYellow;
            }
            else if (IsExposed)
            {
                foreground = Color.White;
            }
            
            var background = Color.White;
            if (IsExposed)
            {
                background = Color.Black;
            }
            else if (IsActive)
            {
                background = Color.Cyan;
            }

            return new()
            {
                Foreground = foreground,
                Background = background,
            };
        }
    }

    public override string ToString()
    {
        if (IsFlagged)
        {
            return Game.Flag;
        }

        if (IsMine && IsExposed)
        {
            return Game.Bomb;
        }

        if (IsExposed)
        {
            var numAdjacentMines = App.Board.Tile(Row, Column).AdjacentMines;
            if (numAdjacentMines > 0)
            {
                return numAdjacentMines.ToString();
            }
        }

        return string.Empty;
    }
}