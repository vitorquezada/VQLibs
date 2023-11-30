using System.Diagnostics.CodeAnalysis;

public static class VQArgumentNullExceptionExtensions
{
    public static void ThrowIfNullOrWhiteSpace([NotNull] this string? text, string? paramName = null, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentNullException(paramName, message);
    }

    public static void ThrowIfNull([NotNull] this object? text, string? paramName = null, string? message = null)
    {
        if (text == null)
            throw new ArgumentNullException(paramName);
    }
}