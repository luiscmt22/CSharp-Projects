using ConsoleRolePlayingGame.Overworld.Entities;
using ConsoleRolePlayingGame.Overworld.Generators;
using ConsoleRolePlayingGame.Overworld.Structure;
    
namespace ConsoleRolePlayingGame.Overworld;

public class WorldMap(MapGenerator map)
{
    private readonly List<IMapEntity> _entities = [];
    public IEnumerable<IMapEntity> Entities => _entities;

    public MapCell[,] GetMapWindow(Pos topleft, int width, int height)
    {
        MapCell[,] mapWindow = new MapCell[width, height];
        for (int y = topleft.Y; y < topleft.Y + height; y++)
        {
            for (int x = topleft.X; x < topleft.X + width; x++)
            {
                Pos pos = new Pos(x, y);
                TerrainType terrain = map.CalculateTerrain(pos);
                mapWindow[x - topleft.X, y - topleft.Y] =
                    new MapCell(terrain, pos);
            }
        }
        return mapWindow;
    }

    public bool IsPositionValid(Pos pos)
    {
        // Check if the position is within the bounds of the map
        if (pos.X < 0 || pos.Y < 0)
        {
            return false;
        }

        // Check if the terrain at the position is walkable
        TerrainType terrain = map.CalculateTerrain(pos);
        return terrain != TerrainType.Mountain
               && terrain != TerrainType.Water 
               && terrain != TerrainType.DeepWater;
    }

    public void AddEntity(IMapEntity entity)
    {
        _entities.Add(entity);
    }

    public void RemoveEntity(IMapEntity entity)
    {
        _entities.Remove(entity);
    }
}
