using ConsoleRolePlayingGame.Overworld;
using ConsoleRolePlayingGame.Overworld.Entities;
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
    }

    public void Update()
    {
        if (Status is GameStatus.Terminated or GameStatus.GameOver) return;

        if (Party.Members.All(m => m.IsDead))
        {
            TriggerGameOver();
            return;
        }

        switch (Status)
        {
            case GameStatus.Overworld:
                IMapEntity? encounter = Map.Entities
                    .FirstOrDefault(g =>
                        g.EntityType == EntityType.Enemy
                        && g.MapPos == Party.MapPos);
                
                if (encounter is ICombatGroup combatant)
                {
                    Map.RemoveEntity(encounter);
                    StartBattle(combatant);
                }
                break;
            
            case GameStatus.Battle:
                if (StartBattle( is not null &&
                Battle.Enemies.Members.All(e => e.IsDead)))
                {
                    EndBattle();
                }
                break;
        }
    }

    public Battle StartBattle(ICombatGroup combatant)
    {
        Battle battle = new(Party, combatant);
        Status = GameStatus.Combat;
        return battle;
    }

    public void EndBattle()
    {
        Battle = null;
        Status = GameStatus.Overworld;
        SpawnNearbyEncounter();
    }

    private void SpawnNearbyEncounter()
    {
        OpenPosSelector selector = new(Map);
        Pos point = selector.GetOpenPositionNear(Party.MapPos, 5, 10);
        Map.AddEntity(new EnemyGroup(point));
    }

    private void TriggerGameOver() => Status = GameStatus.GameOver;
}
