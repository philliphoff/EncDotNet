using System.Collections.Immutable;

namespace EncDotNet.Enc.Charts;

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
public sealed class S57Edge : S57SpatialRecord
{
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
    /// These coordinates define the shape of the edge between the beginning and end nodes.
    /// The beginning and end node positions are not included in this array.
    /// </remarks>
    public ImmutableArray<S57Coordinate2D> IntermediatePoints { get; }

    /// <summary>
    /// Gets a value indicating whether this edge has intermediate points.
    /// </summary>
    public bool HasIntermediatePoints => !IntermediatePoints.IsDefaultOrEmpty;

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
    internal S57Edge(S57VectorRecord source) : base(source)
    {
        IntermediatePoints = source.Coordinates2D;

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
