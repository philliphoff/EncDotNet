namespace EncDotNet.S57;

/// <summary>
/// S-57 Construction surface (SURF): the surface an <see cref="S57ArcCurve"/> is constructed on
/// (S-57 Part 3, clause 7.7.1.8).
/// </summary>
public enum S57ConstructionSurface : byte
{
    /// <summary>
    /// Ellipsoidal (<c>E</c>): the arc or curve is constructed before it is projected onto a
    /// 2-D surface.
    /// </summary>
    Ellipsoidal = 1,

    /// <summary>
    /// Planar (<c>P</c>): the arc or curve is constructed after it is projected onto a 2-D
    /// surface, whatever the projection.
    /// </summary>
    Planar = 2
}
