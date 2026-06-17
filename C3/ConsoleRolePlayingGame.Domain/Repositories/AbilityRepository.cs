using ConsoleRolePlayingGame.CombatSystem;

namespace ConsoleRolePlayingGame.Domain.Repositories;

public class AbilityRepository : FileRepositoryBase
{
    public List<Ability> GetAbilities() => LoadManyFromJsonFile<Ability>("Abilities.json");

    public List<Ability> GetAbilities(IEnumerable<string> ids)
    {
        var abilitiesDict = GetAbilities().ToDictionary(a => a.Id, StringComparer.OrdinalIgnoreCase);

        return [.. ids.Select(id => abilitiesDict[id])];
    }
}