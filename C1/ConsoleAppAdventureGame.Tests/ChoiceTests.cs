using Shouldly;

namespace ConsoleAppAdventureGame.Tests;

public class ChoiceTests
{
    [Fact]
    public void MakingAChoiceShouldAdvanceTheStory()
    {
        //Arrange
        Adventure adventure = SampleAdventure.BuildMinimalAdventureUsingBuilder();
        StoryNode startNode = adventure.CurrentNode!;
        Choice firstChoice = startNode.Choices.First();
        TestAdventureRenderer renderer = new(); // We can use a test renderer here because we won't actually render anything
                                                // or get input from the user, we just want to execute the choice's action.
        IAdventureRenderer substituteRenderer = NSubstitute.Substitute.For<IAdventureRenderer>(); // Or we can use a substitute if we want to verify that certain methods
                                                                                                  // were called on the renderer as a result of executing the choice.

        //Act
        firstChoice.Execute(adventure, substituteRenderer);
        
        //Assert
        adventure.CurrentNode.ShouldNotBe(startNode);

    }

    [Fact]
    public void ExecutingAChoiceShouldAdvanceTheStoryToTargetNode()
    {
        // Arrange
        const string destinationNodeId = "node2";
        Adventure adventure = new AdventureBuilder()
            .WithStartNode(node => { 
                node.HasText("Test");
                node.HasChoice("Go to next").ThatLeadsTo(destinationNodeId);
            })
            .WithNode(destinationNodeId, node =>
                {
                    node.HasText("The story's second node");
                })
            .Build();
        IAdventureRenderer renderer = new TestAdventureRenderer();
        StoryNode startNode = adventure.CurrentNode!;
        
        // Act
        startNode.Choices.First().Execute(adventure, renderer);

        // Assert
        adventure.CurrentNode.ShouldNotBeNull();
        adventure.CurrentNode.Id.ShouldBe(destinationNodeId);
    }
}
