using EnigmaSimulator.Domain;
using EnigmaSimulator.Domain.Utilities;
using Shouldly;

namespace EnigmaSimulator.Tests;

public class BidirectionalCharEncoderTests
{
    [Theory]
    [InlineData('A', 'Y', true, 0)]
    [InlineData('B', 'R', true, 0)]
    [InlineData('C', 'U', true, 0)]
    public void EncodeWithoutOffset_ShouldReturnExpectedOutput(char input, char expected, bool isForward, int offset)
    {
        // Arrange
        BidirectionalCharEncoder encoder = new(ReflectorSets.ReflectorB);

        // Act
        char output = encoder.Encode(input, isForward, offset);

        // Assert
        output.ShouldBe(expected);
    }

    [Theory]
    [InlineData('A', 'Q', true, 1)]
    [InlineData('B', 'G', true, 5)]
    [InlineData('C', 'B', true, 20)]
    public void EncodeWithOffset_ShouldReturnExpectedOutput(char input, char expected, bool isForward, int offset)
    {
        // Arrange
        BidirectionalCharEncoder encoder = new(ReflectorSets.ReflectorB);

        // Act
        char output = encoder.Encode(input, isForward, offset);

        // Assert
        output.ShouldBe(expected);
    }
}