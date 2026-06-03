IAnsiConsole console = AnsiConsole.Console;

console.Write(new FigletText("Console RPG").Centered().Color(Color.GreenYellow));

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

    console.Write(new Markup("[grey]Generating world...[/]"));

    ServiceProvider serviceProvider = services.BuildServiceProvider();

    console.Write(new Markup("[grey]Generating world..2[/]"));

    GameManager game = serviceProvider.GetRequiredService<GameManager>();
    ScreenManager screens = serviceProvider.GetRequiredService<ScreenManager>();

    console.Write(new Markup("[green]World generated![/]\nPress any key to start..."));

    while (game.Status != GameStatus.Terminated)
    {
        console.WriteLine("loop top");
        screens.ShowScreen();
        game.Update();
    }
    
}
catch (Exception ex)
{
    //Console.Write(ex);
    console.WriteException(ex, ExceptionFormats.Default);
    console.Input.ReadKey(intercept: false);
}
