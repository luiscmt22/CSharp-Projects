using ConsoleAppAdventureGame.Engine;
using ConsoleAppAdventureGame.Engine.Builder;

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

    public static Adventure BuildMinimalAdventureUsingBuilder()
    {
        Adventure adventure = new AdventureBuilder()
            .WithStartNode(n =>
            {
                n.HasText("You are a time traveler, and you have just arrived at your destination.", "You look around and see a device that looks like it could be used to manipulate time.");
                n.HasChoice("Turn it on")
                    .WithText("You are now [red] adrift in space[/] without a spacesuit.")
                    .ThatLeadsTo("Stranded");
                n.HasChoice("Destroy it!")
                    .WithText("You [bold yellow] smash the device[/] to pieces.")
                    .ThatLeadsTo("Destroy");
            })
            .WithNode("Stranded", n =>
            {
                n.HasText(
                    "It seems you failed to account for the [yellow italic]Earth being at a different point in its orbit[/] over time.",
                    "You are stranded in space, with no hope of rescue. You will die here, alone and forgotten.");
                n.HasChoice("Accept your fate")
                    .WithText("You accept your fate and drift off into the void.")
                    .ThatLeadsToStart();
                n.HasChoice("Try to find a way out")
                    .WithText("You desperately search for a way to escape, you find a small device that looks like it could be used to manipulate time.")
                    .ThatLeadsTo("IngeniousEscape");
            })
            .WithNode("Destroy", n => n.HasText("The device colapses in on itself, [cyan underline]compressing all of time and space[/] along with it."))
            .WithNode("IngeniousEscape", n => n.HasText("You use the device to create a small time bubble around yourself, allowing you to survive in the void of space until you can find a way back to Earth."))
            .Build();

        return adventure;
    }
}