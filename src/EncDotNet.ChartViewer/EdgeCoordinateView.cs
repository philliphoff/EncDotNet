using NetTopologySuite.Geometries;

namespace EncDotNet.ChartViewer;

/// <summary>
/// A zero-allocation, read-only view over a cached <see cref="Coordinate"/> array
/// that supports reverse orientation and end-node exclusion without copying.
/// </summary>
internal readonly struct EdgeCoordinateView
{
    private readonly Coordinate[] _array;
    private readonly bool _reverse;
    private readonly int _count;

    public EdgeCoordinateView(Coordinate[] array, bool reverse, bool excludeEndNode)
    {
        _array = array;
        _reverse = reverse;
        _count = Math.Max(0, array.Length - (excludeEndNode ? 1 : 0));
    }

    public int Count => _count;

    public Coordinate this[int index] => _reverse
        ? _array[_count - 1 - index]
        : _array[index];

    /// <summary>
    /// Appends coordinates to the target list, starting from <paramref name="startIndex"/>.
    /// </summary>
    public void CopyTo(List<Coordinate> target, int startIndex = 0)
    {
        for (int i = startIndex; i < _count; i++)
            target.Add(this[i]);
    }
}
