namespace ConsoleAppAdventureGame.Engine;

public interface IAdventureRenderer
{
    void Render(StoryNode node);
    void RenderChoiceAction(Choice choice);
    Choice GetChoice(StoryNode node);
}