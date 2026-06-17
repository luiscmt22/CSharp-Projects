using ConsoleRolePlayingGame.CombatSystem;
using ConsoleRolePlayingGame.Domain.Entities;
using ConsoleRolePlayingGame.Overworld.Structure;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleRolePlayingGame.Domain.Repositories;

public class PartyRepository([FromKeyedServices("Player")] IBattleStrategy strategy, 
    AbilityRepository abilityRepository) : FileRepositoryBase
{
    public PlayerParty Load() => new()
    {
        Name = "The Party",
        MapPos = new Pos(0, 0),
        Members =
        [
            ..LoadManyFromJsonFile<CombatantData>("Party.json")
                .Select(c => new Combatant(c, strategy)
                {
                    IsPlayer = true,
                    Abilities = abilityRepository.GetAbilities(c.AbilityIds)
                })
        ]
    };
}