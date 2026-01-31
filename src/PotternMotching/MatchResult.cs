namespace PotternMotching;

using Dunet;

[Union]
public abstract partial record MatchResult
{
    public partial record Success()
    {
        public override string ToString() => "Success";
    }

    public partial record Failure(
        string[] Reasons)
    {
        public override string ToString()
        {
            if (Reasons.Length == 0)
            {
                return "Failure: No reasons provided";
            }

            if (Reasons.Length == 1)
            {
                return $"Failure: {Reasons[0]}";
            }

            return $"Failure:{Environment.NewLine}{string.Join(Environment.NewLine, Reasons.Select(r => $"  - {r}"))}";
        }
    }

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