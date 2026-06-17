using ConsoleRolePlayingGame.CombatSystem;

namespace ConsoleRolePlayingGame.ConsoleApp.Screens;

public class BattleScreen(GameManager game, IAnsiConsole console)
{
    private readonly Layout _layout = new Layout()
        .SplitRows(
            new Layout().SplitColumns(
                new Layout("Enemies").Ratio(3),
                new Layout("Party").Ratio(2)
            ).Size(25)
        );

    public IRenderable GenerateVisual()
    {
        Battle? battle = game.Battle;

        CombatGroupRenderer enemeies = new(battle.Enemies, battle.ActiveMember, new HalfBlockSpriteRenderer());
        _layout["Enemies"].Update(enemeies.GenerateVisual());

        CombatGroupRenderer party = new(battle.Party, battle.ActiveMember, new AsciiArtRenderer(), includeStats: true);
        _layout["Party"].Update(party.GenerateVisual());

        return _layout;
    }

    public async Task HandlePlayerInputAsync()
    {
        string? message = null;
        console.Cursor.SetPosition(0,26);
        Battle battle = game.Battle!;
        Combatant? combatant = battle.ActiveMember;
        if (combatant is null)
        {
            await console.Status()
                .StartAsync("Wait for next combatant...",
                    async _ =>
                    {
                        await Task.Delay(250);
                        battle.AdvanceTime();
                    });
        }
        else
        {
            message = combatant.Strategy.Execute(battle, combatant);
        }

        if (!string.IsNullOrEmpty(message))
        {
            console.WriteLine(message);
            console.MarkupLine("[Blue]Press any key to continue.[/]");
            console.Input.ReadKey(true);
        }
    }
}