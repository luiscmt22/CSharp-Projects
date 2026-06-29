using Riok.Mapperly.Abstractions;

namespace CardTrackerWebApi.Mappings;

[Mapper]
public partial class CardMapper
{
    [MapperIgnoreSource(nameof(card.Decks))]
    [MapDerivedType<ActionCard, ActionCardResponse>]
    [MapDerivedType<CreatureCard, CreatureCardResponse>]
    public partial CardResponse ToResponse(Card card);

    public partial List<CardResponse> ToResponse(IEnumerable<Card> cards);

    [MapperIgnoreTarget(nameof(Card.Id))]      // DB-generated — never from the client
    [MapperIgnoreTarget(nameof(Card.Decks))]
    [MapDerivedType<CreateActionCardRequest, ActionCard>]
    [MapDerivedType<CreateCreatureCardRequest, CreatureCard>]
    public partial Card ToEntity(CreateCardRequest request);
}