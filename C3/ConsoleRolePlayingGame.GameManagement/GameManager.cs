using ConsoleRolePlayingGame.Overworld;

namespace ConsoleRolePlayingGame.GameManagement;

public class GameManager
{
    public PlayerParty Party { get; set; }
    public WorldMap Map { get; set; }
    public GameStatus Status { get; set; }
}
