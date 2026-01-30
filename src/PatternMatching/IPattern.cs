namespace PatternMatching;

using Dunet;

public interface IMatcher<in T>
{
    MatchResult Match(
         T value,
         string path = "");
}

[Union]
public abstract partial record MatchResult
{
    public partial record Success();

    public partial record Failure(
        string Reason);


    public static MatchResult Combine(
        IEnumerable<MatchResult> results)
    {
        var failures = results.OfType<Failure>().ToList();
        if (failures.Any())
        {
            var combinedReason = string.Join("; ", failures.Select(f => f.Reason));
            return new Failure(combinedReason);
        }

        return new Success();
    }
}