namespace EncDotNet.S57;

/// <summary>
/// Represents the Data Set Identification field (DSID) from S-57.
/// </summary>
public sealed record S57DataSetIdentification
{
    /// <summary>Gets the record name.</summary>
    public S57RecordName RecordName { get; init; }

    /// <summary>Gets the exchange purpose code (EXPP).</summary>
    /// <remarks>
    /// S-57 Edition 3.1, Part 3 §7.3.1.1 enumerates 1 = N (data set is new) and 2 = R (data set
    /// is a revision). The value is 0 when the subfield is absent; <see cref="S57DocumentWriter"/>
    /// writes 0 as 1. <see cref="S57ExchangePurpose"/> names the codes; cast between the two.
    /// </remarks>
    public int ExchangePurpose { get; init; }

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
    /// <remarks>
    /// The values are the DSSI codes of S-57 Edition 3.1, Part 3 §7.3.1.2, which
    /// <see cref="S57DataStructure"/> names; cast between the two.
    /// </remarks>
    public int DataStructure { get; init; }

    /// <summary>Gets the lexical level for ATTF (AALL).</summary>
    /// <remarks>
    /// <see cref="S57LexicalLevel"/> names the levels; cast between the two. ATTF allows levels 0
    /// and 1 only. <see cref="S57LexicalLevels.Lowest"/> picks the lowest level for a set of values.
    /// </remarks>
    public int AttfLexicalLevel { get; init; }

    /// <summary>Gets the lexical level for NATF (NALL).</summary>
    /// <remarks>
    /// <see cref="S57LexicalLevel"/> names the levels; cast between the two.
    /// <see cref="S57LexicalLevels.Lowest"/> picks the lowest level for a set of values.
    /// </remarks>
    public int NatfLexicalLevel { get; init; }

    /// <summary>Gets the comment (COMT).</summary>
    /// <remarks>
    /// S-57 makes this basic text (ASCII), but some producers, such as NATO AML cells, put
    /// ISO 8859-1 characters in it, so it's read and written as ISO 8859-1, a superset of ASCII.
    /// </remarks>
    public string Comment { get; init; } = string.Empty;

    /// <summary>Gets the product specification code (PRSP).</summary>
    /// <remarks>
    /// S-57 Edition 3.1, Part 3 §7.3.1.1 enumerates 1 = ENC (Electronic Navigational Chart) and
    /// 2 = ODD (IHO Object Catalogue Data Dictionary). Inland ENC producers declare 10, and NATO
    /// Additional Military Layers (AML) producers declare 51-57 (see
    /// <see cref="Aml.AmlProduct"/>). When the data set uses the ASCII lexical form, the mnemonics
    /// <c>ENC</c> and <c>ODD</c> are mapped to 1 and 2. The value is 0 when the subfield is
    /// absent. <see cref="Product"/> interprets it. To set it, cast from
    /// <see cref="S57ProductSpecification"/> (or <see cref="Aml.AmlProduct"/>).
    /// </remarks>
    public int ProductSpecification { get; init; }

    /// <summary>Gets the product specification description (PSDN).</summary>
    public string ProductSpecificationDescription { get; init; } = string.Empty;

    /// <summary>Gets the product specification edition number (PRED), for example "2.0".</summary>
    public string ProductSpecificationEdition { get; init; } = string.Empty;

    /// <summary>Gets the application profile identification code (PROF).</summary>
    /// <remarks>
    /// S-57 Edition 3.1, Part 3 §7.3.1.1 enumerates 1 = EN (ENC new), 2 = ER (ENC revision) and
    /// 3 = DD (IHO data dictionary). NATO Additional Military Layers (AML) data sets use a base
    /// and an update profile per product (CLB 4/5, ESB 6/7, LBO 10/11, MFF 12/13, RAL 16/17 and
    /// SBO 18/19), and AML 3.0 uses 20 for both. When the data set uses the ASCII lexical form,
    /// the S-57 mnemonics are mapped to their numeric codes. The value is 0 when the subfield is
    /// absent. <see cref="S57ProductIdentity.IsUpdate"/> interprets it.
    /// <see cref="S57ApplicationProfile"/> names the S-57 codes; cast between the two.
    /// </remarks>
    public int ApplicationProfile { get; init; }

    /// <summary>
    /// Gets the product this data set conforms to, derived from <see cref="ProductSpecification"/>,
    /// <see cref="ProductSpecificationEdition"/> and <see cref="ApplicationProfile"/>.
    /// </summary>
    /// <remarks>
    /// Use it to tell an ENC from an Inland ENC or a NATO Additional Military Layers (AML) data
    /// set after reading it. See <see cref="S57ProductIdentity.FromDataSetIdentification"/>.
    /// </remarks>
    public S57ProductIdentity Product => S57ProductIdentity.FromDataSetIdentification(this);
}
