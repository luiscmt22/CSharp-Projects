using Spectre.Console;



StoryNode node = new()
{

    Text = 
    [
        "You wake up in a dark room. You can't remember how you got there.",
        "You see a door to your left and a window to your right."
    ],
    
    index = 0
};


public class StoryNode()
{
    public string Id => 4.ToString();
    public int index { get; init; }
    public required string[] Text { get; init; }
    //public Choice[] Choices { get; init; } = [];
}