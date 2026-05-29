using EnigmaSimulator.Domain;
using Shouldly;

namespace EnigmaSimulator.Tests;

public class PlugboardTests
{
    [Theory]
    [InlineData('H', 'O')]
    [InlineData('O', 'H')]
    [InlineData('X', 'X')]
    public void ConnectionPresentedAfterBeingConfigured(char input, char expected)
    {
        //Arrange
        Plugboard plugboard = new("OH", "WA");

        //Act
        char output = plugboard.Encode(input);

        //Assert
        output.ShouldBe(expected);
    }
}