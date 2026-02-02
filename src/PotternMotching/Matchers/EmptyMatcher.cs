namespace PotternMotching.Matchers;

public record EmptyMatcher<T> : IMatcher<T>
{
    /// <inheritdoc/>
    public MatchResult Evaluate(
        T value,
        string path = "")
    {
        return new MatchResult.Success();
    }
}