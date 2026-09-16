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

    /// <summary>Gets the producing agency code (AGEN).</summary>
    public int ProducingAgency { get; init; }

    /// <summary>Gets the data structure (DSTR).</summary>
    public int DataStructure { get; init; }

    /// <summary>Gets the lexical level for ATTF (AALL).</summary>
    public int AttfLexicalLevel { get; init; }

    /// <summary>Gets the lexical level for NATF (NALL).</summary>
    public int NatfLexicalLevel { get; init; }

    /// <summary>Gets the comment (COMT).</summary>
    public string Comment { get; init; } = string.Empty;

    /// <summary>Gets the product specification code (PRSP).</summary>
    /// <remarks>
    /// S-57 Edition 3.1, Part 3 §7.3.1.1 enumerates 1 = ENC (Electronic Navigational Chart) and
    /// 2 = ODD (IHO Object Catalogue Data Dictionary). Inland ENC producers declare 10.
    /// When the data set uses the ASCII lexical form, the mnemonics <c>ENC</c> and <c>ODD</c> are
    /// mapped to 1 and 2. The value is 0 when the subfield is absent.
    /// </remarks>
    public int ProductSpecification { get; init; }

    /// <summary>Gets the product specification description (PSDN).</summary>
    public string ProductSpecificationDescription { get; init; } = string.Empty;

    /// <summary>Gets the product specification edition number (PRED), for example "2.0".</summary>
    public string ProductSpecificationEdition { get; init; } = string.Empty;

    /// <summary>Gets the application profile identification code (PROF).</summary>
    /// <remarks>
    /// S-57 Edition 3.1, Part 3 §7.3.1.1 enumerates 1 = EN (ENC new), 2 = ER (ENC revision) and
    /// 3 = DD (IHO data dictionary). When the data set uses the ASCII lexical form, those mnemonics
    /// are mapped to their numeric codes. The value is 0 when the subfield is absent.
    /// </remarks>
    public int ApplicationProfile { get; init; }
}
