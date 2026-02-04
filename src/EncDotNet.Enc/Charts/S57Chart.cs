using System.Collections.Immutable;

namespace EncDotNet.Enc.Charts;

/// <summary>
/// Represents a strongly-typed S-57 Electronic Navigational Chart (ENC).
/// </summary>
/// <remarks>
/// <para>
/// This class provides a higher-level, strongly-typed view of an S-57 chart document.
/// It transforms the generic <see cref="S57Document"/> model into specialized types
/// for each kind of spatial and feature record.
/// </para>
/// <para>
/// Use <see cref="FromDocument"/> to create an instance from an <see cref="S57Document"/>,
/// or use <see cref="FromFile"/> to load directly from a file.
/// </para>
/// </remarks>
public sealed class S57Chart
{
    /// <summary>Gets the data set identification information.</summary>
    public S57DataSetIdentification? Identification { get; }

    /// <summary>Gets the data set parameters (coordinate systems, scales, etc.).</summary>
    public S57DataSetParameters? Parameters { get; }

    // Spatial records by type
    /// <summary>Gets isolated nodes indexed by their record name.</summary>
    public ImmutableDictionary<S57RecordName, S57IsolatedNode> IsolatedNodes { get; }

    /// <summary>Gets connected nodes indexed by their record name.</summary>
    public ImmutableDictionary<S57RecordName, S57ConnectedNode> ConnectedNodes { get; }

    /// <summary>Gets edges indexed by their record name.</summary>
    public ImmutableDictionary<S57RecordName, S57Edge> Edges { get; }

    /// <summary>Gets faces indexed by their record name.</summary>
    public ImmutableDictionary<S57RecordName, S57Face> Faces { get; }

    // Feature records by geometry type
    /// <summary>Gets all point features.</summary>
    public ImmutableArray<S57PointFeature> PointFeatures { get; }

    /// <summary>Gets all line features.</summary>
    public ImmutableArray<S57LineFeature> LineFeatures { get; }

    /// <summary>Gets all area features.</summary>
    public ImmutableArray<S57AreaFeature> AreaFeatures { get; }

    /// <summary>Gets all meta features (features without geometry).</summary>
    public ImmutableArray<S57MetaFeature> MetaFeatures { get; }

    /// <summary>Gets all features indexed by their record name.</summary>
    public ImmutableDictionary<S57RecordName, S57TypedFeature> AllFeatures { get; }

    /// <summary>
    /// Gets the coordinate multiplication factor for converting integer coordinates to decimal degrees.
    /// </summary>
    public int CoordinateMultiplicationFactor =>
        Parameters?.CoordinateMultiplicationFactor ?? 10000000;

    /// <summary>
    /// Gets the sounding multiplication factor for converting integer soundings to real values.
    /// </summary>
    public int SoundingMultiplicationFactor =>
        Parameters?.SoundingMultiplicationFactor ?? 10;

    /// <summary>
    /// Gets the compilation scale of the chart.
    /// </summary>
    public int CompilationScale =>
        Parameters?.CompilationScale ?? 0;

    private S57Chart(S57Document document)
    {
        Identification = document.DataSetIdentification;
        Parameters = document.DataSetParameters;

        // Build spatial record dictionaries
        var isolatedNodes = ImmutableDictionary.CreateBuilder<S57RecordName, S57IsolatedNode>();
        var connectedNodes = ImmutableDictionary.CreateBuilder<S57RecordName, S57ConnectedNode>();
        var edges = ImmutableDictionary.CreateBuilder<S57RecordName, S57Edge>();
        var faces = ImmutableDictionary.CreateBuilder<S57RecordName, S57Face>();

        foreach (var vectorRecord in document.VectorRecords)
        {
            var spatialRecord = S57SpatialRecord.Create(vectorRecord);

            switch (spatialRecord)
            {
                case S57IsolatedNode isolatedNode:
                    isolatedNodes[vectorRecord.RecordName] = isolatedNode;
                    break;
                case S57ConnectedNode connectedNode:
                    connectedNodes[vectorRecord.RecordName] = connectedNode;
                    break;
                case S57Edge edge:
                    edges[vectorRecord.RecordName] = edge;
                    break;
                case S57Face face:
                    faces[vectorRecord.RecordName] = face;
                    break;
            }
        }

        IsolatedNodes = isolatedNodes.ToImmutable();
        ConnectedNodes = connectedNodes.ToImmutable();
        Edges = edges.ToImmutable();
        Faces = faces.ToImmutable();

        // Build feature record collections
        var pointFeatures = ImmutableArray.CreateBuilder<S57PointFeature>();
        var lineFeatures = ImmutableArray.CreateBuilder<S57LineFeature>();
        var areaFeatures = ImmutableArray.CreateBuilder<S57AreaFeature>();
        var metaFeatures = ImmutableArray.CreateBuilder<S57MetaFeature>();
        var allFeatures = ImmutableDictionary.CreateBuilder<S57RecordName, S57TypedFeature>();

        foreach (var featureRecord in document.FeatureRecords)
        {
            var typedFeature = S57TypedFeature.Create(featureRecord);
            allFeatures[featureRecord.RecordName] = typedFeature;

            switch (typedFeature)
            {
                case S57PointFeature pointFeature:
                    pointFeatures.Add(pointFeature);
                    break;
                case S57LineFeature lineFeature:
                    lineFeatures.Add(lineFeature);
                    break;
                case S57AreaFeature areaFeature:
                    areaFeatures.Add(areaFeature);
                    break;
                case S57MetaFeature metaFeature:
                    metaFeatures.Add(metaFeature);
                    break;
            }
        }

        PointFeatures = pointFeatures.ToImmutable();
        LineFeatures = lineFeatures.ToImmutable();
        AreaFeatures = areaFeatures.ToImmutable();
        MetaFeatures = metaFeatures.ToImmutable();
        AllFeatures = allFeatures.ToImmutable();
    }

    /// <summary>
    /// Creates a strongly-typed chart from an S-57 document.
    /// </summary>
    /// <param name="document">The source S-57 document.</param>
    /// <returns>A strongly-typed chart model.</returns>
    public static S57Chart FromDocument(S57Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return new S57Chart(document);
    }

    /// <summary>
    /// Loads a chart from an S-57 file.
    /// </summary>
    /// <param name="path">The path to the S-57 file.</param>
    /// <returns>A strongly-typed chart model.</returns>
    public static S57Chart FromFile(string path)
    {
        var document = S57Reader.ReadFromFile(path);
        return FromDocument(document);
    }

    /// <summary>
    /// Asynchronously loads a chart from an S-57 file.
    /// </summary>
    /// <param name="path">The path to the S-57 file.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    public static async Task<S57Chart> FromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var document = await S57Reader.ReadFromFileAsync(path, cancellationToken).ConfigureAwait(false);
        return FromDocument(document);
    }

    /// <summary>
    /// Loads a chart from a stream.
    /// </summary>
    /// <param name="stream">The stream containing S-57 data.</param>
    /// <returns>A strongly-typed chart model.</returns>
    public static S57Chart FromStream(Stream stream)
    {
        var document = S57Reader.Read(stream);
        return FromDocument(document);
    }

    /// <summary>
    /// Asynchronously loads a chart from a stream.
    /// </summary>
    /// <param name="stream">The stream containing S-57 data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    public static async Task<S57Chart> FromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var document = await S57Reader.ReadAsync(stream, cancellationToken).ConfigureAwait(false);
        return FromDocument(document);
    }

    /// <summary>
    /// Gets an isolated node by its record name.
    /// </summary>
    /// <param name="name">The record name.</param>
    /// <returns>The isolated node, or <c>null</c> if not found.</returns>
    public S57IsolatedNode? GetIsolatedNode(S57RecordName name)
    {
        return IsolatedNodes.TryGetValue(name, out var node) ? node : null;
    }

    /// <summary>
    /// Gets a connected node by its record name.
    /// </summary>
    /// <param name="name">The record name.</param>
    /// <returns>The connected node, or <c>null</c> if not found.</returns>
    public S57ConnectedNode? GetConnectedNode(S57RecordName name)
    {
        return ConnectedNodes.TryGetValue(name, out var node) ? node : null;
    }

    /// <summary>
    /// Gets an edge by its record name.
    /// </summary>
    /// <param name="name">The record name.</param>
    /// <returns>The edge, or <c>null</c> if not found.</returns>
    public S57Edge? GetEdge(S57RecordName name)
    {
        return Edges.TryGetValue(name, out var edge) ? edge : null;
    }

    /// <summary>
    /// Gets a face by its record name.
    /// </summary>
    /// <param name="name">The record name.</param>
    /// <returns>The face, or <c>null</c> if not found.</returns>
    public S57Face? GetFace(S57RecordName name)
    {
        return Faces.TryGetValue(name, out var face) ? face : null;
    }

    /// <summary>
    /// Gets a typed feature by its record name.
    /// </summary>
    /// <param name="name">The record name.</param>
    /// <returns>The typed feature, or <c>null</c> if not found.</returns>
    public S57TypedFeature? GetFeature(S57RecordName name)
    {
        return AllFeatures.TryGetValue(name, out var feature) ? feature : null;
    }

    /// <summary>
    /// Gets all features with the specified object code.
    /// </summary>
    /// <param name="objectCode">The object code to filter by.</param>
    /// <returns>All features matching the object code.</returns>
    public IEnumerable<S57TypedFeature> GetFeaturesByObjectCode(int objectCode)
    {
        return AllFeatures.Values.Where(f => f.ObjectCode == objectCode);
    }

    /// <summary>
    /// Gets all point features with the specified object code.
    /// </summary>
    /// <param name="objectCode">The object code to filter by.</param>
    /// <returns>All point features matching the object code.</returns>
    public IEnumerable<S57PointFeature> GetPointFeaturesByObjectCode(int objectCode)
    {
        return PointFeatures.Where(f => f.ObjectCode == objectCode);
    }

    /// <summary>
    /// Gets all line features with the specified object code.
    /// </summary>
    /// <param name="objectCode">The object code to filter by.</param>
    /// <returns>All line features matching the object code.</returns>
    public IEnumerable<S57LineFeature> GetLineFeaturesByObjectCode(int objectCode)
    {
        return LineFeatures.Where(f => f.ObjectCode == objectCode);
    }

    /// <summary>
    /// Gets all area features with the specified object code.
    /// </summary>
    /// <param name="objectCode">The object code to filter by.</param>
    /// <returns>All area features matching the object code.</returns>
    public IEnumerable<S57AreaFeature> GetAreaFeaturesByObjectCode(int objectCode)
    {
        return AreaFeatures.Where(f => f.ObjectCode == objectCode);
    }

    /// <summary>
    /// Converts an integer coordinate to decimal degrees.
    /// </summary>
    /// <param name="coordinate">The integer coordinate.</param>
    /// <returns>The coordinate in decimal degrees.</returns>
    public double ToDecimalDegrees(int coordinate)
    {
        return (double)coordinate / CoordinateMultiplicationFactor;
    }

    /// <summary>
    /// Converts an integer depth value to the real depth.
    /// </summary>
    /// <param name="depth">The integer depth value.</param>
    /// <returns>The depth in real units.</returns>
    public double ToDepth(int depth)
    {
        return (double)depth / SoundingMultiplicationFactor;
    }

    /// <summary>
    /// Converts a 2D coordinate to decimal degrees.
    /// </summary>
    /// <param name="coordinate">The coordinate to convert.</param>
    /// <returns>A tuple of (Longitude, Latitude) in decimal degrees.</returns>
    public (double Longitude, double Latitude) ToDecimalDegrees(S57Coordinate2D coordinate)
    {
        return coordinate.ToDecimalDegrees(CoordinateMultiplicationFactor);
    }

    /// <summary>
    /// Converts a sounding to decimal degrees and depth.
    /// </summary>
    /// <param name="sounding">The sounding to convert.</param>
    /// <returns>A tuple of (Longitude, Latitude, Depth).</returns>
    public (double Longitude, double Latitude, double Depth) ToDecimalValues(S57Sounding sounding)
    {
        return sounding.ToDecimalValues(CoordinateMultiplicationFactor, SoundingMultiplicationFactor);
    }
}
