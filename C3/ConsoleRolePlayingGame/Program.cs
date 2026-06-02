IAnsiConsole console = AnsiConsole.Console;

try
{
    ServiceCollection services = new();

    // Singleton means one will be shared across the entire application
    services.AddSingleton<GameManager>();
    services.AddSingleton(console); // This is an AddSingleton override that registers an existing instance instead of creating a new one.
                                    // I didn't need to specify the type <IAnsiConsole> because it can be inferred from the instance.
    services.AddSingleton<PerlinNoiseProvider>();
    services.AddSingleton<MapGenerator>();
    services.AddSingleton<WorldMap>();
    services.AddSingleton<OpenPosSelector>();
    services.AddSingleton<PlayerParty>();
    
    // Transients will be created each time they are requested
    services.AddTransient<ScreenManager>();
    services.AddTransient<OverworldScreen>();

    ServiceProvider serviceProvider = services.BuildServiceProvider();

    GameManager game = serviceProvider.GetRequiredService<GameManager>();
    ScreenManager screens = serviceProvider.GetRequiredService<ScreenManager>();

    while (game.Status != GameStatus.Terminated)
    {
        screens.ShowScreen();
        game.Update();
    }
    
}
catch (Exception ex)
{
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
    console.Input.ReadKey(intercept: false);
}
