using System.Text;

namespace ConsoleRolePlayingGame.ConsoleApp.Visuals;

// Packs two pixel rows into one text row: '▀' paints the top pixel (foreground)
// over the bottom pixel (background), so a sprite renders at half its grid height
// with near-square, full-colour pixels.
public class HalfBlockSpriteRenderer : ICombatArtRenderer
{
    private const char UpperHalf = '▀'; // ▀  top half filled
    private const char LowerHalf = '▄'; // ▄  bottom half filled

    public IRenderable Render(Combatant combatant)
    {
        string[] grid = combatant.AsciiArt;
        if (grid.Length == 0) return new Markup(string.Empty);

        int width = grid.Max(row => row.Length);
        StringBuilder markup = new();

        for (int y = 0; y < grid.Length; y += 2)
        {
            for (int x = 0; x < width; x++)
            {
                markup.Append(Cell(SpritePalette.At(grid, x, y), SpritePalette.At(grid, x, y + 1)));
            }
            if (y + 2 < grid.Length) markup.Append('\n');
        }

        return new Markup(markup.ToString());
    }

    private static string Cell(Color? top, Color? bottom) => (top, bottom) switch
    {
        (null, null) => " ",
        (not null, not null) => $"[{Hex(top.Value)} on {Hex(bottom.Value)}]{UpperHalf}[/]",
        (not null, _) => $"[{Hex(top.Value)}]{UpperHalf}[/]",      // bottom is transparent
        _ => $"[{Hex(bottom!.Value)}]{LowerHalf}[/]",              // top is transparent
    };

    private static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
}
