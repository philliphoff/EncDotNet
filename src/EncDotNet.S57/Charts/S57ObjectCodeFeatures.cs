using System.Collections.Immutable;

namespace EncDotNet.S57.Charts;

/// <summary>
/// Groups all features sharing the same S-57 object code, subdivided by geometric primitive.
/// </summary>
/// <remarks>
/// A single object code (e.g. LNDARE) can have features with different geometric primitives —
/// area features for the land mass itself and point features for areas too small to render as polygons.
/// This type provides access to all primitives for a given object code in one place.
/// </remarks>
public sealed record S57ObjectCodeFeatures
{
    /// <summary>Gets the object code shared by all features in this group.</summary>
    public S57ObjectCode ObjectCode { get; }

    /// <summary>Gets the point features for this object code.</summary>
    public IReadOnlyList<S57PointFeature> Points { get; }

    /// <summary>Gets the line features for this object code.</summary>
    public IReadOnlyList<S57LineFeature> Lines { get; }

    /// <summary>Gets the area features for this object code.</summary>
    public IReadOnlyList<S57AreaFeature> Areas { get; }

    /// <summary>Gets the meta features (no geometry) for this object code.</summary>
    public IReadOnlyList<S57MetaFeature> Meta { get; }

    internal S57ObjectCodeFeatures(
        S57ObjectCode objectCode,
        ImmutableArray<S57PointFeature>.Builder? points,
        ImmutableArray<S57LineFeature>.Builder? lines,
        ImmutableArray<S57AreaFeature>.Builder? areas,
        ImmutableArray<S57MetaFeature>.Builder? meta)
    {
        ObjectCode = objectCode;
        Points = points?.ToImmutable() ?? [];
        Lines = lines?.ToImmutable() ?? [];
        Areas = areas?.ToImmutable() ?? [];
        Meta = meta?.ToImmutable() ?? [];
    }
}
