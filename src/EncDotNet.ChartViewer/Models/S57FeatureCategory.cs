using System.Collections.Immutable;
using EncDotNet.S57;

namespace EncDotNet.ChartViewer.Models;

/// <summary>
/// Represents a category of S-57 chart features that can be toggled on/off.
/// </summary>
public sealed class S57FeatureCategory
{
    /// <summary>Gets the display name of the category.</summary>
    public string Name { get; }

    /// <summary>Gets the S-57 object codes included in this category.</summary>
    public IReadOnlyList<S57ObjectCode> ObjectCodes { get; }

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

    /// <summary>Sea area (SEAARE, object code 119).</summary>
    public static readonly S57FeatureCategory SeaArea = new("Sea Area", false, S57ObjectCode.SEAARE);

    /// <summary>Dredged area (DRGARE, object code 46).</summary>
    public static readonly S57FeatureCategory DredgedArea = new("Dredged Area", false, S57ObjectCode.DRGARE);

    /// <summary>Lake (LAKARE, object code 69).</summary>
    public static readonly S57FeatureCategory Lake = new("Lake", false, S57ObjectCode.LAKARE);

    /// <summary>River (RIVERS, object code 114).</summary>
    public static readonly S57FeatureCategory River = new("River", false, S57ObjectCode.RIVERS);

    /// <summary>Canal (CANALS, object code 23).</summary>
    public static readonly S57FeatureCategory Canal = new("Canal", false, S57ObjectCode.CANALS);

    /// <summary>Unsurveyed area (UNSARE, object code 154).</summary>
    public static readonly S57FeatureCategory UnsurveyedArea = new("Unsurveyed Area", false, S57ObjectCode.UNSARE);

    /// <summary>Magnetic variation (MAGVAR, object code 81).</summary>
    public static readonly S57FeatureCategory MagneticVariation = new("Magnetic Variation", false, S57ObjectCode.MAGVAR);

    /// <summary>Seabed - seabed area, weed/kelp, swept area (SBDARE 121, WEDKLP 158, SWPARE 134).</summary>
    public static readonly S57FeatureCategory Seabed = new("Seabed", false, S57ObjectCode.SBDARE, S57ObjectCode.WEDKLP, S57ObjectCode.SWPARE);

    /// <summary>Tides and currents (TS_PRH 136, TS_PNH 137, TS_PAD 138, TS_TIS 139, T_HMON 140, T_NHMN 141, T_TIMS 142, CURENT 36, TIDEWY 143, TS_FEB 160).</summary>
    public static readonly S57FeatureCategory TidesAndCurrents = new("Tides & Currents", false, S57ObjectCode.TS_PRH, S57ObjectCode.TS_PNH, S57ObjectCode.TS_PAD, S57ObjectCode.TS_TIS, S57ObjectCode.T_HMON, S57ObjectCode.T_NHMN, S57ObjectCode.T_TIMS, S57ObjectCode.CURENT, S57ObjectCode.TIDEWY, S57ObjectCode.TS_FEB);

    // --- Land ---

    /// <summary>Land area (LNDARE, object code 71).</summary>
    public static readonly S57FeatureCategory LandArea = new("Land Area", false, S57ObjectCode.LNDARE);

    /// <summary>Built-up area (BUAARE, object code 13).</summary>
    public static readonly S57FeatureCategory BuiltUpArea = new("Built-up Area", false, S57ObjectCode.BUAARE);

    /// <summary>Landmark (LNDMRK, object code 74).</summary>
    public static readonly S57FeatureCategory Landmarks = new("Landmarks", false, S57ObjectCode.LNDMRK);

    /// <summary>Land features - vegetation, land region, land elevation, lake shore, river bank (VEGATN 155, LNDRGN 73, LNDELV 72, LAKSHR 70, RIVBNK 115).</summary>
    public static readonly S57FeatureCategory LandFeatures = new("Land Features", false, S57ObjectCode.VEGATN, S57ObjectCode.LNDRGN, S57ObjectCode.LNDELV, S57ObjectCode.LAKSHR, S57ObjectCode.RIVBNK);

    /// <summary>Transport - railway, road, runway, airport/airfield (RAILWY 106, ROADWY 116, RUNWAY 117, AIRARE 2).</summary>
    public static readonly S57FeatureCategory Transport = new("Transport", false, S57ObjectCode.RAILWY, S57ObjectCode.ROADWY, S57ObjectCode.RUNWAY, S57ObjectCode.AIRARE);

    // --- Structures ---

    /// <summary>Shoreline construction (SLCONS, object code 122).</summary>
    public static readonly S57FeatureCategory ShorelineConstruction = new("Shoreline Construction", true, S57ObjectCode.SLCONS);

    /// <summary>Bridge (BRIDGE, object code 11).</summary>
    public static readonly S57FeatureCategory Bridges = new("Bridges", false, S57ObjectCode.BRIDGE);

    /// <summary>Mooring/warping facility (MORFAC, object code 84).</summary>
    public static readonly S57FeatureCategory MooringFacilities = new("Mooring Facilities", false, S57ObjectCode.MORFAC);

    /// <summary>Offshore platform (OFSPLF, object code 87).</summary>
    public static readonly S57FeatureCategory OffshorePlatforms = new("Offshore Platforms", false, S57ObjectCode.OFSPLF);

    /// <summary>Pontoon (PONTON, object code 95).</summary>
    public static readonly S57FeatureCategory Pontoons = new("Pontoons", false, S57ObjectCode.PONTON);

    /// <summary>Dock area (DOCARE, object code 45).</summary>
    public static readonly S57FeatureCategory DockArea = new("Dock Area", false, S57ObjectCode.DOCARE);

    /// <summary>Harbour and port facilities (HRBARE 63, HRBFAC 64, BERTHS 10, SMCFAC 128, CRANES 35).</summary>
    public static readonly S57FeatureCategory HarbourFacilities = new("Harbour Facilities", false, S57ObjectCode.HRBARE, S57ObjectCode.HRBFAC, S57ObjectCode.BERTHS, S57ObjectCode.SMCFAC, S57ObjectCode.CRANES);

    /// <summary>Dams, dykes, and waterway structures (DAMCON 38, DYKCON 49, CAUSWY 26, TUNNEL 151, GATCON 61).</summary>
    public static readonly S57FeatureCategory DamsAndDykes = new("Dams & Dykes", false, S57ObjectCode.DAMCON, S57ObjectCode.DYKCON, S57ObjectCode.CAUSWY, S57ObjectCode.TUNNEL, S57ObjectCode.GATCON);

    // --- Navigation aids ---

    /// <summary>Lights (LIGHTS, object code 75).</summary>
    public static readonly S57FeatureCategory Lights = new("Lights", false, S57ObjectCode.LIGHTS);

    /// <summary>Buoys - lateral, cardinal, installation, isolated danger, safe water, special purpose (BOYLAT 17, BOYCAR 14, BOYINB 15, BOYISD 16, BOYSAW 18, BOYSPP 19).</summary>
    public static readonly S57FeatureCategory Buoys = new("Buoys", false, S57ObjectCode.BOYLAT, S57ObjectCode.BOYCAR, S57ObjectCode.BOYINB, S57ObjectCode.BOYISD, S57ObjectCode.BOYSAW, S57ObjectCode.BOYSPP);

    /// <summary>Beacons - lateral, cardinal, isolated danger, safe water, special purpose (BCNLAT 7, BCNCAR 5, BCNISD 6, BCNSAW 8, BCNSPP 9).</summary>
    public static readonly S57FeatureCategory Beacons = new("Beacons", false, S57ObjectCode.BCNLAT, S57ObjectCode.BCNCAR, S57ObjectCode.BCNISD, S57ObjectCode.BCNSAW, S57ObjectCode.BCNSPP);

    /// <summary>Fog signal (FOGSIG, object code 58).</summary>
    public static readonly S57FeatureCategory FogSignals = new("Fog Signals", false, S57ObjectCode.FOGSIG);

    /// <summary>Radar station (RADSTA, object code 102).</summary>
    public static readonly S57FeatureCategory RadarStations = new("Radar Stations", false, S57ObjectCode.RADSTA);

    /// <summary>Radio calling-in point (RDOCAL, object code 104).</summary>
    public static readonly S57FeatureCategory RadioCallingInPoints = new("Radio Calling-in Points", false, S57ObjectCode.RDOCAL);

    /// <summary>Pilot boarding place (PILBOP, object code 91).</summary>
    public static readonly S57FeatureCategory PilotBoardingPlaces = new("Pilot Boarding Places", false, S57ObjectCode.PILBOP);

    /// <summary>Other navigation aids - light float, light vessel, radar transponder, daymark, topmark (LITFLT 76, LITVES 77, RTPBCN 103, DAYMAR 39, TOPMAR 144).</summary>
    public static readonly S57FeatureCategory OtherNavAids = new("Other Nav Aids", false, S57ObjectCode.LITFLT, S57ObjectCode.LITVES, S57ObjectCode.RTPBCN, S57ObjectCode.DAYMAR, S57ObjectCode.TOPMAR);

    /// <summary>Safety stations - rescue, coastguard, signal stations (RSCSTA 111, CGUSTA 29, SISTAT 123, SISTAW 124).</summary>
    public static readonly S57FeatureCategory SafetyStations = new("Safety Stations", false, S57ObjectCode.RSCSTA, S57ObjectCode.CGUSTA, S57ObjectCode.SISTAT, S57ObjectCode.SISTAW);

    // --- Hazards ---

    /// <summary>Wrecks (WRECKS, object code 159).</summary>
    public static readonly S57FeatureCategory Wrecks = new("Wrecks", false, S57ObjectCode.WRECKS);

    /// <summary>Obstructions (OBSTRN, object code 86).</summary>
    public static readonly S57FeatureCategory Obstructions = new("Obstructions", false, S57ObjectCode.OBSTRN);

    /// <summary>Underwater rocks (UWTROC, object code 153).</summary>
    public static readonly S57FeatureCategory UnderwaterRocks = new("Underwater Rocks", false, S57ObjectCode.UWTROC);

    /// <summary>Sand waves and water turbulence (SNDWAV 118, WATTUR 156).</summary>
    public static readonly S57FeatureCategory SandWavesAndTurbulence = new("Sand Waves & Turbulence", false, S57ObjectCode.SNDWAV, S57ObjectCode.WATTUR);

    // --- Navigation routing ---

    /// <summary>Traffic separation scheme - lane part, roundabout, crossing, boundary, zone, separation line, precautionary area (TSSLPT 148, TSSRON 149, TSSCRS 147, TSSBND 146, TSEZNE 150, TSELNE 145, TWRTPT 152, PRCARE 96, ISTZNE 68).</summary>
    public static readonly S57FeatureCategory TrafficSeparationScheme = new("Traffic Separation Scheme", false, S57ObjectCode.TSSLPT, S57ObjectCode.TSSRON, S57ObjectCode.TSSCRS, S57ObjectCode.TSSBND, S57ObjectCode.TSEZNE, S57ObjectCode.TSELNE, S57ObjectCode.TWRTPT, S57ObjectCode.PRCARE, S57ObjectCode.ISTZNE);

    /// <summary>Deep water routes - centerline and route part (DWRTCL 40, DWRTPT 41).</summary>
    public static readonly S57FeatureCategory DeepWaterRoutes = new("Deep Water Routes", false, S57ObjectCode.DWRTCL, S57ObjectCode.DWRTPT);

    /// <summary>Recommended track (RECTRC, object code 109).</summary>
    public static readonly S57FeatureCategory RecommendedTrack = new("Recommended Track", false, S57ObjectCode.RECTRC, S57ObjectCode.RCRTCL, S57ObjectCode.RCTLPT);

    /// <summary>Navigation line (NAVLNE, object code 85).</summary>
    public static readonly S57FeatureCategory NavigationLine = new("Navigation Line", false, S57ObjectCode.NAVLNE);

    /// <summary>Fairway (FAIRWY, object code 51).</summary>
    public static readonly S57FeatureCategory Fairway = new("Fairway", false, S57ObjectCode.FAIRWY);

    /// <summary>Ferry route (FERYRT, object code 53).</summary>
    public static readonly S57FeatureCategory FerryRoute = new("Ferry Route", false, S57ObjectCode.FERYRT);

    /// <summary>Submarine transit lane (SUBTLN, object code 133).</summary>
    public static readonly S57FeatureCategory SubmarineTransitLane = new("Submarine Transit Lane", false, S57ObjectCode.SUBTLN);

    // --- Regulated/restricted areas ---

    /// <summary>Anchorage area (ACHARE, object code 4).</summary>
    public static readonly S57FeatureCategory AnchorageArea = new("Anchorage Area", false, S57ObjectCode.ACHARE);

    /// <summary>Restricted area (RESARE, object code 112).</summary>
    public static readonly S57FeatureCategory RestrictedArea = new("Restricted Area", false, S57ObjectCode.RESARE);

    /// <summary>Dumping ground (DMPGRD, object code 48).</summary>
    public static readonly S57FeatureCategory DumpingGround = new("Dumping Ground", false, S57ObjectCode.DMPGRD);

    /// <summary>Military practice area (MIPARE, object code 83).</summary>
    public static readonly S57FeatureCategory MilitaryPracticeArea = new("Military Practice Area", false, S57ObjectCode.MIPARE);

    /// <summary>Caution area (CTNARE, object code 27).</summary>
    public static readonly S57FeatureCategory CautionArea = new("Caution Area", false, S57ObjectCode.CTNARE);

    /// <summary>Offshore/industrial areas - production, incineration, free port, cargo transhipment (OSPARE 88, ICNARE 67, FRPARE 60, CTSARE 25).</summary>
    public static readonly S57FeatureCategory OffshoreIndustrialAreas = new("Offshore/Industrial Areas", false, S57ObjectCode.OSPARE, S57ObjectCode.ICNARE, S57ObjectCode.FRPARE, S57ObjectCode.CTSARE);

    /// <summary>Boundaries and zones - contiguous zone, exclusive economic zone, territorial sea, fishery zone, custom zone, administration area (CONZNE 31, EXEZNE 50, TESARE 135, FSHZNE 54, CUSZNE 37, ADMARE 1).</summary>
    public static readonly S57FeatureCategory BoundariesAndZones = new("Boundaries & Zones", false, S57ObjectCode.CONZNE, S57ObjectCode.EXEZNE, S57ObjectCode.TESARE, S57ObjectCode.FSHZNE, S57ObjectCode.CUSZNE, S57ObjectCode.ADMARE);

    /// <summary>Fishing - fishing facility, fishing ground, marine farm/culture (FSHFAC 55, FSHGRD 56, MARCUL 82).</summary>
    public static readonly S57FeatureCategory Fishing = new("Fishing", false, S57ObjectCode.FSHFAC, S57ObjectCode.FSHGRD, S57ObjectCode.MARCUL);

    // --- Cables & pipelines ---

    /// <summary>Cable area (CBLARE, object code 20).</summary>
    public static readonly S57FeatureCategory CableArea = new("Cable Area", false, S57ObjectCode.CBLARE);

    /// <summary>Cable overhead (CBLOHD, object code 21).</summary>
    public static readonly S57FeatureCategory CableOverhead = new("Cable Overhead", false, S57ObjectCode.CBLOHD);

    /// <summary>Cable submarine (CBLSUB, object code 22).</summary>
    public static readonly S57FeatureCategory CableSubmarine = new("Cable Submarine", false, S57ObjectCode.CBLSUB);

    /// <summary>Pipeline area (PIPARE, object code 92).</summary>
    public static readonly S57FeatureCategory PipelineArea = new("Pipeline Area", false, S57ObjectCode.PIPARE);

    /// <summary>Pipeline submarine/on land (PIPSOL, object code 94).</summary>
    public static readonly S57FeatureCategory PipelineSubmarineOnLand = new("Pipeline Submarine/On Land", false, S57ObjectCode.PIPSOL);

    /// <summary>All predefined feature categories.</summary>
    public static readonly ImmutableArray<S57FeatureCategory> All =
    [
        // Hydrographic
        Coastline,
        SeaArea,
        DepthContours,
        DepthAreas,
        Soundings,
        DredgedArea,
        Lake,
        River,
        Canal,
        UnsurveyedArea,
        MagneticVariation,
        Seabed,
        TidesAndCurrents,
        // Land
        LandArea,
        BuiltUpArea,
        Landmarks,
        LandFeatures,
        Transport,
        // Structures
        ShorelineConstruction,
        Bridges,
        MooringFacilities,
        OffshorePlatforms,
        Pontoons,
        DockArea,
        HarbourFacilities,
        DamsAndDykes,
        // Navigation aids
        Lights,
        Buoys,
        Beacons,
        FogSignals,
        RadarStations,
        RadioCallingInPoints,
        PilotBoardingPlaces,
        OtherNavAids,
        SafetyStations,
        // Hazards
        Wrecks,
        Obstructions,
        UnderwaterRocks,
        SandWavesAndTurbulence,
        // Navigation routing
        TrafficSeparationScheme,
        DeepWaterRoutes,
        RecommendedTrack,
        NavigationLine,
        Fairway,
        FerryRoute,
        SubmarineTransitLane,
        // Regulated/restricted areas
        AnchorageArea,
        RestrictedArea,
        DumpingGround,
        MilitaryPracticeArea,
        CautionArea,
        OffshoreIndustrialAreas,
        BoundariesAndZones,
        Fishing,
        // Cables & pipelines
        CableArea,
        CableOverhead,
        CableSubmarine,
        PipelineArea,
        PipelineSubmarineOnLand,
    ];
}
