using EncDotNet.S57.Charts;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Operation.Union;

namespace EncDotNet.ChartViewer;

/// <summary>
/// Extracts and unions M_COVR coverage polygons from S-57 charts for use in
/// clipping lower-scale chart features where higher-scale coverage exists.
/// </summary>
internal static class S57CoverageHelper
{
    /// <summary>
    /// Builds the projected (Spherical Mercator) coverage polygon for a chart by
    /// converting its M_COVR CATCOV=1 area features to NTS geometry and unioning them.
    /// Returns <c>null</c> if the chart has no coverage areas or geometry cannot be built.
    /// </summary>
    public static Geometry? BuildCoverageGeometry(S57Chart chart)
    {
        if (chart.CoverageAreas.Count == 0)
            return null;

        var polygons = new List<Geometry>();
        foreach (var coverageArea in chart.CoverageAreas)
        {
            var geom = S57AreaGeometryBuilder.CreatePolygonFromAreaFeature(chart, coverageArea);
            if (geom != null && !geom.IsEmpty)
                polygons.Add(geom);
        }

        if (polygons.Count == 0)
            return null;

        if (polygons.Count == 1)
            return polygons[0];

        return CascadedPolygonUnion.Union(polygons);
    }

    /// <summary>
    /// Computes the combined exclusion zone for a chart by unioning the coverage
    /// geometries of all loaded charts that have a smaller compilation scale number
    /// (i.e. are more detailed / higher scale).
    /// </summary>
    /// <param name="compilationScale">The compilation scale of the chart being clipped.</param>
    /// <param name="loadedChartCoverages">
    /// Pairs of (compilation scale, coverage geometry) for all currently loaded charts.
    /// </param>
    /// <returns>
    /// A tuple of the raw exclusion geometry and its prepared form, or <c>null</c> if
    /// there is no higher-scale coverage to exclude.
    /// </returns>
    public static (Geometry Zone, IPreparedGeometry Prepared)? ComputeExclusionZone(
        int compilationScale,
        IEnumerable<(int CompilationScale, Geometry? CoverageGeometry)> loadedChartCoverages)
    {
        var higherScalePolygons = new List<Geometry>();

        foreach (var (cscl, coverage) in loadedChartCoverages)
        {
            // Lower CSCL number = more detailed = takes precedence
            if (cscl > 0 && cscl < compilationScale && coverage != null && !coverage.IsEmpty)
                higherScalePolygons.Add(coverage);
        }

        if (higherScalePolygons.Count == 0)
            return null;

        var zone = higherScalePolygons.Count == 1
            ? higherScalePolygons[0]
            : CascadedPolygonUnion.Union(higherScalePolygons);

        if (zone == null || zone.IsEmpty)
            return null;

        return (zone, PreparedGeometryFactory.Prepare(zone));
    }
}
