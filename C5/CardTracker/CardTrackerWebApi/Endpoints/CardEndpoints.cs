using Microsoft.EntityFrameworkCore;

namespace CardTrackerWebApi.Endpoints;
public static class CardEndpoints {
    public static void AddCardEndpoints(this WebApplication app)
    {
        app.MapGet("cards/{id}", HandleGetCardById)
            .AllowAnonymous();
        app.MapGet("cards", HandleGetAllCards)
            .AllowAnonymous();
        app.MapPost("cards", HandleCreateCard)
            .RequireAuthorization("AdminOnly");
        app.MapDelete("cards/{id}", HandleDeleteCard)
            .RequireAuthorization("AdminOnly");
    }

    private static async Task<IResult> HandleGetAllCards(CardsDbContext db, CardMapper mapper)
    {
        List<Card> cards = db.Cards.AsNoTracking().ToList();
        var responses = mapper.ToResponse(cards);
        return Results.Ok(responses);
    }

    private static async Task<IResult> HandleGetCardById(int id, CardsDbContext db, CardMapper mapper)
    {
        Card? card = db.Cards.AsNoTracking().FirstOrDefault(c => c.Id == id);

        if (card is null) return Results.NotFound();

        var response = mapper.ToResponse(card);
        return Results.Ok(response);
    }

    private static async Task<IResult> HandleCreateCard(CreateCardRequest request, CardsDbContext db, CardMapper mapper)
    {
        Card card = mapper.ToEntity(request);
        db.Cards.Add(card);
        db.SaveChanges();
        
        return Results.Created($"/cards/{card.Id}", mapper.ToResponse(card));
    }

    private static async Task<IResult> HandleDeleteCard(int id, CardsDbContext db, CardMapper mapper)
    {
        Card? card = db.Cards.Find(id);
        if (card is null) return Results.NotFound();

        db.Cards.Remove(card);
        db.SaveChanges();

        return Results.NoContent();
    }
}