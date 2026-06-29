namespace CardTrackerWebApi.Requests;

public class CreateCreatureCardRequest : CreateCardRequest
{
    public string? SummonEffect { get; init; }
    public string? PerTurnEffect { get; init; }
    public required int SummonCost { get; init; }
    public required int Power { get; init; }
    public bool CanFly { get; init; }
    public bool CanSwin { get; init; }
    public bool CanClimb { get; init; }
}