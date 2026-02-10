using System.Collections.Immutable;

namespace EncDotNet.ChartViewer.Models;

/// <summary>
/// Represents a category of S-57 chart features that can be toggled on/off.
/// </summary>
public sealed class S57FeatureCategory
{
    /// <summary>Gets the display name of the category.</summary>
    public string Name { get; }

    /// <summary>Gets the S-57 object codes included in this category.</summary>
    public ImmutableArray<int> ObjectCodes { get; }

    /// <summary>Gets whether this category is enabled by default.</summary>
    public bool DefaultEnabled { get; }

    private S57FeatureCategory(string name, bool defaultEnabled, params int[] objectCodes)
    {
        Name = name;
        DefaultEnabled = defaultEnabled;
        ObjectCodes = [..objectCodes];
    }

    // S-57 object class codes
    // See IHO S-57 Appendix A - Object Catalogue

    /// <summary>Coastline (COALNE, object code 30).</summary>
    public static readonly S57FeatureCategory Coastline = new("Coastline", true, 30);

    /// <summary>Depth contours (DEPCNT, object code 43).</summary>
    public static readonly S57FeatureCategory DepthContours = new("Depth Contours", true, 43);

    /// <summary>Depth areas (DEPARE, object code 42).</summary>
    public static readonly S57FeatureCategory DepthAreas = new("Depth Areas", false, 42);

    /// <summary>Land area (LNDARE, object code 71).</summary>
    public static readonly S57FeatureCategory LandArea = new("Land Area", false, 71);

    /// <summary>Soundings (SOUNDG, object code 129).</summary>
    public static readonly S57FeatureCategory Soundings = new("Soundings", false, 129);

    /// <summary>Shoreline construction (SLCONS, object code 122).</summary>
    public static readonly S57FeatureCategory ShorelineConstruction = new("Shoreline Construction", true, 122);

    /// <summary>Lights (LIGHTS, object code 75).</summary>
    public static readonly S57FeatureCategory Lights = new("Lights", false, 75);

    /// <summary>Buoys - lateral, cardinal, isolated danger, safe water, special purpose (BOYLAT 17, BOYCAR 14, BOYISD 15, BOYSAW 16, BOYSPP 18).</summary>
    public static readonly S57FeatureCategory Buoys = new("Buoys", false, 17, 14, 15, 16, 18);

    /// <summary>Beacons - lateral, cardinal, isolated danger, safe water, special purpose (BCNLAT 6, BCNCAR 3, BCNISD 4, BCNSAW 5, BCNSPP 7).</summary>
    public static readonly S57FeatureCategory Beacons = new("Beacons", false, 6, 3, 4, 5, 7);

    /// <summary>Wrecks (WRECKS, object code 159).</summary>
    public static readonly S57FeatureCategory Wrecks = new("Wrecks", false, 159);

    /// <summary>Obstructions (OBSTRN, object code 86).</summary>
    public static readonly S57FeatureCategory Obstructions = new("Obstructions", false, 86);

    /// <summary>Underwater rocks (UWTROC, object code 153).</summary>
    public static readonly S57FeatureCategory UnderwaterRocks = new("Underwater Rocks", false, 153);

    /// <summary>Sea area (SEAARE, object code 112).</summary>
    public static readonly S57FeatureCategory SeaArea = new("Sea Area", false, 112);

    /// <summary>Built-up area (BUAARE, object code 25).</summary>
    public static readonly S57FeatureCategory BuiltUpArea = new("Built-up Area", false, 25);

    /// <summary>All predefined feature categories.</summary>
    public static readonly ImmutableArray<S57FeatureCategory> All =
    [
        Coastline,
        DepthContours,
        DepthAreas,
        LandArea,
        Soundings,
        ShorelineConstruction,
        Lights,
        Buoys,
        Beacons,
        Wrecks,
        Obstructions,
        UnderwaterRocks,
        SeaArea,
        BuiltUpArea,
    ];
}
