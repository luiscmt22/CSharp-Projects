namespace ConsoleRolePlayingGame.CombatSystem;

public record CombatantData
{
    public required string Name { get; init; }
    public required string[] AsciiArt { get; init; }
    public int BaseHealth { get; init; }
    public int BaseMana { get; init; }
    public int BaseDexterity { get; init; }
    public int BaseSpeed { get; init; }
    public int BaseStrength { get; init; }
    public int BaseIntelligence { get; init; }
    public int BaseDefense { get; init; }
    public int BaseMagicDefense { get; init; }
    public int BaseExperienceReward { get; init; }
    public int BaseGoldReward { get; init; }
    public double BaseItemDropChance { get; init; }
    public int BaseItemDropQuantity { get; init; }
    public int BaseItemDropQuality { get; init; }
    public List<string> AbilityIds { get; init; }
}