namespace ConsoleAppAdventureGame.Engine;

public class Adventure
{
    public StoryNode CurrentNode { get; internal set; }
    private readonly Dictionary<string, StoryNode> _nodes;
    public StoryNode GetNode(string id) => _nodes[id];

    public Adventure(IEnumerable<StoryNode> nodes, string startNodeId = "Start")
    {
        var comparison = StringComparer.OrdinalIgnoreCase;

        _nodes = nodes.ToDictionary(n => n.Id, comparison);
        CurrentNode = _nodes[startNodeId];
    }

    public void Run (IAdventureRenderer renderer)
    {
        while (CurrentNode is not null)
        {
            renderer.Render(CurrentNode);

            if (CurrentNode.Choices.Length == 0)
            {
                Console.WriteLine("The end.");
                CurrentNode = null;
            } else
            {
                Choice choice = renderer.GetChoice(CurrentNode);
                choice.Execute(this, renderer);
            }
        }
    }
}