namespace ConsoleAppAdventureGame.Engine;

public record Choice(string Text)
{
    public string[] WhenChosen { get; init; } = [];
    public required string NextNodeId { get; init; }

    public void Execute(Adventure adventure, IAdventureRenderer renderer)
    {
        renderer.RenderChoiceAction(this);

        adventure.CurrentNode = string.IsNullOrEmpty(NextNodeId) ? null : adventure.GetNode(NextNodeId);
    }
}