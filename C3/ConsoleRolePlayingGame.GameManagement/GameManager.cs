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

    public GameManager(PlayerParty party, WorldMap map)
    {
        Party = party;
        Map = map;
        
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

        List<EnemyGroup> enemies = Map.Entities.OfType<EnemyGroup>().ToList();
        foreach (var enemy in enemies)
        {
            enemy.MoveTowards(Party.MapPos, Map);
            if (enemy.MapPos == Party.MapPos)
            {
                // If the enemy is on the same tile as the player, the game is over.
                Map.RemoveEntity(enemy);
                Party.Health--;
            }
        }

        if (Party.Health <= 0) Status = GameStatus.GameOver;

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
}
