namespace CardTrackerWebApi.Responses;

public class CreatureCardResponse : CardResponse
{
    public override string Type => "Creature";
    public string? SummonEffect { get; init; }
    public string? PerTurnEffect { get; init; }
    public required int SummonCost { get; init; }
    public required int Power { get; init; }
    public bool CanFly { get; init; }
    public bool CanSwin { get; init; }
    public bool CanClimb { get; init; }
}