namespace PotternMotching.Patterns;

/// <summary>
/// Internal extension methods for evaluating collection patterns.
/// </summary>
internal static class CollectionPatternExtensions
{
    /// <summary>
    /// Evaluates a subset pattern against a collection.
    /// </summary>
    /// <remarks>
    /// All patterns must be found in the collection, but order doesn't matter.
    /// Each pattern must match at least one element that hasn't been matched by another pattern.
    /// </remarks>
    internal static MatchResult EvaluateSubset<T>(
        this CollectionPattern<T>.Subset anyOrder,
        IEnumerable<T> values,
        string path)
    {
        var patterns = anyOrder.Patterns;
        var matchedPatterns = new HashSet<int>();

        foreach (var item in values)
        {
            // Try to match against any unmatched pattern
            for (int i = 0; i < patterns.Length; i++)
            {
                if (matchedPatterns.Contains(i))
                    continue;

                if (patterns[i].Evaluate(item, $"{path}[?]") is MatchResult.Success)
                {
                    matchedPatterns.Add(i);
                    break;
                }
            }

            // Early exit when all patterns matched
            if (matchedPatterns.Count == patterns.Length)
            {
                return new MatchResult.Success();
            }
        }

        // Build error message with unmatched pattern indices
        if (matchedPatterns.Count < patterns.Length)
        {
            var unmatchedIndices = Enumerable.Range(0, patterns.Length)
                .Where(i => !matchedPatterns.Contains(i))
                .ToList();

            var patternList = string.Join(", ", unmatchedIndices.Select(i => $"pattern[{i}] ({patterns[i]})"));

            return new MatchResult.Failure([
                $"{path}: [CollectionPattern.Subset] Could not match {unmatchedIndices.Count} pattern(s): {patternList}"
            ]);
        }

        return new MatchResult.Success();
    }

    /// <summary>
    /// Evaluates an any-element pattern against a collection.
    /// </summary>
    /// <remarks>
    /// At least one element in the collection must match the pattern.
    /// </remarks>
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

    /// <summary>
    /// Evaluates a sequence pattern against a collection.
    /// </summary>
    /// <remarks>
    /// The collection must have exactly the same length as the pattern array,
    /// and each element must match the corresponding pattern in order.
    /// </remarks>
    internal static MatchResult EvaluateSequence<T>(
        this CollectionPattern<T>.Sequence sequence,
        IEnumerable<T> value,
        string path)
    {
        var patterns = sequence.Patterns;
        var enumerator = value.GetEnumerator();

        // Match each element with fail-fast
        for (int i = 0; i < patterns.Length; i++)
        {
            if (!enumerator.MoveNext())
            {
                // Too short
                return new MatchResult.Failure([
                    $"{path}: [CollectionPattern.Sequence] Expected at least {patterns.Length} items, got {i}"
                ]);
            }

            var result = patterns[i].Evaluate(enumerator.Current, $"{path}[{i}]");
            if (result is MatchResult.Failure)
            {
                return result; // Fail immediately
            }
        }

        // Check if collection is too long
        if (enumerator.MoveNext())
        {
            return new MatchResult.Failure([
                $"{path}: [CollectionPattern.Sequence] Expected length {patterns.Length}, but got more than {patterns.Length} items"
            ]);
        }

        return new MatchResult.Success();
    }

    /// <summary>
    /// Evaluates a starts-with pattern against a collection.
    /// </summary>
    /// <remarks>
    /// The first N elements of the collection must match the patterns in order,
    /// where N is the number of patterns. Additional elements are allowed.
    /// </remarks>
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

        var enumerator = value.GetEnumerator();

        for (int i = 0; i < patterns.Length; i++)
        {
            if (!enumerator.MoveNext())
            {
                return new MatchResult.Failure([
                    $"{path}: [CollectionPattern.StartsWith] Expected to start with {patterns.Length} items, but only {i} items"
                ]);
            }

            var result = patterns[i].Evaluate(enumerator.Current, $"{path}[{i}]");
            if (result is MatchResult.Failure)
            {
                return result; // Fail immediately
            }
        }

        return new MatchResult.Success();
    }

    /// <summary>
    /// Evaluates an ends-with pattern against a collection.
    /// </summary>
    /// <remarks>
    /// The last N elements of the collection must match the patterns in order,
    /// where N is the number of patterns. Additional elements at the beginning are allowed.
    /// </remarks>
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
                $"{path}: [CollectionPattern.EndsWith] Expected to end with {patterns.Length} items, but only {buffer.Count} items"
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