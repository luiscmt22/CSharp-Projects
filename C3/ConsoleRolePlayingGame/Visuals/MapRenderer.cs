using ConsoleRolePlayingGame.GameManagement;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleRolePlayingGame.Visuals;

public class MapRenderer(GameManager game, int sizeX, int sizeY)
{
    public IRenderable GenerateVisual()
    {
        var table = new Table();
        for (var x = 0; x < sizeX; x++)
        {
            table.AddColumn(new TableColumn($"[bold yellow]{x}[/]"));
        }

        return table;
    }
}