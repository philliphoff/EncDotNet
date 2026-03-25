using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EncDotNet.S57;
using MessagePack;
using MessagePack.Resolvers;

namespace EncDotNet.ChartViewer.Baking;

/// <summary>
/// Container for baked chart data: the S57 document plus pre-projected edge coordinates.
/// </summary>
[MessagePackObject]
public sealed class BakedChartData
{
    [Key(0)]
    public S57Document Document { get; set; } = null!;

    /// <summary>
    /// Pre-projected Spherical Mercator coordinates per edge, keyed by edge record ID.
    /// Each value is a flat array of alternating x, y doubles.
    /// </summary>
    [Key(1)]
    public Dictionary<int, double[]> ProjectedEdgeCoords { get; set; } = new();
}

/// <summary>
/// Serializes and deserializes <see cref="BakedChartData"/> instances using MessagePack
/// to skip ISO 8211 parsing and coordinate projection on subsequent loads.
/// </summary>
internal static class ChartBaker
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard
            .WithResolver(ContractlessStandardResolver.Instance);

    /// <summary>
    /// Derives the baked file path from the original chart file path.
    /// </summary>
    public static string GetBakedPath(string chartFilePath) =>
        Path.ChangeExtension(chartFilePath, ".baked");

    /// <summary>
    /// Checks whether a valid baked file exists that is newer than the source chart file.
    /// </summary>
    public static bool HasValidBakedFile(string chartFilePath)
    {
        var bakedPath = GetBakedPath(chartFilePath);
        if (!File.Exists(bakedPath))
            return false;

        var sourceTime = File.GetLastWriteTimeUtc(chartFilePath);
        var bakedTime = File.GetLastWriteTimeUtc(bakedPath);
        return bakedTime >= sourceTime;
    }

    /// <summary>
    /// Serializes chart data (document + projected coordinates) to a baked file.
    /// </summary>
    public static async Task<(string Path, TimeSpan Duration, long Size)> BakeAsync(
        BakedChartData data, string chartFilePath, CancellationToken cancellationToken = default)
    {
        var bakedPath = GetBakedPath(chartFilePath);
        var sw = Stopwatch.StartNew();

        var bytes = MessagePackSerializer.Serialize(data, Options, cancellationToken);
        await File.WriteAllBytesAsync(bakedPath, bytes, cancellationToken).ConfigureAwait(false);

        sw.Stop();
        return (bakedPath, sw.Elapsed, bytes.Length);
    }

    /// <summary>
    /// Deserializes chart data from a baked file.
    /// </summary>
    public static async Task<(BakedChartData Data, TimeSpan Duration)> LoadBakedAsync(
        string chartFilePath, CancellationToken cancellationToken = default)
    {
        var bakedPath = GetBakedPath(chartFilePath);
        var sw = Stopwatch.StartNew();

        var bytes = await File.ReadAllBytesAsync(bakedPath, cancellationToken).ConfigureAwait(false);
        var data = MessagePackSerializer.Deserialize<BakedChartData>(bytes, Options, cancellationToken);

        sw.Stop();
        return (data, sw.Elapsed);
    }
}
