namespace EncDotNet.Enc.Charts;

/// <summary>
/// Represents a connected node in an S-57 chart.
/// </summary>
/// <remarks>
/// <para>
/// Connected nodes (RCNM=120) are point spatial objects that connect edges.
/// They define the start or end points of edges in the vector topology.
/// </para>
/// <para>
/// Unlike isolated nodes, connected nodes always have exactly one position
/// and are part of the topological network.
/// </para>
/// </remarks>
public sealed class S57ConnectedNode : S57SpatialRecord
{
    /// <summary>
    /// Gets the position of this connected node.
    /// </summary>
    public S57Coordinate2D Position { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="S57ConnectedNode"/> class.
    /// </summary>
    /// <param name="source">The source vector record.</param>
    /// <exception cref="InvalidOperationException">Thrown when the vector record has no coordinates.</exception>
    internal S57ConnectedNode(S57VectorRecord source) : base(source)
    {
        if (source.Coordinates2D.IsDefaultOrEmpty)
        {
            throw new InvalidOperationException(
                $"Connected node {source.RecordName} must have at least one coordinate.");
        }

        Position = source.Coordinates2D[0];
    }
}
