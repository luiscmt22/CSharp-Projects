using ConsoleRolePlayingGame.Domain.Entities;
using ConsoleRolePlayingGame.Overworld.Entities;
using ConsoleRolePlayingGame.Overworld.Structure;

namespace ConsoleRolePlayingGame.ConsoleApp.Screens;

public class OverworldScreen(GameManager game, IAnsiConsole console)
{
    public const int Size = 21;
    private readonly Layout _layout = new Layout("Root").SplitRows(
        new Layout("Header").Size(1)
            .Update(new Markup("[bold yellow]Overworld[/]")),
        new Layout("Content").Size(Size)
            .SplitColumns(
                new Layout("Main").Size(Size * 2),
                new Layout("Sidebar")
            )
    );
    
    private readonly HelpRenderer _helpRenderer = new();
    private readonly MapRenderer _mapRenderer = new(game, Size, Size);
    private readonly PartyRenderer _partyRenderer = new(game.Party);

    public IRenderable GenerateVisual()
    {
        _layout["Main"].Update(
            _mapRenderer.GenerateVisual());
        _layout["Sidebar"].Update(new Rows(
            _partyRenderer.GenerateVisual(),
            _helpRenderer.GenerateVisual())
        );
        return _layout;
    }

    public void HandlePlayerInput()
    {
        ConsoleKeyInfo? keyInfo = console.Input.ReadKey(intercept: true);

        if (keyInfo.HasValue)
        {
            IMapEntity party = game.Party;
            switch (keyInfo.Value.Key)
            {
                case ConsoleKey.A:
                case ConsoleKey.LeftArrow:
                    party.Move(Direction.West);
                    MoveEnemies();
                    break;
                case ConsoleKey.W:
                case ConsoleKey.UpArrow:
                    party.Move(Direction.North);
                    MoveEnemies();
                    break;
                case ConsoleKey.S:
                case ConsoleKey.DownArrow:
                    party.Move(Direction.South);
                    MoveEnemies();
                    break;
                case ConsoleKey.D:
                case ConsoleKey.RightArrow:
                    party.Move(Direction.East);
                    MoveEnemies();
                    break;
                case ConsoleKey.Escape:
                    game.Quit(); //Eventually change to open MenuScreen instead of terminating.
                    break;
            }
        } 
    }

    private void MoveEnemies()
    {
        foreach (var group in game.Map.Entities.OfType<EnemyGroup>())
        {
            group.MoveTowards(game.Party.MapPos, game.Map);
        }
    }
}