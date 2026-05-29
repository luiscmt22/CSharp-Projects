namespace EnigmaSimulator.Domain.Utilities;

public class BidirectionalCharEncoder
{
    private readonly Dictionary<char, char> _forwardMappings = [];
    private readonly Dictionary<char, char> _reverseMappings = [];

    public BidirectionalCharEncoder(string mapping)
    {
        for (int i = 0; i < mapping.Length; i++)
        {
            char inputChar = (char)('A' + i);
            char outputChar = mapping[i];

            _forwardMappings.Add(inputChar, outputChar);
            _reverseMappings.Add(outputChar, inputChar);
        }
    }

    public char Encode(char input, bool isForward, 
                       int offset = 0)
    {
        const int AlphabetSize = 26;

        // Adjust input character based on offset
        // Using modulo(%) to wrap around the alphabet
        int inputIndex = input - 'A';
        int adjustedIndex = (inputIndex + offset + AlphabetSize) % AlphabetSize;
        char adjustedInput = (char)('A' + adjustedIndex);

        Dictionary<char, char> mappings = isForward ? _forwardMappings 
            : _reverseMappings;
        
        char encodedChar = mappings.GetValueOrDefault(adjustedInput, input);

        // Adjust the output character back based on offset
        int encodedIndex = encodedChar - 'A';
        int finalIndex = (encodedIndex - offset + AlphabetSize) % AlphabetSize;

        return (char)(finalIndex + 'A');
    }
}