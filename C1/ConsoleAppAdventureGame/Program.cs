using ConsoleAppAdventureGame.Stories;
using ConsoleAppAdventureGame.Renderers;
using ConsoleAppAdventureGame.Engine;
using Spectre.Console;


try
{
    Console.OutputEncoding = System.Text.Encoding.UTF8;

    //Adventure adventure = SampleAdventure.BuildAdventure();
    Adventure adventure = SampleAdventure.BuildMinimalAdventureUsingBuilder();
    SpectreAdventureRenderer renderer = new();

    adventure.Run(renderer);
}
catch (Exception ex)
{
    //AnsiConsole.MarkupLineInterpolated($"[red]An error occurred: {ex.Message}[/]");
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    throw;
}