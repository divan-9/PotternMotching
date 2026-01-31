namespace PotternMotching;

using Dunet;

/// <summary>
/// Represents the result of a pattern match operation.
/// </summary>
/// <remarks>
/// This is a discriminated union type with two cases:
/// <list type="bullet">
/// <item><see cref="Success"/> - The value matched the pattern.</item>
/// <item><see cref="Failure"/> - The value did not match, with detailed reasons.</item>
/// </list>
/// </remarks>
[Union]
public abstract partial record MatchResult
{
    /// <summary>
    /// Represents a successful pattern match.
    /// </summary>
    public partial record Success()
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return "Success";
        }
    }

    /// <summary>
    /// Represents a failed pattern match with detailed reasons.
    /// </summary>
    /// <param name="Reasons">
    /// An array of human-readable strings explaining why the match failed.
    /// Each reason typically includes the path to the mismatched value and details about the mismatch.
    /// </param>
    public partial record Failure(
        string[] Reasons)
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            if (this.Reasons.Length == 0)
            {
                return "Failure: No reasons provided";
            }

            if (this.Reasons.Length == 1)
            {
                return $"Failure: {this.Reasons[0]}";
            }

            return $"Failure:{Environment.NewLine}{string.Join(Environment.NewLine, this.Reasons.Select(r => $"  - {r}"))}";
        }
    }

    /// <summary>
    /// Combines multiple match results into a single result.
    /// </summary>
    /// <param name="results">The collection of match results to combine.</param>
    /// <returns>
    /// A <see cref="Success"/> if all results are successful, or a <see cref="Failure"/>
    /// containing all failure reasons if any result failed.
    /// </returns>
    /// <remarks>
    /// This is useful when matching against composite objects where multiple sub-matches must all succeed.
    /// All failure reasons from all failed matches are combined into a single failure result.
    /// </remarks>
    public static MatchResult Combine(
        IEnumerable<MatchResult> results)
    {
        var failures = results.OfType<Failure>().ToList();
        if (failures.Count != 0)
        {
            var combinedReasons = failures.SelectMany(f => f.Reasons).ToArray();
            return new Failure(combinedReasons);
        }

        return new Success();
    }
}