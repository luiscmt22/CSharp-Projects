using EnigmaSimulator.Domain;
using Shouldly;

namespace EnigmaSimulator.Tests;

public class EnigmaMachineTests
{
    [Theory]
    [InlineData("HELLO", "ILBDA")]
    [InlineData("ILBDA", "HELLO")]
    [InlineData("Hello", "ILBDA")]
    [InlineData("Ilbda", "HELLO")]
    public void EnigmaShouldEncodeStringsCorrectly(string input, string expected)
    {
        //Arrange
        EnigmaMachine enigma = new EnigmaMachine(
            new Plugboard(),
            new Rotor(RotorSets.Enigma3),
            new Rotor(RotorSets.Enigma2),
            new Rotor(RotorSets.Enigma1),
            new Reflector(ReflectorSets.ReflectorB));

        //Act
        var output = enigma.Encode(input);

        //Assert
        output.ShouldBe(expected);
    }
}