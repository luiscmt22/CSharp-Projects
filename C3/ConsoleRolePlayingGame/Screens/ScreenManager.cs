namespace ConsoleRolePlayingGame.Screens;

public class ScreenManager(GameManager game,
                           IAnsiConsole console,
                           OverworldScreen overworldScreen)
{
    //Author note: "Because there aren't many screens in this game, just the map screen and a game over screen,
    //we can accomplish most of this logic with a simple switch statement" - Matt Eland

    //This is a perfect candidate for refactoring using the strategy pattern when we add more screens,
    //like a pause menu with inventary, party, and other subscreens. In order to do that, we should create an IScreen
    //interface that all screens implement, then ScreenManager can hold a reference to the current IScreen and delegate ShowScreen calls to it,
    //instead of using a switch statement. - Luís
    public void Run()
    {
        console.Clear();
        switch (game.Status)
        {
            case GameStatus.Overworld:
                console.Write(overworldScreen.GenerateVisual());
                overworldScreen.HandlePlayerInput();
                break;
            case GameStatus.Terminated:
                console.MarkupLine("[red]Game Over[/]");
                console.MarkupLine("[yellow]Press any key to exit...[/]");
                console.Input.ReadKey(intercept: false);
                break;
        }
    }
    
    public void ShowScreen()
    {
        Run();
    }
}