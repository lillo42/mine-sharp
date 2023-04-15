// See https://aka.ms/new-console-template for more information

using CommandLine;
using MineSharp;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(opt =>
    {
        var game = new Game
        {
            Rows = opt.Rows,
            Columns = opt.Columns,
            Mines = opt.Mines,
            CellWidth = opt.CellWidth,
            CellHeight = opt.CellHeight
        };
        
        game.Run();
    });