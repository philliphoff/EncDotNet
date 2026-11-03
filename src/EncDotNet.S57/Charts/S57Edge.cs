namespace EncDotNet.S57.Charts;

/// <summary>
/// Represents an edge in an S-57 chart.
/// </summary>
/// <remarks>
/// <para>
/// Edges (RCNM=130) are line spatial objects that connect two nodes (beginning and end).
/// They form the basic building blocks of the vector topology and may contain intermediate
/// points that define the shape of the line between the end nodes.
/// </para>
/// <para>
/// In S-57 topology, edges are bounded by connected nodes and may be referenced by faces
/// to form area boundaries.
/// </para>
/// </remarks>
public sealed record S57Edge : S57SpatialRecord
{
    // The COMF that S57Document and S57Chart assume when a data set has no DSPM.
    internal const int DefaultCoordinateMultiplicationFactor = 10_000_000;

    /// <summary>
    /// Gets the record name of the beginning (start) node.
    /// </summary>
    public S57RecordName? BeginningNode { get; }

    /// <summary>
    /// Gets the record name of the end node.
    /// </summary>
    public S57RecordName? EndNode { get; }

    /// <summary>
    /// Gets the intermediate coordinate points along the edge.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These coordinates define the shape of the edge between the beginning and end nodes.
    /// The beginning and end node positions are not included in this array.
    /// </para>
    /// <para>
    /// For an edge whose shape is given by arc/curve definitions (<see cref="ArcCurves"/>)
    /// rather than SG2D coordinates, these are the arcs densified by
    /// <see cref="S57ArcDensifier"/> with its <see cref="S57ArcDensifier.DefaultTolerance"/>,
    /// so that the edge keeps its curve.
    /// </para>
    /// </remarks>
    public IReadOnlyList<S57Coordinate2D> IntermediatePoints { get; }

    /// <summary>
    /// Gets the arc/curve definitions (ARCC) that give the shape of the edge, if any.
    /// </summary>
    /// <remarks>
    /// An edge has either SG2D coordinates or arc/curve definitions (S-57 Part 3,
    /// clause 5.1.4.4). Use <see cref="S57ArcDensifier"/> to densify them with another tolerance.
    /// </remarks>
    public IReadOnlyList<S57ArcCurve> ArcCurves { get; }

    /// <summary>
    /// Gets a value indicating whether the shape of this edge is given by arc/curve definitions.
    /// </summary>
    public bool HasArcCurves => ArcCurves.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this edge has intermediate points.
    /// </summary>
    public bool HasIntermediatePoints => IntermediatePoints.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this edge has a beginning node reference.
    /// </summary>
    public bool HasBeginningNode => BeginningNode.HasValue;

    /// <summary>
    /// Gets a value indicating whether this edge has an end node reference.
    /// </summary>
    public bool HasEndNode => EndNode.HasValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="S57Edge"/> class.
    /// </summary>
    /// <param name="source">The source vector record.</param>
    /// <param name="coordinateMultiplicationFactor">The coordinate multiplication factor (COMF)
    /// of the data set, used to densify arcs.</param>
    internal S57Edge(S57VectorRecord source, int coordinateMultiplicationFactor) : base(source)
    {
        ArcCurves = source.ArcCurves;
        IntermediatePoints = source.Coordinates2D.Count == 0 && source.ArcCurves.Count > 0
            ? S57ArcDensifier.Densify(source.ArcCurves, S57ArcDensifier.DefaultTolerance,
                coordinateMultiplicationFactor > 0 ? coordinateMultiplicationFactor : DefaultCoordinateMultiplicationFactor)
            : source.Coordinates2D;

        // Parse vector pointers for beginning and end nodes
        S57RecordName? beginningNode = null;
        S57RecordName? endNode = null;

        foreach (var pointer in source.VectorPointers)
        {
            switch (pointer.Topology)
            {
                case S57TopologyIndicator.Beginning:
                    beginningNode = pointer.Name;
                    break;
                case S57TopologyIndicator.End:
                    endNode = pointer.Name;
                    break;
            }
        }

        BeginningNode = beginningNode;
        EndNode = endNode;
    }
}
