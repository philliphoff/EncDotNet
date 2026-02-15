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

    // --- Hydrographic ---

    /// <summary>Coastline (COALNE, object code 30).</summary>
    public static readonly S57FeatureCategory Coastline = new("Coastline", true, 30);

    /// <summary>Depth contours (DEPCNT, object code 43).</summary>
    public static readonly S57FeatureCategory DepthContours = new("Depth Contours", true, 43);

    /// <summary>Depth areas (DEPARE, object code 42).</summary>
    public static readonly S57FeatureCategory DepthAreas = new("Depth Areas", false, 42);

    /// <summary>Soundings (SOUNDG, object code 129).</summary>
    public static readonly S57FeatureCategory Soundings = new("Soundings", false, 129);

    /// <summary>Sea area (SEAARE, object code 112).</summary>
    public static readonly S57FeatureCategory SeaArea = new("Sea Area", false, 112);

    /// <summary>Dredged area (DRGARE, object code 47).</summary>
    public static readonly S57FeatureCategory DredgedArea = new("Dredged Area", false, 47);

    /// <summary>Lake (LAKARE, object code 69).</summary>
    public static readonly S57FeatureCategory Lake = new("Lake", false, 69);

    /// <summary>River (RIVERS, object code 114).</summary>
    public static readonly S57FeatureCategory River = new("River", false, 114);

    /// <summary>Canal (CANALS, object code 23).</summary>
    public static readonly S57FeatureCategory Canal = new("Canal", false, 23);

    /// <summary>Unsurveyed area (UNSARE, object code 154).</summary>
    public static readonly S57FeatureCategory UnsurveyedArea = new("Unsurveyed Area", false, 154);

    /// <summary>Magnetic variation (MAGVAR, object code 77).</summary>
    public static readonly S57FeatureCategory MagneticVariation = new("Magnetic Variation", false, 77);

    // --- Land ---

    /// <summary>Land area (LNDARE, object code 71).</summary>
    public static readonly S57FeatureCategory LandArea = new("Land Area", false, 71);

    /// <summary>Built-up area (BUAARE, object code 25).</summary>
    public static readonly S57FeatureCategory BuiltUpArea = new("Built-up Area", false, 25);

    /// <summary>Landmark (LNDMRK, object code 74).</summary>
    public static readonly S57FeatureCategory Landmarks = new("Landmarks", false, 74);

    // --- Structures ---

    /// <summary>Shoreline construction (SLCONS, object code 122).</summary>
    public static readonly S57FeatureCategory ShorelineConstruction = new("Shoreline Construction", true, 122);

    /// <summary>Bridge (BRIDGE, object code 12).</summary>
    public static readonly S57FeatureCategory Bridges = new("Bridges", false, 12);

    /// <summary>Mooring/warping facility (MORFAC, object code 84).</summary>
    public static readonly S57FeatureCategory MooringFacilities = new("Mooring Facilities", false, 84);

    /// <summary>Offshore platform (OFSPLF, object code 87).</summary>
    public static readonly S57FeatureCategory OffshorePlatforms = new("Offshore Platforms", false, 87);

    /// <summary>Pontoon (PONTON, object code 95).</summary>
    public static readonly S57FeatureCategory Pontoons = new("Pontoons", false, 95);

    /// <summary>Dock area (DOCARE, object code 45).</summary>
    public static readonly S57FeatureCategory DockArea = new("Dock Area", false, 45);

    // --- Navigation aids ---

    /// <summary>Lights (LIGHTS, object code 75).</summary>
    public static readonly S57FeatureCategory Lights = new("Lights", false, 75);

    /// <summary>Buoys - lateral, cardinal, isolated danger, safe water, special purpose (BOYLAT 17, BOYCAR 14, BOYISD 15, BOYSAW 16, BOYSPP 18).</summary>
    public static readonly S57FeatureCategory Buoys = new("Buoys", false, 17, 14, 15, 16, 18);

    /// <summary>Beacons - lateral, cardinal, isolated danger, safe water, special purpose (BCNLAT 6, BCNCAR 3, BCNISD 4, BCNSAW 5, BCNSPP 7).</summary>
    public static readonly S57FeatureCategory Beacons = new("Beacons", false, 6, 3, 4, 5, 7);

    /// <summary>Fog signal (FOGSIG, object code 59).</summary>
    public static readonly S57FeatureCategory FogSignals = new("Fog Signals", false, 59);

    /// <summary>Radar station (RADSTA, object code 100).</summary>
    public static readonly S57FeatureCategory RadarStations = new("Radar Stations", false, 100);

    /// <summary>Radio calling-in point (RDOCAL, object code 97).</summary>
    public static readonly S57FeatureCategory RadioCallingInPoints = new("Radio Calling-in Points", false, 97);

    /// <summary>Pilot boarding place (PILBOP, object code 90).</summary>
    public static readonly S57FeatureCategory PilotBoardingPlaces = new("Pilot Boarding Places", false, 90);

    // --- Hazards ---

    /// <summary>Wrecks (WRECKS, object code 159).</summary>
    public static readonly S57FeatureCategory Wrecks = new("Wrecks", false, 159);

    /// <summary>Obstructions (OBSTRN, object code 86).</summary>
    public static readonly S57FeatureCategory Obstructions = new("Obstructions", false, 86);

    /// <summary>Underwater rocks (UWTROC, object code 153).</summary>
    public static readonly S57FeatureCategory UnderwaterRocks = new("Underwater Rocks", false, 153);

    // --- Navigation routing ---

    /// <summary>Traffic separation scheme - lane part, roundabout, crossing, boundary, zone, separation line (TSSLPT 145, TSSRON 146, TSSCRS 148, TSSBND 144, TSEZNE 147, TSELNE 143).</summary>
    public static readonly S57FeatureCategory TrafficSeparationScheme = new("Traffic Separation Scheme", false, 145, 146, 148, 144, 147, 143, 149, 150);

    /// <summary>Recommended track (RECTRC, object code 96).</summary>
    public static readonly S57FeatureCategory RecommendedTrack = new("Recommended Track", false, 96);

    /// <summary>Navigation line (NAVLNE, object code 85).</summary>
    public static readonly S57FeatureCategory NavigationLine = new("Navigation Line", false, 85);

    /// <summary>Fairway (FAIRWY, object code 57).</summary>
    public static readonly S57FeatureCategory Fairway = new("Fairway", false, 57);

    /// <summary>Ferry route (FERYRT, object code 58).</summary>
    public static readonly S57FeatureCategory FerryRoute = new("Ferry Route", false, 58);

    // --- Regulated/restricted areas ---

    /// <summary>Anchorage area (ACHARE, object code 2).</summary>
    public static readonly S57FeatureCategory AnchorageArea = new("Anchorage Area", false, 2);

    /// <summary>Restricted area (RESARE, object code 112).</summary>
    public static readonly S57FeatureCategory RestrictedArea = new("Restricted Area", false, 120);

    /// <summary>Dumping ground (DMPGRD, object code 46).</summary>
    public static readonly S57FeatureCategory DumpingGround = new("Dumping Ground", false, 46);

    /// <summary>Military practice area (MIPARE, object code 83).</summary>
    public static readonly S57FeatureCategory MilitaryPracticeArea = new("Military Practice Area", false, 83);

    /// <summary>Caution area (CTNARE, object code 40).</summary>
    public static readonly S57FeatureCategory CautionArea = new("Caution Area", false, 40);

    // --- Cables & pipelines ---

    /// <summary>Cable area (CBLARE, object code 27).</summary>
    public static readonly S57FeatureCategory CableArea = new("Cable Area", false, 27);

    /// <summary>Cable overhead (CBLOHD, object code 28).</summary>
    public static readonly S57FeatureCategory CableOverhead = new("Cable Overhead", false, 28);

    /// <summary>Cable submarine (CBLSUB, object code 29).</summary>
    public static readonly S57FeatureCategory CableSubmarine = new("Cable Submarine", false, 29);

    /// <summary>Pipeline area (PIPARE, object code 92).</summary>
    public static readonly S57FeatureCategory PipelineArea = new("Pipeline Area", false, 92);

    /// <summary>Pipeline submarine/on land (PIPSOL, object code 93).</summary>
    public static readonly S57FeatureCategory PipelineSubmarineOnLand = new("Pipeline Submarine/On Land", false, 93);

    /// <summary>All predefined feature categories.</summary>
    public static readonly ImmutableArray<S57FeatureCategory> All =
    [
        // Hydrographic
        Coastline,
        DepthContours,
        DepthAreas,
        Soundings,
        SeaArea,
        DredgedArea,
        Lake,
        River,
        Canal,
        UnsurveyedArea,
        MagneticVariation,
        // Land
        LandArea,
        BuiltUpArea,
        Landmarks,
        // Structures
        ShorelineConstruction,
        Bridges,
        MooringFacilities,
        OffshorePlatforms,
        Pontoons,
        DockArea,
        // Navigation aids
        Lights,
        Buoys,
        Beacons,
        FogSignals,
        RadarStations,
        RadioCallingInPoints,
        PilotBoardingPlaces,
        // Hazards
        Wrecks,
        Obstructions,
        UnderwaterRocks,
        // Navigation routing
        TrafficSeparationScheme,
        RecommendedTrack,
        NavigationLine,
        Fairway,
        FerryRoute,
        // Regulated/restricted areas
        AnchorageArea,
        RestrictedArea,
        DumpingGround,
        MilitaryPracticeArea,
        CautionArea,
        // Cables & pipelines
        CableArea,
        CableOverhead,
        CableSubmarine,
        PipelineArea,
        PipelineSubmarineOnLand,
    ];
}
