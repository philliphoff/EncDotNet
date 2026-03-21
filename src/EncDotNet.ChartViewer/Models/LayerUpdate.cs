using System.Collections.Generic;
using Mapsui.Layers;

namespace EncDotNet.ChartViewer.Models;

/// <summary>
/// Describes the result of a <see cref="ViewModels.ChartViewModel.BuildLayersAsync"/> call,
/// including the full set of active layers and the delta from the previous state.
/// </summary>
/// <param name="ActiveLayers">All layers matching the current settings, ordered by render order.</param>
/// <param name="Added">Layers that were not in the previous active set and need to be inserted into the map.</param>
/// <param name="Removed">Layers that were in the previous active set and need to be removed from the map.</param>
public sealed record LayerUpdate(
    IReadOnlyList<MemoryLayer> ActiveLayers,
    IReadOnlyList<MemoryLayer> Added,
    IReadOnlyList<MemoryLayer> Removed);

