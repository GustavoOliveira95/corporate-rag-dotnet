namespace Domain.ValueObjects;

public record ChunkContent(string Value)
{
    public static ChunkContent From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new ChunkContent(value);
    }

    public override string ToString() => Value;
}
