namespace EncDotNet.PerformanceTests;

/// <summary>
/// The recorded memory budgets, in bytes, for the synthetic harbour cell
/// (<see cref="EncDotNet.Testing.SyntheticCellProfile.Harbour"/>, about 400 KB).
/// </summary>
/// <remarks>
/// <para>
/// These figures are measurements, not targets. When a change moves one, the failing test
/// prints the new figure; update the constant here and say why in the pull request.
/// </para>
/// <para>
/// Changing <see cref="EncDotNet.Testing.SyntheticS57Cell"/> or its profiles changes the
/// input, so every budget has to be re-recorded in the same change.
/// </para>
/// </remarks>
internal static class PerformanceBudgets
{
    // Allocated bytes for one operation.
    public const long Iso8211ReaderAllTokens = 0;
    public const long Iso8211DocumentReaderRead = 2_088_976;
    public const long Iso8211DocumentReaderEnumerateRecords = 1_599_720;
    public const long Iso8211DataDescriptiveRecordReaderRead = 32_808;
    public const long Iso8211FieldReaderCoordinates = 846_376;
    public const long Iso8211FieldReaderCoordinatesByOrdinal = 141_264;
    public const long Iso8211DocumentWriterWrite = 2_318_456;
    public const long S57DocumentReaderRead = 3_713_184;
    public const long S57DocumentApplyChanges = 685_440;
    public const long S57ChartFromDocument = 1_030_312;

    // Retained bytes held by the resulting model.
    public const long Iso8211DocumentRetained = 2_048_712;
    public const long S57DocumentRetained = 1_194_928;
    public const long S57ChartRetained = 1_145_696;
}
