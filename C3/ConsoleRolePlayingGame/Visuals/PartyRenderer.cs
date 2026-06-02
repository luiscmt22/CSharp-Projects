using ConsoleRolePlayingGame.GameManagement;    
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleRolePlayingGame.Visuals;

public class PartyRenderer(PlayerParty party)
{
    public IRenderable GenerateVisual()
    {
        IRenderable partyMarkdown =
            new Rows(
                new Markup("[bold]Hero[/]"),
                new Padder(
                    new BarChart()
                        .AddItem("[Red]Health[/]", party.Health, Color.Red)
                        .AddItem("[Blue]MP[/]", party.Mana, Color.Blue)
                        .WithMaxValue(PlayerParty.MaxStat)
                        .ShowValues()
            ));
        return new Panel(partyMarkdown)
            .Header($"[yellow]{party.Name}[/]")
            .Border(BoxBorder.Rounded);
    }
}