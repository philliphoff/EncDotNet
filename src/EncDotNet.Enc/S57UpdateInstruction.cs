namespace EncDotNet.Enc;

/// <summary>
/// S-57 Update Instruction (RUIN).
/// </summary>
public enum S57UpdateInstruction : byte
{
    /// <summary>Insert</summary>
    Insert = 1,

    /// <summary>Delete</summary>
    Delete = 2,

    /// <summary>Modify</summary>
    Modify = 3
}
