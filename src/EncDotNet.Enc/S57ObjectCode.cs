namespace EncDotNet.Enc;

/// <summary>
/// S-57 object class codes (OBJL) as defined in IHO S-57 Appendix A - Object Catalogue.
/// </summary>
public enum S57ObjectCode
{
    // --- Geo Object Classes ---

    /// <summary>Administration area, named (ADMARE).</summary>
    ADMARE = 1,

    /// <summary>Airport/airfield (AIRARE).</summary>
    AIRARE = 2,

    /// <summary>Anchor berth (ACHBRT).</summary>
    ACHBRT = 3,

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

    /// <summary>Berth (BERTHS).</summary>
    BERTHS = 10,

    /// <summary>Bridge (BRIDGE).</summary>
    BRIDGE = 11,

    /// <summary>Building, single (BUISGL).</summary>
    BUISGL = 12,

    /// <summary>Built-up area (BUAARE).</summary>
    BUAARE = 13,

    /// <summary>Buoy, cardinal (BOYCAR).</summary>
    BOYCAR = 14,

    /// <summary>Buoy, installation (BOYINB).</summary>
    BOYINB = 15,

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

    /// <summary>Canal bank (CANBNK).</summary>
    CANBNK = 24,

    /// <summary>Cargo transhipment area (CTSARE).</summary>
    CTSARE = 25,

    /// <summary>Causeway (CAUSWY).</summary>
    CAUSWY = 26,

    /// <summary>Caution area (CTNARE).</summary>
    CTNARE = 27,

    /// <summary>Checkpoint (CHKPNT).</summary>
    CHKPNT = 28,

    /// <summary>Coastguard station (CGUSTA).</summary>
    CGUSTA = 29,

    /// <summary>Coastline (COALNE).</summary>
    COALNE = 30,

    /// <summary>Contiguous zone (CONZNE).</summary>
    CONZNE = 31,

    /// <summary>Continental shelf area (COSARE).</summary>
    COSARE = 32,

    /// <summary>Control point (CTRPNT).</summary>
    CTRPNT = 33,

    /// <summary>Conveyor (CONVYR).</summary>
    CONVYR = 34,

    /// <summary>Crane (CRANES).</summary>
    CRANES = 35,

    /// <summary>Current, non-gravitational (CURENT).</summary>
    CURENT = 36,

    /// <summary>Custom zone (CUSZNE).</summary>
    CUSZNE = 37,

    /// <summary>Dam (DAMCON).</summary>
    DAMCON = 38,

    /// <summary>Daymark (DAYMAR).</summary>
    DAYMAR = 39,

    /// <summary>Deep water route centerline (DWRTCL).</summary>
    DWRTCL = 40,

    /// <summary>Deep water route part (DWRTPT).</summary>
    DWRTPT = 41,

    /// <summary>Depth area (DEPARE).</summary>
    DEPARE = 42,

    /// <summary>Depth contour (DEPCNT).</summary>
    DEPCNT = 43,

    /// <summary>Distance mark (DISMAR).</summary>
    DISMAR = 44,

    /// <summary>Dock area (DOCARE).</summary>
    DOCARE = 45,

    /// <summary>Dredged area (DRGARE).</summary>
    DRGARE = 46,

    /// <summary>Dry dock (DRYDOC).</summary>
    DRYDOC = 47,

    /// <summary>Dumping ground (DMPGRD).</summary>
    DMPGRD = 48,

    /// <summary>Dyke (DYKCON).</summary>
    DYKCON = 49,

    /// <summary>Exclusive economic zone (EXEZNE).</summary>
    EXEZNE = 50,

    /// <summary>Fairway (FAIRWY).</summary>
    FAIRWY = 51,

    /// <summary>Fence/wall (FNCLNE).</summary>
    FNCLNE = 52,

    /// <summary>Ferry route (FERYRT).</summary>
    FERYRT = 53,

    /// <summary>Fishery zone (FSHZNE).</summary>
    FSHZNE = 54,

    /// <summary>Fishing facility (FSHFAC).</summary>
    FSHFAC = 55,

    /// <summary>Fishing ground (FSHGRD).</summary>
    FSHGRD = 56,

    /// <summary>Floating dock (FLODOC).</summary>
    FLODOC = 57,

    /// <summary>Fog signal (FOGSIG).</summary>
    FOGSIG = 58,

    /// <summary>Fortified structure (FORSTC).</summary>
    FORSTC = 59,

    /// <summary>Free port area (FRPARE).</summary>
    FRPARE = 60,

    /// <summary>Gate (GATCON).</summary>
    GATCON = 61,

    /// <summary>Gridiron (GRIDRN).</summary>
    GRIDRN = 62,

    /// <summary>Harbour area, administrative (HRBARE).</summary>
    HRBARE = 63,

    /// <summary>Harbour facility (HRBFAC).</summary>
    HRBFAC = 64,

    /// <summary>Hulk (HULKES).</summary>
    HULKES = 65,

    /// <summary>Ice area (ICEARE).</summary>
    ICEARE = 66,

    /// <summary>Incineration area (ICNARE).</summary>
    ICNARE = 67,

    /// <summary>Inshore traffic zone (ISTZNE).</summary>
    ISTZNE = 68,

    /// <summary>Lake (LAKARE).</summary>
    LAKARE = 69,

    /// <summary>Lake shore (LAKSHR).</summary>
    LAKSHR = 70,

    /// <summary>Land area (LNDARE).</summary>
    LNDARE = 71,

    /// <summary>Land elevation (LNDELV).</summary>
    LNDELV = 72,

    /// <summary>Land region (LNDRGN).</summary>
    LNDRGN = 73,

    /// <summary>Landmark (LNDMRK).</summary>
    LNDMRK = 74,

    /// <summary>Light (LIGHTS).</summary>
    LIGHTS = 75,

    /// <summary>Light float (LITFLT).</summary>
    LITFLT = 76,

    /// <summary>Light vessel (LITVES).</summary>
    LITVES = 77,

    /// <summary>Local magnetic anomaly (LOCMAG).</summary>
    LOCMAG = 78,

    /// <summary>Lock basin (LOKBSN).</summary>
    LOKBSN = 79,

    /// <summary>Log pond (LOGPON).</summary>
    LOGPON = 80,

    /// <summary>Magnetic variation (MAGVAR).</summary>
    MAGVAR = 81,

    /// <summary>Marine farm/culture (MARCUL).</summary>
    MARCUL = 82,

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

    /// <summary>Offshore production area (OSPARE).</summary>
    OSPARE = 88,

    /// <summary>Oil barrier (OILBAR).</summary>
    OILBAR = 89,

    /// <summary>Pile (PILPNT).</summary>
    PILPNT = 90,

    /// <summary>Pilot boarding place (PILBOP).</summary>
    PILBOP = 91,

    /// <summary>Pipeline area (PIPARE).</summary>
    PIPARE = 92,

    /// <summary>Pipeline, overhead (PIPOHD).</summary>
    PIPOHD = 93,

    /// <summary>Pipeline, submarine/on land (PIPSOL).</summary>
    PIPSOL = 94,

    /// <summary>Pontoon (PONTON).</summary>
    PONTON = 95,

    /// <summary>Precautionary area (PRCARE).</summary>
    PRCARE = 96,

    /// <summary>Production/storage area (PRDARE).</summary>
    PRDARE = 97,

    /// <summary>Pylon/bridge support (PYLONS).</summary>
    PYLONS = 98,

    /// <summary>Radar line (RADLNE).</summary>
    RADLNE = 99,

    /// <summary>Radar range (RADRNG).</summary>
    RADRNG = 100,

    /// <summary>Radar reflector (RADRFL).</summary>
    RADRFL = 101,

    /// <summary>Radar station (RADSTA).</summary>
    RADSTA = 102,

    /// <summary>Radar transponder beacon (RTPBCN).</summary>
    RTPBCN = 103,

    /// <summary>Radio calling-in point (RDOCAL).</summary>
    RDOCAL = 104,

    /// <summary>Radio station (RDOSTA).</summary>
    RDOSTA = 105,

    /// <summary>Railway (RAILWY).</summary>
    RAILWY = 106,

    /// <summary>Rapids (RAPIDS).</summary>
    RAPIDS = 107,

    /// <summary>Recommended route centerline (RCRTCL).</summary>
    RCRTCL = 108,

    /// <summary>Recommended track (RECTRC).</summary>
    RECTRC = 109,

    /// <summary>Recommended traffic lane part (RCTLPT).</summary>
    RCTLPT = 110,

    /// <summary>Rescue station (RSCSTA).</summary>
    RSCSTA = 111,

    /// <summary>Restricted area (RESARE).</summary>
    RESARE = 112,

    /// <summary>Retro-reflector (RETRFL).</summary>
    RETRFL = 113,

    /// <summary>River (RIVERS).</summary>
    RIVERS = 114,

    /// <summary>River bank (RIVBNK).</summary>
    RIVBNK = 115,

    /// <summary>Road (ROADWY).</summary>
    ROADWY = 116,

    /// <summary>Runway (RUNWAY).</summary>
    RUNWAY = 117,

    /// <summary>Sand waves (SNDWAV).</summary>
    SNDWAV = 118,

    /// <summary>Sea area/named water area (SEAARE).</summary>
    SEAARE = 119,

    /// <summary>Sea-plane landing area (SPLARE).</summary>
    SPLARE = 120,

    /// <summary>Seabed area (SBDARE).</summary>
    SBDARE = 121,

    /// <summary>Shoreline construction (SLCONS).</summary>
    SLCONS = 122,

    /// <summary>Signal station, traffic (SISTAT).</summary>
    SISTAT = 123,

    /// <summary>Signal station, warning (SISTAW).</summary>
    SISTAW = 124,

    /// <summary>Silo/tank (SILTNK).</summary>
    SILTNK = 125,

    /// <summary>Slope topline (SLOTOP).</summary>
    SLOTOP = 126,

    /// <summary>Sloping ground (SLOGRD).</summary>
    SLOGRD = 127,

    /// <summary>Small craft facility (SMCFAC).</summary>
    SMCFAC = 128,

    /// <summary>Sounding (SOUNDG).</summary>
    SOUNDG = 129,

    /// <summary>Spring (SPRING).</summary>
    SPRING = 130,

    /// <summary>Square (SQUARE).</summary>
    SQUARE = 131,

    /// <summary>Straight territorial sea baseline (STSLNE).</summary>
    STSLNE = 132,

    /// <summary>Submarine transit lane (SUBTLN).</summary>
    SUBTLN = 133,

    /// <summary>Swept area (SWPARE).</summary>
    SWPARE = 134,

    /// <summary>Territorial sea area (TESARE).</summary>
    TESARE = 135,

    /// <summary>Tidal stream, harmonic prediction (TS_PRH).</summary>
    TS_PRH = 136,

    /// <summary>Tidal stream, non-harmonic prediction (TS_PNH).</summary>
    TS_PNH = 137,

    /// <summary>Tidal stream panel data (TS_PAD).</summary>
    TS_PAD = 138,

    /// <summary>Tidal stream, time series (TS_TIS).</summary>
    TS_TIS = 139,

    /// <summary>Tide, harmonic prediction (T_HMON).</summary>
    T_HMON = 140,

    /// <summary>Tide, non-harmonic prediction (T_NHMN).</summary>
    T_NHMN = 141,

    /// <summary>Tide, time series (T_TIMS).</summary>
    T_TIMS = 142,

    /// <summary>Tideway (TIDEWY).</summary>
    TIDEWY = 143,

    /// <summary>Topmark (TOPMAR).</summary>
    TOPMAR = 144,

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

    /// <summary>Tunnel (TUNNEL).</summary>
    TUNNEL = 151,

    /// <summary>Two-way route part (TWRTPT).</summary>
    TWRTPT = 152,

    /// <summary>Underwater/awash rock (UWTROC).</summary>
    UWTROC = 153,

    /// <summary>Unsurveyed area (UNSARE).</summary>
    UNSARE = 154,

    /// <summary>Vegetation (VEGATN).</summary>
    VEGATN = 155,

    /// <summary>Water turbulence (WATTUR).</summary>
    WATTUR = 156,

    /// <summary>Waterfall (WATFAL).</summary>
    WATFAL = 157,

    /// <summary>Weed/Kelp (WEDKLP).</summary>
    WEDKLP = 158,

    /// <summary>Wreck (WRECKS).</summary>
    WRECKS = 159,

    /// <summary>Tidal stream, flood/ebb (TS_FEB).</summary>
    TS_FEB = 160,

    // --- Meta Object Classes ---

    /// <summary>Accuracy of data (M_ACCY).</summary>
    M_ACCY = 300,

    /// <summary>Compilation scale of data (M_CSCL).</summary>
    M_CSCL = 301,

    /// <summary>Coverage (M_COVR).</summary>
    M_COVR = 302,

    /// <summary>Horizontal datum of data (M_HDAT).</summary>
    M_HDAT = 303,

    /// <summary>Horizontal datum shift parameters (M_HOPA).</summary>
    M_HOPA = 304,

    /// <summary>Nautical publication information (M_NPUB).</summary>
    M_NPUB = 305,

    /// <summary>Navigational system of marks (M_NSYS).</summary>
    M_NSYS = 306,

    /// <summary>Production information (M_PROD).</summary>
    M_PROD = 307,

    /// <summary>Quality of data (M_QUAL).</summary>
    M_QUAL = 308,

    /// <summary>Sounding datum (M_SDAT).</summary>
    M_SDAT = 309,

    /// <summary>Survey reliability (M_SREL).</summary>
    M_SREL = 310,

    /// <summary>Units of measurement of data (M_UNIT).</summary>
    M_UNIT = 311,

    /// <summary>Vertical datum of data (M_VDAT).</summary>
    M_VDAT = 312,

    // --- Collection Object Classes ---

    /// <summary>Aggregation (C_AGGR).</summary>
    C_AGGR = 400,

    /// <summary>Association (C_ASSO).</summary>
    C_ASSO = 401,

    /// <summary>Stacked on/stacked under (C_STAC).</summary>
    C_STAC = 402,
}
