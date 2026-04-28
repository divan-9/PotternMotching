namespace PotternMotching;

/// <summary>
/// The exception that is thrown when a pattern assertion fails.
/// </summary>
/// <remarks>
/// This exception is thrown by the <see cref="Ossertions.AssertPattern{T}(T, IPattern{T}, string?)"/> method when a value
/// does not match the specified pattern. The exception message contains detailed information
/// about why the pattern match failed.
/// </remarks>
public class AssertionFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssertionFailedException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the assertion failure, including detailed match failure reasons.</param>
    public AssertionFailedException(string message) : base(message) { }
}