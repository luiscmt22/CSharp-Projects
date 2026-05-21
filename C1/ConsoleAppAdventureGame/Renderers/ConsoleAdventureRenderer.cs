using ConsoleAppAdventureGame.Engine;

namespace ConsoleAppAdventureGame.Renderers;

public class ConsoleAdventureRenderer : IAdventureRenderer
{
    public void Render(StoryNode node)
    {
        foreach (string line in node.Text)
        {
            Console.WriteLine(line);
        }
    }

    public void RenderChoiceAction(Choice choice)
    {
        foreach (string line in choice.WhenChosen)
        {
            Console.WriteLine(line);
        }
    }

    public Choice GetChoice(StoryNode node)
    {
        Console.WriteLine("What do you want to do?");
        Console.WriteLine();
        Choice? choice = null;

        do
        {
            for (int i = 0; i < node.Choices.Length; i++)
            {
                string choiceText = node.Choices[i].Text;
                Console.WriteLine($" {i + 1}. {choiceText}");
            }

            Console.WriteLine();
            Console.Write("Enter your choice: ");
            string? input = Console.ReadLine();

            if (int.TryParse(input, out int choiceIndex) 
                && choiceIndex > 0 
                && choiceIndex <= node.Choices.Length)
            {
                choice = node.Choices[choiceIndex - 1];
            }
            else
            {
                Console.WriteLine("Invalid choice, try again.");
            }

        } while(choice is null);

        Console.WriteLine($"You chose: {choice.Text}");

        return choice;
    }
}