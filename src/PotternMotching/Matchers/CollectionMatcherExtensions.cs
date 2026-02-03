namespace PotternMotching.Matchers;

internal static class CollectionMatcherExtensions
{
    internal static MatchResult EvaluateAnyOrder<T>(
        this CollectionMatcher<T>.AnyOrder anyOrder,
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
                    $"{path}: [CollectionMatcher.AnyOrder] Expected to find an element matching pattern {pattern}, but none was found."
                ]);
            }
        }

        return new MatchResult.Success();
    }

    internal static MatchResult EvaluateAnyElement<T>(
        this CollectionMatcher<T>.AnyElement anyElement,
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
            $"{path}: [CollectionMatcher.AnyElement] Expected to find matches for pattern {anyElement.Pattern}, but no elements matched."
        ]);
    }

    internal static MatchResult EvaluateSequence<T>(
        this CollectionMatcher<T>.Sequence sequence,
        IEnumerable<T> value,
        string path)
    {
        throw new NotImplementedException();
    }
}