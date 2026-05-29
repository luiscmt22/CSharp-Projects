using EnigmaSimulator.Domain.Utilities;

namespace EnigmaSimulator.Domain;

public class Rotor : IEnigmaModule
{
    private readonly HashSet<char> _notches;
    private readonly BidirectionalCharEncoder _mappings;

    public int Position { get; private set; }

    public Rotor(string characterMapping, int position = 1)
    {
        Position = position;
        string[] parts = characterMapping.Split('-');
        _mappings = new BidirectionalCharEncoder(parts[0]);
        _notches = parts.Length > 1 ? [..parts[1]] : [];
    }

    public IEnigmaModule? NextModule { get; set; }
    public char Encode(char input, bool isForward) => _mappings.Encode(input, isForward: isForward, offset: Position - 1);

    public bool HasNotch(int position) => _notches.Contains((char)('A' + position - 1));

    public bool Advance()
    {
        bool hadNotch = HasNotch(Position);
        const int alphabetSize = 26;
        Position = (Position % alphabetSize) + 1;
        return hadNotch;
    }
}