namespace ConsoleRolePlayingGame.ConsoleApp.Visuals;

public class PixelSpriteRenderer : ICombatArtRenderer
{
    public IRenderable Render(Combatant combatant)
    {
        string[] grid = combatant.AsciiArt;
        int width = grid.Max(row => row.Length);
        Canvas canvas = new(width, grid.Length) { PixelWidth = 1 };

        for (int y = 0; y < grid.Length; y++)
            for (int x = 0; x < grid[y].Length; x++)
                if (SpritePalette.At(grid, x, y) is { } color)
                    canvas.SetPixel(x, y, color);

        return canvas;
    }
}
