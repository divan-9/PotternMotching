namespace PotternMotching.Patterns;

internal static class DictionaryPatternExtensions
{
    internal static MatchResult EvaluateItems<TKey, TValue>(
        this DictionaryPattern<TKey, TValue>.Items pattern,
        IDictionary<TKey, TValue> value,
        string path)
        where TKey : notnull
    {
        var results = new List<MatchResult>();

        foreach (var (key, valuePattern) in pattern.RequiredItems)
        {
            if (!value.TryGetValue(key, out var actualValue))
            {
                results.Add(new MatchResult.Failure([
                    $"{path}: Missing required key '{key}'"
                ]));
                continue;
            }

            results.Add(valuePattern.Evaluate(actualValue, $"{path}[{key}]"));
        }

        return MatchResult.Combine(results);
    }

    internal static MatchResult EvaluateExactItems<TKey, TValue>(
        this DictionaryPattern<TKey, TValue>.ExactItems pattern,
        IDictionary<TKey, TValue> value,
        string path)
        where TKey : notnull
    {
        var required = pattern.RequiredItems;

        // Check for extra keys
        var extraKeys = value.Keys.Except(required.Keys).ToList();
        if (extraKeys.Any())
        {
            return new MatchResult.Failure([
                $"{path}: Unexpected keys: {string.Join(", ", extraKeys.Select(k => $"'{k}'"))}"
            ]);
        }

        // Check for missing keys and value matches
        var results = new List<MatchResult>();

        foreach (var (key, valuePattern) in required)
        {
            if (!value.TryGetValue(key, out var actualValue))
            {
                results.Add(new MatchResult.Failure([
                    $"{path}: Missing required key '{key}'"
                ]));
                continue;
            }

            results.Add(valuePattern.Evaluate(actualValue, $"{path}[{key}]"));
        }

        return MatchResult.Combine(results);
    }
}
