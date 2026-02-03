namespace PotternMotching.Patterns;

internal static class CollectionPatternExtensions
{
    internal static MatchResult EvaluateAnyOrder<T>(
        this CollectionPattern<T>.AnyOrder anyOrder,
        IEnumerable<T> values,
        string path)
    {
        // TODO: implement extra items detection
        var unmatchedItems = new List<T>(values);

        foreach (var pattern in anyOrder.Patterns)
        {
            var matched = false;

            for (var i = 0; i < unmatchedItems.Count; i++)
            {
                var item = unmatchedItems[i];
                if (pattern.Evaluate(item, $"{path}[?]") is MatchResult.Success)
                {
                    unmatchedItems.RemoveAt(i);
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                return new MatchResult.Failure([
                    $"{path}: [CollectionPattern.AnyOrder] Expected to find an element matching pattern {pattern}, but none was found."
                ]);
            }
        }

        return new MatchResult.Success();
    }

    internal static MatchResult EvaluateAnyElement<T>(
        this CollectionPattern<T>.AnyElement anyElement,
        IEnumerable<T> value,
        string path)
    {
        foreach (var item in value)
        {
            if (anyElement.Pattern.Evaluate(item, $"{path}[?]") is MatchResult.Success)
            {
                return new MatchResult.Success();
            }
        }

        return new MatchResult.Failure([
            $"{path}: [CollectionPattern.AnyElement] Expected to find matches for pattern {anyElement.Pattern}, but no elements matched."
        ]);
    }

    internal static MatchResult EvaluateSequence<T>(
        this CollectionPattern<T>.Sequence sequence,
        IEnumerable<T> value,
        string path)
    {
        var valueArray = value.ToArray();
        var patterns = sequence.Patterns;

        // Length check
        if (valueArray.Length != patterns.Length)
        {
            return new MatchResult.Failure([
                $"{path}: Expected exactly {patterns.Length} items, but got {valueArray.Length}"
            ]);
        }

        // Match each element
        var results = new List<MatchResult>();
        for (int i = 0; i < patterns.Length; i++)
        {
            results.Add(patterns[i].Evaluate(valueArray[i], $"{path}[{i}]"));
        }

        return MatchResult.Combine(results);
    }

    internal static MatchResult EvaluateStartsWith<T>(
        this CollectionPattern<T>.StartsWith pattern,
        IEnumerable<T> value,
        string path)
    {
        var patterns = pattern.Patterns;

        if (patterns.Length == 0)
        {
            return new MatchResult.Success();
        }

        var results = new List<MatchResult>();
        var enumerator = value.GetEnumerator();

        for (int i = 0; i < patterns.Length; i++)
        {
            if (!enumerator.MoveNext())
            {
                return new MatchResult.Failure([
                    $"{path}: Expected at least {patterns.Length} items, but collection ended at index {i}"
                ]);
            }

            results.Add(patterns[i].Evaluate(enumerator.Current, $"{path}[{i}]"));
        }

        return MatchResult.Combine(results);
    }

    internal static MatchResult EvaluateEndsWith<T>(
        this CollectionPattern<T>.EndsWith pattern,
        IEnumerable<T> value,
        string path)
    {
        var patterns = pattern.Patterns;

        if (patterns.Length == 0)
        {
            return new MatchResult.Success();
        }

        // Use queue as circular buffer to get last N items
        var buffer = new Queue<T>(patterns.Length);

        foreach (var item in value)
        {
            if (buffer.Count == patterns.Length)
            {
                buffer.Dequeue();
            }
            buffer.Enqueue(item);
        }

        // Check we have enough items
        if (buffer.Count < patterns.Length)
        {
            return new MatchResult.Failure([
                $"{path}: Expected at least {patterns.Length} items, but got {buffer.Count}"
            ]);
        }

        // Match from end using hat notation
        var results = new List<MatchResult>();
        var bufferArray = buffer.ToArray();

        for (int i = 0; i < patterns.Length; i++)
        {
            var hatIndex = patterns.Length - i;
            results.Add(patterns[i].Evaluate(bufferArray[i], $"{path}[^{hatIndex}]"));
        }

        return MatchResult.Combine(results);
    }
}