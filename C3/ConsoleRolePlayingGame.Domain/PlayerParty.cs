using ConsoleRolePlayingGame.Overworld.Structure;
using ConsoleRolePlayingGame.Overworld.Entities;

namespace ConsoleRolePlayingGame.GameManagement;

public class PlayerParty : IMapEntity
{
    public EntityType EntityType => EntityType.Party;
    public Pos MapPos { get; set; } = new(0, 0);
    public string Name { get; init; } = "The Party";

    public const int MaxStat = 10;

    public int Health { get; set; } = MaxStat;
    public int Mana { get; set; } = MaxStat;


    public void Move(Direction direction)
    {
        MapPos = MapPos.Move(direction);
    }
}