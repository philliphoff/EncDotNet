namespace EncDotNet.S57;

/// <summary>
/// Represents the Data Set Identification field (DSID) from S-57.
/// </summary>
public sealed record S57DataSetIdentification
{
    /// <summary>Gets the record name.</summary>
    public S57RecordName RecordName { get; init; }

    /// <summary>Gets the intended usage code (INTU).</summary>
    public int IntendedUsage { get; init; }

    /// <summary>Gets the data set name (DSNM).</summary>
    public string DataSetName { get; init; } = string.Empty;

    /// <summary>Gets the edition number (EDTN).</summary>
    public string EditionNumber { get; init; } = string.Empty;

    /// <summary>Gets the update number (UPDN).</summary>
    public string UpdateNumber { get; init; } = string.Empty;

    /// <summary>Gets the update application date (UADT).</summary>
    public string UpdateApplicationDate { get; init; } = string.Empty;

    /// <summary>Gets the issue date (ISDT).</summary>
    public string IssueDate { get; init; } = string.Empty;

    /// <summary>Gets the edition date (STED).</summary>
    public string S57EditionNumber { get; init; } = string.Empty;

    /// <summary>Gets the producing agency code (PRSP).</summary>
    public int ProducingAgency { get; init; }

    /// <summary>Gets the data structure (DSTR).</summary>
    public int DataStructure { get; init; }

    /// <summary>Gets the lexical level for ATTF (AALL).</summary>
    public int AttfLexicalLevel { get; init; }

    /// <summary>Gets the lexical level for NATF (NALL).</summary>
    public int NatfLexicalLevel { get; init; }

    /// <summary>Gets the comment (COMT).</summary>
    public string Comment { get; init; } = string.Empty;
}
