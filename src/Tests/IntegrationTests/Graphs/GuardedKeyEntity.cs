public class GuardedKeyEntity
{
    Guid id;

    public Guid Id
    {
        get => id;
        set => throw new InvalidOperationException("Id is generated from initial EmailAddress");
    }

    public string? EmailAddress { get; set; }

    public GuardedKeyEntity(string emailAddress)
    {
        id = CreateDeterministicGuid(emailAddress);
        EmailAddress = emailAddress;
    }

    // EF Core needs a parameterless constructor; it will set the backing field directly
    GuardedKeyEntity()
    {
    }

    static Guid CreateDeterministicGuid(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return new(hash.AsSpan(0, 16));
    }
}
