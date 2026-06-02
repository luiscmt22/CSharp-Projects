namespace ConsoleRolePlayingGame.Screens;

public class OverworldScreen(GameManager game, IAnsiConsole console)
{
    public const int Size = 21;
    private readonly Layout _layout = new Layout("Root")
        .SplitRows(
            new Layout("Header").Size(1)
                .Update(new Markup("[bold yellow]Overworld[/]")),
            new Layout("Content").Size(Size).SplitColumns(
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
}