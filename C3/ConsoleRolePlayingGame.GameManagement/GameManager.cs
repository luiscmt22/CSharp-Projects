using ConsoleRolePlayingGame.Overworld;
using ConsoleRolePlayingGame.Overworld.Generators;
using ConsoleRolePlayingGame.Overworld.Structure;

namespace ConsoleRolePlayingGame.GameManagement;

public class GameManager
{
    public GameStatus Status { get; private set; } = GameStatus.Overworld;
    public WorldMap Map { get; }
    public PlayerParty Party { get; }
    public const int MaxEnemies = 5;
    public const int EnemyArea = 8;

    public GameManager(WorldMap map, PlayerParty party)
    {
        Map = map;
        Party = party;
        
        map.AddEntity(party);
        for (int i = 0; i < MaxEnemies; i++)
        {
            SpawnNearbyEncounter();
        }
    }

    public void Quit() => Status = GameStatus.Terminated;

    public void MoveParty(Direction direction)
    {
        Party.Move(direction);
        RemoveEnemyOnPlayerPosition();
    }

    public void Update()
    {
        if (Status != GameStatus.Overworld) return;

        var AggroDistance = 7;
        List<EnemyGroup> enemies = Map.Entities.OfType<EnemyGroup>().ToList();
        foreach (var enemy in enemies)
        {
            if (Math.Abs(enemy.MapPos.X - Party.MapPos.X) < AggroDistance 
                || Math.Abs(enemy.MapPos.Y - Party.MapPos.Y) < AggroDistance)
            {
                // If the enemy is within a 3-tile radius of the player, it will move towards the player.
                enemy.MoveTowards(Party.MapPos, Map);
                if (enemy.MapPos == Party.MapPos)
                {
                    // If the enemy is on the same tile as the player, the game is over.
                    Map.RemoveEntity(enemy);
                    Party.Health--;
                }
            }
            else
            {
                Direction direction;
                Pos newPos;
                do
                {
                    direction = (Direction)Random.Shared.Next(4);
                    newPos = enemy.MapPos.Move(direction);
                }while (!Map.IsPositionValid(newPos));

                enemy.MapPos = newPos;
            }
        }

        if (Party.Health <= 0) Status = GameStatus.GameOver;

        RemoveEnemyOutsideVisibleMap();

        if (Map.Entities.OfType<EnemyGroup>().Count() < MaxEnemies) SpawnNearbyEncounter();
    }

    private void RemoveEnemyOnPlayerPosition()
    {
        List<EnemyGroup> enemies = Map.Entities.OfType<EnemyGroup>()
            .Where(e => e.MapPos == Party.MapPos)
            .ToList();

        foreach (var group in enemies)
        {
            Map.RemoveEntity(group);
        }
    }

    private void SpawnNearbyEncounter()
    {
        OpenPosSelector selector = new(Map);
        Pos point = selector.GetOpenPositionNear(Party.MapPos, 5, 10);
        Map.AddEntity(new EnemyGroup(point));
    }

    private void RemoveEnemyOutsideVisibleMap()
    {
        List<EnemyGroup> enemies = Map.Entities.OfType<EnemyGroup>().ToList();

        foreach (var enemy in enemies)
        {
            if ((Math.Abs(enemy.MapPos.X) + Math.Abs(enemy.MapPos.Y) 
                    - Math.Abs(Party.MapPos.X) + Math.Abs(Party.MapPos.Y)) > EnemyArea) Map.RemoveEntity(enemy);
        }
    }
}
