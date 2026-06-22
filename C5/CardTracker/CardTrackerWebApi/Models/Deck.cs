namespace CardTrackerWebApi.Models;

public class Deck : BaseEntity
{
    public required string Name { get; set; }
    public required int UserId { get; set; }
    public List<CardDeck> CardDecks { get; set; } = [];
}