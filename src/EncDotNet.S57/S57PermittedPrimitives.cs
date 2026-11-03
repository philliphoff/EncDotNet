namespace EncDotNet.S57;

/// <summary>
/// The geometric primitives an object class may be encoded with.
/// </summary>
/// <remarks>
/// S-57 Edition 3.1, Appendix B.1 (ENC Product Specification), Annex A lists the primitives each
/// object class may use.
/// The value is <see cref="None"/> for classes that have no geometry, such as collection objects,
/// and for classes whose primitives the catalogue doesn't give.
/// </remarks>
[Flags]
public enum S57PermittedPrimitives
{
    /// <summary>No geometric primitive is permitted, or the catalogue doesn't give one.</summary>
    None = 0,

    /// <summary>The class may be encoded as a point.</summary>
    Point = 1,

    /// <summary>The class may be encoded as a line.</summary>
    Line = 2,

    /// <summary>The class may be encoded as an area.</summary>
    Area = 4,
}
