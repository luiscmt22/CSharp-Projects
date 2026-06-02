namespace ConsoleRolePlayingGame.Screens;

public class ScreenManager(OverWorldScreen overworldScreen)
{
    public void ShowScreen()
    {
        overworldScreen.GenerateVisual();
    }
}