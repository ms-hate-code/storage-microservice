using System.Text.Json;

namespace BuildingBlocks.Validation
{
    public record ValidationResponse(List<ValidationError> Errors)
    {
        public override string ToString() => JsonSerializer.Serialize(this);
    }
}
