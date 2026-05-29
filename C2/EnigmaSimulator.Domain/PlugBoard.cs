namespace EnigmaSimulator.Domain;

public class Plugboard : IEnigmaModule
{
    private readonly Dictionary<char, char> _mappings = [];

    public Plugboard(params string[] pairs)
    {
        foreach (var pair in pairs)
        {
            _mappings.Add(char.ToUpper(pair[0]), char.ToUpper(pair[1]));
            _mappings.Add(char.ToUpper(pair[1]), char.ToUpper(pair[0]));
        }
    }

    public IEnigmaModule? NextModule { get; set; }

    public char Encode(char input, bool isForward = true) => _mappings.GetValueOrDefault(input, defaultValue: input);
}