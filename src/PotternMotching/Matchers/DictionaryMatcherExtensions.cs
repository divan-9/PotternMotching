namespace PotternMotching.Matchers;

internal static class DictionaryMatcherExtensions
{
    internal static MatchResult EvaluateMatchAll<TKey, TValue>(
        this DictionaryMatcher<TKey, TValue>.MatchAll matchAll,
        IDictionary<TKey, TValue> value,
        string path)
        where TKey : notnull
    {
        var failures = new List<string>();

        foreach (var (key, matcher) in matchAll.items)
        {
            if (!value.TryGetValue(key, out var actualValue))
            {
                failures.Add($"{path}[{key}]: [DictionaryMatcher.MatchAll] Key '{key}' not found in dictionary");
                continue;
            }

            var result = matcher.Evaluate(actualValue, $"{path}[{key}]");
            if (result is MatchResult.Failure failure)
            {
                failures.AddRange(failure.Reasons);
            }
        }

        if (failures.Count > 0)
        {
            return new MatchResult.Failure([.. failures]);
        }

        return new MatchResult.Success();
    }

    internal static MatchResult EvaluateExactKeys<TKey, TValue>(
        this DictionaryMatcher<TKey, TValue>.ExactKeys exactKeys,
        IDictionary<TKey, TValue> value,
        string path)
        where TKey : notnull
    {
        var failures = new List<string>();

        // Check for missing keys and value mismatches
        foreach (var (key, matcher) in exactKeys.items)
        {
            if (!value.TryGetValue(key, out var actualValue))
            {
                failures.Add($"{path}[{key}]: [DictionaryMatcher.ExactKeys] Key '{key}' not found in dictionary");
                continue;
            }

            var result = matcher.Evaluate(actualValue, $"{path}[{key}]");
            if (result is MatchResult.Failure failure)
            {
                failures.AddRange(failure.Reasons);
            }
        }

        // Check for extra keys
        foreach (var key in value.Keys)
        {
            if (!exactKeys.items.ContainsKey(key))
            {
                failures.Add($"{path}[{key}]: [DictionaryMatcher.ExactKeys] Unexpected key '{key}' found in dictionary");
            }
        }

        if (failures.Count > 0)
        {
            return new MatchResult.Failure([.. failures]);
        }

        return new MatchResult.Success();
    }
}