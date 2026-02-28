namespace EncDotNet.Enc;

/// <summary>
/// Represents an update control field (FSPC, FFPC, VRPC, or SGCC) that describes
/// how to splice a subfield array during an S-57 update.
/// </summary>
/// <remarks>
/// <para>
/// Update files use control fields to specify insert, delete, or modify operations
/// on repeating subfield arrays (spatial pointers, feature pointers, vector pointers,
/// or coordinates) at a specific index.
/// </para>
/// <para>
/// The <see cref="Index"/> is 1-based, matching the S-57 specification convention.
/// </para>
/// </remarks>
public readonly record struct S57FieldUpdateControl
{
    /// <summary>Gets the update instruction (insert, delete, or modify).</summary>
    public S57UpdateInstruction UpdateInstruction { get; init; }

    /// <summary>Gets the 1-based index in the target array where the operation begins.</summary>
    public int Index { get; init; }

    /// <summary>Gets the number of entries affected by the operation.</summary>
    public int Count { get; init; }
}
