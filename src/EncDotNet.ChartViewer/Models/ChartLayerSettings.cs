using System.Collections.Immutable;
using EncDotNet.S57;

namespace EncDotNet.ChartViewer.Models;

/// <summary>
/// Immutable snapshot of settings that affect chart layer generation.
/// Two instances with the same values produce identical layer content.
/// </summary>
/// <param name="DepthUnit">The unit for displaying sounding depths (affects SOUNDG layers).</param>
/// <param name="RequestedObjectCodes">The set of currently-enabled S-57 object codes to include as active layers.</param>
/// <param name="ChartName">Display name of the chart (baked into default-rendered point features).</param>
public sealed record ChartLayerSettings(
    DepthUnit DepthUnit,
    ImmutableHashSet<S57ObjectCode> RequestedObjectCodes,
    string ChartName);

