namespace CardTrackerWebApi.Models;

public abstract class Card : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public List<Deck> Decks { get; set; } = [];
}