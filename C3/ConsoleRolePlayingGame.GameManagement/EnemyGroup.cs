using ConsoleRolePlayingGame.Overworld;
using ConsoleRolePlayingGame.Overworld.Structure;
using ConsoleRolePlayingGame.Overworld.Entities;

namespace ConsoleRolePlayingGame.GameManagement;

public class EnemyGroup(Pos initialPos) : IMapEntity
{
    public EntityType EntityType => EntityType.Enemy;
    public Pos MapPos { get; set; } = initialPos;

    public void MoveTowards(Pos target, WorldMap map)
    {
        if (MapPos == target) return;

        int dx = target.X - MapPos.X;
        int dy = target.Y - MapPos.Y;

        Direction direction;
        if (Math.Abs(dx) > Math.Abs(dy))
        {
            direction = dx > 0 ? Direction.East : Direction.West;
        }
        else
        {
            direction = dy > 0 ? Direction.South : Direction.North;
        }

        Pos newPos = MapPos.Move(direction);
        bool blocked = map.Entities.OfType<EnemyGroup>().Any(e => e.MapPos == newPos && e != this); //Clean

        if (!blocked) MapPos = newPos;
    }
}