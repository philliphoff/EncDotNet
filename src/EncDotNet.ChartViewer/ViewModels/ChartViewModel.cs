using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using EncDotNet.ChartViewer.Charts;
using EncDotNet.ChartViewer.Models;
using EncDotNet.S57;
using EncDotNet.S57.Charts;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using NetTopologySuite.Geometries;
using ReactiveUI;

namespace EncDotNet.ChartViewer.ViewModels;

/// <summary>
/// ViewModel for a chart in the chart browser panel.
/// </summary>
public sealed class ChartViewModel : ViewModelBase
{
    /// <summary>Gets the chart index entry.</summary>
    public ChartIndexEntry Entry { get; }

    /// <summary>Gets the display name.</summary>
    public string Name => Entry.Name;

    /// <summary>Gets or sets the compilation scale (CSCL) of this chart, set after loading.</summary>
    public int CompilationScale { get; set; }

    /// <summary>Gets the full resolved path to the chart file.</summary>
    public string FullPath => Path.Combine(AppDataPaths.ExpandedDirectory, Entry.Path);

    /// <summary>Command to copy the chart's full path to the clipboard.</summary>
    public ICommand CopyPathCommand { get; }

    // ── Layer cache ──────────────────────────────────────────────────

    /// <summary>
    /// Per-object-code cache of generated layers. Persists across load/unload
    /// cycles so re-entering the viewport is instant when settings haven't changed.
    /// </summary>
    private readonly Dictionary<S57ObjectCode, CachedLayerEntry> _layerCache = new();

    /// <summary>The active (visible-to-map) layers, ordered by render order.</summary>
    private ImmutableArray<MemoryLayer> _activeLayers = [];

    /// <summary>Gets the currently active layers for this chart, ordered by render order.</summary>
    public IReadOnlyList<MemoryLayer> Layers => _activeLayers;

    /// <summary>
    /// Tracks a generated layer along with the settings that produced it,
    /// so we can determine whether it needs regeneration.
    /// </summary>
    private sealed record CachedLayerEntry(
        MemoryLayer Layer,
        DepthUnit DepthUnit);

    // ── Coverage geometry ────────────────────────────────────────────

    /// <summary>
    /// Gets the projected coverage geometry (M_COVR CATCOV=1) for this chart,
    /// or <c>null</c> if the chart has no coverage areas. Computed once on first
    /// layer build and cached for the lifetime of the chart.
    /// </summary>
    public Geometry? CoverageGeometry { get; private set; }

    private bool _coverageGeometryComputed;

    /// <summary>
    /// Pre-computed Mercator-projected bounds for the chart, or null if the chart has no geographic bounds.
    /// Computed once in the constructor to avoid repeated trigonometry during viewport evaluation.
    /// </summary>
    public MRect? ProjectedBounds { get; }

    // ── Constructor ──────────────────────────────────────────────────

    public ChartViewModel(ChartIndexEntry entry)
    {
        Entry = entry;

        if (entry.SouthLatitude is { } south
            && entry.NorthLatitude is { } north
            && entry.WestLongitude is { } west
            && entry.EastLongitude is { } east)
        {
            var (minX, minY) = SphericalMercator.FromLonLat(west, south);
            var (maxX, maxY) = SphericalMercator.FromLonLat(east, north);
            ProjectedBounds = new MRect(minX, minY, maxX, maxY);
        }

        CopyPathCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
            {
                await window.Clipboard!.SetTextAsync(FullPath);
            }
        });
    }

    // ── Layer pipeline ───────────────────────────────────────────────

    /// <summary>
    /// Builds (or returns cached) layers for this chart given the current settings.
    /// Layers are cached per object code; only layers affected by settings changes
    /// are regenerated. Safe to call from a background thread.
    /// </summary>
    /// <param name="chart">The parsed S-57 chart data.</param>
    /// <param name="settings">Snapshot of current layer generation settings.</param>
    /// <returns>A <see cref="LayerUpdate"/> describing the active layers and the diff from the previous state.</returns>
    public Task<LayerUpdate> BuildLayersAsync(S57Chart chart, ChartLayerSettings settings)
    {
        return Task.Run(() => BuildLayersCore(chart, settings));
    }

    private LayerUpdate BuildLayersCore(S57Chart chart, ChartLayerSettings settings)
    {
        var tags = new KeyValuePair<string, object?>("chart.name", Name);
        ChartViewerDiagnostics.ChartsLoading.Add(1, tags);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Ensure coverage geometry is computed (once, ever).
            if (!_coverageGeometryComputed)
            {
                CoverageGeometry = S57CoverageHelper.BuildCoverageGeometry(chart);
                _coverageGeometryComputed = true;
            }

            CompilationScale = chart.CompilationScale;

            ChartViewerDiagnostics.ChartLoadDuration.Record(stopwatch.Elapsed.TotalMilliseconds, tags);

            var previousActive = _activeLayers;
            var previousSet = new HashSet<MemoryLayer>(previousActive);

            // ── Phase 1: Ensure cache entries exist for all requested codes ──

            var layerStopwatch = Stopwatch.StartNew();

            foreach (var code in settings.RequestedObjectCodes)
            {
                // Skip codes not present in this chart.
                if (!chart.FeaturesByObjectCode.ContainsKey(code))
                    continue;

                if (_layerCache.TryGetValue(code, out var cached) && !NeedsRegeneration(code, cached, settings))
                    continue; // Cache hit — still valid.

                // Generate (or regenerate) the layer.
                var singleLayerStopwatch = Stopwatch.StartNew();

                var layer = S57LayerFactory.CreateLayerForObjectCodes(
                    chart,
                    ImmutableArray.Create(code),
                    code.ToString(),
                    settings.DepthUnit,
                    settings.ChartName);

                singleLayerStopwatch.Stop();

                if (layer is not null)
                {
                    _layerCache[code] = new CachedLayerEntry(layer, settings.DepthUnit);

                    var layerTags = new TagList
                    {
                        { "chart.name", Name },
                        { "object.code", code.ToString() },
                    };
                    ChartViewerDiagnostics.LayersCreated.Add(1, layerTags);
                    ChartViewerDiagnostics.FeaturesPerLayer.Record(layer.Features.Count(), layerTags);
                    ChartViewerDiagnostics.SingleLayerCreationDuration.Record(
                        singleLayerStopwatch.Elapsed.TotalMilliseconds, layerTags);
                }
                else
                {
                    _layerCache.Remove(code);
                }
            }

            layerStopwatch.Stop();
            ChartViewerDiagnostics.LayerCreationDuration.Record(layerStopwatch.Elapsed.TotalMilliseconds, tags);

            // ── Phase 2: Build active layer list from cache ──

            var active = new List<MemoryLayer>();
            foreach (var code in settings.RequestedObjectCodes)
            {
                if (_layerCache.TryGetValue(code, out var entry))
                    active.Add(entry.Layer);
            }

            // Sort by render order so that background areas (land, water, depth)
            // are drawn first and point features (buoys, beacons) are drawn on top.
            active.Sort((a, b) =>
            {
                var orderA = Enum.TryParse<S57ObjectCode>(a.Name, out var codeA)
                    ? S57LayerTemplates.GetRenderOrder(codeA) : int.MaxValue;
                var orderB = Enum.TryParse<S57ObjectCode>(b.Name, out var codeB)
                    ? S57LayerTemplates.GetRenderOrder(codeB) : int.MaxValue;
                return orderA.CompareTo(orderB);
            });

            var newActive = active.ToImmutableArray();

            // ── Phase 3: Compute diff ──

            var newSet = new HashSet<MemoryLayer>(newActive);

            var added = newActive.Where(l => !previousSet.Contains(l)).ToList();
            var removed = previousActive.Where(l => !newSet.Contains(l)).ToList();

            _activeLayers = newActive;

            Debug.WriteLine($"Built layers for chart: {Name} " +
                $"(active={newActive.Length}, added={added.Count}, removed={removed.Count}, cached={_layerCache.Count})");

            return new LayerUpdate(newActive, added, removed);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error building chart {Name}: {ex.Message}");
            ChartViewerDiagnostics.ChartLoadErrors.Add(1, tags);
            return new LayerUpdate([], [], []);
        }
        finally
        {
            ChartViewerDiagnostics.ChartsLoading.Add(-1, tags);
        }
    }

    /// <summary>
    /// Determines whether a cached layer needs regeneration given new settings.
    /// </summary>
    private static bool NeedsRegeneration(
        S57ObjectCode code,
        CachedLayerEntry cached,
        ChartLayerSettings settings)
    {
        // Only SOUNDG content is affected by depth unit changes.
        if (code == S57ObjectCode.SOUNDG && cached.DepthUnit != settings.DepthUnit)
            return true;

        // All other layers are pure functions of chart data — never stale.
        return false;
    }

    /// <summary>
    /// Clears the layer cache entirely. Called when chart data itself changes
    /// (e.g. catalog reload) or when resetting all data.
    /// </summary>
    public void ClearLayerCache()
    {
        _layerCache.Clear();
        _activeLayers = [];
        _coverageGeometryComputed = false;
        CoverageGeometry = null;
    }

    // ── Exclusion zone clipping ─────────────────────────────────────

    /// <summary>
    /// The exclusion zone geometry most recently applied to active layers,
    /// or <c>null</c> if no clipping has been applied.
    /// </summary>
    private Geometry? _appliedExclusionZone;

    /// <summary>
    /// Object codes whose area fills should be clipped by exclusion zones.
    /// These are the primary area-fill codes that cause visual bleed-through
    /// when coarse chart polygons extend beyond their actual coastline at
    /// cell boundary edges.
    /// </summary>
    private static readonly HashSet<S57ObjectCode> ClippableAreaCodes = new()
    {
        S57ObjectCode.LNDARE,
        S57ObjectCode.DEPARE,
        S57ObjectCode.SEAARE,
    };

    /// <summary>
    /// Applies exclusion zone clipping to this chart's area layers, creating
    /// clipped copies of area features that intersect the exclusion zone.
    /// The base (unclipped) layers remain in the cache; only the active layers
    /// are replaced with clipped versions.
    /// </summary>
    /// <param name="exclusionZone">
    /// The combined coverage geometry of all loaded charts with finer (lower)
    /// compilation scales, or <c>null</c> to remove any previous clipping.
    /// </param>
    /// <returns>A <see cref="LayerUpdate"/> describing the changes to apply to the map.</returns>
    public LayerUpdate ApplyExclusionZone(Geometry? exclusionZone)
    {
        // If the exclusion zone hasn't changed, no work needed.
        if (ReferenceEquals(exclusionZone, _appliedExclusionZone))
            return new LayerUpdate(_activeLayers, [], []);

        _appliedExclusionZone = exclusionZone;

        var previousActive = _activeLayers;

        // Rebuild active layers from the unclipped cache, applying clipping.
        var active = new List<MemoryLayer>();
        foreach (var (code, entry) in _layerCache)
        {
            if (exclusionZone != null && !exclusionZone.IsEmpty && ClippableAreaCodes.Contains(code))
            {
                var clipped = ClipAreaLayer(entry.Layer, exclusionZone);
                active.Add(clipped ?? entry.Layer);
            }
            else
            {
                active.Add(entry.Layer);
            }
        }

        // Re-sort by render order (same as BuildLayersCore Phase 2).
        active.Sort((a, b) =>
        {
            var orderA = Enum.TryParse<S57ObjectCode>(a.Name, out var codeA)
                ? S57LayerTemplates.GetRenderOrder(codeA) : int.MaxValue;
            var orderB = Enum.TryParse<S57ObjectCode>(b.Name, out var codeB)
                ? S57LayerTemplates.GetRenderOrder(codeB) : int.MaxValue;
            return orderA.CompareTo(orderB);
        });

        var newActive = active.ToImmutableArray();
        _activeLayers = newActive;

        // Compute diff.
        var previousSet = new HashSet<MemoryLayer>(previousActive);
        var newSet = new HashSet<MemoryLayer>(newActive);
        var added = newActive.Where(l => !previousSet.Contains(l)).ToList();
        var removed = previousActive.Where(l => !newSet.Contains(l)).ToList();

        return new LayerUpdate(newActive, added, removed);
    }

    /// <summary>
    /// Creates a clipped copy of a MemoryLayer by computing
    /// <c>Geometry.Difference(exclusionZone)</c> for each polygon feature.
    /// Returns <c>null</c> if no features were clipped (the original layer
    /// can be reused as-is).
    /// </summary>
    private static MemoryLayer? ClipAreaLayer(MemoryLayer layer, Geometry exclusionZone)
    {
        var prepared = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(exclusionZone);
        var clippedFeatures = new List<IFeature>();
        bool anyClipped = false;

        foreach (var feature in layer.Features)
        {
            if (feature is GeometryFeature gf && gf.Geometry is Polygon or MultiPolygon)
            {
                if (prepared.Intersects(gf.Geometry))
                {
                    try
                    {
                        var diff = gf.Geometry.Difference(exclusionZone);
                        if (diff != null && !diff.IsEmpty)
                        {
                            var clippedFeature = new GeometryFeature(diff);
                            foreach (var style in gf.Styles)
                                clippedFeature.Styles.Add(style);
                            foreach (var field in gf.Fields)
                                clippedFeature[field] = gf[field];
                            clippedFeatures.Add(clippedFeature);
                            anyClipped = true;
                        }
                        // Feature entirely within exclusion zone — drop it.
                    }
                    catch
                    {
                        clippedFeatures.Add(feature); // Clipping failed — keep original.
                    }
                }
                else
                {
                    clippedFeatures.Add(feature); // No intersection — keep as-is.
                }
            }
            else
            {
                clippedFeatures.Add(feature); // Non-polygon (outline lines, labels) — keep.
            }
        }

        if (!anyClipped)
            return null;

        return new MemoryLayer
        {
            Name = layer.Name,
            Features = clippedFeatures,
            Style = layer.Style,
            MinVisible = layer.MinVisible,
            MaxVisible = layer.MaxVisible,
        };
    }
}
