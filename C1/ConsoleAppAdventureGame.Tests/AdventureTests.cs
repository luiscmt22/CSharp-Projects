using Shouldly;

namespace ConsoleAppAdventureGame.Tests;

public class AdventureTests
{
    [Fact]
    public void StartNodeShouldBeCurrentNodeOnStart()
    {
        // Arrange
        AdventureBuilder adventureBuilder = new AdventureBuilder()
            .WithStartNode(node => node.HasText("Test"));
        
        // Act
        Adventure adventure = adventureBuilder.Build();

        // Assert
        adventure.CurrentNode.ShouldNotBeNull();
        adventure.CurrentNode.Text.Length.ShouldBe(1);
        adventure.CurrentNode.Text[0].ShouldBe("Test");
        adventure.CurrentNode.Id.ShouldBe("Start");
    }
}