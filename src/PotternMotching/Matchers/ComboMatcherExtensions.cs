namespace PotternMotching.Matchers;

public static class ComboMatcherExtensions
{
    public static MatchResult EvaluateAll<T>(
        this ComboMatcher<T>.All matcher,
        T value,
        string path)
    {
        var results = matcher.InnerMatchers
            .Select(m => m.Evaluate(value, path))
            .ToList();

        return MatchResult.Combine(results);
    }
}
