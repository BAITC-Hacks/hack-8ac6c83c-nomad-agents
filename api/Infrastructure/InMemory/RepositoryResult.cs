namespace TaskForge.Api.Infrastructure.InMemory;

public enum RepositoryOutcome
{
    Success,
    NotFound,
    Conflict
}

public sealed record RepositoryResult<T>(RepositoryOutcome Outcome, T? Value) where T : class
{
    public bool IsSuccess => Outcome == RepositoryOutcome.Success;

    public static RepositoryResult<T> Success(T value) => new(RepositoryOutcome.Success, value);
    public static RepositoryResult<T> NotFound() => new(RepositoryOutcome.NotFound, null);
    public static RepositoryResult<T> Conflict(T value) => new(RepositoryOutcome.Conflict, value);
}
