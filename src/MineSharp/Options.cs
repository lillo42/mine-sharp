using CommandLine;

namespace MineSharp;

public class Options
{
    [Option("rows", Required = false, HelpText = "The number of rows in the minefield", Default = 9)]
    public int Rows { get; set; } = 9;
    
    [Option("cols", Required = false, HelpText = "The number of columns in the minefield", Default = 9)]
    public int Columns { get; set; } = 9;
    
    [Option("mines", Required = false, HelpText = "The number of mines in the minefield", Default = 10)]
    public int Mines { get; set; } = 10;
    
    [Option("cell-width", Required = false, HelpText = "The width of each cell in the minefield", Default = 5)]
    public int CellWidth { get; set; } = 5;
    
    [Option("cell-height", Required = false, HelpText = "The height of each cell in the minefield", Default = 3)]
    public int CellHeight { get; set; } = 3;
}