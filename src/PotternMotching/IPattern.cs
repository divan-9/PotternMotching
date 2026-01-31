namespace PotternMotching;

public interface IMatcher<in T>
{
    MatchResult Evaluate(
         T value,
         string path = "");
}