namespace EncDotNet.S57;

/// <summary>
/// The horizontal geodetic datums of the DSPM subfield HDAT, which takes its values from the
/// attribute <c>HORDAT</c> (S-57 Edition 3.1, Part 3 §7.3.2.1; Appendix A, chapter 2,
/// attribute 400).
/// </summary>
/// <remarks>
/// The values are the exact S-57 codes, so cast between this enum and
/// <see cref="S57DataSetParameters.HorizontalDatum"/> (or a <c>HORDAT</c> attribute value) in
/// either direction. A code S-57 doesn't define casts to an unnamed value rather than failing.
/// The ENC product specification requires <see cref="WGS84"/>. The member names follow the
/// datum names in Appendix A.
/// </remarks>
public enum S57HorizontalDatum
{
    /// <summary>WGS 72 (code 1).</summary>
    WGS72 = 1,

    /// <summary>WGS 84 (code 2).</summary>
    WGS84 = 2,

    /// <summary>European 1950 (code 3).</summary>
    European1950 = 3,

    /// <summary>Potsdam Datum (code 4).</summary>
    PotsdamDatum = 4,

    /// <summary>Adindan (code 5).</summary>
    Adindan = 5,

    /// <summary>Afgooye (code 6).</summary>
    Afgooye = 6,

    /// <summary>Ain el Abd 1970 (code 7).</summary>
    AinElAbd1970 = 7,

    /// <summary>Anna 1 Astro 1965 (code 8).</summary>
    Anna1Astro1965 = 8,

    /// <summary>Antigua Island Astro 1943 (code 9).</summary>
    AntiguaIslandAstro1943 = 9,

    /// <summary>Arc 1950 (code 10).</summary>
    Arc1950 = 10,

    /// <summary>Arc 1960 (code 11).</summary>
    Arc1960 = 11,

    /// <summary>Ascension Island 1958 (code 12).</summary>
    AscensionIsland1958 = 12,

    /// <summary>Astro beacon "E" 1945 (code 13).</summary>
    AstroBeaconE1945 = 13,

    /// <summary>Astro DOS 71/4 (code 14).</summary>
    AstroDOS714 = 14,

    /// <summary>Astro Tern Island (FRIG) 1961 (code 15).</summary>
    AstroTernIsland1961 = 15,

    /// <summary>Astronomical Station 1952 (code 16).</summary>
    AstronomicalStation1952 = 16,

    /// <summary>Australian Geodetic 1966 (code 17).</summary>
    AustralianGeodetic1966 = 17,

    /// <summary>Australian Geodetic 1984 (code 18).</summary>
    AustralianGeodetic1984 = 18,

    /// <summary>Ayabelle Lighthouse (code 19).</summary>
    AyabelleLighthouse = 19,

    /// <summary>Bellevue (IGN) (code 20).</summary>
    BellevueIGN = 20,

    /// <summary>Bermuda 1957 (code 21).</summary>
    Bermuda1957 = 21,

    /// <summary>Bissau (code 22).</summary>
    Bissau = 22,

    /// <summary>Bogota Observatory (code 23).</summary>
    BogotaObservatory = 23,

    /// <summary>Bukit Rimpah (code 24).</summary>
    BukitRimpah = 24,

    /// <summary>Camp Area Astro (code 25).</summary>
    CampAreaAstro = 25,

    /// <summary>Campo Inchauspe 1969 (code 26).</summary>
    CampoInchauspe1969 = 26,

    /// <summary>Canton Astro 1966 (code 27).</summary>
    CantonAstro1966 = 27,

    /// <summary>Cape (code 28).</summary>
    Cape = 28,

    /// <summary>Cape Canaveral (code 29).</summary>
    CapeCanaveral = 29,

    /// <summary>Carthage (code 30).</summary>
    Carthage = 30,

    /// <summary>Chatam Island Astro 1971 (code 31).</summary>
    ChatamIslandAstro1971 = 31,

    /// <summary>Chua Astro (code 32).</summary>
    ChuaAstro = 32,

    /// <summary>Corrego Alegre (code 33).</summary>
    CorregoAlegre = 33,

    /// <summary>Dabola (code 34).</summary>
    Dabola = 34,

    /// <summary>Djakarta (Batavia) (code 35).</summary>
    Djakarta = 35,

    /// <summary>DOS 1968 (code 36).</summary>
    DOS1968 = 36,

    /// <summary>Easter Island 1967 (code 37).</summary>
    EasterIsland1967 = 37,

    /// <summary>European 1979 (code 38).</summary>
    European1979 = 38,

    /// <summary>Fort Thomas 1955 (code 39).</summary>
    FortThomas1955 = 39,

    /// <summary>Gan 1970 (code 40).</summary>
    Gan1970 = 40,

    /// <summary>Geodetic Datum 1949 (code 41).</summary>
    GeodeticDatum1949 = 41,

    /// <summary>Graciosa Base SW 1948 (code 42).</summary>
    GraciosaBaseSW1948 = 42,

    /// <summary>Guam 1963 (code 43).</summary>
    Guam1963 = 43,

    /// <summary>Gunung Segara (code 44).</summary>
    GunungSegara = 44,

    /// <summary>GUX 1 Astro (code 45).</summary>
    GUX1Astro = 45,

    /// <summary>Herat North (code 46).</summary>
    HeratNorth = 46,

    /// <summary>Hjorsey 1955 (code 47).</summary>
    Hjorsey1955 = 47,

    /// <summary>Hong Kong 1963 (code 48).</summary>
    HongKong1963 = 48,

    /// <summary>Hu-Tzu-Shan (code 49).</summary>
    HuTzuShan = 49,

    /// <summary>Indian (code 50).</summary>
    Indian = 50,

    /// <summary>Indian 1954 (code 51).</summary>
    Indian1954 = 51,

    /// <summary>Indian 1975 (code 52).</summary>
    Indian1975 = 52,

    /// <summary>Ireland 1965 (code 53).</summary>
    Ireland1965 = 53,

    /// <summary>ISTS 061 Astro 1968 (code 54).</summary>
    ISTS061Astro1968 = 54,

    /// <summary>ISTS 073 Astro 1969 (code 55).</summary>
    ISTS073Astro1969 = 55,

    /// <summary>Johnston Island 1961 (code 56).</summary>
    JohnstonIsland1961 = 56,

    /// <summary>Kandawala (code 57).</summary>
    Kandawala = 57,

    /// <summary>Kerguelen Island 1949 (code 58).</summary>
    KerguelenIsland1949 = 58,

    /// <summary>Kertau 1948 (code 59).</summary>
    Kertau1948 = 59,

    /// <summary>Kusaie Astro 1951 (code 60).</summary>
    KusaieAstro1951 = 60,

    /// <summary>L. C. 5 Astro 1961 (code 61).</summary>
    LC5Astro1961 = 61,

    /// <summary>Leigon (code 62).</summary>
    Leigon = 62,

    /// <summary>Liberia 1964 (code 63).</summary>
    Liberia1964 = 63,

    /// <summary>Luzon (code 64).</summary>
    Luzon = 64,

    /// <summary>Mahe 1971 (code 65).</summary>
    Mahe1971 = 65,

    /// <summary>Massawa (code 66).</summary>
    Massawa = 66,

    /// <summary>Merchich (code 67).</summary>
    Merchich = 67,

    /// <summary>Midway Astro 1961 (code 68).</summary>
    MidwayAstro1961 = 68,

    /// <summary>Minna (code 69).</summary>
    Minna = 69,

    /// <summary>Montserrat Island Astro 1958 (code 70).</summary>
    MontserratIslandAstro1958 = 70,

    /// <summary>M'Poraloko (code 71).</summary>
    MPoraloko = 71,

    /// <summary>Nahrwan (code 72).</summary>
    Nahrwan = 72,

    /// <summary>Naparima, BWI (code 73).</summary>
    NaparimaBWI = 73,

    /// <summary>North American 1927 (code 74).</summary>
    NorthAmerican1927 = 74,

    /// <summary>North American 1983 (code 75).</summary>
    NorthAmerican1983 = 75,

    /// <summary>Observatorio Meteorologico 1939 (code 76).</summary>
    ObservatorioMeteorologico1939 = 76,

    /// <summary>Old Egyptian 1907 (code 77).</summary>
    OldEgyptian1907 = 77,

    /// <summary>Old Hawaiian (code 78).</summary>
    OldHawaiian = 78,

    /// <summary>Oman (code 79).</summary>
    Oman = 79,

    /// <summary>Ordnance Survey of Great Britain 1936 (code 80).</summary>
    OrdnanceSurveyOfGreatBritain1936 = 80,

    /// <summary>Pico de las Nieves (code 81).</summary>
    PicoDeLasNieves = 81,

    /// <summary>Pitcairn Astro 1967 (code 82).</summary>
    PitcairnAstro1967 = 82,

    /// <summary>Point 58 (code 83).</summary>
    Point58 = 83,

    /// <summary>Pointe Noire 1948 (code 84).</summary>
    PointeNoire1948 = 84,

    /// <summary>Porto Santo 1936 (code 85).</summary>
    PortoSanto1936 = 85,

    /// <summary>Provisional South American 1956 (code 86).</summary>
    ProvisionalSouthAmerican1956 = 86,

    /// <summary>Provisional South Chilean 1963 (also known as Hito XVIII 1963) (code 87).</summary>
    ProvisionalSouthChilean1963 = 87,

    /// <summary>Puerto Rico (code 88).</summary>
    PuertoRico = 88,

    /// <summary>Qatar national (code 89).</summary>
    QatarNational = 89,

    /// <summary>Qornoq (code 90).</summary>
    Qornoq = 90,

    /// <summary>Reunion (code 91).</summary>
    Reunion = 91,

    /// <summary>Rome 1940 (code 92).</summary>
    Rome1940 = 92,

    /// <summary>Santo (DOS) 1965 (code 93).</summary>
    SantoDOS1965 = 93,

    /// <summary>Sao Braz (code 94).</summary>
    SaoBraz = 94,

    /// <summary>Sapper Hill 1943 (code 95).</summary>
    SapperHill1943 = 95,

    /// <summary>Schwarzeck (code 96).</summary>
    Schwarzeck = 96,

    /// <summary>Selvagem Grande 1938 (code 97).</summary>
    SelvagemGrande1938 = 97,

    /// <summary>South American 1969 (code 98).</summary>
    SouthAmerican1969 = 98,

    /// <summary>South Asia (code 99).</summary>
    SouthAsia = 99,

    /// <summary>Tananarive Observatory 1925 (code 100).</summary>
    TananariveObservatory1925 = 100,

    /// <summary>Timbalai 1948 (code 101).</summary>
    Timbalai1948 = 101,

    /// <summary>Tokyo (code 102).</summary>
    Tokyo = 102,

    /// <summary>Tristan Astro 1968 (code 103).</summary>
    TristanAstro1968 = 103,

    /// <summary>Viti Levu 1916 (code 104).</summary>
    VitiLevu1916 = 104,

    /// <summary>Wake-Eniwetok 1960 (code 105).</summary>
    WakeEniwetok1960 = 105,

    /// <summary>Wake Island Astro 1952 (code 106).</summary>
    WakeIslandAstro1952 = 106,

    /// <summary>Yacare (code 107).</summary>
    Yacare = 107,

    /// <summary>Zanderij (code 108).</summary>
    Zanderij = 108,

    /// <summary>American Samoa 1962 (code 109).</summary>
    AmericanSamoa1962 = 109,

    /// <summary>Deception Island (code 110).</summary>
    DeceptionIsland = 110,

    /// <summary>Indian 1960 (code 111).</summary>
    Indian1960 = 111,

    /// <summary>Indonesian 1974 (code 112).</summary>
    Indonesian1974 = 112,

    /// <summary>North Sahara 1959 (code 113).</summary>
    NorthSahara1959 = 113,

    /// <summary>Pulkovo 1942 (code 114).</summary>
    Pulkovo1942 = 114,

    /// <summary>S-42 (Pulkovo 1942) (code 115).</summary>
    S42Pulkovo1942 = 115,

    /// <summary>S-JYSK (code 116).</summary>
    SJYSK = 116,

    /// <summary>Voirol 1950 (code 117).</summary>
    Voirol1950 = 117,

    /// <summary>Average Terrestrial System 1977 (code 118).</summary>
    AverageTerrestrialSystem1977 = 118,

    /// <summary>Compensation Géodésique du Québec 1977 (code 119).</summary>
    CompensationGeodesiqueDuQuebec1977 = 119,

    /// <summary>Finnish (KKJ) (code 120).</summary>
    FinnishKKJ = 120,

    /// <summary>Ordnance Survey of Ireland (code 121).</summary>
    OrdnanceSurveyOfIreland = 121,

    /// <summary>Revised Kertau (code 122).</summary>
    RevisedKertau = 122,

    /// <summary>Revised Nahrwan (code 123).</summary>
    RevisedNahrwan = 123,

    /// <summary>GGRS 76 (Greece) (code 124).</summary>
    GGRS76 = 124,

    /// <summary>Nouvelle Triangulation de France (code 125).</summary>
    NouvelleTriangulationDeFrance = 125,

    /// <summary>RT 90 (Sweden) (code 126).</summary>
    RT90 = 126,

    /// <summary>Geocentric Datum of Australia (GDA) (code 127).</summary>
    GeocentricDatumOfAustralia = 127,

    /// <summary>BJZ54 (A954 Beijing Coordinates) (code 128).</summary>
    BJZ54 = 128,

    /// <summary>Modified BJZ54 (code 129).</summary>
    ModifiedBJZ54 = 129,

    /// <summary>GDZ80 (code 130).</summary>
    GDZ80 = 130,

    /// <summary>Local datum (code 131).</summary>
    LocalDatum = 131
}
