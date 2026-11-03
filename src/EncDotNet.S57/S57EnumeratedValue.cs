namespace EncDotNet.S57;

/// <summary>
/// One of the predefined values of an enumerated (<c>E</c>) or list (<c>L</c>) S-57 attribute.
/// </summary>
/// <remarks>
/// S-57 Edition 3.1, Appendix A, Chapter 2 calls these the attribute's expected input, each with an
/// ID and a meaning.
/// </remarks>
/// <param name="Code">The value's code (ID), as it's encoded in <c>ATVL</c>.</param>
/// <param name="Name">The value's meaning, for example <c>white</c>.</param>
public sealed record S57EnumeratedValue(int Code, string Name)
{
    /// <inheritdoc/>
    public override string ToString() => $"{Code}: {Name}";
}
