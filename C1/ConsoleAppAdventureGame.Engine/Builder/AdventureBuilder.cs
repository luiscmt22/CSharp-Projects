namespace ConsoleAppAdventureGame.Engine.Builder;

public class AdventureBuilder
{
    private readonly List<NodeBuilder> _nodes = [];

    public Adventure Build()
    {
        Validate();

        IEnumerable<StoryNode> nodes = _nodes.Select(n => n.Build());
        Adventure adventure = new Adventure(nodes);

        return adventure;
    }

    public AdventureBuilder WithNode(string id, Action<NodeBuilder> configure)
    {
        NodeBuilder nodeBuilder = new(id);
        configure(nodeBuilder);
        _nodes.Add(nodeBuilder);
        
        return this;
    }

    public AdventureBuilder WithStartNode(Action<NodeBuilder> configure) 
        => WithNode(Adventure.StartNodeId, configure);
    
    private void Validate()
    {        
        if (_nodes.Count == 0)
        {
            throw new InvalidOperationException("Adventure must have at least one node");
        }

        foreach (var node in _nodes)
        {
            node.Validate();

            // For nodes with choices that go to other nodes, ensure those nodes exist
            foreach (var choice in node.Choices)
            {
                if (!string.IsNullOrWhiteSpace(choice.NextNodeId) && _nodes.All(n => !Adventure.NodeIdComparer.Equals(n.Id, choice.NextNodeId)))
                {
                    throw new InvalidOperationException($"Node '{node.Id}' references a non-existent node '{choice.NextNodeId}' in choice '{choice.Text}'");
                }
            }
        }
    }
}