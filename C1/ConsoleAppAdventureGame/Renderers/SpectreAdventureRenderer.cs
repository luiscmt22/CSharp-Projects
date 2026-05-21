using ConsoleAppAdventureGame.Engine;
using Spectre.Console;

namespace ConsoleAppAdventureGame.Renderers;

public class SpectreAdventureRenderer : IAdventureRenderer
{
    public void Render(StoryNode node)
    {
        foreach (string line in node.Text)
        {
            AnsiConsole.MarkupLine(line);
        }
    }

    public void RenderChoiceAction(Choice choice)
    {
        foreach (string line in choice.WhenChosen)
        {
            AnsiConsole.MarkupLine(line);
        }
    }

    public Choice GetChoice(StoryNode node)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<Choice>()
                .Title("[Yellow]What do you want to do?[/]")
                .AddChoices(node.Choices)
                .UseConverter(c => c.Text));

        AnsiConsole.MarkupLineInterpolated($"[yellow]>[/] [bold blue]{choice.Text}[/]");

        return choice;
    }
}