using AutoMapper;
using CardTrackerWebApi.Models;
using Microsoft.EntityFrameworkCore;

public static class CardEnpoints {
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

    private static IResult HandleGetAllCards(CardsDbContext db, IMapper auto)
    {
        List<Card> cards = db.Cards.AsNoTracking().ToList();
        var responses = auto.Map<List<Card>, List<CardResponse>>(cards);
        return Results.Ok(responses);
    }

    private static IResult HandleGetCardById(int id, CardsDbContext db, IMapper auto)
    {
        Card? card = db.Cards.AsNoTracking().FirstOrDefault(c => c.Id == id);

        if (card is null) return Results.NotFound();

        var response = auto.Map<Card, CardResponce>(card);
        return Results.Ok(response);
    }

    private static IResult HandleCreateCard(CardsDbContext db, IMapper auto) {
        
    }
}