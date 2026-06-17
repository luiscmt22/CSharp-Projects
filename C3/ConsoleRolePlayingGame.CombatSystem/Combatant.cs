namespace ConsoleRolePlayingGame.CombatSystem;

public record Combatant
{
    public Combatant(CombatantData data, IBattleStrategy strategy, int Level = 1)
    {
        Name = data.Name;
        AsciiArt = data.AsciiArt;
        MaxHealth = data.BaseHealth + (Level * 10);
        Health = MaxHealth;
        MaxMana = data.BaseMana + (Level * 5);
        Mana = MaxMana;
        Dexterity = data.BaseDexterity + (Level * 2);
        Speed = data.BaseSpeed + (Level * 1);
        Strength = data.BaseStrength + (Level * 2);
        Intelligence = data.BaseIntelligence + (Level * 2);
        Defense = data.BaseDefense + (Level * 1);
        MagicDefense = data.BaseMagicDefense + (Level * 1);
        ExperienceReward = data.BaseExperienceReward + (Level * 20);
        GoldReward = data.BaseGoldReward + (Level * 10);
        ItemDropChance = data.BaseItemDropChance;
        ItemDropQuantity = data.BaseItemDropQuantity;
        ItemDropQuality = data.BaseItemDropQuality;
        Strategy = strategy;
    }

    public string Name { get; init; }
    public string[] AsciiArt { get; init; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; init; }
    public int Dexterity { get; init; }
    public int Speed { get; init; }
    public int Strength { get; init; }
    public int Intelligence { get; init; }
    public int Defense { get; init; }
    public int MagicDefense { get; init; }
    public int ExperienceReward { get; init; }
    public int GoldReward { get; init; }
    public double ItemDropChance { get; init; }
    public int ItemDropQuantity { get; init; }
    public int ItemDropQuality { get; init; }
    public IEnumerable<Ability> Abilities { get; set; } = [];
    public int Level { get; set; }
    public int TimeUntilTurn { get; internal set; }
    public bool IsDead => Health <= 0;
    public bool IsReady => TimeUntilTurn <= 0 && !IsDead;
    public bool IsPlayer { get; set; }
    public IBattleStrategy Strategy { get;}
}