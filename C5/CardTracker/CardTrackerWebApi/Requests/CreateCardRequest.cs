using System.Text.Json.Serialization;

namespace CardTrackerWebApi.Requests;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CreateActionCardRequest),   "Action")]
[JsonDerivedType(typeof(CreateCreatureCardRequest), "Creature")]
public abstract class CreateCardRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
}