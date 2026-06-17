namespace ConsoleRolePlayingGame.ConsoleApp.Visuals;

public class AsciiArtRenderer : ICombatArtRenderer
{
    public IRenderable Render(Combatant combatant) =>
        new Rows(combatant.AsciiArt.Select(line => new Markup(line).Justify(Justify.Center)));
}