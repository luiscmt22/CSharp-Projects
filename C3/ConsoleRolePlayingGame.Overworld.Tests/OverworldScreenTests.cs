using ConsoleRolePlayingGame.Screens;
using ConsoleRolePlayingGame.GameManagement;
using ConsoleRolePlayingGame.Overworld.Generators;
using Spectre.Console.Rendering;
using Spectre.Console.Testing;
using Shouldly;

namespace ConsoleRolePlayingGame.Overworld.Tests;

public class OverworldScreenTests
{
    [Fact]
    public void OverworldScreenShouldRenderInfo()
    {
        //Arrange
        GameManager game = CreateGameManager();
        TestConsole console = new ();
        OverworldScreen screen = new (game, console);

        //Act
        IRenderable visual = screen.GenerateVisual();
        console.Write(visual);
        
        //Assert
        console.ShouldNotBeNull();
        console.Lines.Count.ShouldBeGreaterThan(10);
        console.Lines[0].ShouldStartWith("Overworld");
        console.Output.ShouldContain("Hero");
        console.Output.ShouldContain("HP");
        console.Output.ShouldContain("10");
    }

    [Fact]
    public void OverworldScreenShouldHandleQuitKey()
    {
        // Arrange
        GameManager game = CreateGameManager();
        TestConsole console = new();
        console.Input.PushKey(ConsoleKey.Escape);
        OverworldScreen screen = new(game, console);

        // Act
        screen.HandlePlayerInput();

        // Assert
        game.Status.ShouldBe(GameStatus.Terminated);
    }

    private static GameManager CreateGameManager()
    {
        PlayerParty party = new();
        WorldMap map = new(new MapGenerator());
        return new GameManager(party, map);
    }
}