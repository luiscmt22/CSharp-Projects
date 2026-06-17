namespace ConsoleRolePlayingGame.ConsoleApp.Visuals;

public class CombatGroupRenderer(ICombatGroup group, Combatant? active, ICombatArtRenderer renderer, bool includeStats = false)                            
{
    public IRenderable GenerateVisual()
    {
        IEnumerable<IRenderable> items = group.Members.Select(GenerateCharacterVisual);
        return new Panel(new Columns(items))
            .Header($"[Yellow] {group.Name}[/]")
            .RoundedBorder()
            .Expand();
    }

    private IRenderable GenerateCharacterVisual(Combatant c)
    {
        List<IRenderable> visuals = 
        [
            c.IsDead
                ? new Markup(" ")
                : renderer.Render(c),
            new Markup(active == c
                ? $"[bold yellow]{c.Name}[/]"
                : c.Name).Justify(Justify.Center)
        ];
        
        if (includeStats)
        {
            visuals.Add(BuildStatsBlock(c));
        }

        return new Padder(
            new Rows(visuals),
            new Padding(1, 0, 1, 1)
        );
    }

    private IRenderable BuildStatsBlock(Combatant c)
    {
        string text = $"[red]HP: {c.Health}/{c.MaxHealth}[/]" +
                      $"[blue]MP: {c.Mana}/{c.MaxMana}[/]";

        return new Markup(text).Justify(Justify.Center);
    }
}