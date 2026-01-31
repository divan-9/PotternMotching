namespace PotternMotching.Matchers;

internal static class CollectionMatcherExtensions
{
    internal static MatchResult EvaluateEndsWith<T>(
        this CollectionMatcher<T>.EndsWith endsWith,
        IEnumerable<T> value,
        string path)
    {
        var patternLength = endsWith.items.Length;

        if (patternLength == 0)
        {
            return new MatchResult.Success();
        }

        var buffer = new Queue<T>(patternLength);

        foreach (var item in value)
        {
            if (buffer.Count == patternLength)
            {
                buffer.Dequeue();
            }
            buffer.Enqueue(item);
        }

        if (buffer.Count < patternLength)
        {
            return new MatchResult.Failure([
                $"{path}: [CollectionMatcher.EndsWith] Expected collection to end with {patternLength} items, but collection has only {buffer.Count} items"
            ]);
        }

        var index = 0;
        foreach (var item in buffer)
        {
            var result = endsWith.items[index].Evaluate(item, $"{path}[^{patternLength - index}]");
            if (result is MatchResult.Failure)
            {
                return result;
            }
            index++;
        }

        return new MatchResult.Success();
    }

    internal static MatchResult EvaluateMatchAll<T>(
        this CollectionMatcher<T>.MatchAll matchAll,
        IEnumerable<T> value,
        string path)
    {
        var remainingPatterns = new List<(int Index, IMatcher<T> Matcher)>();
        for (var i = 0; i < matchAll.items.Length; i++)
        {
            remainingPatterns.Add((i, matchAll.items[i]));
        }

        if (remainingPatterns.Count == 0)
        {
            return new MatchResult.Success();
        }

        foreach (var item in value)
        {
            for (var i = remainingPatterns.Count - 1; i >= 0; i--)
            {
                var result = remainingPatterns[i].Matcher.Evaluate(item, $"{path}[?]");
                if (result is MatchResult.Success)
                {
                    remainingPatterns.RemoveAt(i);
                    if (remainingPatterns.Count == 0)
                    {
                        return new MatchResult.Success();
                    }
                }
            }
        }

        var failedPatterns = string.Join(", ", remainingPatterns.Select(p => $"pattern[{p.Index}]: {p.Matcher}"));
        return new MatchResult.Failure([
            $"{path}: [CollectionMatcher.MatchAll] Expected to find matches for {remainingPatterns.Count} pattern(s): {failedPatterns}"
        ]);
    }

    internal static MatchResult EvaluateSequence<T>(
        this CollectionMatcher<T>.Sequence sequence,
        IEnumerable<T> value,
        string path)
    {
        var index = 0;

        using var valueEnumerator = value.GetEnumerator();
        using var patternEnumerator = ((IEnumerable<IMatcher<T>>)sequence.items).GetEnumerator();

        while (true)
        {
            var hasValue = valueEnumerator.MoveNext();
            var hasPattern = patternEnumerator.MoveNext();

            if (!hasValue && !hasPattern)
            {
                return new MatchResult.Success();
            }

            if (!hasValue)
            {
                return new MatchResult.Failure([
                    $"{path}: [CollectionMatcher.Sequence] Expected sequence of length at least {index + 1}, got {index}"
                ]);
            }

            if (!hasPattern)
            {
                return new MatchResult.Failure([
                    $"{path}: [CollectionMatcher.Sequence] Expected sequence of length {sequence.items.Length}, got more than {index}"
                ]);
            }

            var result = patternEnumerator.Current.Evaluate(valueEnumerator.Current, $"{path}[{index}]");
            if (result is MatchResult.Failure)
            {
                return result;
            }

            index++;
        }
    }

    internal static MatchResult EvaluateStartsWith<T>(
        this CollectionMatcher<T>.StartsWith startsWith,
        IEnumerable<T> value,
        string path)
    {
        var patternIndex = 0;

        using var valueEnumerator = value.GetEnumerator();

        while (patternIndex < startsWith.items.Length)
        {
            if (!valueEnumerator.MoveNext())
            {
                return new MatchResult.Failure([
                    $"{path}: [CollectionMatcher.StartsWith] Expected collection to start with {startsWith.items.Length} items, but collection has only {patternIndex} items"
                ]);
            }

            var result = startsWith.items[patternIndex].Evaluate(valueEnumerator.Current, $"{path}[{patternIndex}]");
            if (result is MatchResult.Failure)
            {
                return result;
            }

            patternIndex++;
        }

        return new MatchResult.Success();
    }
}