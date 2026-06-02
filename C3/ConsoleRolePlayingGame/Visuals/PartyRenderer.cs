using ConsoleRolePlayingGame.GameManagement;    
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleRolePlayingGame.Visuals;

public class PartyRenderer(List<string> party)
{
    public IRenderable GenerateVisual()
    {
        var table = new Table();
        table.AddColumn(new TableColumn("[bold yellow]Party[/]"));
        foreach (var member in party)
        {
            table.AddRow(member);
        }

        return table;
    }
}