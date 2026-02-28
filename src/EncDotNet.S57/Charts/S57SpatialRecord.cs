using System.Collections.Immutable;

namespace EncDotNet.S57.Charts;

/// <summary>
/// Base class for all strongly-typed spatial records in an S-57 chart.
/// </summary>
public abstract class S57SpatialRecord
{
    /// <summary>Gets the record name that uniquely identifies this spatial object.</summary>
    public S57RecordName RecordName { get; }

    /// <summary>Gets the record version.</summary>
    public int RecordVersion { get; }

    /// <summary>Gets the update instruction for this record.</summary>
    public S57UpdateInstruction UpdateInstruction { get; }

    /// <summary>Gets the attributes associated with this spatial object.</summary>
    public ImmutableArray<S57AttributeValue> Attributes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="S57SpatialRecord"/> class from a generic vector record.
    /// </summary>
    protected S57SpatialRecord(S57VectorRecord source)
    {
        ArgumentNullException.ThrowIfNull(source);

        RecordName = source.RecordName;
        RecordVersion = source.RecordVersion;
        UpdateInstruction = source.UpdateInstruction;
        Attributes = source.Attributes;
    }

    /// <summary>
    /// Creates the appropriate strongly-typed spatial record from a generic vector record.
    /// </summary>
    /// <param name="source">The source vector record.</param>
    /// <returns>A strongly-typed spatial record.</returns>
    /// <exception cref="ArgumentException">Thrown when the record type is not recognized.</exception>
    public static S57SpatialRecord Create(S57VectorRecord source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.RecordName.RecordNameCode switch
        {
            S57RecordNameCodes.IsolatedNode => new S57IsolatedNode(source),
            S57RecordNameCodes.ConnectedNode => new S57ConnectedNode(source),
            S57RecordNameCodes.Edge => new S57Edge(source),
            S57RecordNameCodes.Face => new S57Face(source),
            _ => throw new ArgumentException(
                $"Unknown spatial record type: {source.RecordName.RecordNameCode}",
                nameof(source))
        };
    }
}
