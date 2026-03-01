using System.Collections.Immutable;

namespace EncDotNet.S57.Charts;

/// <summary>
/// Represents an isolated node in an S-57 chart.
/// </summary>
/// <remarks>
/// <para>
/// Isolated nodes (RCNM=110) are point spatial objects that are not connected to edges.
/// They are used to represent either a single geographic position or sounding points.
/// </para>
/// <para>
/// An isolated node contains either a 2D position (<see cref="Position"/>) or 3D soundings 
/// (<see cref="Soundings"/>), but not both.
/// </para>
/// </remarks>
public sealed record S57IsolatedNode : S57SpatialRecord
{
    /// <summary>
    /// Gets the position of this node, if it represents a single point.
    /// </summary>
    /// <remarks>
    /// This property is <c>null</c> when the node represents soundings instead of a single position.
    /// Use <see cref="HasSoundings"/> to determine if this node has sounding data.
    /// </remarks>
    public S57Coordinate2D? Position { get; }

    /// <summary>
    /// Gets the sounding values if this node represents sounding points.
    /// </summary>
    /// <remarks>
    /// Soundings are 3D coordinates (X, Y, depth) used for bathymetric data.
    /// When present, <see cref="Position"/> will be <c>null</c>.
    /// </remarks>
    public IReadOnlyList<S57Sounding> Soundings { get; }

    /// <summary>
    /// Gets a value indicating whether this node contains sounding data.
    /// </summary>
    public bool HasSoundings => Soundings.Count > 0;

    /// <summary>
    /// Gets a value indicating whether this node contains a single position.
    /// </summary>
    public bool HasPosition => Position.HasValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="S57IsolatedNode"/> class.
    /// </summary>
    /// <param name="source">The source vector record.</param>
    internal S57IsolatedNode(S57VectorRecord source) : base(source)
    {
        Soundings = source.Soundings;

        // Position is the first coordinate if soundings are not present
        if (source.Soundings.Count > 0)
        {
            Position = null;
        }
        else if (source.Coordinates2D.Count > 0)
        {
            Position = source.Coordinates2D[0];
        }
        else
        {
            Position = null;
        }
    }
}
