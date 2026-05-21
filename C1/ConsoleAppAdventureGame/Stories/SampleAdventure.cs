using ConsoleAppAdventureGame.Engine;

namespace ConsoleAppAdventureGame.Stories;

public static class SampleAdventure
{
    public static Adventure BuildAdventure()
    {
        StoryNode stranded = new("Stranded"){
        Text = ["It seems you failed to account for the [yellow italic]Earth being at a different point in its orbit[/] over time.", 
            "You are stranded in space, with no hope of rescue. You will die here, alone and forgotten."]
        };

        StoryNode destroy = new("Destroy"){
            Text = ["The device colapses in on itself, [cyan underline]compressing all of time and space[/] along with it."]  
        };

        StoryNode start = new("Start"){
            Text = ["You are a time traveler, and you have just arrived at your destination.", "You look around and see a device that looks like it could be used to manipulate time."],
            Choices = [
                new Choice("Turn it on")
                {
                    WhenChosen = ["You are now [red bold] adrift in space without a spacesuit[/]."],
                    NextNodeId = stranded.Id
                },
                new Choice("Destroy it!")
                {
                    WhenChosen = ["You [bold yellow] smash the device[/] to pieces."],
                    NextNodeId = destroy.Id
                },
            ]
        };

        return new Adventure([ start, stranded, destroy ]);
    }
}