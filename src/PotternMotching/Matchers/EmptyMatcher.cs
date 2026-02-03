namespace PotternMotching.Matchers;

public record EmptyMatcher<T> : IMatcher<T>
{
    public static IMatcher<T> From(
        T value)
    {
        return new EmptyMatcher<T>();
    }

    /// <inheritdoc/>
    public MatchResult Evaluate(
        T value,
        string path = "")
    {
        return new MatchResult.Success();
    }
}