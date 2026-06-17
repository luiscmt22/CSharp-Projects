namespace ConsoleRolePlayingGame.ConsoleApp.Visuals;

public interface ICombatArtRenderer
{
    IRenderable Render(Combatant c);
}