using EnigmaSimulator;
using EnigmaSimulator.Domain;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;

try
{
    AnsiConsole.Write(new FigletText("Enigma")
        .Color(Color.Black));
    AnsiConsole.WriteLine();

    ServiceCollection services = new();
    services.AddScoped<EnigmaMachine>(_ => new EnigmaMachine(
        new Plugboard(),
        new Rotor(RotorSets.Enigma3),
        new Rotor(RotorSets.Enigma2),
        new Rotor(RotorSets.Enigma1),
        new Reflector(ReflectorSets.ReflectorB)
        ));

    CommandApp app = new(new TypeRegistrar(services));
    app.Configure(config =>
    {
        config.AddCommand<InteractiveEnigmaCommand>("interactive")
            .WithAlias("i")
            .WithDescription("Encrypts keystrokes as you type them using" +
                             "Enigma.");
        config.AddCommand<EncodeCommand>("encode")
            .WithAlias("e")
            .WithDescription("Encrypts a message using Enigma and display the output.")
            .WithExample("encode Hello");
    });

    return app.Run(args);
}
catch(Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}