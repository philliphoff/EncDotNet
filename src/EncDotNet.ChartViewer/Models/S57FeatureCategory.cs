using System.Collections.Immutable;
using EncDotNet.Enc;

namespace EncDotNet.ChartViewer.Models;

/// <summary>
/// Represents a category of S-57 chart features that can be toggled on/off.
/// </summary>
public sealed class S57FeatureCategory
{
    /// <summary>Gets the display name of the category.</summary>
    public string Name { get; }

    /// <summary>Gets the S-57 object codes included in this category.</summary>
    public ImmutableArray<S57ObjectCode> ObjectCodes { get; }

    /// <summary>Gets whether this category is enabled by default.</summary>
    public bool DefaultEnabled { get; }

    private S57FeatureCategory(string name, bool defaultEnabled, params S57ObjectCode[] objectCodes)
    {
        Name = name;
        DefaultEnabled = defaultEnabled;
        ObjectCodes = [..objectCodes];
    }

    // S-57 object class codes
    // See IHO S-57 Appendix A - Object Catalogue

    // --- Hydrographic ---

    /// <summary>Coastline (COALNE, object code 30).</summary>
    public static readonly S57FeatureCategory Coastline = new("Coastline", true, S57ObjectCode.COALNE);

    /// <summary>Depth contours (DEPCNT, object code 43).</summary>
    public static readonly S57FeatureCategory DepthContours = new("Depth Contours", true, S57ObjectCode.DEPCNT);

    /// <summary>Depth areas (DEPARE, object code 42).</summary>
    public static readonly S57FeatureCategory DepthAreas = new("Depth Areas", false, S57ObjectCode.DEPARE);

    /// <summary>Soundings (SOUNDG, object code 129).</summary>
    public static readonly S57FeatureCategory Soundings = new("Soundings", false, S57ObjectCode.SOUNDG);

    /// <summary>Sea area (SEAARE, object code 112).</summary>
    public static readonly S57FeatureCategory SeaArea = new("Sea Area", false, S57ObjectCode.SEAARE);

    /// <summary>Dredged area (DRGARE, object code 47).</summary>
    public static readonly S57FeatureCategory DredgedArea = new("Dredged Area", false, S57ObjectCode.DRGARE);

    /// <summary>Lake (LAKARE, object code 69).</summary>
    public static readonly S57FeatureCategory Lake = new("Lake", false, S57ObjectCode.LAKARE);

    /// <summary>River (RIVERS, object code 114).</summary>
    public static readonly S57FeatureCategory River = new("River", false, S57ObjectCode.RIVERS);

    /// <summary>Canal (CANALS, object code 23).</summary>
    public static readonly S57FeatureCategory Canal = new("Canal", false, S57ObjectCode.CANALS);

    /// <summary>Unsurveyed area (UNSARE, object code 154).</summary>
    public static readonly S57FeatureCategory UnsurveyedArea = new("Unsurveyed Area", false, S57ObjectCode.UNSARE);

    /// <summary>Magnetic variation (MAGVAR, object code 77).</summary>
    public static readonly S57FeatureCategory MagneticVariation = new("Magnetic Variation", false, S57ObjectCode.MAGVAR);

    // --- Land ---

    /// <summary>Land area (LNDARE, object code 71).</summary>
    public static readonly S57FeatureCategory LandArea = new("Land Area", false, S57ObjectCode.LNDARE);

    /// <summary>Built-up area (BUAARE, object code 25).</summary>
    public static readonly S57FeatureCategory BuiltUpArea = new("Built-up Area", false, S57ObjectCode.BUAARE);

    /// <summary>Landmark (LNDMRK, object code 74).</summary>
    public static readonly S57FeatureCategory Landmarks = new("Landmarks", false, S57ObjectCode.LNDMRK);

    // --- Structures ---

    /// <summary>Shoreline construction (SLCONS, object code 122).</summary>
    public static readonly S57FeatureCategory ShorelineConstruction = new("Shoreline Construction", true, S57ObjectCode.SLCONS);

    /// <summary>Bridge (BRIDGE, object code 12).</summary>
    public static readonly S57FeatureCategory Bridges = new("Bridges", false, S57ObjectCode.BRIDGE);

    /// <summary>Mooring/warping facility (MORFAC, object code 84).</summary>
    public static readonly S57FeatureCategory MooringFacilities = new("Mooring Facilities", false, S57ObjectCode.MORFAC);

    /// <summary>Offshore platform (OFSPLF, object code 87).</summary>
    public static readonly S57FeatureCategory OffshorePlatforms = new("Offshore Platforms", false, S57ObjectCode.OFSPLF);

    /// <summary>Pontoon (PONTON, object code 95).</summary>
    public static readonly S57FeatureCategory Pontoons = new("Pontoons", false, S57ObjectCode.PONTON);

    /// <summary>Dock area (DOCARE, object code 45).</summary>
    public static readonly S57FeatureCategory DockArea = new("Dock Area", false, S57ObjectCode.DOCARE);

    // --- Navigation aids ---

    /// <summary>Lights (LIGHTS, object code 75).</summary>
    public static readonly S57FeatureCategory Lights = new("Lights", false, S57ObjectCode.LIGHTS);

    /// <summary>Buoys - lateral, cardinal, isolated danger, safe water, special purpose (BOYLAT 17, BOYCAR 14, BOYISD 15, BOYSAW 16, BOYSPP 18).</summary>
    public static readonly S57FeatureCategory Buoys = new("Buoys", false, S57ObjectCode.BOYLAT, S57ObjectCode.BOYCAR, S57ObjectCode.BOYISD, S57ObjectCode.BOYSAW, S57ObjectCode.BOYSPP);

    /// <summary>Beacons - lateral, cardinal, isolated danger, safe water, special purpose (BCNLAT 6, BCNCAR 3, BCNISD 4, BCNSAW 5, BCNSPP 7).</summary>
    public static readonly S57FeatureCategory Beacons = new("Beacons", false, S57ObjectCode.BCNLAT, S57ObjectCode.BCNCAR, S57ObjectCode.BCNISD, S57ObjectCode.BCNSAW, S57ObjectCode.BCNSPP);

    /// <summary>Fog signal (FOGSIG, object code 59).</summary>
    public static readonly S57FeatureCategory FogSignals = new("Fog Signals", false, S57ObjectCode.FOGSIG);

    /// <summary>Radar station (RADSTA, object code 100).</summary>
    public static readonly S57FeatureCategory RadarStations = new("Radar Stations", false, S57ObjectCode.RADSTA);

    /// <summary>Radio calling-in point (RDOCAL, object code 97).</summary>
    public static readonly S57FeatureCategory RadioCallingInPoints = new("Radio Calling-in Points", false, S57ObjectCode.RDOCAL);

    /// <summary>Pilot boarding place (PILBOP, object code 90).</summary>
    public static readonly S57FeatureCategory PilotBoardingPlaces = new("Pilot Boarding Places", false, S57ObjectCode.PILBOP);

    // --- Hazards ---

    /// <summary>Wrecks (WRECKS, object code 159).</summary>
    public static readonly S57FeatureCategory Wrecks = new("Wrecks", false, S57ObjectCode.WRECKS);

    /// <summary>Obstructions (OBSTRN, object code 86).</summary>
    public static readonly S57FeatureCategory Obstructions = new("Obstructions", false, S57ObjectCode.OBSTRN);

    /// <summary>Underwater rocks (UWTROC, object code 153).</summary>
    public static readonly S57FeatureCategory UnderwaterRocks = new("Underwater Rocks", false, S57ObjectCode.UWTROC);

    // --- Navigation routing ---

    /// <summary>Traffic separation scheme - lane part, roundabout, crossing, boundary, zone, separation line (TSSLPT 145, TSSRON 146, TSSCRS 148, TSSBND 144, TSEZNE 147, TSELNE 143).</summary>
    public static readonly S57FeatureCategory TrafficSeparationScheme = new("Traffic Separation Scheme", false, S57ObjectCode.TSSLPT, S57ObjectCode.TSSRON, S57ObjectCode.TSSCRS, S57ObjectCode.TSSBND, S57ObjectCode.TSEZNE, S57ObjectCode.TSELNE, S57ObjectCode.TWRTPT, S57ObjectCode.PRCARE);

    /// <summary>Recommended track (RECTRC, object code 96).</summary>
    public static readonly S57FeatureCategory RecommendedTrack = new("Recommended Track", false, S57ObjectCode.RECTRC);

    /// <summary>Navigation line (NAVLNE, object code 85).</summary>
    public static readonly S57FeatureCategory NavigationLine = new("Navigation Line", false, S57ObjectCode.NAVLNE);

    /// <summary>Fairway (FAIRWY, object code 57).</summary>
    public static readonly S57FeatureCategory Fairway = new("Fairway", false, S57ObjectCode.FAIRWY);

    /// <summary>Ferry route (FERYRT, object code 58).</summary>
    public static readonly S57FeatureCategory FerryRoute = new("Ferry Route", false, S57ObjectCode.FERYRT);

    // --- Regulated/restricted areas ---

    /// <summary>Anchorage area (ACHARE, object code 2).</summary>
    public static readonly S57FeatureCategory AnchorageArea = new("Anchorage Area", false, S57ObjectCode.ACHARE);

    /// <summary>Restricted area (RESARE, object code 120).</summary>
    public static readonly S57FeatureCategory RestrictedArea = new("Restricted Area", false, S57ObjectCode.RESARE);

    /// <summary>Dumping ground (DMPGRD, object code 46).</summary>
    public static readonly S57FeatureCategory DumpingGround = new("Dumping Ground", false, S57ObjectCode.DMPGRD);

    /// <summary>Military practice area (MIPARE, object code 83).</summary>
    public static readonly S57FeatureCategory MilitaryPracticeArea = new("Military Practice Area", false, S57ObjectCode.MIPARE);

    /// <summary>Caution area (CTNARE, object code 40).</summary>
    public static readonly S57FeatureCategory CautionArea = new("Caution Area", false, S57ObjectCode.CTNARE);

    // --- Cables & pipelines ---

    /// <summary>Cable area (CBLARE, object code 27).</summary>
    public static readonly S57FeatureCategory CableArea = new("Cable Area", false, S57ObjectCode.CBLARE);

    /// <summary>Cable overhead (CBLOHD, object code 28).</summary>
    public static readonly S57FeatureCategory CableOverhead = new("Cable Overhead", false, S57ObjectCode.CBLOHD);

    /// <summary>Cable submarine (CBLSUB, object code 29).</summary>
    public static readonly S57FeatureCategory CableSubmarine = new("Cable Submarine", false, S57ObjectCode.CBLSUB);

    /// <summary>Pipeline area (PIPARE, object code 92).</summary>
    public static readonly S57FeatureCategory PipelineArea = new("Pipeline Area", false, S57ObjectCode.PIPARE);

    /// <summary>Pipeline submarine/on land (PIPSOL, object code 93).</summary>
    public static readonly S57FeatureCategory PipelineSubmarineOnLand = new("Pipeline Submarine/On Land", false, S57ObjectCode.PIPSOL);

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
