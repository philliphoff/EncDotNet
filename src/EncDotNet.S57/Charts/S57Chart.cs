using System.Collections.Immutable;

namespace EncDotNet.S57.Charts;

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
public sealed record S57Chart
{
    /// <summary>Gets the data set identification information.</summary>
    public S57DataSetIdentification? Identification { get; }

    /// <summary>Gets the data set parameters (coordinate systems, scales, etc.).</summary>
    public S57DataSetParameters? Parameters { get; }

    // Spatial records by type
    /// <summary>Gets isolated nodes indexed by their record name.</summary>
    public IReadOnlyDictionary<S57RecordName, S57IsolatedNode> IsolatedNodes { get; }

    /// <summary>Gets connected nodes indexed by their record name.</summary>
    public IReadOnlyDictionary<S57RecordName, S57ConnectedNode> ConnectedNodes { get; }

    /// <summary>Gets edges indexed by their record name.</summary>
    public IReadOnlyDictionary<S57RecordName, S57Edge> Edges { get; }

    /// <summary>Gets faces indexed by their record name.</summary>
    public IReadOnlyDictionary<S57RecordName, S57Face> Faces { get; }

    // Feature records by geometry type
    /// <summary>Gets all point features.</summary>
    public IReadOnlyList<S57PointFeature> PointFeatures { get; }

    /// <summary>Gets all line features.</summary>
    public IReadOnlyList<S57LineFeature> LineFeatures { get; }

    /// <summary>Gets all area features.</summary>
    public IReadOnlyList<S57AreaFeature> AreaFeatures { get; }

    /// <summary>Gets all meta features (features without geometry).</summary>
    public IReadOnlyList<S57MetaFeature> MetaFeatures { get; }

    /// <summary>Gets all features indexed by their record name.</summary>
    public IReadOnlyDictionary<S57RecordName, S57TypedFeature> AllFeatures { get; }

    /// <summary>
    /// Gets the reverse feature-pointer index: maps a feature's record name to all features
    /// that reference it via FFPT (feature-to-feature pointers).
    /// </summary>
    public IReadOnlyDictionary<S57RecordName, IReadOnlyList<S57TypedFeature>> ReferencingFeatures { get; }

    /// <summary>
    /// Gets the spatial co-location index: maps a spatial record name to all point features
    /// that reference it via FSPT (feature-to-spatial pointers).
    /// </summary>
    public IReadOnlyDictionary<S57RecordName, IReadOnlyList<S57PointFeature>> ColocatedPointFeatures { get; }

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

    /// <summary>
    /// Initializes a new instance of the <see cref="S57Chart"/> class with explicit spatial and feature data.
    /// </summary>
    /// <remarks>
    /// This constructor is intended primarily for testing scenarios where chart data
    /// needs to be constructed without parsing an S-57 document.
    /// </remarks>
    public S57Chart(
        IReadOnlyDictionary<S57RecordName, S57IsolatedNode>? isolatedNodes = null,
        IReadOnlyDictionary<S57RecordName, S57ConnectedNode>? connectedNodes = null,
        IReadOnlyDictionary<S57RecordName, S57Edge>? edges = null,
        IReadOnlyDictionary<S57RecordName, S57Face>? faces = null,
        IReadOnlyList<S57PointFeature>? pointFeatures = null,
        IReadOnlyList<S57LineFeature>? lineFeatures = null,
        IReadOnlyList<S57AreaFeature>? areaFeatures = null,
        IReadOnlyList<S57MetaFeature>? metaFeatures = null,
        S57DataSetIdentification? identification = null,
        S57DataSetParameters? parameters = null)
    {
        Identification = identification;
        Parameters = parameters;
        IsolatedNodes = isolatedNodes ?? ImmutableDictionary<S57RecordName, S57IsolatedNode>.Empty;
        ConnectedNodes = connectedNodes ?? ImmutableDictionary<S57RecordName, S57ConnectedNode>.Empty;
        Edges = edges ?? ImmutableDictionary<S57RecordName, S57Edge>.Empty;
        Faces = faces ?? ImmutableDictionary<S57RecordName, S57Face>.Empty;
        PointFeatures = pointFeatures ?? [];
        LineFeatures = lineFeatures ?? [];
        AreaFeatures = areaFeatures ?? [];
        MetaFeatures = metaFeatures ?? [];
        AllFeatures = BuildAllFeaturesIndex(PointFeatures, LineFeatures, AreaFeatures, MetaFeatures);
        ReferencingFeatures = BuildReferencingFeaturesIndex(AllFeatures);
        ColocatedPointFeatures = BuildColocatedPointFeaturesIndex(PointFeatures);
    }

    private static ImmutableDictionary<S57RecordName, S57TypedFeature> BuildAllFeaturesIndex(
        IReadOnlyList<S57PointFeature> pointFeatures,
        IReadOnlyList<S57LineFeature> lineFeatures,
        IReadOnlyList<S57AreaFeature> areaFeatures,
        IReadOnlyList<S57MetaFeature> metaFeatures)
    {
        var builder = ImmutableDictionary.CreateBuilder<S57RecordName, S57TypedFeature>();
        foreach (var f in pointFeatures) builder[f.RecordName] = f;
        foreach (var f in lineFeatures) builder[f.RecordName] = f;
        foreach (var f in areaFeatures) builder[f.RecordName] = f;
        foreach (var f in metaFeatures) builder[f.RecordName] = f;
        return builder.ToImmutable();
    }

    private static ImmutableDictionary<S57RecordName, IReadOnlyList<S57TypedFeature>> BuildReferencingFeaturesIndex(
        IReadOnlyDictionary<S57RecordName, S57TypedFeature> allFeatures)
    {
        var builder = new Dictionary<S57RecordName, ImmutableArray<S57TypedFeature>.Builder>();

        foreach (var feature in allFeatures.Values)
        {
            if (!feature.HasRelatedFeatures) continue;

            foreach (var pointer in feature.RelatedFeatures)
            {
                if (!builder.TryGetValue(pointer.Name, out var list))
                {
                    list = ImmutableArray.CreateBuilder<S57TypedFeature>();
                    builder[pointer.Name] = list;
                }
                list.Add(feature);
            }
        }

        var result = ImmutableDictionary.CreateBuilder<S57RecordName, IReadOnlyList<S57TypedFeature>>();
        foreach (var (name, list) in builder)
        {
            result[name] = list.ToImmutable();
        }
        return result.ToImmutable();
    }

    private static ImmutableDictionary<S57RecordName, IReadOnlyList<S57PointFeature>> BuildColocatedPointFeaturesIndex(
        IReadOnlyList<S57PointFeature> pointFeatures)
    {
        var builder = new Dictionary<S57RecordName, ImmutableArray<S57PointFeature>.Builder>();

        foreach (var feature in pointFeatures)
        {
            if (!feature.HasSpatialReferences) continue;

            var spatialName = feature.PrimarySpatialReference!.Value.Name;
            if (!builder.TryGetValue(spatialName, out var list))
            {
                list = ImmutableArray.CreateBuilder<S57PointFeature>();
                builder[spatialName] = list;
            }
            list.Add(feature);
        }

        var result = ImmutableDictionary.CreateBuilder<S57RecordName, IReadOnlyList<S57PointFeature>>();
        foreach (var (name, list) in builder)
        {
            result[name] = list.ToImmutable();
        }
        return result.ToImmutable();
    }

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
        ReferencingFeatures = BuildReferencingFeaturesIndex(AllFeatures);
        ColocatedPointFeatures = BuildColocatedPointFeaturesIndex(PointFeatures);
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
        var document = S57DocumentReader.ReadFromFile(path);
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
        var document = await S57DocumentReader.ReadFromFileAsync(path, cancellationToken: cancellationToken).ConfigureAwait(false);
        return FromDocument(document);
    }

    /// <summary>
    /// Loads a chart from a stream.
    /// </summary>
    /// <param name="stream">The stream containing S-57 data.</param>
    /// <returns>A strongly-typed chart model.</returns>
    public static S57Chart FromStream(Stream stream)
    {
        var document = S57DocumentReader.Read(stream);
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
        var document = await S57DocumentReader.ReadAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
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
    /// Gets all point features that reference the specified spatial record (co-located features).
    /// </summary>
    /// <param name="spatialName">The record name of the spatial record.</param>
    /// <returns>All point features at the same spatial node, or an empty array if none.</returns>
    public IReadOnlyList<S57PointFeature> GetColocatedPointFeatures(S57RecordName spatialName)
    {
        return ColocatedPointFeatures.TryGetValue(spatialName, out var features) ? features : [];
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
