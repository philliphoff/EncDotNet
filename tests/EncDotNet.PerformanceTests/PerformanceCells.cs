using EncDotNet.Iso8211;
using EncDotNet.S57;
using EncDotNet.Testing;

namespace EncDotNet.PerformanceTests;

/// <summary>
/// The synthetic cells the performance budget tests measure, generated once per test run.
/// </summary>
internal static class PerformanceCells
{
    private static readonly Lazy<Inputs> LazyHarbour = new(() => new Inputs(SyntheticCellProfile.Harbour));
    private static readonly Lazy<Inputs> LazyHarbourX4 = new(() => new Inputs(SyntheticCellProfile.Harbour.Scale(4)));

    /// <summary>
    /// Gets the harbour-scale cell, shaped like an earlier edition of NOAA US5WA50M.
    /// </summary>
    public static Inputs Harbour => LazyHarbour.Value;

    /// <summary>
    /// Gets a cell with four times the harbour cell's records, for growth-rate checks.
    /// </summary>
    public static Inputs HarbourX4 => LazyHarbourX4.Value;

    /// <summary>
    /// A base cell, three updates and the parsed models built from them.
    /// </summary>
    internal sealed class Inputs
    {
        public Inputs(SyntheticCellProfile profile)
        {
            Profile = profile;
            Bytes = SyntheticS57Cell.Create(profile);
            Iso8211Document = Iso8211DocumentReader.Read(Bytes);
            S57Document = S57DocumentReader.Read(Bytes);
            Updates = [.. Enumerable.Range(1, 3).Select(n => S57DocumentReader.Read(SyntheticS57Cell.CreateUpdate(profile, n)))];
        }

        public SyntheticCellProfile Profile { get; }

        public byte[] Bytes { get; }

        public Iso8211Document Iso8211Document { get; }

        public S57Document S57Document { get; }

        public IReadOnlyList<S57Document> Updates { get; }
    }
}
