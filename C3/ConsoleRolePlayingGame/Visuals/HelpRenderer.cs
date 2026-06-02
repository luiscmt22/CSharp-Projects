using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleRolePlayingGame.Visuals;

public class HelpRenderer
{
    public IRenderable GenerateVisual() => new Panel(new Rows(
        new Markup("The [yellow]yellow block[/] is your party."),
        new Markup("Use [cyan]arrow keys[/] or [cyan]WASD[/] to move"),
        new Markup("Enter [red]red blocks[/] to begin combat. "),
        new Markup("Press [cyan]q[/] to quit.")
    ))
    .Header("[yellow]Instructions[/]")
    .Padding(1, 1, 1, 0)
    .Border(BoxBorder.Rounded);
    
}