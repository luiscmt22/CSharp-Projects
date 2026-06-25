using CardTrackerWebApi.Mappings;
using CardTrackerWebApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddDbContext<CardsDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<UserMapper>();

var app = builder.Build();

// app.UseAuthentication();
// app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Endpoints
app.AddLoginEndpoints();
app.AddDeckEndpoints();
app.AddUsersEndpoints();
app.AddCardEndpoints();
app.MapGet("/tea", () => Results.StatusCode(StatusCodes.Status418ImATeapot));
app.MapGet("/", () => Results.Ok("Hello World! Are you good?"));

app.Run();
