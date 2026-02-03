namespace PotternMotching.Patterns;

public record EmptyPattern<T> : IPattern<T>
{
    public static IPattern<T> From(
        T value)
    {
        return new EmptyPattern<T>();
    }

    /// <inheritdoc/>
    public MatchResult Evaluate(
        T value,
        string path = "")
    {
        return new MatchResult.Success();
    }
}