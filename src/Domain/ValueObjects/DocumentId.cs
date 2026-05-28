namespace Domain.ValueObjects;

public record DocumentId(Guid Value)
{
    public static DocumentId New() => new(Guid.NewGuid());
    public static DocumentId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
