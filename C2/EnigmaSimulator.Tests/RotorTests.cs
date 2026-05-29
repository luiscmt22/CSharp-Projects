using EnigmaSimulator.Domain;
using Shouldly;

namespace EnigmaSimulator.Tests;

public class RotorTests
{
    [Theory]
    [InlineData('X', RotorSets.Enigma3, 2, true, 'P')]
    [InlineData('Z', RotorSets.Enigma3, 1, true, 'O')]
    [InlineData('Z', RotorSets.Enigma3, 2, true, 'A')]
    [InlineData('D', RotorSets.Enigma3, 3, true, 'J')]
    [InlineData('D', RotorSets.Enigma3, 3, false, 'A')]
    [InlineData('S', RotorSets.Enigma1, 1, false, 'S')]
    [InlineData('S', RotorSets.Enigma2, 1, false, 'E')]
    [InlineData('E', RotorSets.Enigma3, 1, false, 'P')]
    public void RotorMappingTests(char input, string mapping, int position, bool isForward, char expected)
    {
        // Arrange
        Rotor rotor = new(mapping, position);
        
        // Act
        char output = rotor.Encode(input, isForward);
        
        // Assert
        output.ShouldBe(expected);
    }

    [Theory]
    [InlineData(1, 2, false)]
    [InlineData(17, 18, true)]
    [InlineData(26, 1, false)]
    public void RotorShouldAdvance(int position, int expectedPosition, bool expectNotch)
    {
        // Arrange
        Rotor rotor = new(RotorSets.Enigma1, position);

        // Act
        bool encounteredNotch = rotor.Advance();

        // Assert
        rotor.Position.ShouldBe(expectedPosition);
        encounteredNotch.ShouldBe(expectNotch);
    }

}