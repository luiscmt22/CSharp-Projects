namespace ConsoleRolePlayingGame.ConsoleApp.Visuals;

// Shared char -> colour map for data-driven sprites. Any char not listed (e.g. '.') is transparent.
public static class SpritePalette
{
    public static readonly Dictionary<char, Color> Colors = new()
    {
        ['D'] = new Color(30, 62, 16),   ['G'] = new Color(74, 139, 44),  ['E'] = new Color(255, 100, 0),
        ['R'] = new Color(95, 12, 12),   ['W'] = new Color(238, 238, 224),['N'] = new Color(40, 80, 22),
        ['B'] = new Color(235, 232, 215),['S'] = new Color(175, 172, 150),['K'] = new Color(8, 8, 10),
        ['C'] = new Color(60, 120, 210), ['A'] = new Color(35, 80, 160),  ['H'] = new Color(150, 200, 245),
    };

    // Colour at (x, y), or null when out of bounds or transparent.
    public static Color? At(string[] grid, int x, int y)
    {
        if (y < 0 || y >= grid.Length) return null;
        string row = grid[y];
        if (x < 0 || x >= row.Length) return null;
        return Colors.TryGetValue(row[x], out Color color) ? color : null;
    }
}
