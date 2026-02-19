namespace EncDotNet.Enc;

/// <summary>
/// S-57 object class codes (OBJL) as defined in IHO S-57 Appendix A - Object Catalogue.
/// </summary>
public enum S57ObjectCode
{
    /// <summary>Anchorage area (ACHARE).</summary>
    ACHARE = 4,

    /// <summary>Beacon, cardinal (BCNCAR).</summary>
    BCNCAR = 5,

    /// <summary>Beacon, isolated danger (BCNISD).</summary>
    BCNISD = 6,

    /// <summary>Beacon, lateral (BCNLAT).</summary>
    BCNLAT = 7,

    /// <summary>Beacon, safe water (BCNSAW).</summary>
    BCNSAW = 8,

    /// <summary>Beacon, special purpose/general (BCNSPP).</summary>
    BCNSPP = 9,

    /// <summary>Bridge (BRIDGE).</summary>
    BRIDGE = 11,

    /// <summary>Built-up area (BUAARE).</summary>
    BUAARE = 13,

    /// <summary>Buoy, cardinal (BOYCAR).</summary>
    BOYCAR = 14,

    /// <summary>Buoy, isolated danger (BOYISD).</summary>
    BOYISD = 16,

    /// <summary>Buoy, lateral (BOYLAT).</summary>
    BOYLAT = 17,

    /// <summary>Buoy, safe water (BOYSAW).</summary>
    BOYSAW = 18,

    /// <summary>Buoy, special purpose/general (BOYSPP).</summary>
    BOYSPP = 19,

    /// <summary>Cable area (CBLARE).</summary>
    CBLARE = 20,

    /// <summary>Cable, overhead (CBLOHD).</summary>
    CBLOHD = 21,

    /// <summary>Cable, submarine (CBLSUB).</summary>
    CBLSUB = 22,

    /// <summary>Canal (CANALS).</summary>
    CANALS = 23,

    /// <summary>Caution area (CTNARE).</summary>
    CTNARE = 27,

    /// <summary>Coastline (COALNE).</summary>
    COALNE = 30,

    /// <summary>Depth area (DEPARE).</summary>
    DEPARE = 42,

    /// <summary>Depth contour (DEPCNT).</summary>
    DEPCNT = 43,

    /// <summary>Dock area (DOCARE).</summary>
    DOCARE = 45,

    /// <summary>Dredged area (DRGARE).</summary>
    DRGARE = 46,

    /// <summary>Dumping ground (DMPGRD).</summary>
    DMPGRD = 48,

    /// <summary>Fairway (FAIRWY).</summary>
    FAIRWY = 51,

    /// <summary>Ferry route (FERYRT).</summary>
    FERYRT = 53,

    /// <summary>Fog signal (FOGSIG).</summary>
    FOGSIG = 58,

    /// <summary>Lake (LAKARE).</summary>
    LAKARE = 69,

    /// <summary>Land area (LNDARE).</summary>
    LNDARE = 71,

    /// <summary>Landmark (LNDMRK).</summary>
    LNDMRK = 74,

    /// <summary>Light (LIGHTS).</summary>
    LIGHTS = 75,

    /// <summary>Magnetic variation (MAGVAR).</summary>
    MAGVAR = 81,

    /// <summary>Military practice area (MIPARE).</summary>
    MIPARE = 83,

    /// <summary>Mooring/warping facility (MORFAC).</summary>
    MORFAC = 84,

    /// <summary>Navigation line (NAVLNE).</summary>
    NAVLNE = 85,

    /// <summary>Obstruction (OBSTRN).</summary>
    OBSTRN = 86,

    /// <summary>Offshore platform (OFSPLF).</summary>
    OFSPLF = 87,

    /// <summary>Pilot boarding place (PILBOP).</summary>
    PILBOP = 91,

    /// <summary>Pipeline area (PIPARE).</summary>
    PIPARE = 92,

    /// <summary>Pipeline, submarine/on land (PIPSOL).</summary>
    PIPSOL = 94,

    /// <summary>Pontoon (PONTON).</summary>
    PONTON = 95,

    /// <summary>Precautionary area (PRCARE).</summary>
    PRCARE = 96,

    /// <summary>Radar station (RADSTA).</summary>
    RADSTA = 102,

    /// <summary>Radio calling-in point (RDOCAL).</summary>
    RDOCAL = 104,

    /// <summary>Recommended track (RECTRC).</summary>
    RECTRC = 109,

    /// <summary>Restricted area (RESARE).</summary>
    RESARE = 112,

    /// <summary>River (RIVERS).</summary>
    RIVERS = 114,

    /// <summary>Sea area/named water area (SEAARE).</summary>
    SEAARE = 119,

    /// <summary>Shoreline construction (SLCONS).</summary>
    SLCONS = 122,

    /// <summary>Sounding (SOUNDG).</summary>
    SOUNDG = 129,

    /// <summary>Traffic separation line (TSELNE).</summary>
    TSELNE = 145,

    /// <summary>Traffic separation scheme boundary (TSSBND).</summary>
    TSSBND = 146,

    /// <summary>Traffic separation scheme crossing (TSSCRS).</summary>
    TSSCRS = 147,

    /// <summary>Traffic separation scheme lane part (TSSLPT).</summary>
    TSSLPT = 148,

    /// <summary>Traffic separation scheme roundabout (TSSRON).</summary>
    TSSRON = 149,

    /// <summary>Traffic separation zone (TSEZNE).</summary>
    TSEZNE = 150,

    /// <summary>Two-way route part (TWRTPT).</summary>
    TWRTPT = 152,

    /// <summary>Underwater/awash rock (UWTROC).</summary>
    UWTROC = 153,

    /// <summary>Unsurveyed area (UNSARE).</summary>
    UNSARE = 154,

    /// <summary>Wreck (WRECKS).</summary>
    WRECKS = 159,

    /// <summary>Coverage (M_COVR) - meta object.</summary>
    M_COVR = 302,
}
