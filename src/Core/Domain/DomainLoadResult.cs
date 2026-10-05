namespace Core.Domain;

public sealed record DomainLoadResult<T>(
    IReadOnlyList<T> Entities,
    IReadOnlyList<string> DomainErrors);