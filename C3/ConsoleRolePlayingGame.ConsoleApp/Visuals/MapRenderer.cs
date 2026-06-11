using ConsoleRolePlayingGame.Overworld.Entities;
using ConsoleRolePlayingGame.Overworld.Structure;

namespace ConsoleRolePlayingGame.Visuals;

public class MapRenderer(GameManager game, int width, int height)
{
    public IRenderable GenerateVisual()
    {
        Pos center = game.Party.MapPos;
        int offsetX = (int)Math.Ceiling(width / 2.0);
        int offsetY = (int)Math.Ceiling(height / 2.0);
        Pos upperLeft = new Pos(center.X - offsetX, center.Y - offsetY);
        
        MapCell[,] window = game.Map.GetMapWindow(upperLeft, width, height);
        Canvas canvas = new (window.GetLength(0), window.GetLength(1));

        for (int y = 0; y < window.GetLength(1); y++)
        {
            for (int x = 0; x < window.GetLength(0); x++)
            {
                MapCell cell = window[x, y];
                IMapEntity? entity = game.Map.Entities.
                    FirstOrDefault(e => e.MapPos == cell.Position);
                canvas.SetPixel(x, y, GetCellColor(entity, cell.Terrain));
            }
        }
        return canvas;
    }

    private Color GetCellColor(IMapEntity? entity, TerrainType terrain)
    {
        return entity is not null
            ? entity.EntityType switch
            {
                EntityType.Party => Color.Yellow,
                EntityType.Enemy => Color.Red,
                _ => Color.DarkMagenta
            }
            : terrain switch
            {
            TerrainType.Grass => Color.Green,
            TerrainType.Water => Color.Blue,
            TerrainType.DeepWater => Color.Blue3_1,
            TerrainType.Mountain => new Color(128, 128, 128),
            TerrainType.Forest => Color.DarkGreen,
            TerrainType.Desert => Color.MistyRose1, 
            _ => Color.DarkMagenta
            };
    }
}