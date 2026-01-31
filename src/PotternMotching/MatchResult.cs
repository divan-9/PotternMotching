namespace PotternMotching;

using Dunet;

[Union]
public abstract partial record MatchResult
{
    public partial record Success();

    public partial record Failure(
        string[] Reasons);

    public static MatchResult Combine(
        IEnumerable<MatchResult> results)
    {
        var failures = results.OfType<Failure>().ToList();
        if (failures.Any())
        {
            var combinedReasons = failures.SelectMany(f => f.Reasons).ToArray();
            return new Failure(combinedReasons);
        }

        return new Success();
    }
}