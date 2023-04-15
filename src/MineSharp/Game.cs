using System.Text;
using Boto.Layouts;
using Boto.Styles;
using Boto.Terminals;
using Boto.Tutu;
using Boto.Widget;
using NodaTime;
using Tutu.Events;
using Tutu.Extensions;
using Tutu.Terminal;
using static Tutu.Commands.Events;
using static Tutu.Commands.Terminal;

namespace MineSharp;

public class Game
{
    internal const string Bomb = "💣";
    internal const string Flag = "🚩";

    public int Rows { get; set; }
    public int Columns { get; set; }
    public int Mines { get; set; }
    public int CellWidth { get; set; }
    public int CellHeight { get; set; }

    public void Run()
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        const int padding = 1;
        var gridWidth = CellWidth * Columns + 2 * padding;
        var gridHeight = CellHeight * Rows + 2 * padding;

        var app = new App(new Board(Rows, Columns, Mines), gridWidth, gridHeight);

        var stdout = Console.Out;

        SystemTerminal.EnableRawMode();
        stdout.Execute(EnterAlternateScreen, EnableMouseCapture);

        var error = string.Empty;
        var terminal = new Terminal<TutuBackend>(new TutuBackend(stdout));

        try
        {
            InternalRun(terminal, app);
        }
        catch (Exception ex)
        {
            error = ex.ToString();
        }

        stdout.Execute(DisableMouseCapture, LeaveAlternateScreen);
        SystemTerminal.DisableRawMode();

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine(error);
        }
    }

    private void InternalRun<T>(ITerminal<T> terminal, App app)
        where T : class, IBackend
    {
        while (true)
        {
            terminal.Draw(f => Ui(f, app));

            if (SystemEventReader.Poll(Duration.FromMilliseconds(10)))
            {
                var @event = SystemEventReader.Read();
                if (@event is Event.KeyEventEvent { Event.Kind: KeyEventKind.Release } keyEvent)
                {
                    var code = keyEvent.Event.Code;
                    if (code == KeyCode.Up || code is KeyCode.CharKeyCode { Character: "k" })
                    {
                        app.Up();
                    }
                    else if (code == KeyCode.Down || code is KeyCode.CharKeyCode { Character: "j" })
                    {
                        app.Down();
                    }
                    else if (code == KeyCode.Left || code is KeyCode.CharKeyCode { Character: "h" })
                    {
                        app.Left();
                    }
                    else if (code == KeyCode.Right || code is KeyCode.CharKeyCode { Character: "l" })
                    {
                        app.Right();
                    }
                    else if (code is KeyCode.CharKeyCode { Character: "f" } && app is { Lost: false, Won: false })
                    {
                        app.FlagActiveCells();
                    }
                    else if (code is KeyCode.CharKeyCode { Character: " " } && app is { Lost: false, Won: false })
                    {
                        app.Lost = app.ExposeActiveCells();
                        if (app.Lost)
                        {
                            app.ExposeAll();
                        }
                    }
                    else if (code is KeyCode.CharKeyCode { Character: "q" } ||
                             code is KeyCode.CharKeyCode { Character: "c" } &&
                             keyEvent.Event.Modifiers == KeyModifiers.Control)
                    {
                        break;
                    }
                }
            }
        }
    }

    private readonly StringBuilder _sb = new();
    private void Ui<T>(Frame<T> frame, App app)
        where T : class, IBackend
    {
        var rowConstraints = Enumerable.Repeat(Constraints.Length(CellHeight), Rows)
            .ToList();

        var columnConstraints = Enumerable.Repeat(Constraints.Length(CellWidth), Columns)
            .ToList();

        frame.Render(new Block()
            .AllBorders()
            .Title("MineSharp", new() { Foreground = Color.LightYellow, AddModifier = Modifier.Bold })
            .RoundedBorderType(), frame.Size);

        var outerRects = new Layout()
            .VerticalDirection()
            .Margin(1, 1)
            .AddConstraint(Constraints.Min(app.GridHeight))
            .Split(frame.Size);

        var mineRects = outerRects[0];

        var availableFlags = app.Board.AvailableFlags;
        var infoText = new Gauge()
            .Block(new Block()
                .AllBorders()
                .Title(Flag, new() { Foreground = Color.LightMagenta, AddModifier = Modifier.Bold }))
            .GaugeStyle(new() { Foreground = Color.LightMagenta, Background = Color.Black, AddModifier = Modifier.Bold })
            .Label($"{availableFlags}")
            .Ratio(availableFlags / Mines);

        var horizontalPadBlockWidth = (frame.Size.Width - app.GridWidth) / 2;
        var minesRects = new Layout()
            .HorizontalDirection()
            .AddConstraints(
                Constraints.Min(horizontalPadBlockWidth),
                Constraints.Length(app.GridWidth),
                Constraints.Min(horizontalPadBlockWidth))
            .Split(mineRects);

        var verticalPadBlockHeight = (mineRects.Height - app.GridHeight) / 2;
        var middleMinesRects = new Layout()
            .VerticalDirection()
            .AddConstraints(
                Constraints.Min(verticalPadBlockHeight),
                Constraints.Length(app.GridHeight),
                Constraints.Min(verticalPadBlockHeight))
            .Split(minesRects[1]);

        var helpTextBlock = new List()
            .AddItems(
                new ListItem("movement: hjkl / ← ↓ ↑ →"),
                new ListItem("expose tile: spacebar"),
                new ListItem("flag tile: f"),
                new ListItem("quit: q")
            )
            .Block(new Block().NoneBorders());

        frame.Render(helpTextBlock, middleMinesRects[2]);

        var infoTextSplitRects = new Layout()
            .VerticalDirection()
            .AddConstraints(
                Constraints.Min(verticalPadBlockHeight - 3),
                Constraints.Length(3))
            .Split(middleMinesRects[0]);

        var infoMinesRects = new Layout()
            .HorizontalDirection()
            .AddConstraints(
                Constraints.Percentage(50),
                Constraints.Percentage(50))
            .Split(infoTextSplitRects[1]);
        frame.Render(infoText, infoMinesRects[0]);

        var mineText = new Paragraph()
            .Text(Mines.ToString())
            .Block(new Block()
                .AllBorders()
                .Title(Bomb, new() { Foreground = Color.LightYellow, AddModifier = Modifier.Bold }))
            .Alignment(Alignment.Center);
        frame.Render(mineText, infoMinesRects[1]);


        var minesBlock = new Block()
            .AllBorders()
            .RoundedBorderType();
        
        var finalMinesRect = middleMinesRects[1];
        frame.Render(minesBlock, finalMinesRect);
        
        var rowRects = new Layout()
            .VerticalDirection()
            .Margin(new Margin(1, 0))
            .AddConstraints(rowConstraints)
            .Split(middleMinesRects[1]);

        for (var rowIndex = 0; rowIndex < rowRects.Count; rowIndex++)
        {
            var rowRect = rowRects[rowIndex];

            var colRects = new Layout()
                .HorizontalDirection()
                .Margin(new Margin(0, 1))
                .AddConstraints(columnConstraints)
                .Split(rowRect);

            for (var colIndex = 0; colIndex < colRects.Count; colIndex++)
            {
                var cell = app.Cell(rowIndex, colIndex);
                var cellRect = colRects[colIndex];
                _sb.Clear();
                _sb.Append(cell);
                _sb.Append(' ', CellWidth);
                _sb.Length = CellWidth - 2;
                var singleRowText = _sb.ToString();
                var padline = new string(' ', CellWidth);

                // 1 line for the text, 1 line each for the top and bottom of the cell == 3 lines
                // that are not eligible for padding
                var numPadLine = CellHeight - 3;

                // text is:
                //   pad with half the pad lines budget
                //   the interesting text
                //   pad with half the pad lines budget
                //   join with newlines
                var tmp = Enumerable.Repeat(padline, numPadLine / 2)
                    .Append(singleRowText)
                    .ToList();
                tmp.AddRange(Enumerable.Repeat(padline, numPadLine / 2));

                var text = string.Join(Environment.NewLine, tmp);

                var cellText = new Paragraph()
                    .Text(text)
                    .Block(cell.Block(app.Lost))
                    .Style(cell.TextStyle);
                frame.Render(cellText, cellRect);
            }
        }

        // if the user has lost or won, display a banner indicating so
        if (app.Lost || app.Won)
        {
            var area = CenteredRect(20, 3, middleMinesRects[1], app);

            frame.Render(new Clear(), area);
            frame.Render(new Paragraph($"You {(app.Lost ? "lose" :"won")}")
                .Block(new Block()
                    .AllBorders()
                    .ThickBorderType()
                    .BorderStyle(new() { Foreground = app.Lost ? Color.Magenta :  Color.Green, AddModifier = Modifier.Bold})
                    .Style(new() { AddModifier = Modifier.Bold }))
                .Alignment(Alignment.Center)
                .Style(new()), 
                area);
        }
    }

    /*private static List<string> AlignStringsToChar(string[] strings, char c)
    {
        var max = strings.Max(s => s.IndexOf(c));
        return strings.Select(s => s.Insert(s.IndexOf(c), new string(' ', max - s.IndexOf(c)))).ToList();
    }*/
    private static Rect CenteredRect(int width, int height, Rect area, App app)
    {
        var popupLayout = new Layout()
            .VerticalDirection()
            .AddConstraints(
                Constraints.Length(app.GridHeight / 2 - height / 2),
                Constraints.Length(height),
                Constraints.Length(app.GridHeight / 2 - height / 2))
            .Split(area);

        return new Layout()
            .HorizontalDirection()
            .AddConstraints(
                Constraints.Length(app.GridWidth / 2 - width / 2),
                Constraints.Length(width),
                Constraints.Length(app.GridWidth / 2 - width / 2))
            .Split(popupLayout[1])[1];
    }
}