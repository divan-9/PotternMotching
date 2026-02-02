namespace PotternMotching.Matchers;

using Dunet;

[Union]
public partial record ComboMatcher<T> : IMatcher<T>
{
    /// <inheritdoc/>
    public MatchResult Evaluate(
        T value,
        string path = "")
    {
        return this.Match(
            all => all.EvaluateAll(value, path));
    }

    public partial record All(
        IMatcher<T>[] InnerMatchers);

    public static implicit operator DefaultMatcher<T>(
        ComboMatcher<T> matcher)
    {
        return new DefaultMatcher<T>(matcher);
    }
}

public static class ComboMatcher
{
    public static ComboMatcher<T>.All All<T>(
        IMatcher<T>[] innerMatchers)
    {
        return new ComboMatcher<T>.All(innerMatchers);
    }
}