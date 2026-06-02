namespace ConsoleRolePlayingGame.Screens;

public class ScreenManager(OverworldScreen overworldScreen)
{
    public void ShowScreen()
    {
        overworldScreen.GenerateVisual();
    }
}