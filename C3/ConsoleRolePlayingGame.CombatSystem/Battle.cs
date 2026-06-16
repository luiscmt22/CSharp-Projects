using System.Text;

namespace ConsoleRolePlayingGame.CombatSystem;

public class Battle
{
    private readonly ICombatGroup _party;
    private readonly ICombatGroup _enemies;
    private readonly Random _random = Random.Shared;
    private const int TimeBetweenTurns = 100;

    public ICombatGroup Enemies => _enemies;
    public ICombatGroup Party => _party;

    public IEnumerable<Combatant> AllCharacters => [..Party.Members, ..Enemies.Members];

    public Combatant? ActiveMember => AllCharacters
        .Where(c => c.IsReady)
        .OrderBy(c => c.TimeUntilTurn)
        .FirstOrDefault();

    public Battle(ICombatGroup party, ICombatGroup enemies)
    {
        _party = party;
        _enemies = enemies;

        foreach (var member in AllCharacters)
        {
            member.TimeUntilTurn = TimeBetweenTurns;
        }
    }

    public void AdvanceTime()
    {
        foreach (var member in AllCharacters)
        {
            member.TimeUntilTurn -= member.Speed;
        }
    }

    public string RunTurn(Combatant character, Ability ability, IEnumerable<Combatant> targets)
    {
        if (ability.ManaCost > 0 && character.Mana < ability.ManaCost)
            return $"{character.Name} does not have enough mana!";

        StringBuilder sb = new();
        sb.AppendLine($"{character.Name} uses {ability.Name}!");

        character.Mana -= ability.ManaCost;
        character.TimeUntilTurn = TimeBetweenTurns;

        foreach (var target in targets.Where(t => !t.IsDead))
        {
            string message = ActivateAbilityOnTarget(character, ability, target);
            sb.AppendLine(message);
        }

        return sb.ToString();
    }

    private string ActivateAbilityOnTarget(Combatant character, Ability ability, Combatant target)
    {
        int amount = ability.CalculateAmount(character, _random);

        if (ability.IsHeal)
        {
            target.Health = Math.Min(target.MaxHealth, target.Health + amount);

            return $"{target.Name} heals for {amount}!";
        }

        target.Health -= Math.Max(0, target.Health - amount);
        return target.IsDead
            ? $"{target.Name} takes {amount} damage and dies!"
            : $"{target.Name} takes {amount} damage";
    }
}