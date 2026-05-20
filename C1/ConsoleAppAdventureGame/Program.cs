using Spectre.Console;


StoryNode stranded = new("Stranded")
{
    Text = ["It seems you failed to account for the Earth being at a different point in its orbit over time.", "You are stranded in space, with no hope of rescue. You will die here, alone and forgotten."]
};

StoryNode destroy = new("Destroy")
{
    Text = ["The device colapses in on itself, compressing all of time and space along with it."]  
};

StoryNode start = new("Start")
{
    Text = ["You are a time traveler, and you have just arrived at your destination.", "You look around and see a device that looks like it could be used to manipulate time.", "What do you do?"],
    Choices = [
        new Choice("Turn it on")
        {
            WhenChosen = ["You decide to use the device to travel to a different point in time."],
            NextNodeId = stranded.Id
        },
        new Choice("Destroy it!")
        {
            WhenChosen = ["You decide to smash the device to pieces."],
            NextNodeId = destroy.Id
        },
    ]
};

Adventure adventure = new(new[] { stranded, destroy, start });

do
{
    StoryNode node = adventure.CurrentNode;

    foreach (string line in node.Text)
    {
        Console.WriteLine(line);
    }

    if (node.Choices.Length == 0)
    {
        Console.WriteLine("The end.");
        Console.WriteLine("Press any key to exit.");
        Console.ReadLine();
        return;
    }

    foreach (Choice choice in node.Choices)
    {
        Console.WriteLine($" {choice.Text}");
    }

    Console.WriteLine("What do you do?");

    var input = Console.ReadLine();

    if (input is null)
    {
        Console.WriteLine("Invalid input, try again.");
        continue;
    }
    if (!node.Choices.Any(c => c.Text.Equals(input, StringComparison.OrdinalIgnoreCase)))
    {
        Console.WriteLine("Invalid choice, try again.");
        continue;
    }
    var option = node.Choices.First(c => c.Text.Equals(input, StringComparison.OrdinalIgnoreCase));

    adventure.CurrentNode = adventure.GetNode(option.NextNodeId);

} while (true);

public class StoryNode(string id)
{
    public string Id => id;
    public required string[] Text { get; init; }
    public Choice[] Choices { get; init; } = [];
}

public record Choice(string Text)
{
    public string[] WhenChosen { get; init; } = [];
    public required string NextNodeId { get; init; }
}

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
}