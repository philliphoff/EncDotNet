S-57 APPENDIX B.1
Annex D – INT1 to S-57/52
Edition 1.0

INT1 to S-57/52 for ENCs
INTRODUCTION
General Appendix B.1 Annex D (INT 1 - ENC) provides a reference for interpreting the many symbols and abbreviations found on International charts into
ENC objects and attributes. Users should be aware that only generic objects and attributes interpretations are shown. For more detailed
descriptions of objects and attributes users should use the S-57 ENC document links included with each symbol.
Schematic Layout of Appendix B.1 Annex D (INT 1 - ENC)
(1) IM Tracks, Routes (2)
Tracks (4) Tracks Marked by Lights IP (3) Leading Beacons IQ (3)
Leading line NAVLNE CATNAV 3 L 10.1.1 310.1
1
(the continuous 310.2
firm lineis the track
to be followed)
(5) (6) (7) (8) (9) (10) (11) (12) (13) (14) (15)
(1) Section designation: The letter "I" means International. (10) Column 6: Object acronym with S-57 Appendix A Chapter 1 “Object
Classes” reference.
(2) Section.
(11) Column 7: Generic attribute acronym with S-57 Appendix A Chapter 2
(3) Cross-reference to terms in other sections. “Attributes “reference. Mandatory attributes are depicted in
bold Mandatory attributes which are designated as “at least
(4) Sub-section one of” are depicted in bold italic
(5) Column 1: Numbering following the International "Chart Specifications of (12) Column 8: Generic attribute value. Values are only generally given where
the IHO" a specific category should be used for that object.
(6) Column 2: International (INT) symbol. (13) Column 9: Geometric primitives (P = point, L = line, A = area).
(7) Column 3: Term in the English language. (14) Column 10: Use of the Object Catalogue ENC. Referenced in S-57
Appendix B.1 Annex A.
(8) Column 4: S-52 symbol used in a SENC.
(15) Column 11: Comment or link to National encoding requirements.
(9) Column 5: S-52 symbol name.
(16) Column 12: Numbering following the "Chart Specifications of the IHO".
Edition 1.0 November 2000 S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
INDEX
A B C D E F G H I J K L M N O P Q R S T U V W Y Z
S-57 Appendix B.1 - Annex D November 2000 Edition 1.0

INT1 to S-57/52 for ENCs
CONTENTS
IB IL
Positions, Distances, Directions, Compass Offfshore Installations
IC Natural Features IM Tracks, Routes
ID
Cultural Features IN
Areas, Limits
IE
Landmarks IO
Hydrographic Terms
IF
Ports
IP
Lights
IG
Topographic Terms
IQ
Buoys, Beacons
IH
Tides, Currents
IR
Fog Signals
II
Depths Depths
IS
Radar, Radio, Electronic Position Fixing Systems
IJ
Nature of the Seabed
IT
Services
IK
Rocks, Wrecks, Obstructions
IU
Small Craft Facilities
Edition 1.0 November 2000 S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
IB Positions, Distances, Directions, Compass

Geographical Positions
| 1-6  |           | No applicable information for ENCs  |     |             |      |        |     |
| ---- | --------- | ----------------------------------- | --- | ----------- | ---- | ------ | --- |
| 7    |           |                                     |     |             |      |        |     |
|      | Position  |                                     |     | As  QUAPOS  | 4    | 2.2.3  |     |
PA
|     | approximate        |     |     | applicable  |      |        |     |
| --- | ------------------ | --- | --- | ----------- | ---- | ------ | --- |
|     |                    |     |     |             |      |        |     |
| 8   |                    |     |     |             |      |        |     |
|     | Position doubtful  |     |     | As  QUAPOS  | 5    | 2.2.3  |     |
PD
applicable

| 9-16  |     | No applicable information for ENCs  |     |     |     |     |     |
| ----- | --- | ----------------------------------- | --- | --- | --- | --- | --- |

Control Points

| 20  | Triangulation point  |     |     | CTRPNT  CATCTR  | 1  P  | 4.3  |     |
| --- | -------------------- | --- | --- | --------------- | ----- | ---- | --- |

| 21  | Observation spot  |     |     | CTRPNT  CATCTR  | 2  P  | 4.3  |     |
| --- | ----------------- | --- | --- | --------------- | ----- | ---- | --- |

| 22  | Fixed point  |     |     | CTRPNT  CATCTR  | 3  P  | 4.3  |     |
| --- | ------------ | --- | --- | --------------- | ----- | ---- | --- |

| 23  | Benchmark  |     |     | CTRPNT  CATCTR  | 4  P  | 4.3  |     |
| --- | ---------- | --- | --- | --------------- | ----- | ---- | --- |

| 24  | Boundary mark  |     |     | CTRPNT  CATCTR  | 5  P  | 4.3  |     |
| --- | -------------- | --- | --- | --------------- | ----- | ---- | --- |

Symbolised Positions (Examples)
30-33  Objects should be encoded in actual position using QUAPOS as appropriate

Units
| 40-54  |     | No applicable information for ENCs  |     |     |     |     |     |
| ------ | --- | ----------------------------------- | --- | --- | --- | --- | --- |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
Magnetic Compass

| 60  |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
Variation    MAGVAR  VALMAG    P/A/L  3.1.1  UKR1  (cid:13)
VALACM
RYRMGV
| 61-65  |     |     | No applicable information for ENCs  |     |     |     |     |     |
| ------ | --- | --- | ----------------------------------- | --- | --- | --- | --- | --- |
|        |     |     |                                     |     |     |     |     |     |
66  Variation    MAGVAR  VALACM    P/A/L  3.1.1    (cid:13)
VALACM
RYRMGV
67-68.2
No applicable information for ENCs

70  Compass  Roses,  True  and  Magnetic Variation      MAGVAR  VALMAG    P/A  3.1.1

Magnetic. 4°30´W 1998 (9´E) on
| magnetic  | north  arrow  | means  |     |     |     |     |     |     |
| --------- | ------------- | ------ | --- | --- | --- | --- | --- | --- |
VALACM
| Magnetic       | Variation  4°30´W  | in          |     |     |     |     |     |     |
| -------------- | ------------------ | ----------- | --- | --- | --- | --- | --- | --- |
| 1998,  annual  | change             | 9´E  (i.e.  |     |     |     |     |     |     |
RYRMGV
magnetic variation decreasing 9´
annually)
71  Isogonals (lines of      MAGVAR  VALMAG    L/A  3.1.1
equal magnetic
variation)  VALACM

RYRMGV

| 82.1  |     | Local Magnetic  |     |   LOCMAG  | VALLMA  |   A  | 3.1.2  |     |
| ----- | --- | --------------- | --- | --------- | ------- | ---- | ------ | --- |
Anomaly(cid:13) Within
|     |     | the enclosed area  |     |     |     |     |     |     |
| --- | --- | ------------------ | --- | --- | --- | --- | --- | --- |
the magnetic

variation may
deviate from the
normal by the value
shown.
| 82.2  |     | Where the area  |     |   LOCMAG  | VALLMA  |   P  | 3.1.2  |     |
| ----- | --- | --------------- | --- | --------- | ------- | ---- | ------ | --- |

| Local Magnetic Anomaly(cid:13) |        | (see  affected cannot       |     |     |         |     |     |     |
| ------------------------------ | ------ | --------------------------- | --- | --- | ------- | --- | --- | --- |
|                                | Note)  | be(cid:13) easily defined,  |     |     | TXTDSC  |     |     |     |
a legend only is
(cid:13) shown at the
position.
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
 IC Natural Features

Coastline        Foreshore     II, IJ
| 1   |     |     | Coastline, surveyed  |     |     | COALNE  | QUAPOS  | 1  L  | 4.5.1  |     |
| --- | --- | --- | -------------------- | --- | --- | ------- | ------- | ----- | ------ | --- |

| 2   |     |     |             |     |     |         |         |       |        |     |
| --- | --- | --- | ----------- | --- | --- | ------- | ------- | ----- | ------ | --- |
|     |     |     | Coastline,  |     |     | COALNE  | QUAPOS  | 2  L  | 4.5.1  |     |
unsurveyed
| 3   |     |     |              |     |     | COALNE  | CATCOA  | 1  L  |        |     |
| --- | --- | --- | ------------ | --- | --- | ------- | ------- | ----- | ------ | --- |
|     |     |     | Steep Coast  |     |     |         |         |       | 4.5.1  |     |
|     |     |     |              |     |     |         |         |       |        |     |

|     |     |     |     |     |     | SLOTOP  | CATSLO  | 6  L  | 4.7.5  |     |
| --- | --- | --- | --- | --- | --- | ------- | ------- | ----- | ------ | --- |
Cliffs
| 4   |     |     | Coastal  hillocks  |     |     | SLOGRD  | CATSLO  | 4  P/A  | 4.7.5  |     |
| --- | --- | --- | ------------------ | --- | --- | ------- | ------- | ------- | ------ | --- |

|     |     |     |     |     |     | SLOTOP  | CATSLO  | 4  L  | 4.7.5  |     |
| --- | --- | --- | --- | --- | --- | ------- | ------- | ----- | ------ | --- |

| 5   |     |     |              |     |     |         |         |       |        |     |
| --- | --- | --- | ------------ | --- | --- | ------- | ------- | ----- | ------ | --- |
|     |     |     | Flat coast   |     |     | COALNE  | CATCOA  | 2  L  | 4.5.1  |     |
| 6   |     |     |              |     |     |         |         |       |        |     |
|     |     |     | Sandy Shore  |     |     | COALNE  | CATCOA  | 3  L  | 4.5.1  |     |
|     |     |     | Stony Shore  |     |     | COALNE  | CATCOA  | 4  L  |        |     |
| 7   |     |     |              |     |     |         |         |       | 4.5.1  |     |
|     |     |     |              |     |     |         |         |       |        |     |

|     |     |     | Shingly Shore  |     |     | COALNE  | CATCOA  | 5  L    | 4.5.1  |     |
| --- | --- | --- | -------------- | --- | --- | ------- | ------- | ------- | ------ | --- |
| 8   |     |     |                |     |     |         |         |         |        |     |
|     |     |     | Sand hills     |     |     | SLOGRD  | CATSLO  | 3  P/A  | 4.7.5  |     |
|     |     |     |                |     |     |         | NATSUR  | 4       |        |     |

|     |     |     |  Dunes  |     |     | SLOTOP  | CATSLO  | 4  L  | 4.7.5  |     |
| --- | --- | --- | ------- | --- | --- | ------- | ------- | ----- | ------ | --- |

|     |     |     |     |     |     |     | NATSUR  | 4   |     |     |
| --- | --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

Relief         Plane of Reference for Heights     IH
|     |     |     | Contour lines  |     |     |           |         |      |        |     |
| --- | --- | --- | -------------- | --- | --- | --------- | ------- | ---- | ------ | --- |
| 10  |     |     |                |     |     | LNDELV    | ELEVAT  |   L  | 4.7.2  |     |
| 11  |     |     | Spot heights   |     |     |   LNDELV  | ELEVAT  |   P  | 4.7.2  |     |
|     |     |     | Approximate    |     |     |           |         |      |        |     |
| 12  |     |     |                |     |     | LNDELV    | ELEVAT  |   L  | 4.7.2  |     |
contour lines
|     |     |     |     |     |     |     | QUAPOS  | 4   |     |     |
| --- | --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |

| 13  |     |     | Form lines with  |     |     |         |         |      |        |     |
| --- | --- | --- | ---------------- | --- | --- | ------- | ------- | ---- | ------ | --- |
|     |     |     |                  |     |     | LNDELV  | ELEVAT  |   L  | 4.7.2  |     |
spot height
|     |     |     | Approximate  |     |     |         |         |              |     |     |
| --- | --- | --- | ------------ | --- | --- | ------- | ------- | ------------ | --- | --- |
| 14  |     |     |              |     |     | VEGATN  | CATVEG  | 6/13  L/P/A  |     |     |
height of top of
|     |     |     | trees  |     |     |     | HEIGHT  |     | 4.7.11  |     |
| --- | --- | --- | ------ | --- | --- | --- | ------- | --- | ------- | --- |
(above height
datum)

Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs

Water Features, Lava
| 20  |     |     | River, Stream  |     |   RIVERS   | OBJNAM  |   L/A  | 4.7.6  |     |
| --- | --- | --- | -------------- | --- | ---------- | ------- | ------ | ------ | --- |

(non navigable)
|     |     |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |

| 21  |     |     |                     |     |           |         |          |          |     |
| --- | --- | --- | ------------------- | --- | --------- | ------- | -------- | -------- | --- |
|     |     |     | Intermittent river  |     |   RIVERS  | OBJNAM  | 5  L/A   | 4.7.6    |     |
|     |     |     |                     |     |           | STATUS  | 5        |          |     |
| 22  |     |     | Rapids              |     |   RAPIDS  |         |   P/L/A  | 4.7.7.1  |     |
|     |     |     | Waterfalls          |     |   WATFAL  |         |   P/L    | 4.7.7.2  |     |
| 23  |     |     |                     |     |           |         |          |          |     |
|     |     |     | Lakes               |     |   LAKARE  | OBJNAM  |   A      | 4.7.8    |     |
(non navigable)
| 24  |     |     | Salt pans  |     |   LNDRGN  | CATLND  | 15  A  |     |     |
| --- | --- | --- | ---------- | --- | --------- | ------- | ------ | --- | --- |

|     |     |     |     |     |         | OBJNAM  |       | 4.7.9  |     |
| --- | --- | --- | --- | --- | ------- | ------- | ----- | ------ | --- |
|     |     |     |     |     |         |         |       |        |     |
|     |     |     |     |     | COALNE  | CATCOA  | 2  L  |        |     |
4.7.9
| 25  |     |     | Glacier  |     |   ICEARE  | CATICE  | 5  A  | 4.7.10  |     |
| --- | --- | --- | -------- | --- | --------- | ------- | ----- | ------- | --- |

|     |     |     |            |     | COALNE    | CATCOA  | 6  L   | 4.7.10  |     |
| --- | --- | --- | ---------- | --- | --------- | ------- | ------ | ------- | --- |
| 26  |     |     | Lava flow  |     |   LNDRGN  | CATLND  | 14  A  | 4.7.12  |     |
OBJNAM

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

Vegetation
|     |     |     | Wood in general  |     |   VEGATN   | CATVEG  | 6  P/A  | 4.7.11  |     |
| --- | --- | --- | ---------------- | --- | ---------- | ------- | ------- | ------- | --- |
| 30  |     |     |                  |     |            |         |         |         |     |
31    Prominent isolated      VEGATN  CATVEG  6  P/A  4.7.11
tree
  CONVIS
Deciduous tree, or    VEGATN  CATVEG  4/20   A/P   4.7.11
| 31.1  |     |     |     |     |     |     |     |     |     |
| ----- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
unspecified tree.
  CONVIS
Evergreen (except    VEGATN  CATV  EG  14   A/P   4.7.11
| 31.2  |     |     |     |     |     |     |     |     |     |
| ----- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
conifer)
CONVIS
| 31.3  |     |     | Conifer  |     |   VEGATN  | CATVEG  | 5/15   A/P   | 4.7.11  |     |
| ----- | --- | --- | -------- | --- | --------- | ------- | ------------ | ------- | --- |
CONVIS
|       |     |     | Palm  |     |   VEGATN  | CATV  EG  | A/P   |         |     |
| ----- | --- | --- | ----- | --- | --------- | --------- | ----- | ------- | --- |
| 31.4  |     |     |       |     |           |           | 16    | 4.7.11  |     |
CONVIS
|       |     |     | Nipa palm  |     |   VEGATN  | CATVEG  | 17   A/P   | 4.7.11  |     |
| ----- | --- | --- | ---------- | --- | --------- | ------- | ---------- | ------- | --- |
| 31.5  |     |     |            |     |           |         |            |         |     |
CONVIS
|       |     |     | Casuarina  |     |   VEGATN  | CATVEG  | 18   A/P   | 4.7.11  |     |
| ----- | --- | --- | ---------- | --- | --------- | ------- | ---------- | ------- | --- |
| 31.6  |     |     |            |     |           |         |            |         |     |
CONVIS
| 31.7  |     |     | Filao  |     |   VEGATN  | CATVEG  | 22   A/P   | 4.7.11  |     |
| ----- | --- | --- | ------ | --- | --------- | ------- | ---------- | ------- | --- |
CONVIS
|       |     |     | Eucalypt  |     |   VEGATN  | CATVEG  | A/P   |         |     |
| ----- | --- | --- | --------- | --- | --------- | ------- | ----- | ------- | --- |
| 31.8  |     |     |           |     |           |         | 19    | 4.7.11  |     |
CONVIS
|     |     |     | Mangrove       |     |   COALNE  | CATCOA  | 7   L   | 4.5.1   |     |
| --- | --- | --- | -------------- | --- | --------- | ------- | ------- | ------- | --- |
| 32  |     |     |                |     |           |         |         |         |     |
|     |     |     |                |     |           | QUAPOS  | 4       |         |     |
|     |     |     |                |     | VEGATN    | CATVEG  | 7  A    | 4.7.11  |     |
|     |     |     | Marsh, Swamp,  |     |   LNDRGN  | CATLND  | 7  A/P  |         |     |
| 33  |     |     |                |     |           |         |         | 4.7.3   |     |
Saltmarsh
OBJNAM

|     |     |     |     |     | COALNE  | CATCOA  | 8  L  |        |     |
| --- | --- | --- | --- | --- | ------- | ------- | ----- | ------ | --- |
|     |     |     |     |     |         | QUAPOS  | 4     | 4.7.3  |     |

Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs

ID Cultural Features

Settlements, Buildings      Height of objects   IE      Landmarks   IE

| 1   |     |     | Urban Area  |     |     |   BUAARE  | CATBUA  | 1  A  | 4.8.14  |     |
| --- | --- | --- | ----------- | --- | --- | --------- | ------- | ----- | ------- | --- |

OBJNAM
| 2   |     |     | Settlement with  |     |     |   BUAARE  | CATBUA  | 2  A  | 4.8.14  |     |
| --- | --- | --- | ---------------- | --- | --- | --------- | ------- | ----- | ------- | --- |

scattered buildings
OBJNAM
| 3   |     |     | Settlement (on  |     |     |   BUAARE  | CATBUA  | 2  P  | 4.8.14  |     |
| --- | --- | --- | --------------- | --- | --- | --------- | ------- | ----- | ------- | --- |

med. and small
|     |     |     | scale charts)   |     |     |           | OBJNAM  |         |         |     |
| --- | --- | --- | --------------- | --- | --- | --------- | ------- | ------- | ------- | --- |
| 4   |     |     | Inland Village  |     |     |   BUAARE  | CATBUA  | 2/3  P  | 4.8.14  |     |

|     |     |     |           |     |     |           | OBJNAM  | NB     |         |     |
| --- | --- | --- | --------- | --- | --- | --------- | ------- | ------ | ------- | --- |
| 5   |     |     | Building  |     |     |   BUISGL  |         |   P/A  | 4.8.15  |     |

| 6   |     |     |                     |     |     |           |         |        |         |     |
| --- | --- | --- | ------------------- | --- | --- | --------- | ------- | ------ | ------- | --- |
|     |     |     | Important building  |     |     |   BUISGL  | OBJNAM  |   P/A  | 4.8.15  |     |
in built-up area
FUNCTN
CONVIS
| 7   |     |     |                    |     |     |           |         |      |        |     |
| --- | --- | --- | ------------------ | --- | --- | --------- | ------- | ---- | ------ | --- |
|     |     |     | Street name, Road  |     |     |   ROADWY  | CATROD  |   L  | 4.8.8  |     |
name
OBJNAM
| 8   |     |     | Ruin, Ruined  |     |     |   LNDMRK  |         | 2  P/A  |         |     |
| --- | --- | --- | ------------- | --- | --- | --------- | ------- | ------- | ------- | --- |
|     |     |     |               |     |     |           | CATLMK  |         | 4.8.15  |     |
landmark
  CONVIS
BUISGL  CONDTN

ID 4 Individual buildings as depicted should be captured as per ID6

Roads, Railways, Airfields
| 10  |     |     | Motorway  |     |     |   ROADWY  | CATROD  | 1  L  | 4.8.8  |     |
| --- | --- | --- | --------- | --- | --- | --------- | ------- | ----- | ------ | --- |

| 11  |     |     |             |     |     |           |         |      |        |     |
| --- | --- | --- | ----------- | --- | --- | --------- | ------- | ---- | ------ | --- |
|     |     |     | Road (hard  |     |     |   ROADWY  | CATROD  |   L  | 4.8.8  |     |
surfaced)
| 12  |     |     |                     |     |     | ROADWY  | CATROD  | 4  L  | 4.8.8  |     |
| --- | --- | --- | ------------------- | --- | --- | ------- | ------- | ----- | ------ | --- |
|     |     |     | Track, Path (loose  |     |     |         |         |       |        |     |
|     |     |     | or unsurfaced)      |     |     |         |         |       |        |     |
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
| 13  |     |                |     |     |           |     | L   | 4.8.2  |     |
| --- | --- | -------------- | --- | --- | --------- | --- | --- | ------ | --- |
|     |     | Railway, with  |     |     |   RAILWY  |     |     |        |     |
station
|     |     |          |     |     | BUISGL    | FUNCTN  | 8  P      |        |     |
| --- | --- | -------- | --- | --- | --------- | ------- | --------- | ------ | --- |
| 14  |     | Cutting  |     |     |   SLOGRD  | CATSLO  | 1  L/P/A  | 4.8.4  |     |

| 15  |     | Embankment         |     |     |   SLOTOP  | CATSLO  | 2  L     | 4.8.4   |     |
| --- | --- | ------------------ | --- | --- | --------- | ------- | -------- | ------- | --- |
| 16  |     |                    |     |     |           |         |          |         |     |
|     |     | Tunnel             |     |     |   TUNNEL  |         |   L/A    | 4.8.3   |     |
| 17  |     |                    |     |     | RUNWAY    | CATRUN  |   P/L/A  | 4.8.12  |     |
|     |     | Airport, Airfield  |     |     |           |         |          |         |     |
|     |     |                    |     |     |           |         |          |         |     |
|     |     |                    |     |     | AIRARE    | CATAIR  |   P/A    | 4.8.12  |     |

Other Cultural Features
20      Vertical clearance      BRIDGE  CATBRG    L/A  4.8.10
above High Water
|     |     |                    |     |     |           | VERCLR  |         |         |     |
| --- | --- | ------------------ | --- | --- | --------- | ------- | ------- | ------- | --- |
| 21  |     |                    |     |     |           |         |         |         |     |
|     |     | Horizontal         |     |     |   BRIDGE  | CATBRG  |   L/A   | 4.8.10  |     |
|     |     | clearance          |     |     |           | HORCLR  |         | 2.2.42  |     |
|     |     |                    |     |     |           | HORACC  |         | 2.2.42  |     |
| 22  |     | Fixed Bridge with  |     |     |   BRIDGE  | CATBRG  | 1  L/A  | 4.8.10  |     |

vertical clearance
|       |     |                 |     |     |         | VERCLR  |     |         |     |
| ----- | --- | --------------- | --- | --- | ------- | ------- | --- | ------- | --- |
| 23.1  |     | Opening bridge  |     |     |         | CATBRG  | 2   | 4.8.10  |     |
|       |     |                 |     |     | BRIDGE  |         |     |         |     |
(in general)
|     |     |     |     |     |     | VERCCL  |     |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |
with vertical
|     |     | clearance  |     |     |     | VERCOP  |     |     |     |
| --- | --- | ---------- | --- | --- | --- | ------- | --- | --- | --- |
23.2    Swing bridge with    BRIDGE  CATBRG  3    4.8.10

vertical clearance
|       |     |                      |     |     |           | VERCCL  |      |         |     |
| ----- | --- | -------------------- | --- | --- | --------- | ------- | ---- | ------- | --- |
| 23.3  |     |                      |     |     |           | CATBRG  | 4    | 4.8.10  |     |
|       |     | Lifting bridge with  |     |     |   BRIDGE  |         | L/A  |         |     |
|       |     | vertical clearance   |     |     |           | VERCCL  |      |         |     |
(closed and open)
VERCOP
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 23.4  |     |                      |     |           |         |         |         |     |
| ----- | --- | -------------------- | --- | --------- | ------- | ------- | ------- | --- |
|       |     | Bascule bridge with  |     |   BRIDGE  | CATBRG  | 5  L/A  | 4.8.10  |     |
|       |     | vertical clearance   |     |           | VERCCL  |         |         |     |
|       |     |                      |     |           | VERCOP  | nn.n    |         |     |
23.5    Pontoon Bridge    BRIDGE  CATBRG  6  L/A  4.8.10

|       |     |                   |     |           | VERCLR  |         |         |     |
| ----- | --- | ----------------- | --- | --------- | ------- | ------- | ------- | --- |
| 23.6  |     |                   |     |           |         |         |         |     |
|       |     | Draw bridge with  |     |   BRIDGE  | CATBRG  | 7  L/A  | 4.8.10  |     |
vertical clearance
|     |     |     |     |     | VERCLR  |     |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |

24  Transporter bridge with      BRIDGE  CATBRG  8  L/A  4.8.10
vertical clearance
|     |     |     |     |     | VERCLR  |     |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |
between HW and lowest
part of structure
| 25  |     | Overhead  |     |   CONVYR  | VERCLR  | nn.n  L  | 4.8.11  |     |
| --- | --- | --------- | --- | --------- | ------- | -------- | ------- | --- |

transporter, Aerial
|     |     | cableway with  |     |     |     |     |     |     |
| --- | --- | -------------- | --- | --- | --- | --- | --- | --- |
vertical clearance
| 26  |     |                       |     |           | CATCBL  | 1  L  | 11.5.2  |     |
| --- | --- | --------------------- | --- | --------- | ------- | ----- | ------- | --- |
|     |     | Power transmission    |     |   CBLOHD  |         |       |         |     |
|     |     | line with pylons and  |     |           | VERCSA  |       |         |     |
|     |     | safe overhead         |     |           | VERCLR  |       |         |     |
clearance
|     |     |     |     | PYLONS  | CATPYL  | 1  P/A  | 4.8.18  |     |
| --- | --- | --- | --- | ------- | ------- | ------- | ------- | --- |

| 27  |     |                  |     |           | CATCBL  | 3/4  L  | 11.5.2  |     |
| --- | --- | ---------------- | --- | --------- | ------- | ------- | ------- | --- |
|     |     | Overhead cable,  |     |   CBLOHD  |         |         |         |     |
|     |     | Telephone        |     |           | VERCSA  |         |         |     |
line,Telegraph line
VERCLR
with vertical
clearance
| 28  |     | Overhead pipe with  |     |   PIPOHD  | CATPIP  |   L  | 11.6.3  |     |
| --- | --- | ------------------- | --- | --------- | ------- | ---- | ------- | --- |

vertical clearance
VERCLR
| 29  |     |                   |     |           |         |      | 11.5.2  |     |
| --- | --- | ----------------- | --- | --------- | ------- | ---- | ------- | --- |
|     |     | Pipeline on land  |     |   PIPSOL  | CATPIP  |   L  |         |     |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
Landmarks  IE

General         Plane of Reference for Heights    IH       Lighthouses   IP        Beacons   IQ
| 1   |     |     |              |     |           |         |      |         |     |
| --- | --- | --- | ------------ | --- | --------- | ------- | ---- | ------- | --- |
|     |     |     | Examples of  |     |   LNDMRK  | CATLMK  |   P  | 4.8.15  |     |
landmarks
CONVIS

| 2   |     |     | Conspicuous  |     |   LNDMRK  | CATLMK  |   P  | 4.8.15  |     |
| --- | --- | --- | ------------ | --- | --------- | ------- | ---- | ------- | --- |
landmarks
|     |     |     |     |     |     | CONVIS  | 1   |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |

| 3.1  |     |     |                    |     |           |         |      |         |     |
| ---- | --- | --- | ------------------ | --- | --------- | ------- | ---- | ------- | --- |
|      |     |     | Pictorial symbols  |     |   LNDMRK  | CATLMK  |   P  | 4.8.15  |     |
(in true position)
|     |     |     |     |     |     | CONVIS  |     | 4.8.20  |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | ------- | --- |
PICREP
| 3.2  |     |     |                  |     |           |         |      |         |     |
| ---- | --- | --- | ---------------- | --- | --------- | ------- | ---- | ------- | --- |
|      |     |     | Sketches, Views  |     |   LNDMRK  | CATLMK  |   P  | 4.8.15  |     |
(out of position)
|     |     |     |     |     |     | CONVIS  |     | 4.8.20  |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | ------- | --- |
PICREP
| 4   |     |     |                     |     |           |         |      |         |     |
| --- | --- | --- | ------------------- | --- | --------- | ------- | ---- | ------- | --- |
|     |     |     | Height of top of a  |     |   LNDMRK  | CATLMK  |   P  | 4.8.15  |     |
structure above
|     |     |     |     |     |     | CONVIS  |     | 2.1.2  |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | ------ | --- |
height datum
HEIGHT
| 5   |     |     |                     |     |           |         |      |         |     |
| --- | --- | --- | ------------------- | --- | --------- | ------- | ---- | ------- | --- |
|     |     |     | Height of top of a  |     |   LNDMRK  | CATLMK  |   P  | 4.8.15  |     |
structure above
|     |     |     |     |     |     | CONVIS  |     | 4.2.2  |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | ------ | --- |
ground level
VERLEN

Landmarks
| 10.1  |     |     |               |     |           |         |          |         |     |
| ----- | --- | --- | ------------- | --- | --------- | ------- | -------- | ------- | --- |
|       |     |     | Church        |     |   BUISGL  | FUNCTN  | 20  P/A  | 4.8.15  |     |
| 10.2  |     |     | Church tower  |     |   LNDMRK  | CATLMK  | 17  P    | 4.8.15  |     |

|       |     |     |               |     |         | FUNCTN  | 20     |         |     |
| ----- | --- | --- | ------------- | --- | ------- | ------- | ------ | ------- | --- |
|       |     |     |               |     |         | CONVIS  |        |         |     |
| 10.3  |     |     |               |     | LNDMRK  | CATLMK  | 20  P  |         |     |
|       |     |     | Church spire  |     |         |         |        | 4.8.15  |     |
|       |     |     |               |     |         | FUNCTN  |        |         |     |

|       |     |     |                |     |         | CONVIS  |        |         |     |
| ----- | --- | --- | -------------- | --- | ------- | ------- | ------ | ------- | --- |
| 10.4  |     |     |                |     | LNDMRK  | CATLMK  | 15  P  |         |     |
|       |     |     | Church cupola  |     |         |         |        | 4.8.15  |     |
|       |     |     |                |     |         | FUNCTN  |        |         |     |

|     |     |     |     |     |     | CONVIS  |     |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 11  |     |     |                 |     |           |         |        |         |     |
| --- | --- | --- | --------------- | --- | --------- | ------- | ------ | ------- | --- |
|     |     |     | Chapel          |     |   BUISGL  | FUNCTN  | 21  P  | 4.8.15  |     |
| 12  |     |     | Cross, calvary  |     |   LNDMRK  | CATLMK  | 14  P  | 4.8.15  |     |

CONVIS
| 13  |     |     |                 |     |           |         |        |         |     |
| --- | --- | --- | --------------- | --- | --------- | ------- | ------ | ------- | --- |
|     |     |     | Temple          |     |   BUISGL  | FUNCTN  | 22  P  | 4.8.15  |     |
| 14  |     |     |                 |     |           |         |        |         |     |
|     |     |     | Pagoda          |     |   BUISGL  | FUNCTN  | 23  P  | 4.8.15  |     |
| 15  |     |     |                 |     |           |         |        |         |     |
|     |     |     | Shinto shrine,  |     |   BUISGL  | FUNCTN  | 24  P  | 4.8.15  |     |
Josshouse
| 16  |     |     |                  |     |           |         |        |         |     |
| --- | --- | --- | ---------------- | --- | --------- | ------- | ------ | ------- | --- |
|     |     |     | Buddhist temple  |     |   BUISGL  | FUNCTN  | 25  P  | 4.8.15  |     |
| 17  |     |     |                  |     |           |         |        |         |     |
|     |     |     | Mosque,          |     |   BUISGL  | FUNCTN  | 26  P  | 4.8.15  |     |
|     |     |     | Minaret          |     | LNDMRK    | CATLMK  | 20     |         |     |
|     |     |     |                  |     |           | FUNCTN  | 26     |         |     |
| 18  |     |     | Marabout         |     |   BUISGL  | FUNCTN  | 27  P  | 4.8.15  |     |

| 19  |     |     | Cemetery (all  |     |   LNDMRK  | CATLMK  | 2  P/L/A  | 4.8.15  |     |
| --- | --- | --- | -------------- | --- | --------- | ------- | --------- | ------- | --- |

|     |     |     | religious       |     |           | CONVIS  |        |         |     |
| --- | --- | --- | --------------- | --- | --------- | ------- | ------ | ------- | --- |
|     |     |     | denominations)  |     |           |         |        |         |     |
| 20  |     |     | Tower           |     |   LNDMRK  | CATLMK  | 17  P  | 4.8.15  |     |

|     |     |     |                     |     |         | CONVIS  |          |         |     |
| --- | --- | --- | ------------------- | --- | ------- | ------- | -------- | ------- | --- |
| 21  |     |     |                     |     | SILTNK  | CATSIL  | 4     P  |         |     |
|     |     |     | Water tower, Water  |     |         |         |          | 4.8.15  |     |
|     |     |     | tank on a tower     |     |         | PRODCT  | 3/8      |         |     |
| 22  |     |     |                     |     | LNDMRK  | CATLMK  | 3  P     |         |     |
|     |     |     | Chimney             |     |         |         |          | 4.8.15  |     |
|     |     |     |                     |     |         | CONVIS  |          |         |     |
| 23  |     |     |                     |     | LNDMRK  | CATLMK  | 6   P    |         |     |
|     |     |     | Flare stack (on     |     |         |         |          | 4.8.15  |     |
|     |     |     |                     |     |         | CONVIS  |          |         |     |
land)
| 24    |     |     |           |     | LNDMRK  | CATLMK  | 9   P   |         |     |
| ----- | --- | --- | --------- | --- | ------- | ------- | ------- | ------- | --- |
|       |     |     | Monument  |     |         |         |         | 4.8.15  |     |
|       |     |     |           |     |         | CONVIS  |         |         |     |
| 25.1  |     |     |           |     | LNDMRK  | CATLMK  | 1 8  P  |         |     |
|       |     |     | Windmill  |     |         |         |         | 4.8.15  |     |
|       |     |     |           |     |         | CONVIS  |         |         |     |
25.2    Windmill (without    LNDMRK  CATL MK  18    P  4.8.15

|     |     |     |     |     |     | CONVIS  |     |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |
sails)
|     |     |     |            |     |         | CONDTN  | 4      |         |     |
| --- | --- | --- | ---------- | --- | ------- | ------- | ------ | ------- | --- |
|     |     |     |            |     | LNDMRK  | CATLMK  | 19  P  |         |     |
| 26  |     |     | Windmotor  |     |         |         |        | 4.8.15  |     |
|     |     |     |            |     |         | CONVIS  |        |         |     |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

|     |     |     |                      |     | LNDMRK  | CATLMK  | 5  P  |         |     |
| --- | --- | --- | -------------------- | --- | ------- | ------- | ----- | ------- | --- |
| 27  |     |     | Flagstaff, Flagpole  |     |         |         |       | 4.8.15  |     |
|     |     |     |                      |     |         | CONVIS  |       |         |     |
7
| 28  |     |     | Radio mast,  |     |   LNDMRK  | CATLMK  | P   | 4.8.15  |     |
| --- | --- | --- | ------------ | --- | --------- | ------- | --- | ------- | --- |

|       |     |     | Television mast   |     |           | CONVIS  |         |         |     |
| ----- | --- | --- | ----------------- | --- | --------- | ------- | ------- | ------- | --- |
|       |     |     |                   |     |           | FUNCTN  | 31/30   |         |     |
| 29    |     |     |                   |     | LNDMRK    | CATLMK  | 17  P   |         |     |
|       |     |     | Radio tower,      |     |           |         |         | 4.8.15  |     |
|       |     |     | Television tower  |     |           | CONVIS  |         |         |     |
|       |     |     |                   |     |           | FUNCTN  | 31/30   |         |     |
| 30.1  |     |     |                   |     | LNDMRK    | CATLMK  | 7  P    |         |     |
|       |     |     | Radar mast        |     |           |         |         | 4.8.15  |     |
|       |     |     |                   |     |           | CONVIS  |         |         |     |
|       |     |     |                   |     |           | FUNCTN  | 32      |         |     |
|       |     |     | Radar tower       |     | LNDMRK    | CATLMK  | 17  P   |         |     |
| 30.2  |     |     |                   |     |           |         |         | 4.8.15  |     |
|       |     |     |                   |     |           | CONVIS  |         |         |     |
|       |     |     |                   |     |           | FUNCTN  | 32      |         |     |
| 30.3  |     |     | Radar scanner     |     |   LNDMRK  | CATLMK  | 16  P   |         |     |
|       |     |     |                   |     |           |         |         | 4.8.15  |     |
|       |     |     |                   |     |           | CONVIS  |         |         |     |
| 30.4  |     |     | Radar dome        |     |   LNDMRK  | CATLMK  | 15   P  |         |     |
|       |     |     |                   |     |           |         |         | 4.8.15  |     |
|       |     |     |                   |     |           | CONVIS  |         |         |     |
|       |     |     |                   |     |           | FUNCTN  | 32      |         |     |
|       |     |     | Dish Aerial       |     |           |         |         |         |     |
| 31    |     |     |                   |     | LNDMRK    | CATLMK  | 4  P    | 4.8.15  |     |

|     |     |     |        |     |           | CONVIS  |            |         |     |
| --- | --- | --- | ------ | --- | --------- | ------- | ---------- | ------- | --- |
| 32  |     |     | Tanks  |     |   SILTNK  | CATSIL  | 2     P/A  |         |     |
|     |     |     |        |     |           |         |            | 4.8.15  |     |
|     |     |     |        |     | PRDARE    | CATPRA  | 8  P/A     | 4.8.15  |     |
| 33  |     |     |        |     | SILTNK    | CATSIL  | 1  P/A     |         |     |
|     |     |     | Silo   |     |           |         |            | 4.8.15  |     |
34.1    Fortified structure    FORSTC  CATFOR  1/2/4  P/A  4.8.17
|       |     |     |                |     |           |         |      |         |     |
| ----- | --- | --- | -------------- | --- | --------- | ------- | ---- | ------- | --- |
| 34.2  |     |     | Castle, Fort,  |     |   FORSTC  | CATFOR  |   P  | 4.8.17  |     |
|       |     |     |                |     |           |         |      |         |     |
Blockhouse

34.3    Battery, Small Fort    FORSTC  CATFOR  2/3  P  4.8.17

| 35.1  |     |     | Quarry   |     |   PRDARE  | CATPRA  | 1  A  | 4.8.13  |     |
| ----- | --- | --- | -------- | --- | --------- | ------- | ----- | ------- | --- |

|       |     |     |          |     | SLOGRD    |         | A     |         |     |
| ----- | --- | --- | -------- | --- | --------- | ------- | ----- | ------- | --- |
|       |     |     |          |     | SLOTOP    |         | L     |         |     |
| 35.2  |     |     | Quarry   |     |   PRDARE  | CATPRA  | 1  P  | 4.8.13  |     |
|       |     |     |          |     |           |         |       |         |     |
| 36    |     |     | Mine     |     |   PRDARE  | CATPRA  | 2  P  | 4.8.13  |     |
|       |     |     |          |     |           |         |       |         |     |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
IF Ports

Artificial Features
| 1    |     |     | Dyke, Levees         |     |   DYKCON  |         |   L/A  | 4.8.7  |     |
| ---- | --- | --- | -------------------- | --- | --------- | ------- | ------ | ------ | --- |
|      |     |     |                      |     |           |         |        |        |     |
| 2.1  |     |     |                      |     | SLCONS    | CATSLC  | 10  A  | 4.5.2  |     |
|      |     |     | Seawall (on large-   |     |           |         |        |        |     |
|      |     |     | scale charts)        |     |           | WATLEV  | 2/4    |        |     |
| 2.2  |     |     |                      |     |           |         |        |        |     |
|      |     |     | Seawall (on smaller- |     |   SLCONS  | CATSLC  | 10  L  | 4.5.2  |     |
scale charts)
|     |     |     |     |     |     | WATLEV  | 2   |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |

| 3    |     |     |                |     | CAUSWY    | WATLEV  | 2/4  L/A  | 4.8.9  |     |
| ---- | --- | --- | -------------- | --- | --------- | ------- | --------- | ------ | --- |
|      |     |     | Causeway       |     |           |         |           |        |     |
| 4.1  |     |     | Breakwater in  |     |   SLCONS  | CATSLC  | 1  L/A    | 4.5.2  |     |

|     |     |     | general  |     |     | WATLEV  | 2   |     |     |
| --- | --- | --- | -------- | --- | --- | ------- | --- | --- | --- |

| 4.2  |     |     |                      |     | SLCONS  | CATSLC  | 1  A  | 4.5.2  |     |
| ---- | --- | --- | -------------------- | --- | ------- | ------- | ----- | ------ | --- |
|      |     |     | Breakwater (loose    |     |         |         |       |        |     |
|      |     |     | boulders, tetrapods  |     |         | WATLEV  | 3/4   |        |     |
|      |     |     | etc.)                |     |         | NATCON  | 3     |        |     |
| 4.3  |     |     |                      |     | SLCONS  | CATSLC  | 1  A  | 4.5.2  |     |
|      |     |     | Breakwater (slope    |     |         |         |       |        |     |
|      |     |     | of concrete or       |     |         | WATLEV  | 4     |        |     |
masonry)
|     |     |     |                |     |           | NATCON  | 1/2     |        |     |
| --- | --- | --- | -------------- | --- | --------- | ------- | ------- | ------ | --- |
| 5   |     |     |                |     |           |         |         |        |     |
|     |     |     | Training wall  |     |   SLCONS  | CATSLC  | 7  L/A  |        |     |
|     |     |     |                |     |           | WATLEV  | 2       | 4.5.2  |     |
|     |     |     |                |     |           | WATLEV  | 4 NB    |        |     |
|     |     |     |                |     |           | WATLEV  | 3 NB    |        |     |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

|      |     |     | Groyne (always  |     |         |         |         |        |     |
| ---- | --- | --- | --------------- | --- | ------- | ------- | ------- | ------ | --- |
| 6.1  |     |     |                 |     | SLCONS  | CATSLC  | 2  L/A  | 4.5.2  |     |

dry)
|     |     |     |     |     |     | WATLEV  | 2   |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |
6.2  Groyne (intertidal)       SLCONS  CATSLC  2  L/A  4.5.2
|      |     |     |                 |     |         | WATLEV  | 4       |        |     |
| ---- | --- | --- | --------------- | --- | ------- | ------- | ------- | ------ | --- |
|      |     |     |                 |     | SLCONS  | CATSLC  | 2  L/A  | 4.5.2  |     |
| 6.3  |     |     | Groyne (always  |     |         |         |         |        |     |
|      |     |     | underwater)     |     |         | WATLEV  | 3       |        |     |

Harbour Installations     Depths   II     Anchorages   IN     Beacons and other fixed marks    Marina   IU
| 10  |     |     |                      |     | HRBFAC    | CATHAF  | 4  P/A   | 4.6.1  |     |
| --- | --- | --- | -------------------- | --- | --------- | ------- | -------- | ------ | --- |
|     |     |     | Fishing harbour      |     |           |         |          |        |     |
| 12  |     |     |                      |     |           |         |          |        |     |
|     |     |     | Mole (with berthing  |     |   SLCONS  | CATSLC  | 3  L/A   | 4.5.2  |     |
|     |     |     | facility)            |     |           | WATLEV  | 2        |        |     |
|     |     |     |                      |     | BERTHS    | OBJNAM  |   P/L/A  | 4.6.2  |     |

| 13  |     |          |                 |     | SLCONS    | CATSLC  | 6  L/A  | 4.5.2    |     |
| --- | --- | -------- | --------------- | --- | --------- | ------- | ------- | -------- | --- |
|     |     |          | Quay, Wharf     |     |           |         |         |          |     |
|     |     |          |                 |     |           | WATLEV  | 2       |          |     |
| 14  |     |          |                 |     | SLCONS    | CATSLC  | 4  L/A  | 4.5.2    |     |
|     |     |          | Pier, Jetty     |     |           |         |         |          |     |
|     |     |          |                 |     |           | WATLEV  | 2       |          |     |
| 15  |     |          |                 |     |           |         |         |          |     |
|     |     |          | Promenade pier  |     |   SLCONS  | CATSLC  | 5  L/A  | 4.5.2    |     |
|     |     |          |                 |     |           | WATLEV  | 2       |          |     |
| 16  |     |          |                 |     | PONTON    |         |   L/A   | 4.6.7.3  |     |
|     |     | Pontoon  | Pontoon         |     |           |         |         |          |     |
17      Landing for boats    SLCONS  CATSLC  4/12  L/A  4.5.2

|     |     |     |     |     |         | WATLEV  | 4/2/3  |     |     |
| --- | --- | --- | --- | --- | ------- | ------- | ------ | --- | --- |
|     |     |     |     |     | SMCFAC  | CATSCF  | 28     |     |     |
18    Steps, Landing      SLCONS  CATSLC  11  L/A  4.5.2
Steps
|     |     |     | stairs          |     |         | WATLEV  | 1      |        |     |
| --- | --- | --- | --------------- | --- | ------- | ------- | ------ | ------ | --- |
| 19  |     |     |                 |     | BERTHS  | OBJNAM  |   P/L  | 4.6.2  |     |
|     |     |     | Designation of  |     |         |         |        |        |     |
berth
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 20  |     |     |                    |          |     |     |         |         |     |               |     |
| --- | --- | --- | ------------------ | -------- | --- | --- | ------- | ------- | --- | ------------- | --- |
|     |     |     |                    | Dolphin  |     |     | MORFAC  | CATMOR  | 1   | P/A  4.6.7.1  |     |
|     |     |     |                    |          |     |     |         | WATLEV  | 2   |               |     |
| 21  |     |     |                    |          |     |     |         |         |     |               |     |
|     |     |     | Deviation dolphin  |          |     |     | MORFAC  | CATMOR  | 2   | P  4.6.7.1    |     |
|     |     |     |                    |          |     |     |         | WATLEV  | 2   |               |     |
22    Minor post or pile    MORFAC  CATMOR  5  P/A  4.6.7.1

|     |     |           |                    |     |     |     | PILPNT  | CATPLE  |        | P  4.6.7.2  |     |
| --- | --- | --------- | ------------------ | --- | --- | --- | ------- | ------- | ------ | ----------- | --- |
| 23  |     |           |                    |     |     |     | SLCONS  | CATSLC  |        | L/A         |     |
|     |     |           | Slipway, Patent    |     |     |     |         |         | 13/12  |             |     |
|     |     |           | slip, Ramp         |     |     |     |         | WATLEV  | 2      | 4.5.2       |     |
|     |     |           |                    |     |     |     |         | WATLEV  | 4 NB   |             |     |
|     |     |           |                    |     |     |     |         |         |        |             |     |
|     |     |           |                    |     |     |     |         | WATLEV  | 3 NB   |             |     |
| 24  |     |           |                    |     |     |     | GRIDRN  | WATLEV  | 4      | A  4.6.6.6  |     |
|     |     | Gridiron  |                    |     |     |     |         |         |        |             |     |
| 25  |     |           |                    |     |     |     | DRYDOC  |         |        | A  4.6.6.1  |     |
|     |     |           | Dry dock, Graving  |     |     |     |         |         |        |             |     |
Dry Dock
dock
| 26  |     |     | Floating dock  |     |     |     | FLODOC  |     |     | A  4.6.6.2  |     |
| --- | --- | --- | -------------- | --- | --- | --- | ------- | --- | --- | ----------- | --- |
Floating Dock
| 27  |     |     |                   |     |     |     | DOCARE  | CATDOC  | 2   | A  4.6.6.3  |     |
| --- | --- | --- | ----------------- | --- | --- | --- | ------- | ------- | --- | ----------- | --- |
|     |     |     | Non-tidal basin,  |     |     |     |         |         |     |             |     |
Wet dock
| 28  |     |     |                     |     |     |     | DOCARE  | CATDOC  | 1   | A  4.6.6.3  |     |
| --- | --- | --- | ------------------- | --- | --- | --- | ------- | ------- | --- | ----------- | --- |
|     |     |     | Tidal basin, Tidal  |     |     |     |         |         |     |             |     |
harbour
| 29.1  |     |     |                        |     |     |     |         |         |     |            |     |
| ----- | --- | --- | ---------------------- | --- | --- | --- | ------- | ------- | --- | ---------- | --- |
|       |     |     | Floating oil barrier   |     |     |     | OILBAR  | CATOLB  | 2   | L  4.8.19  |     |
| 29.2  |     |     |                        |     |     |     | OILBAR  | CATOLB  | 1   | L  4.8.19  |     |
|       |     |     | Oil retention barrier  |     |     |     |         |         |     |            |     |
30    Works on land, with      DOCARE  CONDTN  1  A  4.6.6.3
|     |     |     | year date           |     |     |     |         | SORDAT  |     |             |     |
| --- | --- | --- | ------------------- | --- | --- | --- | ------- | ------- | --- | ----------- | --- |
| 31  |     |     | Works at sea, Area  |     |     |     | SLCONS  | CONDTN  | 3   | L/A  4.5.2  |     |
|     |     |     |                     |     |     |     |         |         |     |             |     |
under reclamation,
SORDAT
with year date
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
32
|     |     |     | Works under  |     |   SLCONS  | CONDTN  | 1  L/A  | 4.5.2  |     |
| --- | --- | --- | ------------ | --- | --------- | ------- | ------- | ------ | --- |
Under Construction (1991)
|     |     |     | construction with  |     |     | SORDAT  |     |     |     |
| --- | --- | --- | ------------------ | --- | --- | ------- | --- | --- | --- |
Works in progress (1991)
year date
| 33.1  |     |     |       |     | SLCONS  | CONDTN  | 2    | 4.5.2  |     |
| ----- | --- | --- | ----- | --- | ------- | ------- | ---- | ------ | --- |
|       |     |     | Ruin  |     |         |         |      |        |     |

| 33.2  |     |     |                      |     |     |   CONDTN  | 2  L/A  | 4.5.2  |     |
| ----- | --- | --- | -------------------- | --- | --- | --------- | ------- | ------ | --- |
|       |     |     | Ruined pier, partly  |     |     |           |         |        |     |
|       |     |     | submerged at high    |     |     | WATLEV    | 1       |        |     |
water
| 34  |     |     |       |     |           |         |        |        |     |
| --- | --- | --- | ----- | --- | --------- | ------- | ------ | ------ | --- |
|     |     |     | Hulk  |     |   HULKES  | CATHLK  |   P/A  | 4.6.8  |     |

Canals, Barrages         Clearances  ID      Signal Stations   IT
40      Canal with distant      CANALS  CATCAN    L/A  4.8.1    361.3
mark
|     |     |     |     |     |     | DISMAR  CATDIS  |   P  | 4.4  | 361.5  |
| --- | --- | --- | --- | --- | --- | --------------- | ---- | ---- | ------ |

|     |     |     |     |     |     | INFORM  |     |     | 307  |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | ---- |
41.1    Lock (on large-     GATCON  CATGAT  4  L/A  4.6.6.4    326.6
|     |     |     | scale charts)  |     |     | HORCLR  |     |     |     |
| --- | --- | --- | -------------- | --- | --- | ------- | --- | --- | --- |
361.6
41.2      Lock (on  smaller-   GATCON  CATGAT  4  L/A  4.6.6.4

|     |     |     | scale charts)  |     |         | HORCLR  |         |          |        |
| --- | --- | --- | -------------- | --- | ------- | ------- | ------- | -------- | ------ |
| 42  |     |     |                |     | GATCON  | CATGAT  | 3  L/A  | 4.6.6.4  |        |
|     |     |     | Caisson        |     |         |         |         |          | 326.5  |
HORCLR
| 43  |     |     |                |     | GATCON  | CATGAT    | 2  L/A  | 4.6.6.4  |          |
| --- | --- | --- | -------------- | --- | ------- | --------- | ------- | -------- | -------- |
|     |     |     | Flood barrage  |     |         |           |         |          | 326.7    |
|     |     |     |                |     |         |   HORCLR  |         | 4.8.6    |          |
|     |     |     |                |     | DAMCON  | CATDAM    | 3       |          |          |
| 44  |     |     |                |     | DAMCON  | CATDAM    | 2  L/A  | 4.8.5    |          |
|     |     |     | Dam            |     |         |           |         |          |   364.2  |

Transhipment Facilities         Roads   ID       Railways   ID       Tanks   IE
50
Roll on, Roll off      HRBFAC  CATHAF  1  L/A  4.8.5    321.5
RoRo
Ferry Terminal
|     |     |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
51  Transit shed,      BUISGL  FUNCTN  15  P/A  4.8.15    328.1
|     |     |     | Warehouse (with  |     |     | OBJNAM  |     |     |     |
| --- | --- | --- | ---------------- | --- | --- | ------- | --- | --- | --- |
designation)
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 52  |     |              |     |           |         |         |     |          |
| --- | --- | ------------ | --- | --------- | ------- | ------- | --- | -------- |
|     |     | Timber yard  |     |   PRDARE  | CATPRA  | 6  P/A  |     |   328.2  |
4.8.13

| 53.1  |     | Crane  |     | CRANES  | CATCRN  |   P  | 4.6.9.3  |        |
| ----- | --- | ------ | --- | ------- | ------- | ---- | -------- | ------ |
|       |     |        |     |         |         |      |          | 328.3  |
|       |     |        |     |         | LIFCAP  |      |          |        |
53.2    Container crane      CRANES  CATCRN  2  P  4.6.9.3    328.3
LIFCAP
| 53.3  |     | Sheerlegs  |     |   CRANES  | CATCRN  | 3  P  | 4.6.9.3  |     |
| ----- | --- | ---------- | --- | --------- | ------- | ----- | -------- | --- |
LIFCAP

Public Buildings
| 60  |     | Harbour Master’s  |     |   BUISGL  | FUNCTN  | 2  P  | 4.6.3  |          |
| --- | --- | ----------------- | --- | --------- | ------- | ----- | ------ | -------- |
|     |     |                   |     |           |         |       |        |   325.1  |
Office
| 61    |     |                 |     | BUISGL    | FUNCTN  | 3  P  | 4.6.3  |          |
| ----- | --- | --------------- | --- | --------- | ------- | ----- | ------ | -------- |
|       |     | Custom Office   |     |           |         |       |        |   325.2  |
| 62.1  |     | Health office,  |     |   BUISGL  | FUNCTN  | 4  P  | 4.6.3  |          |
|       |     |                 |     |           |         |       |        | 325.3    |
Quarantine building
INFORM
| 62.2  |     |           |     |           |         |       |     |     |
| ----- | --- | --------- | --- | --------- | ------- | ----- | --- | --- |
|       |     | Hospital  |     |   BUISGL  | FUNCTN  | 5  P  |     |     |
4.8.15
| 63  |     |              |     | BUISGL  | FUNCTN  | 6  P  | 4.8.15  |          |
| --- | --- | ------------ | --- | ------- | ------- | ----- | ------- | -------- |
|     |     | Post office  |     |         |         |       |         |   372.1  |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

IG Topographic Terms

Coast
| 1   |     |         |     |           |         |        |      |     |
| --- | --- | ------- | --- | --------- | ------- | ------ | ---- | --- |
|     |     | Island  |     |   LNDARE  | OBJNAM  |   P/A  | 4.1  |     |
| 2   |     | Islet   |     |   LNDARE  | OBJNAM  |   P/A  | 4.1  |     |

| 3   |     | Cay          |     |   LNDRGN  | CATLND  | 20  P/A  |        |     |
| --- | --- | ------------ | --- | --------- | ------- | -------- | ------ | --- |
|     |     |              |     |           |         |          |        |     |
|     |     |              |     |           | OBJNAM  |          | 4.7.1  |     |
| 4   |     |              |     | LNDRGN    | OBJNAM  |   P/A    | 4.7.1  |     |
|     |     | Peninsula    |     |           |         |          |        |     |
| 5   |     | Archipelago  |     |   LNDRGN  | OBJNAM  |   P/A    | 4.7.1  |     |
| 6   |     | Atoll        |     |   LNDARE  | OBJNAM  |   P/A    | 4.1    |     |

| 7   |     |                 |     | LNDRGN    | OBJNAM  |   P/A  | 4.7.1  |     |
| --- | --- | --------------- | --- | --------- | ------- | ------ | ------ | --- |
|     |     | Cape            |     |           |         |        |        |     |
| 8   |     |                 |     |           |         |        |        |     |
|     |     | Head, Headland  |     |   LNDRGN  | OBJNAM  |   P/A  | 4.7.1  |     |
| 9   |     | Point           |     |   LNDRGN  | OBJNAM  |   P/A  | 4.7.1  |     |

| 10  |     | Spit  |     |   LNDRGN  | OBJNAM  |   P/A    | 4.7.1  |     |
| --- | --- | ----- | --- | --------- | ------- | -------- | ------ | --- |
|     |     |       |     |           |         |          |        |     |
| 11  |     |       |     |           |         |          |        |     |
|     |     | Rock  |     |   LNDRGN  | CATLND  | 19  P/A  | 4.7.1  |     |
OBJNAM
| 12  |     | Salmarsh, Saltings  |     |   LNDRGN  | CATLND  | 15  P/A  | 4.7.1  |     |
| --- | --- | ------------------- | --- | --------- | ------- | -------- | ------ | --- |
|     |     |                     |     |           |         |          |        |     |
OBJNAM
| 13  |     | Lagoon  |     |   SEAARE  | OBJNAM  |   P/A  | 8   |     |
| --- | --- | ------- | --- | --------- | ------- | ------ | --- | --- |

Natural Inland Features
| 20  |     | Promontory  |     |   LNDRGN  | OBJNAM  |   P/A   | 4.7.1  |     |
| --- | --- | ----------- | --- | --------- | ------- | ------- | ------ | --- |
|     |     |             |     |           |         |         |        |     |
| 21  |     |             |     |           |         |         |        |     |
|     |     | Range       |     |   LNDRGN  | CATLND  | 5  P/A  | 4.7.1  |     |
OBJNAM
| 22  |     | Ridge  |     |   LNDRGN  | OBJNAM  |   L/A  | 4.7.1  |     |
| --- | --- | ------ | --- | --------- | ------- | ------ | ------ | --- |
|     |     |        |     |           |         |        |        |     |
SLOTOP  4.7.5
| 23  |     | Mountain, Mount  |     |   LNDRGN  | CATLND  | 5  P/A  | 4.7.1  |     |
| --- | --- | ---------------- | --- | --------- | ------- | ------- | ------ | --- |

OBJNAM
| 24  |     |         |     |           |         |      |        |     |
| --- | --- | ------- | --- | --------- | ------- | ---- | ------ | --- |
|     |     | Summit  |     |   LNDELV  | ELEVAT  |   P  | 4.7.2  |     |
OBJNAM
| 25  |     |       |     | LNDELV  | ELEVAT  |   P  | 4.7.2  |     |
| --- | --- | ----- | --- | ------- | ------- | ---- | ------ | --- |
|     |     | Peak  |     |         |         |      |        |     |
OBJNAM
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 26  |     |          |     |           |         |     |             |        |     |
| --- | --- | -------- | --- | --------- | ------- | --- | ----------- | ------ | --- |
|     |     | Volcano  |     |   LNDRGN  | CATLND  |     | 14/17  P/A  | 4.7.1  |     |
OBJNAM
| 27  |     |          |     | SLOGRD    | CATSLO  |     | 4  P/A  | 4.7.4      |     |
| --- | --- | -------- | --- | --------- | ------- | --- | ------- | ---------- | --- |
|     |     | Hill     |     |           |         |     |         |            |     |
| 28  |     | Boulder  |     |   LNDMRK  |         |     |         | P  4.8.15  |     |
|     |     |          |     |           | CATLMK  |     | 21      |            |     |
CONVIS
| 29  |     |             |     | LNDRGN    | OBJNAM  |     |   P/A  | 4.7.1  |     |
| --- | --- | ----------- | --- | --------- | ------- | --- | ------ | ------ | --- |
|     |     | Table-land  |     |           |         |     |        |        |     |
| 30  |     | Plateau     |     |   LNDRGN  | OBJNAM  |     |   P/A  | 4.7.1  |     |

| 31  |     |              |     | LNDRGN    | OBJNAM  |     |   P/A   | 4.7.1   |     |
| --- | --- | ------------ | --- | --------- | ------- | --- | ------- | ------- | --- |
|     |     | Valley       |     |           |         |     |         |         |     |
| 32  |     |              |     | LNDRGN    | OBJNAM  |     |   P/A   | 4.7.1   |     |
|     |     | Ravine, Cut  |     |           |         |     |         |         |     |
| 33  |     |              |     |           |         |     |         |         |     |
|     |     | Gorge        |     |   LNDRGN  | OBJNAM  |     |   P/A   | 4.7.1   |     |
| 34  |     |              |     | VEGATN    | CATVEG  |     |   P/A   | 4.7.11  |     |
|     |     | Vegetation   |     |           |         |     |         |         |     |
| 35  |     | Grassland    |     |   VEGATN  | CATVEG  |     | 1  P/A  | 4.7.11  |     |

|     |     |             |     | LNDRGN    | CATLND  |     | 10  P/A  | 4.7.1      |     |
| --- | --- | ----------- | --- | --------- | ------- | --- | -------- | ---------- | --- |
| 36  |     |             |     |           |         |     |          |            |     |
|     |     | Paddyfield  |     |   LNDRGN  | CATLND  |     | 8  P/A   | 4.7.1      |     |
| 37  |     |             |     | VEGATN    | CATVEG  |     | 3  P/A   | 4.7.11     |     |
|     |     | Bushes      |     |           |         |     |          |            |     |
| 38  |     | Deciduous   |     |   VEGATN  | CATVEG  |     | 4        | A  4.7.11  |     |

woodland
| 39  |     | Coniferous  |     |   VEGATN  | CATVEG  |     | 5   | A  4.7.11  |     |
| --- | --- | ----------- | --- | --------- | ------- | --- | --- | ---------- | --- |
woodland

Settlements
| 50  |     |                  |     |           |         |     |           |         |     |
| --- | --- | ---------------- | --- | --------- | ------- | --- | --------- | ------- | --- |
|     |     | City, Town       |     |   BUAARE  | CATBUA  |     | 4/5  P/A  | 4.8.14  |     |
| 51  |     |                  |     | BUAARE    | CATBUA  |     | 3  P/A    | 4.8.14  |     |
|     |     | Village          |     |           |         |     |           |         |     |
| 52  |     | Fishing village  |     |   BUAARE  | CATBUA  |     | 3  P/A    | 4.8.14  |     |

| 53  |     |        |     |         |     |     |        |         |     |
| --- | --- | ------ | --- | ------- | --- | --- | ------ | ------- | --- |
|     |     | Farm   |     |         |     |     |   P/A  |         |     |
|     |     |        |     | BUAARE  |     |     |        | 4.8.14  |     |
| 54  |     |        |     | BUAARE  |     |     |        | 4.8.14  |     |
|     |     | Saint  |     |         |     |     |        |         |     |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
Buildings
| 60  |     | Structure  |     |   BUISGL  |         |   P/A  | 4.8.15  |     |
| --- | --- | ---------- | --- | --------- | ------- | ------ | ------- | --- |
|     |     |            |     | LNDMRK    | CATLMK  |   P/A  |         |     |
CONVIS
| 61  |     |               |     | BUISGL    |     |   P/A  | 4.8.15  |     |
| --- | --- | ------------- | --- | --------- | --- | ------ | ------- | --- |
|     |     | House         |     |           |     |        |         |     |
| 62  |     |               |     | BUISGL    |     |   P/A  | 4.8.15  |     |
|     |     | Hut           |     |           |     |        |         |     |
| 63  |     |               |     |           |     |        |         |     |
|     |     | Multi-storey  |     |   BUISGL  |     |   P/A  | 4.8.15  |     |
building
| 64  |     |          |     | FORSTC    | CATFOR  | 1  P/A   | 4.8.17  |     |
| --- | --- | -------- | --- | --------- | ------- | -------- | ------- | --- |
|     |     | Castle   |     |           |         |          |         |     |
| 65  |     |          |     | BUISGL    | BUISHP  | 6  P/A   | 4.8.15  |     |
|     |     | Pyramid  |     |           |         |          |         |     |
| 66  |     |          |     |           |         |          |         |     |
|     |     | Column   |     |   LNDMRK  | CATLMK  | 10  P/A  | 4.8.15  |     |
CONVIS
| 67  |     |       |     | LNDMRK  | CATLMK  | 7  P/A  | 4.8.15  |     |
| --- | --- | ----- | --- | ------- | ------- | ------- | ------- | --- |
|     |     | Mast  |     |         |         |         |         |     |
CONVIS
| 68  |     |                |     | LNDMRK  | CATLMK  | 17  P/A  | 4.8.15  |     |
| --- | --- | -------------- | --- | ------- | ------- | -------- | ------- | --- |
|     |     | Lattice tower  |     |         |         |          |         |     |
CONVIS
| 69  |     | Mooring mast  |     |   LNDMRK  | CATLMK  | 7  P/A  | 4.8.15  |     |
| --- | --- | ------------- | --- | --------- | ------- | ------- | ------- | --- |
|     |     |               |     |           | CONVIS  | 40      |         |     |
FUNCTN
| 70  |     |             |     | LIGHTS  | CATLIT  | 8  P  | 12.8.7  |     |
| --- | --- | ----------- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Floodlight  |     |         |         |       |         |     |
COLOUR
LITCHR
| 71  |     | Town Hall    |     |   BUISGL  | OBJNAM  |   P/A    | 4.8.15  |     |
| --- | --- | ------------ | --- | --------- | ------- | -------- | ------- | --- |
| 72  |     |              |     | BUISGL    | FUNCTN  | 18  P/A  | 4.8.15  |     |
|     |     | Office       |     |           |         |          |         |     |
| 73  |     |              |     | BUISGL    | FUNCTN  | 36  P/A  | 4.8.15  |     |
|     |     | Observatory  |     |           |         |          |         |     |
| 74  |     |              |     |           |         |          |         |     |
|     |     | Institute    |     |   BUISGL  | FUNCTN  | 19  P/A  | 4.8.15  |     |
| 75  |     |              |     | BUISGL    | FUNCTN  | 20  P/A  | 4.8.15  |     |
|     |     | Cathedral    |     |           |         |          |         |     |
| 76  |     | Monastery,   |     |   BUISGL  | INFORM  |   P/A    | 4.8.15  |     |
|     |     | Convent      |     |           |         |          |         |     |

77  Lookout Station,      LNDMRK  CATLMK  17  P/A  4.8.15
|     |     | Watch tower        |     |           | FUNCTN  | 28       |         |     |
| --- | --- | ------------------ | --- | --------- | ------- | -------- | ------- | --- |
| 78  |     |                    |     |           |         |          |         |     |
|     |     | Navigation school  |     |   BUISGL  | FUNCTN  | 19  P/A  | 4.8.15  |     |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 79  |     |                |     |         |                 |     |      |           |     |
| --- | --- | -------------- | --- | ------- | --------------- | --- | ---- | --------- | --- |
|     |     | Naval college  |     |         | BUISGL  FUNCTN  | 19  | P/A  | 4.8.15    |     |
| 80  |     |                |     |         | BUISGL  FUNCTN  | 16  | P/A  | 4.8.15    |     |
|     |     | Factory        |     |         |                 |     |      |           |     |
|     |     |                |     | PRDARE  | CATPRA          | 7   | P/A  |           |     |
81    Brick kiln, Brick    BUISGL  FUNCTN  16  P/A  4.8.15

|     |     | works         |     | PRDARE  | CATPRA          | 7   | P/A  | 4.8.13    |     |
| --- | --- | ------------- | --- | ------- | --------------- | --- | ---- | --------- | --- |
| 82  |     | Cement works  |     |         | BUISGL  FUNCTN  | 16  | P/A  | 4.8.15    |     |
|     |     |               |     | PRDARE  | CATPRA          | 7   | P/A  | 4.8.13    |     |
| 83  |     |               |     |         |                 |     |      |           |     |
|     |     | Water mill    |     |         | BUISGL          |     | P/A  | 4.8.15    |     |
| 84  |     |               |     |         | BUISGL          |     | P/A  | 4.8.15    |     |
|     |     | Greenhouse    |     |         |                 |     |      |           |     |
| 85  |     | Warehouse,    |     |         | BUISGL  FUNCTN  | 15  | P/A  |           |     |
|     |     | Storehouse    |     |         |                 |     |      | 4.6.9.1   |     |

| 86  |     | Cold store,   |     |     | BUISGL  FUNCTN  | 15  | P/A  | 4.8.15    |     |
| --- | --- | ------------- | --- | --- | --------------- | --- | ---- | --------- | --- |
|     |     | Refrigerated  |     |     |                 |     |      | 4.6.9.1   |     |
storage house
| 87  |     | Refinery  |     |   PRDARE  | CATPRA  | 5   | P/A  | 4.8.13    |     |
| --- | --- | --------- | --- | --------- | ------- | --- | ---- | --------- | --- |

| 88  |     |                 |     |           |         |                   |      |           |     |
| --- | --- | --------------- | --- | --------- | ------- | ----------------- | ---- | --------- | --- |
|     |     | Power station   |     |   PRDARE  | CATPRA  | 4                 | P/A  | 4.8.13    |     |
| 89  |     |                 |     | PRDARE    |         | INFORM  Electric  | P/A  | 4.8.13    |     |
|     |     | Electric works  |     |           |         |                   |      |           |     |
Works
| 90  |     | Gas works  |     |   PRDARE  |     | INFORM  Gas  | P/A  | 4.8.13    |     |
| --- | --- | ---------- | --- | --------- | --- | ------------ | ---- | --------- | --- |
Works
| 91  |     |              |     |           |     |                |      |           |     |
| --- | --- | ------------ | --- | --------- | --- | -------------- | ---- | --------- | --- |
|     |     | Water works  |     |   PRDARE  |     | INFORM  Water  | P/A  | 4.8.13    |     |
Works
| 92  |     |               |     | PRDARE  |     | INFORM  | P/A  | 4.8.13  |     |
| --- | --- | ------------- | --- | ------- | --- | ------- | ---- | ------- | --- |
|     |     | Sewage works  |     |         |     | Sewage  |      |         |     |
Works
| 93  |     | Machine house,  |     |     | BUISGL  | INFORM    | P/A  | 4.8.15    |     |
| --- | --- | --------------- | --- | --- | ------- | --------- | ---- | --------- | --- |

|     |     | Pump house  |     | SMCFAC  | CATSCF  |     |     |     |     |
| --- | --- | ----------- | --- | ------- | ------- | --- | --- | --- | --- |

| 94  |     |                   |     |     |                 |           |      |           |     |
| --- | --- | ----------------- | --- | --- | --------------- | --------- | ---- | --------- | --- |
|     |     | Well              |     |     |                 |           |      |           |     |
| 95  |     |                   |     |     | BUISGL          | INFORM    | P/A  | 4.8.15    |     |
|     |     | Telegraph office  |     |     |                 |           |      |           |     |
| 96  |     |                   |     |     |                 |           |      |           |     |
|     |     | Hotel             |     |     | BUISGL  FUNCTN  | 7         | P/A  | 4.8.15    |     |
| 97  |     |                   |     |     | BUISGL          | INFORM    | P/A  | 4.8.15    |     |
|     |     | Sailors’ home     |     |     |                 |           |      |           |     |
| 98  |     | Spa hotel         |     |     | BUISGL  FUNCTN  | 7         | P/A  | 4.8.15    |     |

INFORM
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
Road, Rail and Air Traffic
| 110  |     | Street, Road  |     |   ROADWY  |     |   L  | 4.8.8  |     |
| ---- | --- | ------------- | --- | --------- | --- | ---- | ------ | --- |
| 111  |     |               |     | ROADWY    |     |   L  | 4.8.8  |     |
|      |     | Avenue        |     |           |     |      |        |     |
| 112  |     |               |     | ROADWY    |     |   L  | 4.8.8  |     |
|      |     | Tramway       |     |           |     |      |        |     |
or
|      |     |          |     | RAILWY  |         |   L      | 4.8.2   |     |
| ---- | --- | -------- | --- | ------- | ------- | -------- | ------- | --- |
| 113  |     |          |     | BRIDGE  | CATBRG  | 10  L/A  | 4.8.10  |     |
|      |     | Viaduct  |     |         |         |          |         |     |
VERCLR
114    Suspension bridge    BRIDGE  CATBRG  12  L/A  4.8.10

VERCLR
| 115  |     |             |     |           |         |         |         |     |
| ---- | --- | ----------- | --- | --------- | ------- | ------- | ------- | --- |
|      |     | Footbridge  |     |   BRIDGE  | CATBRG  | 9  L/A  | 4.8.10  |     |
VERCLR
| 116  |     |                 |     | RUNWAY    | CATRUN  | 1  P/L/A  | 4.8.12  |     |
| ---- | --- | --------------- | --- | --------- | ------- | --------- | ------- | --- |
|      |     | Runway          |     |           |         |           |         |     |
| 117  |     | Landing lights  |     |   LIGHTS  | COLOUR  |   P       | 12.8    |     |

LITCHAR
| 118  |     |     |     |     |     |     |     |     |
| ---- | --- | --- | --- | --- | --- | --- | --- | --- |
Helicopter landing      RUNWAY  CATRUN  2  P/L/A  4.8.12
site

Ports, Harbours
| 130  |     |                       |     |           |         |           |          |     |
| ---- | --- | --------------------- | --- | --------- | ------- | --------- | -------- | --- |
|      |     | Tidal barrier         |     |   DAMCON  | CATDAM  | 3  L/A    | 4.8.5    |     |
| 131  |     |                       |     | SMCFAC    | CATSCF  | 3  P/A    | 4.6.5    |     |
|      |     | Boat lift, Ship lift  |     |           |         |           |          |     |
| 132  |     |                       |     |           |         |           |          |     |
|      |     | Minor canal           |     |   CANALS  | CATCAN  |   L/A     | 4.8.1    |     |
| 133  |     |                       |     | GATCON    | CATGAT  | 6  P/L/A  | 4.6.6.4  |     |
|      |     | Sluice                |     |           |         |           |          |     |
| 134  |     | Basin                 |     |   SEAARE  | CATSEA  | 7  A      | 8        |     |

OBJNAM
| 135  |     |                   |     |           |         |       |          |     |
| ---- | --- | ----------------- | --- | --------- | ------- | ----- | -------- | --- |
|      |     | Reservoir         |     |   LAKARE  |         |   A   | 4.7.8    |     |
| 136  |     |                   |     | LNDARE    | CONDTN  | 3  A  | 4.1      |     |
|      |     | Reclamation area  |     |           |         |       |          |     |
| 137  |     | Port              |     |   HBRFAC  | CATHAF  |   A   | 4.6.1    |     |
| 138  |     |                   |     | HBRARE    |         |   A   | 9.1.1    |     |
|      |     | Harbour           |     |           |         |       |          |     |
| 139  |     |                   |     | SEAARE    | CATSEA  |       | 4.6.6.3  |     |
|      |     | Haven             |     |           |         |       |          |     |
|      |     |                   |     |           | OBJNAM  |       | 8        |     |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 140  |     |                |     |           |           |     |             |     |
| ---- | --- | -------------- | --- | --------- | --------- | --- | ----------- | --- |
|      |     | Inner Harbour  |     |   SEAARE  | CATSEA    |     | A  4.6.6.3  |     |
|      |     |                |     |           |   OBJNAM  |     | 8           |     |
INFORM
| 141  |     |                |     | SEAARE  | CATSEA    |     | A  4.6.6.3  |     |
| ---- | --- | -------------- | --- | ------- | --------- | --- | ----------- | --- |
|      |     | Outer Harbour  |     |         |           |     |             |     |
|      |     |                |     |         |   OBJNAM  |     | 8           |     |
INFORM
| 142  |     |                     |     |           |         |     |           |     |
| ---- | --- | ------------------- | --- | --------- | ------- | --- | --------- | --- |
|      |     | Deep water harbour  |     |   SEAARE  | CATSEA  |     | A  9.1.1  |     |
  OBJNAM
INFORM
| 143  |     |                  |     | FRPARE    |         |     | A  11.2.3  |     |
| ---- | --- | ---------------- | --- | --------- | ------- | --- | ---------- | --- |
|      |     | Free port        |     |           |         |     |            |     |
| 144  |     |                  |     |           |         |     |            |     |
|      |     | Customs harbour  |     |   CUSZNE  |         |     |   11.2.2   |     |
| 145  |     |                  |     | HBRFAC    | CATHAF  | 6   | A  4.6.1   |     |
|      |     | Naval port       |     |           |         |     |            |     |
146    Industrial harbour      HBRARE  INFORM    A  9.1.1
| 147  |     |                   |     | HBRFAC  |         | INFORM    |   4.6.1   |     |
| ---- | --- | ----------------- | --- | ------- | ------- | --------- | --------- | --- |
|      |     | Commercial port,  |     |         |         |           |           |     |
|      |     | Trade port        |     |         | OBJNAM  |           |           |     |
| 148  |     |                   |     | HBRFAC  | CATHAF  | 9         | A  4.6.1  |     |
|      |     | Building harbour  |     |         |         |           |           |     |
  INFORM
| 149  |     |              |     | HBRFAC  | CATHAF  | 7   | A  4.6.1  |     |
| ---- | --- | ------------ | --- | ------- | ------- | --- | --------- | --- |
|      |     | Oil harbour  |     |         |         |     |           |     |
  INFORM
| 150  |     |              |     |           |         |     |           |     |
| ---- | --- | ------------ | --- | --------- | ------- | --- | --------- | --- |
|      |     | Ore harbour  |     |   HBRFAC  | CATHAF  | 11  | A  4.6.1  |     |
  INFORM
| 151  |     |                |     | HBRFAC  | CATHAF  |     | A  4.6.1  |     |
| ---- | --- | -------------- | --- | ------- | ------- | --- | --------- | --- |
|      |     | Grain harbour  |     |         |         |     |           |     |
  INFORM
| 152  |     |                    |     | HBRFAC  | CATHAF  | 10  | A  4.6.1  |     |
| ---- | --- | ------------------ | --- | ------- | ------- | --- | --------- | --- |
|      |     | Container harbour  |     |         |         |     |           |     |
  INFORM
| 153  |     | Timber harbour  |     |   HBRFAC  | CATHAF  | 11  | A  4.6.1  |     |
| ---- | --- | --------------- | --- | --------- | ------- | --- | --------- | --- |
  INFORM
| 154  |     |               |     |           |         |     |           |     |
| ---- | --- | ------------- | --- | --------- | ------- | --- | --------- | --- |
|      |     | Coal harbour  |     |   HBRFAC  | CATHAF  | 11  | A  4.6.1  |     |
  INFORM
| 155  |     |                |     | HBRFAC  | CATHAF  | 3   | A  4.6.1  |     |
| ---- | --- | -------------- | --- | ------- | ------- | --- | --------- | --- |
|      |     | Ferry harbour  |     |         |         |     |           |     |
  INFORM
| 156  |     | Police  |     |     |     |     |     |     |
| ---- | --- | ------- | --- | --- | --- | --- | --- | --- |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

Harbour Installations
| 170  |     |           |     | HBRFAC  | CATHAF  |   A  | 4.6.1  |     |
| ---- | --- | --------- | --- | ------- | ------- | ---- | ------ | --- |
|      |     | Terminal  |     |         |         |      |        |     |
INFORM
171    Building slip      SLCONS  CATSLC  13  P/L/A  4.5.2
| 172  |     |                |     | HBRFAC  | CATHAF  |   A  | 4.6.1  |     |
| ---- | --- | -------------- | --- | ------- | ------- | ---- | ------ | --- |
|      |     | Building yard  |     |         |         |      |        |     |
INFORM
| 173  |     |                  |     | HBRFAC  | CATHAF  |   A  | 4.6.1  |     |
| ---- | --- | ---------------- | --- | ------- | ------- | ---- | ------ | --- |
|      |     | Buoy yard, Buoy  |     |         |         |      |        |     |
|      |     | dump             |     |         | INFORM  |      |        |     |
| 174  |     |                  |     | HBRFAC  | CATHAF  |   A  | 4.6.1  |     |
|      |     | Bunker Station   |     |         |         |      |        |     |
INFORM
| 175  |     |                       |     |           |         |            |          |     |
| ---- | --- | --------------------- | --- | --------- | ------- | ---------- | -------- | --- |
|      |     | Reception facilities  |     |   HBRFAC  | CATHAF  |   A        | 4.6.1    |     |
|      |     | for oily wastes       |     |           | INFORM  |            |          |     |
| 176  |     |                       |     |           |         |            |          |     |
|      |     | Tanker cleaning       |     |   HBRFAC  | CATHAF  |   A        | 4.6.1    |     |
|      |     | facilities            |     |           | INFORM  |            |          |     |
| 177  |     |                       |     |           |         |            |          |     |
|      |     | Cooling water         |     |   HBRFAC  | CATHAF  |   A        | 4.6.1    |     |
|      |     | intake/outfall        |     |           | INFORM  |            |          |     |
| 178  |     |                       |     | OILBAR    | CATOLB  | 2  L       |          |     |
|      |     | Floating barrier,     |     |           |         |            |          |     |
|      |     | Boom                  |     | OBSTRN    | CATOBS  | 10  P/L/A  |          |     |
| 179  |     |                       |     | PILPNT    | CATPLE  |   P        | 4.6.7.1  |     |
|      |     | Piling                |     |           |         |            |          |     |
or
|     |     |     |     | OBSTRN  | CATOBS  | 2  P/L/A  | 4.6.7.1  |     |
| --- | --- | --- | --- | ------- | ------- | --------- | -------- | --- |
|     |     |     |     |         | WATLEV  | 2         |          |     |
HEIGHT
| 180  |     |               |     |           |         |           |          |     |
| ---- | --- | ------------- | --- | --------- | ------- | --------- | -------- | --- |
|      |     | Row of piles  |     |   PILPNT  | CATPLE  |   P       | 4.6.7.1  |     |
|      |     |               |     | OBSTRN    | CATOBS  | 2  P/L/A  | 4.6.7.1  |     |
|      |     |               |     |           | WATLEV  | 2         |          |     |
HEIGHT
| 181  |     | Bollard  |     |   MORFAC  | CATMOR  | 3  P/A  | 4.6.7.1  |     |
| ---- | --- | -------- | --- | --------- | ------- | ------- | -------- | --- |

| 182  |     |           |     |           |         |        |         |     |
| ---- | --- | --------- | --- | --------- | ------- | ------ | ------- | --- |
|      |     | Conveyor  |     |   CONVYR  | VERCLR  |   L/A  | 4.8.11  |     |
CATCON
| 183  |     |                 |     | HBRFAC  | CATHAF  |   P/A  | 4.6.1  |     |
| ---- | --- | --------------- | --- | ------- | ------- | ------ | ------ | --- |
|      |     | Storage tanker  |     |         |         |        |        |     |
INFORM
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 184  |     |                      |     |           |         |        |        |     |
| ---- | --- | -------------------- | --- | --------- | ------- | ------ | ------ | --- |
|      |     | Lighter aboard ship  |     |   HBRFAC  | CATHAF  |   P/A  | 4.6.1  |     |
INFORM
| 185  |     |                    |     | HBRFAC  | CATHAF  |   P/A  | 4.6.1  |     |
| ---- | --- | ------------------ | --- | ------- | ------- | ------ | ------ | --- |
|      |     | Liquefied Natural  |     |         |         |        |        |     |
|      |     | Gas                |     |         | INFORM  |        |        |     |
| 186  |     |                    |     | HBRFAC  | CATHAF  |   P/A  | 4.6.1  |     |
|      |     | Liquefied          |     |         |         |        |        |     |
|      |     | Petroleum Gas      |     |         | INFORM  |        |        |     |
| 187  |     |                    |     | HBRFAC  | CATHAF  |   A    | 4.6.1  |     |
|      |     | Very Large Crude   |     |         |         |        |        |     |
|      |     | Carrier            |     |         | INFORM  |        |        |     |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

IH Tides, Currents

Terms Relating to Tidal Levals
| 1-17  |     |          |     |     |         |         |     |        |     |
| ----- | --- | -------- | --- | --- | ------- | ------- | --- | ------ | --- |
|       |     | Various  |     |     | M_VDAT  | VERDAT  |     | 2.1.2  |     |

|     |     |     |     |     | M_SDAT  | VERDAT  |     | 2.1.2  |     |
| --- | --- | --- | --- | --- | ------- | ------- | --- | ------ | --- |
|     |     |     |     |     |         |         |     |        |     |

Tidal Levels and Charted Data       Vertical   ID      Tide Gauge   IT
| 20  |     |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
|     |     |     |     |     |     |     |     |     |     |

Tide Tables
| 30  |     |     |     |     |     |     |     |     | 406.2  |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | ------ |
|     |     |     |     |     |     |     |     |     |        |
406.3
406.4
406.5
| 31  |     |                     |     |     | TS_PAD  | TS_TSP  |   P/A  | 3.3.5  | 407.2  |
| --- | --- | ------------------- | --- | --- | ------- | ------- | ------ | ------ | ------ |
|     |     | Tidal stream table  |     |     |         |         |        |        |        |
407.3

Tidal Streams and Currents
| 40  |     |                    |     |     | TS_FEB  | CAT_TS  | 1  P  | 3.3.1  |          |
| --- | --- | ------------------ | --- | --- | ------- | ------- | ----- | ------ | -------- |
|     |     | Flood tide stream  |     |     |         |         |       |        |   407.4  |
|     |     | (with rate)        |     |     |         | CURVEL  |       |        |          |
408.2
ORIENT
| 41  |     |                  |     |     | TS_FEB  | CAT_TS  | 2  P  | 3.3.1  |        |
| --- | --- | ---------------- | --- | --- | ------- | ------- | ----- | ------ | ------ |
|     |     | Ebb tide stream  |     |     |         |         |       |        | 407.4  |
|     |     | (with rate)      |     |     |         | CURVEL  |       |        |        |
408.2
ORIENT
| 42  |     |                        |     |     | CURENT  | CURVEL  |   P  | 3.4  | 408.2    |
| --- | --- | ---------------------- | --- | --- | ------- | ------- | ---- | ---- | -------- |
|     |     | Current in restricted  |     |     |         |         |      |      |          |
|     |     | waters                 |     |     |         | ORIENT  |      |      |          |
| 43  |     |                        |     |     | CURENT  | CURVEL  |   P  | 3.4  |          |
|     |     | Ocean current          |     |     |         |         |      |      |   408.3  |
ORIENT

TXTDSC
44    Overfalls, tide rips,    WATTUR  CATWAT  3/4  P/A  6.4    423.1

races
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 45  |     |              |     |           |         |         |        |          |
| --- | --- | ------------ | --- | --------- | ------- | ------- | ------ | -------- |
|     |     | Eddies       |     |   WATTUR  | CATWAT  | 2  P/A  | 6.4    |   423.3  |
| 46  |     |              |     | TS_PAD    | TS_TSP  |   P     | 3.3.5  |   407.2  |
|     |     | Position of  |     |           |         |         |        |          |
tabulated tidal
stream data with
designation

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
II Depths

General
| 1   |     |     |                     |     |     |     | STATUS  | 18  P/L/A  | 5.3  | 417  |
| --- | --- | --- | ------------------- | --- | --- | --- | ------- | ---------- | ---- | ---- |
|     | ED  |     | Existence doubtful  |     |     |     |         |            |      |      |
424.3
| 2   |     SD  |     | Sounding of     |     |   SOUNDG  |         |     |   P  | 5.3    |   417  |
| --- | ------- | --- | --------------- | --- | --------- | ------- | --- | ---- | ------ | ------ |
|     |         |     | doubtful depth  |     |           | QUASOU  |     | 3    |        | 424.3  |
|     |         |     |                 |     | OBSTRN    | VALSOU  |     |   P  | 6.3.1  |        |
QUASOU
| 3.1  |      |     |                      |     |     |   QUASOU  |     | 9    | 5.3  |     |
| ---- | ---- | --- | -------------------- | --- | --- | --------- | --- | ---- | ---- | --- |
|      | Rep  |     | Reported, but not    |     |     |           |     |      |      |     |
|      |      |     | confirmed            |     |     | QUAPOS    |     | 8    |      |     |
| 3.2  |      |     |                      |     |     |   QUASOU  |     | 9    | 5.3  |     |
|      |      |     | Reported, with year  |     |     |           |     |      |      |     |
Rep (1973)
|     |     |     | of report, but not  |     |     | QUAPOS  |     | 8   |     |     |
| --- | --- | --- | ------------------- | --- | --- | ------- | --- | --- | --- | --- |
confirmed
SORDAT
| 4   |     |     |                      |     |           |         |     |       |        |     |
| --- | --- | --- | -------------------- | --- | --------- | ------- | --- | ----- | ------ | --- |
|     |     |     | Reported, but not    |     |   SOUNDG  | QUASOU  |     | 9  P  | 5.3    |     |
|     |     |     | confirmed,           |     |           | QUAPOS  |     | 8     |        |     |
|     |     |     | sounding or danger   |     | OBSTRN    | VALSOU  |     |   P   | 6.2.2  |     |
|     |     |     |                      |     |           | WATLEV  |     | 3     | 6.3.1  |     |

Soundings and Drying Heights       Plane of Reference for Depths  IH   Plane of Reference for Heights  IH
| 10    |     |     |                   |     | SOUNDG  |         |     |   P  | 5.3  |     |
| ----- | --- | --- | ----------------- | --- | ------- | ------- | --- | ---- | ---- | --- |
|       |     |     | Sounding in true  |     |         |         |     |      |      |     |
|       |     |     | position          |     |         | QUASOU  |     | 1    |      |     |
| 11    |     |     |                   |     | SOUNDG  |         |     |   P  | 5.3  |     |
|       |     |     | Sounding out of   |     |         |         |     |      |      |     |
#
|     |     |     | position        |     |           |     |     |      |      |     |
| --- | --- | --- | --------------- | --- | --------- | --- | --- | ---- | ---- | --- |
| 12  |     |     | Least depth in  |     |   SOUNDG  |     |     |   P  | 5.3  |     |

|     |     |     | narrow channel       |     |           | QUASOU  |     | 6     |      |     |
| --- | --- | --- | -------------------- | --- | --------- | ------- | --- | ----- | ---- | --- |
| 13  |     |     | No bottom found at   |     |   SOUNDG  |         |     |   P   | 5.3  |     |
|     |     |     | depth shown          |     |           | QUASOU  |     | 5     |      |     |
|     |     |     |                      |     |           |         |     |       |      |     |
| 14  |     |     |                      |     | SOUNDG    | QUASOU  |     | 4  P  | 5.3  |     |
|     |     |     | Soundings taken      |     |           |         |     |       |      |     |
|     |     |     | from old or smaller  |     |           | QUAPOS  |     | 4     |      |     |
scale sources
| 15  |     |     |                    |     | SOUNDG  |         |     |   P  | 5.3  |     |
| --- | --- | --- | ------------------ | --- | ------- | ------- | --- | ---- | ---- | --- |
|     |     |     | Drying heights     |     |         |         |     |      |      |     |
|     |     |     | above chart datum  |     |         | QUASOU  |     | 1    |      |     |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs

Depths in Fairways and Areas         Plane of Reference for Depths  IH
| 20  |     |                   |     | DRGARE  | DRVAL1  |   A  | 5.5  |     |
| --- | --- | ----------------- | --- | ------- | ------- | ---- | ---- | --- |
|     |     | Limit of dredged  |     |         |         |      |      |     |
area (major and
minor)
| 21  |     |                     |     | DRGARE  | DRVAL1  |   A  | 5.5  |     |
| --- | --- | ------------------- | --- | ------- | ------- | ---- | ---- | --- |
|     |     | Dredged channel or  |     |         |         |      |      |     |
|     |     | area                |     |         | DRVAL2  |      |      |     |
|     |     |                     |     |         | QUASOU  | 11   |      |     |

| 22  |     | Dredged channel or   |     |   DRGARE  | DRVAL1  |   A   | 5.5    |     |
| --- | --- | -------------------- | --- | --------- | ------- | ----- | ------ | --- |
|     |     | area with year date  |     |           | DRVAL2  |       |        |     |
|     |     |                      |     |           | QUASOU  | 11    |        |     |
|     |     |                      |     |           | INFORM  | year  |        |     |
| 23  |     |                      |     | DRGARE    | DRVAL1  |   A   | 5.5    |     |
|     |     | Dredged channel or   |     |           |         |       |        |     |
|     |     | area maintained      |     |           | DRVAL2  |       |        |     |
|     |     |                      |     |           | QUASOU  | 10    |        |     |
| 24  |     |                      |     | SWPARE    | DRVAL1  |   A   | 5.6    |     |
|     |     | Swept area           |     |           |         |       |        |     |
|     |     |                      |     |           | TECSOU  | 6     |        |     |
| 25  |     | Unsurveyed area      |     |   UNSARE  |         |   A   | 5.8.1  |     |

5.8.2

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
Depth Contours

| 30  |     |     |     |     | DEPCNT  | VALDCO  |   L  | 5.2  |     |
| --- | --- | --- | --- | --- | ------- | ------- | ---- | ---- | --- |
| 31  |     |     |     |     | DEPCNT  | VALDCO  |   L  | 5.2  |     |
|     |     |     |     |     |         |         |      |      |     |
|     |     |     |     |     |         | QUAPOS  | 4    |      |     |

Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs

IJ Nature of the Seabed

Types of Seabed                            Rocks   IK
| 1     | S    |                  |     |           |         |           |      |     |
| ----- | ---- | ---------------- | --- | --------- | ------- | --------- | ---- | --- |
|       |      | Sand             |     |   SBDARE  | NATSUR  | 4  P/A    | 7.1  |     |
| 2     | M    |                  |     | SBDARE    | NATSUR  | 1  P/A    | 7.1  |     |
|       |      | Mud              |     |           |         |           |      |     |
| 3     | Cy   | Clay             |     |   SBDARE  | NATSUR  | 2  P/A    | 7.1  |     |
| 4     | Si   |                  |     | SBDARE    | NATSUR  | 3  P/A    | 7.1  |     |
|       |      | Silt             |     |           |         |           |      |     |
| 5     | St   |                  |     | SBDARE    | NATSUR  | 5  P/A    | 7.1  |     |
|       |      | Stones           |     |           |         |           |      |     |
| 6     | G    |                  |     |           |         |           |      |     |
|       |      | Gravel           |     |   SBDARE  | NATSUR  | 6  P/A    | 7.1  |     |
| 7     | P    |                  |     | SBDARE    | NATSUR  | 7  P/A    | 7.1  |     |
|       |      | Pebbles          |     |           |         |           |      |     |
| 8     | Cb   | Cobbles          |     |   SBDARE  | NATSUR  | 8  P/A    | 7.1  |     |
| 9     | R    |                  |     | SBDARE    | NATSUR  | 9  P/A    | 7.1  |     |
|       |      | Rock             |     |           |         |           |      |     |
| 10    | Co   |                  |     | SBDARE    | NATSUR  | 14  P/A   | 7.1  |     |
|       |      | Coral            |     |           |         |           |      |     |
| 11    | Sh   |                  |     |           |         |           |      |     |
|       |      | Shells           |     |   SBDARE  | NATSUR  | 17  P/A   | 7.1  |     |
| 12.1  | S/M  |                  |     | SBDARE    | NATSUR  | 4/1  P/A  | 7.1  |     |
|       |      | Two layesr e.g.  |     |           |         |           |      |     |
Sand over Mud
| 12.2  | FS.M.Sh  |                          |     | SBDARE  | NATSUR  | 4,1,17  P/A  | 7.1  |     |
| ----- | -------- | ------------------------ | --- | ------- | ------- | ------------ | ---- | --- |
|       |          | Mixed bottom, where the  |     |         |         |              |      |     |
seabed comprises a
|     |     |     |     |     | NATQUA  | 1,,  |     |     |
| --- | --- | --- | --- | --- | ------- | ---- | --- | --- |
mixture of materials, the
main constituent is given
first , e.g. fine Sand with
Mud and Shells
| 13.1  | Wd  |                  |     | WEDKLP  | CATWED  | 2  P/A  |        |     |
| ----- | --- | ---------------- | --- | ------- | ------- | ------- | ------ | --- |
|       |     | Weed (including  |     |         |         |         |        |     |
|       |     | Kelp)            |     |         |         |         | 7.2.2  |     |
| 13.2  |     |                  |     | WEDKLP  | CATWED  | 1  P/A  |        |     |
|       |     | Kelp             |     |         |         |         |        |     |
7.2.2
| 14  |     |                   |     | SNDWAV    |     |   P/L/A  | 7.2.1  |     |
| --- | --- | ----------------- | --- | --------- | --- | -------- | ------ | --- |
|     |     | Sandwaves         |     |           |     |          |        |     |
| 15  |     |                   |     |           |     |          |        |     |
|     |     | Spring in seabed  |     |   SPRING  |     |   P      | 7.2.3  |     |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
Intertidal Areas
20    Areas of sand and      SBDARE  NATSUR  4/1/5/6  A  7.1
|     |     | mud with patches  |     |     | WATLEV  | 4   |     |     |
| --- | --- | ----------------- | --- | --- | ------- | --- | --- | --- |
of stones or gravel
| 21  |     |             |     | SBDARE    | NATSUR  | 9  A   | 7.1  |     |
| --- | --- | ----------- | --- | --------- | ------- | ------ | ---- | --- |
|     |     | Rocky area  |     |           |         |        |      |     |
|     |     |             |     |           | WATLEV  | 4      |      |     |
| 22  |     | Coral reef  |     |   SBDARE  | NATSUR  | 14  A  | 7.1  |     |

|     |     |     |     |     | WATLEV  | 4   |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |

Qualifying Terms
f
| 30  |     | Fine    |     |   SBDARE  | NATQUA  | 1  P/A  | 7.1  |     |
| --- | --- | ------- | --- | --------- | ------- | ------- | ---- | --- |
| 31  | m   |         |     | SBDARE    | NATQUA  | 2  P/A  | 7.1  |     |
|     |     | Medium  |     |           |         |         |      |     |
| 32  | c   | Coarse  |     |   SBDARE  | NATQUA  | 3  P/A  | 7.1  |     |

| 33  | bk  |         |     |           |         |         |      |     |
| --- | --- | ------- | --- | --------- | ------- | ------- | ---- | --- |
|     |     | Broken  |     |   SBDARE  | NATQUA  | 4  P/A  | 7.1  |     |
| 34  | sy  |         |     | SBDARE    | NATQUA  | 5  P/A  | 7.1  |     |
|     |     | Sticky  |     |           |         |         |      |     |
so
| 35  |     | Soft      |     |   SBDARE  | NATQUA  | 6  P/A  | 7.1  |     |
| --- | --- | --------- | --- | --------- | ------- | ------- | ---- | --- |
| 36  | sf  |           |     | SBDARE    | NATQUA  | 7  P/A  | 7.1  |     |
|     |     | Stiff     |     |           |         |         |      |     |
| 37  | v   | Volcanic  |     |   SBDARE  | NATQUA  | 8  P/A  | 7.1  |     |

| 38  | ca  |             |     |           |         |          |      |     |
| --- | --- | ----------- | --- | --------- | ------- | -------- | ---- | --- |
|     |     | Calcareous  |     |   SBDARE  | NATQUA  | 9  P/A   | 7.1  |     |
| 39  | h   |             |     | SBDARE    | NATQUA  | 10  P/A  | 7.1  |     |
|     |     | hard        |     |           |         |          |      |     |

Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs

IK Rocks, Wrecks, Obstructions

General
| 1   |     |              |     |     |           |         |      |             |     |
| --- | --- | ------------ | --- | --- | --------- | ------- | ---- | ----------- | --- |
|     |     | Danger line  |     |     |   OBSTRN  | WATLEV  | 3/4  | A/P  6.2.2  |     |
VALSOU

| 2   |     | Depth cleared by  |     |     |     |   TECSOU  | 6   |   2.2.3  |     |
| --- | --- | ----------------- | --- | --- | --- | --------- | --- | -------- | --- |
wire drag sweep

Rocks                   Plane of Reference for Depths  IH   Plane of Reference for Heights  IH
| 10  |     |                     |     |     | LNDARE    |         |           | P/A  4.1  |     |
| --- | --- | ------------------- | --- | --- | --------- | ------- | --------- | --------- | --- |
|     |     | Rock which does     |     |     |           |         |           |           |     |
|     |     | not cover; height   |     |     |           |         |           |           |     |
|     |     | above chart datum   |     |     |           |         |           |           |     |
|     |     |                     |     |     |           | LNDELV  | ELEVAT    | P  4.7.2  |     |
| 11  |     | Rock which covers   |     |     |   UWTROC  | VALSOU  |           | P  6.1.2  |     |
|     |     | and uncovers,       |     |     |           | WATLEV  | 4         |           |     |
|     |     | height above Chart  |     |     |           | NATSUR  | 9         |           |     |
Datum, where
|     |     |     |     |     |     | QUASOU  | 1   |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |
known
|     |     |                    |     |     |   SBDARE  | NATSUR  | 9   | A  7.1    |     |
| --- | --- | ------------------ | --- | --- | --------- | ------- | --- | --------- | --- |
|     |     |                    |     |     |           | WATLEV  | 4   |           |     |
| 12  |     |                    |     |     |   UWTROC  | NATSUR  | 9   | P  6.1.2  |     |
|     |     | Rock awash at the  |     |     |           |         |     |           |     |
|     |     | level of Chart     |     |     |           | WATLEV  | 5   |           |     |
Datum
|     |     |                   |     |     |         | VALSOU  | 0   |           |     |
| --- | --- | ----------------- | --- | --- | ------- | ------- | --- | --------- | --- |
| 13  |     |                   |     |     | UWTROC  | NATSUR  | 9   | P  6.1.2  |     |
|     |     | Underwater rock   |     |     |         |         |     |           |     |
|     |     | over which the    |     |     |         | WATLEV  | 3   |           |     |
|     |     | depth is unknown  |     |     |         | QUASOU  | 2   |           |     |
VALSOU
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
| 14    |     |     |                     |     |           |         |      |        |     |
| ----- | --- | --- | ------------------- | --- | --------- | ------- | ---- | ------ | --- |
|       |     |     | Dangerous           |     |   UWTROC  | VALSOU  |   P  | 6.1.2  |     |
|       |     |     | underwater rock of  |     |           | NATSUR  | 9    |        |     |
|       |     |     | known depth         |     |           | WATLEV  | 3    |        |     |
|       |     |     |                     |     |           | QUASOU  | 1    |        |     |
| 14.1  |     |     | Inside the          |     |   UWTROC  | VALSOU  |   P  | 6.1.2  |     |
|       |     |     | corresponding       |     |           | NATSUR  | 9    |        |     |
|       |     |     | depth area          |     |           | WATLEV  | 3    |        |     |
|       |     |     |                     |     |           | QUASOU  | 1    |        |     |
|       |     |     |                     |     |           | EXPSOU  | 1    |        |     |
| 14.2  |     |     | Outside the         |     |   UWTROC  | VALSOU  |   P  | 6.1.2  |     |

|     |     |     | corresponding  |     |     | NATSUR  | 9   |     |     |
| --- | --- | --- | -------------- | --- | --- | ------- | --- | --- | --- |
depth area
|     |     |     |                   |     |         | WATLEV  | 3    |        |     |
| --- | --- | --- | ----------------- | --- | ------- | ------- | ---- | ------ | --- |
|     |     |     |                   |     |         | QUASOU  | 1    |        |     |
|     |     |     |                   |     |         | EXPSOU  | 2    |        |     |
| 15  |     |     |                   |     | UWTROC  | VALSOU  |   P  | 6.1.2  |     |
|     |     |     | Underwater rock   |     |         |         |      |        |     |
|     |     |     | not dangerous to  |     |         | NATSUR  | 9    |        |     |
surface navigation
|     |     |     |                      |     |           | WATLEV    | 3     |        |     |
| --- | --- | --- | -------------------- | --- | --------- | --------- | ----- | ------ | --- |
| 16  |     |     |                      |     |           |           |       |        |     |
|     |     |     | Coral reef which is  |     |   OBSTRN  | CATOBS    | 6  A  | 6.2.2  |     |
|     |     |     | always covered       |     |           |   WATLEV  | 3     |        |     |
|     |     |     |                      |     |           | NATSUR    | 14    |        |     |
VALSOU
|     |     |     |           |     | UWTROC    | NATSUR  | 14  P   | 6.1.2  |     |
| --- | --- | --- | --------- | --- | --------- | ------- | ------- | ------ | --- |
|     |     |     |           |     |           | WATLEV  | 3       |        |     |
| 17  |     |     |           |     |           |         |         |        |     |
|     |     |     | Breakers  |     |   WATTUR  | CATWAT  | 1  P/A  | 6.4    |     |
|     |     |     |           |     | WATTUR    | CATWAT  | 1  P/A  | 6.4    |     |
SOUNDG

Wrecks        Plane of Reference for Depths  IH   Historic Wreck   IN
| 20  |     |     |                    |     | WRECKS  | CATWRK  | 5  A  |        |     |
| --- | --- | --- | ------------------ | --- | ------- | ------- | ----- | ------ | --- |
|     |     |     | Wreck which does   |     |         |         |       |        |     |
|     |     |     | not cover, height  |     |         | WATLEV  | 2     | 6.2.1  |     |
above chart datum
|     |     |     |     |     |     | LNDELV  ELEVAT  |   P   |     |     |
| --- | --- | --- | --- | --- | --- | --------------- | ----- | --- | --- |
|     |     |     |     |     |     | INFORM          | mast  |     |     |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 21  |     |                   |     |           |           |         |        |     |
| --- | --- | ----------------- | --- | --------- | --------- | ------- | ------ | --- |
|     |     | Wreck which       |     |   WRECKS  | CATWRK    | 4/5  A  |        |     |
|     |     | covers and        |     |           |   WATLEV  | 4       | 6.2.1  |     |
|     |     | uncovers, height  |     |           |   VALSOU  |         |        |     |
above chart datum
| 22  |     |                   |     | WRECKS  | CATWRK    | 2  A  |        |     |
| --- | --- | ----------------- | --- | ------- | --------- | ----- | ------ | --- |
|     |     | Submerged wreck,  |     |         |           |       |        |     |
|     |     | depth known       |     |         |   WATLEV  | 5     | 6.2.1  |     |
  VALSOU
| 23  |     |                    |     | WRECKS  | CATWRK  | 2  A    |        |     |
| --- | --- | ------------------ | --- | ------- | ------- | ------- | ------ | --- |
|     |     | Submerged wreck,   |     |         |         |         |        |     |
|     |     | depth unknown      |     |         | WATLEV  | 3       | 6.2.1  |     |
|     |     |                    |     |         | QUASOU  | 2       |        |     |
| 24  |     |                    |     | WRECKS  | CATWRK  | 4/5  P  |        |     |
|     |     | Wreck showing any  |     |         |         |         |        |     |
|     |     | part of hull or    |     |         | WATLEV  | 2/4     | 6.2.1  |     |
superstructure
| 25  |     |                   |     | WRECKS  | CATWRK  | 4  P  |        |     |
| --- | --- | ----------------- | --- | ------- | ------- | ----- | ------ | --- |
|     |     | Wreck of which    |     |         |         |       |        |     |
|     |     | only the mast(s)  |     |         | WATLEV  | 2/4   | 6.2.1  |     |
only are visible at
Chart Datum

| 26  |     | Wreck, obtained by  |     |   WRECKS  | CATWRK    | 1  P    |        |     |
| --- | --- | ------------------- | --- | --------- | --------- | ------- | ------ | --- |
|     |     | sounding            |     |           |   WATLEV  | 5       | 6.2.1  |     |
|     |     |                     |     |           |   VALSOU  |         |        |     |
|     |     |                     |     |           | QUASOU    | 6       |        |     |
| 27  |     |                     |     | WRECKS    | CATWRK    | 2  P    |        |     |
|     |     | Wreck, swept by     |     |           |           |         |        |     |
|     |     | wire                |     |           |   WATLEV  | 3       | 6.2.1  |     |
|     |     |                     |     |           |   VALSOU  |         |        |     |
|     |     |                     |     |           | QUASOU    | 6       |        |     |
|     |     |                     |     |           | TECSOU    | 6       |        |     |
| 28  |     |                     |     | WRECKS    | CATWRK    | 2  P    |        |     |
|     |     | Dangerous wreck,    |     |           |           |         |        |     |
|     |     | depth unknown       |     |           |   WATLEV  | 3       | 6.2.1  |     |
|     |     |                     |     |           |   QUASOU  | 2       |        |     |
| 29  |     | Non-dangerous       |     |           |           |         |        |     |
|     |     |                     |     | WRECKS    | CATWRK    | 1  P    |        |     |
|     |     | wreck, depth        |     |           | WATLEV    | 3       | 6.2.1  |     |
|     |     | unknown             |     |           | QUASOU    | 2       |        |     |
| 30  |     |                     |     | WRECKS    | CATWRK    | 1/2  P  |        |     |
|     |     | Wreck, depth        |     |           |           |         |        |     |
|     |     | unknown with safe   |     |           |   WATLEV  | 3       | 6.2.1  |     |
clearance at depth
|     |     |     |     |     |   VALSOU  |     |     |     |
| --- | --- | --- | --- | --- | --------- | --- | --- | --- |
shown
|     |     |     |     |     | QUASOU  | 7   |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
| 31  |     |     |     |     |         |         |         |        |     |
| --- | --- | --- | --- | --- | ------- | ------- | ------- | ------ | --- |
|     |     |     |     |     | OBSTRN  | CATOBS  | 7  P/A  |        |     |
|     |     |     |     |     |         | WATLEV  | 3       | 6.2.2  |     |
|     |     |     |     |     |         | VALSOU  |         |        |     |
|     |     |     |     |     |         | QUASOU  | 2       |        |     |
|     |     |     |     |     | WRECKS  | CATWRK  | 3  P    | 6.2.1  |     |
|     |     |     |     |     |         | WATLEV  | 3       |        |     |

Obstructions               Plane of Reference for Depths  IH      Kelp, Sea Weed   IJ      Wells   IL
| 40    |     |                       |     |     |         |         |         |        |     |
| ----- | --- | --------------------- | --- | --- | ------- | ------- | ------- | ------ | --- |
|       |     | Obstruction, depth    |     |     | OBSTRN  | WATLEV  | 3  P/A  | 6.2.2  |     |
|       |     | unknown               |     |     |         | VALSOU  |         |        |     |
|       |     |                       |     |     |         | QUASOU  | 2       |        |     |
| 41    |     |                       |     |     | OBSTRN  | WATLEV  | 3  P/A  | 6.2.2  |     |
|       |     | Obstruction, depth    |     |     |         |         |         |        |     |
|       |     | known                 |     |     |         | VALSOU  |         | 6.3.1  |     |
|       |     |                       |     |     |         | QUASOU  | 6       |        |     |
| 42    |     |                       |     |     |         |         |         |        |     |
|       |     | Obstruction swept by  |     |     | OBSTRN  | WATLEV  | 3  P/A  | 6.2.2  |     |
|       |     | wire drag             |     |     |         | VALSOU  |         | 6.3.1  |     |
|       |     |                       |     |     |         | QUASOU  | 6       |        |     |
|       |     |                       |     |     |         | TECSOU  | 6       |        |     |
|       |     | Stump of posts or     |     |     |         |         |         |        |     |
| 43.1  |     |                       |     |     | OBSTRN  | CATOBS  | 1  P/A  | 6.2.2  |     |

piles, wholly
|     |     |     |     |     |     | WATLEV  | 3   |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |
submerged
|         |     |                   |     |     |         | VALSOU  |       |        |     |
| ------- | --- | ----------------- | --- | --- | ------- | ------- | ----- | ------ | --- |
| 43.2    |     |                   |     |     | OBSTRN  | CATOBS  | 1  P  | 6.2.2  |     |
|         |     | Submerged, pile,  |     |     |         |         |       |        |     |
|         |     | stake etc.        |     |     |         | WATLEV  | 3     |        |     |

|       |     |                        |     |     |         | VALSOU  |             |         |     |
| ----- | --- | ---------------------- | --- | --- | ------- | ------- | ----------- | ------- | --- |
| #     |     |                        |     |     |         | QUAPOS  | 1           |         |     |
| 44.1  |     |                        |     |     | FSHFAC  | CATFIF  | 1  L        | 11.9.1  |     |
|       |     | Fishing stakes         |     |     |         |         |             |         |     |
| 44.2  |     |                        |     |     | FSHFAC  | CATFIF  | 2/3/4  A/P  | 11.9.1  |     |
|       |     | Fish trap, fish weir,  |     |     |         |         |             |         |     |
tunny nets
| 45  |     |                        |     |     | FSHFAC  | CATFIF  | 2/4  A  | 11.9.1  |     |
| --- | --- | ---------------------- | --- | --- | ------- | ------- | ------- | ------- | --- |
|     |     | Fish trap area, tunny  |     |     |         |         |         |         |     |
nets area
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 46.1  |     |             |     |           |           |         |         |     |
| ----- | --- | ----------- | --- | --------- | --------- | ------- | ------- | --- |
|       |     | Fish haven  |     |   OBSTRN  | CATOBS    | 5  P/A  | 6.2.2   |     |
|       |     |             |     |           |   WATLEV  | 3       | 11.9.3  |     |
|       |     |             |     |           | VALSOU    |         |         |     |
|       |     |             |     |           | QUASOU    | 2       |         |     |
46.2    Fish haven, depth      OBSTRN  CATOBS  5  P/A  6.2.2
|     |     | known           |     |         |   WATLEV  | 3         | 11.9.3  |     |
| --- | --- | --------------- | --- | ------- | --------- | --------- | ------- | --- |
|     |     |                 |     |         | VALSOU    |           |         |     |
|     |     |                 |     |         | QUASOU    | 1         |         |     |
| 47  |     |                 |     | MARCUL  | CATMFA    | 1/2  P/A  | 11.9.2  |     |
|     |     | Shellfish beds  |     |         |           |           |         |     |
|     |     |                 |     |         | WATLEV    | 3??       |         |     |
VALSOU
| 48.1  |     |                     |     |           |         |         |         |     |
| ----- | --- | ------------------- | --- | --------- | ------- | ------- | ------- | --- |
|       |     | Marine farm (large- |     |   MARCUL  | CATMFA  | 3  P/A  | 11.9.2  |     |
|       |     | scale charts)       |     |           | WATLEV  | 3/4     |         |     |
|       |     |                     |     |           | VALSOU  |         |         |     |
48.2    Marine farm (small-     MARCUL  CATMFA  3  P  11.9.2
|     |     | scale charts)  |     |     | WATLEV  | 3/4  |     |     |
| --- | --- | -------------- | --- | --- | ------- | ---- | --- | --- |
VALSOU

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

IL Offshore Installations

Oilfields and gasfields      Areas, Limits   IN
1         EKOFISK
|     |     | Name of oilfield or  |     |     | OSPARE  | PRODCT  | 1/2    | 11.7.4  |     |
| --- | --- | -------------------- | --- | --- | ------- | ------- | ------ | ------- | --- |
OBJNAM
|                         OILFIELD  |     | gasfield  |     |     |     |     |     |     |     |
| --------------------------------- | --- | --------- | --- | --- | --- | --- | --- | --- | --- |

| 2   |     | Platform with  |     |     | OFSPLF  | CATOFP  | 2  P  | 11.7.2  |     |
| --- | --- | -------------- | --- | --- | ------- | ------- | ----- | ------- | --- |

designation/name
OBJNAM
| 3   |     |                       |     |     |         |         |       |         |     |
| --- | --- | --------------------- | --- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Limit of safety zone  |     |     | RESARE  | CATREA  | 1  A  | 11.7.3  |     |
|     |     | around offshore       |     |     |         | RESTRN  | 8     |         |     |
installations

4    Limit of development    OSPARE  PRODCT  1/2  A  11.7.4
area
|     |     |     |     |     |     | RESTRN  |     |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |

Platforms and Moorings       Mooring Buoys   IQ
| 10  |     |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
Production platform,      OFSPLF  CATOFP  1/2  P  11.7.2
Platform, Oil derrick
| 11  |     |                     |     |     |         |         |       |         |     |
| --- | --- | ------------------- | --- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Flare stack at sea  |     |     | LNDMRK  | CATLMK  | 6  P  | 11.7.6  |     |

|     |     |                       |     |     | OFSPLF  | CATOFP  | 2  P    | 11.7.2  |     |
| --- | --- | --------------------- | --- | --- | ------- | ------- | ------- | ------- | --- |
| 12  |     |                       |     |     | OFSPLF  | CATOFP  | 4/5  P  | 11.7.2  |     |
|     |     | Single Point Mooring  |     |     |         |         |         |         |     |
including SALM and
ALP
| 13  |     |                       |     |     | OFSPLF  | CATOFP  | 3  P  | 11.7.2  |     |
| --- | --- | --------------------- | --- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Observation platform  |     |     |         |         |       |         |     |

| 14  |     |                   |     |     | OFSPLF  | CATOFP  | 2  P  | 11.7.2  |     |
| --- | --- | ----------------- | --- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Disused platform  |     |     |         |         |       |         |     |
|     |     |                   |     |     |         | STATUS  | 4     |         |     |
15    Artificial island      OFSPLF  CATOFP  7  P/A  11.7.2
OBJNAM
| 16  |     |                 |     |     |         |         |       |         |     |
| --- | --- | --------------- | --- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Tanker mooring  |     |     | BOYINB  | BOYSHP  | 7  P  | 11.7.5  |     |
|     |     |                 |     |     |         | CATINB  | 1/2   |         |     |
COLOUR
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 17  |     |                 |     |     |                 |       |         |     |
| --- | --- | --------------- | --- | --- | --------------- | ----- | ------- | --- |
|     |     | Moored storage  |     |     | OFSPLF  CATOFP  | 8  P  | 11.7.2  |     |
tanker

Underwater Installations        Plane of Reference for Depths  IH        Obstructions   IK
| 20    |     |                        |     | OBSTRN  | CATOBS    | 2  P  | 11.7.1  |     |
| ----- | --- | ---------------------- | --- | ------- | --------- | ----- | ------- | --- |
|       |     | Production well, with  |     |         |           |       |         |     |
|       |     | depth where known      |     |         |   WATLEV  | 3     |         |     |
|       |     |                        |     |         | VALSOU    |       |         |     |
| 21.1  |     |                        |     | OBSTRN  | CATOBS    | 2  P  | 11.7.1  |     |
|       |     | Suspended well         |     |         |           |       |         |     |
|       |     |                        |     |         |   WATLEV  | 3     |         |     |
|       |     |                        |     |         | VALSOU    |       |         |     |
|       |     |                        |     |         | QUASOU    | 2     |         |     |
|       |     |                        |     |         | STATUS    | 4     |         |     |
| 21.2  |     |                        |     | OBSTRN  | CATOBS    | 2  P  | 11.7.1  |     |
|       |     | Suspended well,        |     |         |           |       |         |     |
|       |     | depth known            |     |         |   WATLEV  | 3     |         |     |
|       |     |                        |     |         | VALSOU    |       |         |     |
|       |     |                        |     |         | STATUS    | 4     |         |     |
| 21.3  |     |                        |     | OBSTRN  | CATOBS    | 2  P  | 11.7.1  |     |
|       |     | Suspended well with    |     |         |           |       |         |     |
|       |     | height of wellhead     |     |         |   WATLEV  | 3     |         |     |
|       |     | above the seabed       |     |         | VALSOU    |       |         |     |
|       |     |                        |     |         | VERLEN    |       |         |     |
|       |     |                        |     |         | STATUS    | 4     |         |     |
| 22    |     |                        |     | OBSTRN  | CATOBS    | 7  P  | 11.7.1  |     |
|       |     | Site of cleared        |     |         |           |       |         |     |
|       |     | platform               |     |         | WATLEV    |       |         |     |
VALSOU
| 23  |     |              |     | OBSTRN  | CATOBS    | 2  P  | 11.7.1  |     |
| --- | --- | ------------ | --- | ------- | --------- | ----- | ------- | --- |
|     |     | Above water  |     |         |           |       |         |     |
|     |     | wellhead     |     |         |   WATLEV  | 2     |         |     |
|     |     |              |     |         | HEIGHT    |       |         |     |

Submarine Cables
| 30.1  |     |                  |     | CBLSUB  | CATCBL  |   L  | 11.5.1  |     |
| ----- | --- | ---------------- | --- | ------- | ------- | ---- | ------- | --- |
|       |     | Submarine cable  |     |         |         |      |         |     |
| 30.2  |     |                  |     | CBLARE  | CATCBL  |   A  | 11.5.3  |     |
|       |     | Submarine cable  |     |         |         |      |         |     |
|       |     | area             |     |         | RESTRN  |      |         |     |
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
| 31.1  |     |                  |     |     |         |         |       |         |     |
| ----- | --- | ---------------- | --- | --- | ------- | ------- | ----- | ------- | --- |
|       |     | Submarine power  |     |     | CBLSUB  | CATCBL  | 1  L  | 11.5.1  |     |
cable
| 31.2  |     |                    |     |     | CBLARE  | CATCBL  | 1  A  | 11.5.3  |     |
| ----- | --- | ------------------ | --- | --- | ------- | ------- | ----- | ------- | --- |
|       |     | Submarine power    |     |     |         |         |       |         |     |
|       |     | cable area         |     |     |         | RESTRN  |       |         |     |
| 32    |     |                    |     |     | CBLSUB  | CATCBL  |   L   | 11.5.1  |     |
|       |     | Disused submarine  |     |     |         |         |       |         |     |
|       |     | cable              |     |     |         | STATUS  | 4     |         |     |

Submarine Pipelines
| 40.1  |     | Supply pipelines   |     |     | PIPSOL  | CATPIP  | 6  L     | 11.6.1  |     |
| ----- | --- | ------------------ | --- | --- | ------- | ------- | -------- | ------- | --- |
|       |     |                    |     |     |         |         |          |         |     |
|       |     |                    |     |     |         | PRODCT  | 1/2/3/7  |         |     |
| 40.2  |     |                    |     |     | PIPARE  | CATPIP  | 6  A     | 11.6.4  |     |
|       |     | Supply pipelines,  |     |     |         |         |          |         |     |
|       |     | area               |     |     |         | PRODCT  |          |         |     |
1/2/3/7
RESTRN

41.1  Discharge pipe      PIPSOL  CATPIP  2/3/4  L  11.6.1
|     |     |     |     |     |     | PRODCT  | 3   |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |

41.2  Discharge pipe, area      PIPSOL  CATPIP  2/3/4  A  11.6.4
|     |     |     |     |     |     | PRODCT  | 3   |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |
RESTRN

42  Buried pipeline/pipe      PIPSOL  BURDEP    L  11.6.1
(with nominal depth
to which buried)
| 43  |     | Diffuser  |     |     | OBSTRN  | CATOBS  | 3  P  | 11.6.2  |     |
| --- | --- | --------- | --- | --- | ------- | ------- | ----- | ------- | --- |

|     |     |          |     |     |         | WATLEV  | 3     |         |     |
| --- | --- | -------- | --- | --- | ------- | ------- | ----- | ------- | --- |
|     |     |          |     |     |         | VALSOU  |       |         |     |
| 44  |     |          |     |     | PIPSOL  | STATUS  | 4  L  | 11.6.1  |     |
|     |     | Disused  |     |     |         |         |       |         |     |
pipelines/pipe

Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs

IM Tracks, Routes

Tracks             Tracks Marked by Lights   IP        Leading Beacons   IQ
| 1   |     |                    |     |           |         |       |         |     |
| --- | --- | ------------------ | --- | --------- | ------- | ----- | ------- | --- |
|     |     | Recommended track  |     |   NAVLNE  | CATNAV  | 3  L  | 10.1.1  |     |
|     |     |                    |     |           | ORIENT  |       |         |     |
on leading line
|     |     |     |     | RECTRC  | CATTRK  | 1  L  | 10.1.1  |     |
| --- | --- | --- | --- | ------- | ------- | ----- | ------- | --- |
|     |     |     |     |         | ORIENT  |       |         |     |
TRAFIC
2    Transit / clearing line    NAVLNE  CATNAV    L  10.1.1

ORIENT
| 3         |     | Recommended track     |     |         |         |       |         |     |
| --------- | --- | --------------------- | --- | ------- | ------- | ----- | ------- | --- |
|           |     |                       |     | RECTRC  | CATTRK  | 1  L  | 10.1.1  |     |
| 2 Bns  ‡  |     | based on a system of  |     |         | ORIENT  |       |         |     |
fixed marks
|     |     |                    |     |         | TRAFIC  | 4     |         |     |
| --- | --- | ------------------ | --- | ------- | ------- | ----- | ------- | --- |
| 4   |     |                    |     | RECTRC  | CATTRK  | 2  L  | 10.1.1  |     |
|     |     | Recommended track  |     |         |         |       |         |     |
|     |     | not based on a     |     |         | ORIENT  |       |         |     |
system of fixed
|     |     |     |     |     | TRAFIC  | 4   |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |
marks
| 5.1  |     |                |     |           |         |      |         |     |
| ---- | --- | -------------- | --- | --------- | ------- | ---- | ------- | --- |
|      |     | One way track  |     |   RECTRC  | CATTRK  |   L  | 10.1.1  |     |
|      |     |                |     |           | ORIENT  |      |         |     |
combined with
|     |     | routing element  |     |     | TRAFIC  | 1 or 3  |     |     |
| --- | --- | ---------------- | --- | --- | ------- | ------- | --- | --- |

| 5.2  |     |                |     | RECTRC  | CATTRK  |   L  | 10.1.1  |     |
| ---- | --- | -------------- | --- | ------- | ------- | ---- | ------- | --- |
|      |     | Two way track  |     |         |         |      |         |     |
|      |     | combined with  |     |         | ORIENT  |      |         |     |
routing element
|     |     |     |     |     | TRAFIC  | 4   |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |
(including a
TXTDSC
regulation described
INFORM
in a note)
| 6   |     |                    |     | RECTRC  | CATTRK  |   L  | 10.1.1  |     |
| --- | --- | ------------------ | --- | ------- | ------- | ---- | ------- | --- |
|     |     | Recommended track  |     |         |         |      |         |     |
|     |     | with maximum       |     |         | ORIENT  |      |         |     |
authorised draught
|     |     |     |     |     | TRAFIC  | 4    |     |     |
| --- | --- | --- | --- | --- | ------- | ---- | --- | --- |
|     |     |     |     |     | INFORM  | N.B  |     |     |

IM 6 INFORM should be used to indicate the authorised draught i.e. “7.3 is the maximum authorised draft”.
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
Routeing Measures – Basic Symbols

10    Established direction    TSSLPT  CATTSS    A  10.2.1.1
|     |     | of traffic flow  |     |     |         | ORIENT  |        |         |     |
| --- | --- | ---------------- | --- | --- | ------- | ------- | ------ | ------- | --- |
| 11  |     |                  |     |     |         |         |        |         |     |
|     |     | Recommended      |     |     | RCTLPT  | ORIENT  |   P/A  |         |     |
|     |     |                  |     |     |         | STATUS  |        | 10.2.5  |     |
direction of traffic flow
| 12  |     |                     |     |     |         |         |      |           |     |
| --- | --- | ------------------- | --- | --- | ------- | ------- | ---- | --------- | --- |
|     |     | Separation line     |     |     | TSELNE  | CATTSS  |   L  | 10.2.1.3  |     |
| 13  |     |                     |     |     | TSEZNE  | CATTSS  |   A  | 10.2.1.4  |     |
|     |     | Traffic Separation  |     |     |         |         |      |           |     |
Zone

| 14  |     |                           |     |     | ISTZNE  | CATTSS  | 1/2  L/A  | 10.2.1.7  |     |
| --- | --- | ------------------------- | --- | --- | ------- | ------- | --------- | --------- | --- |
|     |     | Limit of restricted area  |     |     |         |         |           |           |     |
(e.g. Inshore Traffic
Zone)
| 15  |     |                    |     |     | TSSBND  | CATTSS  |   L  | 10.2.1.2  |     |
| --- | --- | ------------------ | --- | --- | ------- | ------- | ---- | --------- | --- |
|     |     | Limit of routeing  |     |     |         |         |      |           |     |
measure
| 16  |     |                     |     |     | PRCARE  | INFORM  |   A  | 10.2.1.8  |     |
| --- | --- | ------------------- | --- | --- | ------- | ------- | ---- | --------- | --- |
|     |     | Precautionary Area  |     |     |         |         |      |           |     |
TXTDSC

| a   |     | TSS, roundabout  |     |     | TSSRON  | CATTSS  |   A  |     |     |
| --- | --- | ---------------- | --- | --- | ------- | ------- | ---- | --- | --- |

10.2.1.6
| b   |     |                   |     |     |         |         |      |           |     |
| --- | --- | ----------------- | --- | --- | ------- | ------- | ---- | --------- | --- |
|     |     | TSS, crossing     |     |     | TSSCRS  | CATTSS  |   A  | 10.2.1.5  |     |
| c   |     |                   |     |     | DWRTPT  | TRAFIC  |   A  |           |     |
|     |     | Deep Water Route  |     |     |         |         |      |           |     |
|     |     | Area              |     |     |         | ORIENT  |      | 10.2.2.1  |     |
DRVAL1

| d   |     | Deep Water Route  |     |     | DWRTCL  | TRAFIC  |   L  | 10.2.2.2  |     |
| --- | --- | ----------------- | --- | --- | ------- | ------- | ---- | --------- | --- |

|     |     | Centreline          |     |     |         | ORIENT  |        |           |     |
| --- | --- | ------------------- | --- | --- | ------- | ------- | ------ | --------- | --- |
|     |     |                     |     |     |         | CATTRK  |        |           |     |
| e   |     |                     |     |     | TWRTPT  | CATTRK  |   L/A  | 10.2.2.2  |     |
|     |     | Two-way deep water  |     |     |         |         |        |           |     |
|     |     | route               |     |     |         | TRAFIC  |        |           |     |
ORIENT

| f   |     | Safety Fairway  |     |     | FAIRWY  | INFORM  |     | 10.4  |     |
| --- | --- | --------------- | --- | --- | ------- | ------- | --- | ----- | --- |

TXTDSC
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
Examples of Routeing Measures
S-57 Appendix B.1 - Annex D November 2000 Edition 1.0

INT1 to S-57/52 for ENCs

Radar Surveillance System
| 30  |     |                |     | RADSTA  | CATRAS  | 1  P  | 12.11.3  |     |
| --- | --- | -------------- | --- | ------- | ------- | ----- | -------- | --- |
|     |     | Radar traffic  |     |         |         |       |          |     |
Radar Surveillance Station
surveillance station
| 31  |     |              |     | RADRNG  | OBJNAM  |   A  | 12.11.1  |     |
| --- | --- | ------------ | --- | ------- | ------- | ---- | -------- | --- |
|     |     | Radar range  |     |         |         |      |          |     |
32.1    Radar reference line      RADLNE  ORIENT    L  12.11.2
32.2    Radar reference line  RADLNE  ORIENT    L  12.11.2
|     |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
coinciding with a
|     |     |     |     | NAVLNE  | CATNAV  | 3  L  | 10.1.1  |     |
| --- | --- | --- | --- | ------- | ------- | ----- | ------- | --- |
leading line
|     |     |     |     |     | ORIENT  |     |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |

|     |     |     |     | RECTRC  | CATTRK  |   L  | 10.1.1  |     |
| --- | --- | --- | --- | ------- | ------- | ---- | ------- | --- |
|     |     |     |     |         | ORIENT  |      |         |     |
|     |     |     |     |         | TRAFIC  | 4    |         |     |

Radio Reporting

| 40  |     |                              |     | RDOCAL  | ORIENT  |   P/L  | 12.13  |     |
| --- | --- | ---------------------------- | --- | ------- | ------- | ------ | ------ | --- |
|     |     | Radio calling-in point, way  |     |         |         |        |        |     |
|     |     | point or reporting point     |     |         | TRAFIC  |        |        |     |
(with designation, if any)
OBJNAM
showing direction(s) of
vessel movement

Ferries
| 50  |     | Ferry  |     |   FERYRT  | CATFRY  | 1  L/A  | 10.3  |     |
| --- | --- | ------ | --- | --------- | ------- | ------- | ----- | --- |
|     |     |        |     |           |         |         |       |     |

| 51  |     | Cable ferry  |     |   FERYRT  | CATFRY  | 2  L/A  | 10.3  |     |
| --- | --- | ------------ | --- | --------- | ------- | ------- | ----- | --- |

Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs

IN Areas, Limits

General             Dredged and Swept Areas   II       Submarine Cables, Submarine Pipelines   IL       Tracks Routes   IM

| 1.1  |     |                             |     |     |     |     |     |     |     |     |
| ---- | --- | --------------------------- | --- | --- | --- | --- | --- | --- | --- | --- |
|      |     | Maritime limit in general,  |     |     |     |     |     |     |     |     |
usually implying physical
obstructions
| 1.2  |     |                             |     |     |     |     |     |     |     |     |
| ---- | --- | --------------------------- | --- | --- | --- | --- | --- | --- | --- | --- |
|      |     | Maritime limit in general,  |     |     |     |     |     |     |     |     |
usually implying no
physical obstructions

| 2.1  |     | Limit of restricted    |     |     |     | RESARE  | RESTRN  |   A   | 11.1  |     |
| ---- | --- | ---------------------- | --- | --- | --- | ------- | ------- | ----- | ----- | --- |
|      |     | area                   |     |     |     |         | CATREA  |       |       |     |
| 2.2  |     |                        |     |     |     |         |         |       |       |     |
|      |     | Limit of restricted    |     |     |     | RESARE  | RESTRN  | 7  A  | 11.1  |     |
|      |     | area into which entry  |     |     |     |         | CATREA  |       |       |     |
prohibited

Anchorages, Anchorage Areas
| 10  |     | Recommended  |     |     |     | ACHARE  | CATACH  | 1  P/A  | 9.2.1  |     |
| --- | --- | ------------ | --- | --- | --- | ------- | ------- | ------- | ------ | --- |
|     |     | anchorage    |     |     |     |         | STATUS  | 3       |        |     |

| 11.1  |     |     |     |     |     | ACHBRT  | CATACH  | 1  P  | 9.2.2  |     |
| ----- | --- | --- | --- | --- | --- | ------- | ------- | ----- | ------ | --- |
|       |     |     |     |     |     |         |         |       |        |     |
Anchor berths
|     |     |     |     |     |     |     | STATUS  | 1/3  |     |     |
| --- | --- | --- | --- | --- | --- | --- | ------- | ---- | --- | --- |
OBJNAM
11.2    Anchor berths with      ACHBRT  CATACH  1  P  9.2.2
|     |     | swinging circle  |     |     |     |     | RADIUS  |     |     |     |
| --- | --- | ---------------- | --- | --- | --- | --- | ------- | --- | --- | --- |
OBJNAM
| 12.1  |     |                    |     |     |     | ACHARE  | CATACH  | 1  A  | 9.2.1  |     |
| ----- | --- | ------------------ | --- | --- | --- | ------- | ------- | ----- | ------ | --- |
|       |     | Anchorage area in  |     |     |     |         |         |       |        |     |
|       |     | general            |     |     |     |         |         |       |        |     |
| 12.2  |     |                    |     |     |     |         |         |       |        |     |
|       |     | Numbered           |     |     |     | ACHARE  | CATACH  | 1  A  | 9.2.1  |     |
|       |     |                    |     |     |     |         | OBJNAM  |       |        |     |
anchorage area
| 12.3  |     |                  |     |     |     |         |         |       |        |     |
| ----- | --- | ---------------- | --- | --- | --- | ------- | ------- | ----- | ------ | --- |
|       |     | Named anchorage  |     |     |     | ACHARE  | CATACH  | 1  A  | 9.2.1  |     |
|       |     | area             |     |     |     |         | OBJNAM  |       |        |     |
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
| 12.4  |     |                     |     |           |         |       |        |     |
| ----- | --- | ------------------- | --- | --------- | ------- | ----- | ------ | --- |
|       |     | Deep water          |     |   ACHARE  | CATACH  | 2  A  | 9.2.1  |     |
|       |     | anchorage area      |     |           |         |       |        |     |
| 12.5  |     |                     |     | ACHARE    | CATACH  | 3  A  | 9.2.1  |     |
|       |     | Tanker anchorage    |     |           |         |       |        |     |
|       |     | area                |     |           |         |       |        |     |
| 12.6  |     |                     |     | ACHARE    | CATACH  | 9  A  | 9.2.1  |     |
|       |     | Anchorage area for  |     |           |         |       |        |     |
|       |     | periods up to 24    |     |           |         |       |        |     |
hours
| 12.7  |     |                     |     |           |         |       |        |     |
| ----- | --- | ------------------- | --- | --------- | ------- | ----- | ------ | --- |
|       |     | Explosives          |     |   ACHARE  | CATACH  | 4  A  | 9.2.1  |     |
|       |     | anchorage area      |     |           |         |       |        |     |
| 12.8  |     |                     |     | ACHARE    | CATACH  | 5  A  | 9.2.1  |     |
|       |     | Quarantine          |     |           |         |       |        |     |
|       |     | anchorage area      |     |           |         |       |        |     |
| 12.9  |     |                     |     | ACHARE    | STATUS  | 6  A  | 9.2.1  |     |
|       |     | Reserved anchorage  |     |           |         |       |        |     |
|       |     | area                |     |           | INFORM  |       |        |     |
TXTDSC

| 13  |     |                   |     |           |     |      |        |     |
| --- | --- | ----------------- | --- | --------- | --- | ---- | ------ | --- |
|     |     | Seaplane landing  |     |   SPLARE  |     |   A  | 11.12  |     |
area
| 14  |     |                |     |           |         |       |        |     |
| --- | --- | -------------- | --- | --------- | ------- | ----- | ------ | --- |
|     |     | Anchorage for  |     |   ACHARE  | CATACH  | 6  P  | 9.2.1  |     |
seaplanes

Restricted Areas
| 20  |     |                       |     | RESARE  | RESTRN  | 1  A  | 11.1  |     |
| --- | --- | --------------------- | --- | ------- | ------- | ----- | ----- | --- |
|     |     | Anchoring prohibited  |     |         |         |       |       |     |
21    Fishing prohibited      RESARE  RESTRN  3  A  11.1
| 22  |     |                  |     | RESARE  | CATREA  | 4  A  | 11.1  |     |
| --- | --- | ---------------- | --- | ------- | ------- | ----- | ----- | --- |
|     |     | Limit of nature  |     |         |         |       |       |     |
|     |     | reserve          |     |         | RESTRN  |       |       |     |
INFORM
TXTDSC
23.1    Explosives dumping    DMPGRD  CATDPG  4  A  11.4

ground
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 23.2  |     |                     |     |           |         |       |       |     |
| ----- | --- | ------------------- | --- | --------- | ------- | ----- | ----- | --- |
|       |     | Explosives dumping  |     |   DMPGRD  | CATDPG  | 4  A  | 11.4  |     |
|       |     | ground (disused)    |     |           | STATUS  | 4     |       |     |
RESTRN
| 24  |     |                     |     | DMPGRD    | CATDPG  | 2  A  | 11.4  |     |
| --- | --- | ------------------- | --- | --------- | ------- | ----- | ----- | --- |
|     |     | Dumping ground for  |     |           |         |       |       |     |
|     |     | chemical waste      |     |           | RESTRN  |       |       |     |
| 25  |     | Degaussing range    |     |   RESARE  | CATREA  | 8  A  | 11.1  |     |

RESTRN
| 26  |     |                 |     | RESARE  | CATREA  | 10  A  | 11.1  |     |
| --- | --- | --------------- | --- | ------- | ------- | ------ | ----- | --- |
|     |     | Historic wreck  |     |         |         |        |       |     |
RESTRN

Military Practice Areas
30    Firing practice area      MIPARE  CATMPA  4  A  11.3.1
RESTRN
| 31  |     |                       |     | RESARE  | CATREA  | 9  A  | 11.1    |     |
| --- | --- | --------------------- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Military restricted   |     |         |         |       |         |     |
|     |     | area                  |     |         | RESTRN  | 7     |         |     |
| 32  |     |                       |     | MIPARE  | CATMPA  | 5  A  | 11.3.1  |     |
|     |     | Mine laying practice  |     |         |         |       |         |     |
area
| 33  |     |                    |     |           |     |      |         |     |
| --- | --- | ------------------ | --- | --------- | --- | ---- | ------- | --- |
|     |     | Submarine transit  |     |   SUBTLN  |     |   A  | 11.3.2  |     |
line
|     |     | Submarine exercise  |     |   MIPARE  | CATMPA  | 3  A  | 11.3.1  |     |
| --- | --- | ------------------- | --- | --------- | ------- | ----- | ------- | --- |
INFORM
area
TXTDSC
| 34  |     |            |     | RESARE  | CATREA  | 14  A  | 11.3.3  |     |
| --- | --- | ---------- | --- | ------- | ------- | ------ | ------- | --- |
|     |     | Minefield  |     |         |         |        |         |     |
|     |     |            |     |         | RESTRN  |        |         |     |
INFORM
TXTDSC

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
International Boundaries and National Limits
| 40  |     | International     |     |     |   ADMARE  | JRSDTN  | 2  A  | 11.2.1  |     |
| --- | --- | ----------------- | --- | --- | --------- | ------- | ----- | ------- | --- |
|     |     | boundary on land  |     |     |           | OBJNAM  |       |         |     |
NATION
| 41  |     |                |     |     | TESARE  | NATION  |   A  | 11.2.4  |     |
| --- | --- | -------------- | --- | --- | ------- | ------- | ---- | ------- | --- |
|     |     | International  |     |     |         |         |      |         |     |
maritime boundary
| 42  |     |                           |     |     | STSLNE  | NATION  |   A  | 11.2.4  |     |
| --- | --- | ------------------------- | --- | --- | ------- | ------- | ---- | ------- | --- |
|     |     | Straight territorial sea  |     |     |         |         |      |         |     |
boundary
| 43  |     |                           |     |     | TESARE    | NATION  |   A  | 11.2.4  |     |
| --- | --- | ------------------------- | --- | --- | --------- | ------- | ---- | ------- | --- |
|     |     | Limit of territorial sea  |     |     |           |         |      |         |     |
| 44  |     |                           |     |     |           |         |      |         |     |
|     |     | Limit of contiguous       |     |     |   CONZNE  | NATION  |   A  | 11.2.5  |     |
zone
| 45  |     |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
National fishery limits      FSHZNE  NATION    A  11.2.6
INFORM
| 46  |     |                       |     |     | COSARE  | NATION  |   A  | 11.2.7  |     |
| --- | --- | --------------------- | --- | --- | ------- | ------- | ---- | ------- | --- |
|     |     | Limit of Continental  |     |     |         |         |      |         |     |
Shelf
| 47  |     |                     |     |     | EXEZNE  | NATION  |   A  | 11.2.8  |     |
| --- | --- | ------------------- | --- | --- | ------- | ------- | ---- | ------- | --- |
|     |     | Limit of Exclusive  |     |     |         |         |      |         |     |
Economic Zone
| 48  |     |                |     |     | CUSZNE    | NATION  |   A  | 11.2.2  |     |
| --- | --- | -------------- | --- | --- | --------- | ------- | ---- | ------- | --- |
|     |     | Customs limit  |     |     |           |         |      |         |     |
| 49  |     |                |     |     |           |         |      |         |     |
|     |     | Harbour limit  |     |     |   HRBARE  | OBJNAM  |   A  | 9.1.1   |     |

Various Limits
| 60.1  |     |                    |     |     | ICEARE    | CATICE  | 1  A  | 11.13.1  |     |
| ----- | --- | ------------------ | --- | --- | --------- | ------- | ----- | -------- | --- |
|       | #   | Limit of fast ice  |     |     |           |         |       |          |     |
| 60.2  |     |                    |     |     | ICEARE    | CATICE  | 8  A  | 11.13.1  |     |
|       | #   | Limit of sea ice   |     |     |           |         |       |          |     |
| 61    |     |                    |     |     | LOGPON    |         |   A   | 11.13.2  |     |
|       |     | Log pond           |     |     |           |         |       |          |     |
| 62.1  |     |                    |     |     | DMPGRD    | CATDPG  | 5  A  | 11.8     |     |
|       |     | Spoil ground       |     |     |           |         |       |          |     |
| 62.2  |     | Spoil ground       |     | )   |   DMPGRD  | CATDPG  | 5  A  | 11.8     |     |
(disused
|     |     |                |     |     |           | STATUS  | 4      |      |     |
| --- | --- | -------------- | --- | --- | --------- | ------- | ------ | ---- | --- |
| 63  |     |                |     |     |           |         |        |      |     |
|     |     | Dredging area  |     |     |   RESARE  | CATREA  | 21  A  | 5.5  |     |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 64  |     |                     |     |           |     |             |     |
| --- | --- | ------------------- | --- | --------- | --- | ----------- | --- |
|     |     | Cargo transhipment  |     |   CTSARE  |     | A  11.13.4  |     |
area
| 65  |     | Incineration area  |     |   ICNARE  |     | A  11.13.3  |     |
| --- | --- | ------------------ | --- | --------- | --- | ----------- | --- |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

IO Hydrographic Terms

Hydrographic Terms
| 1   |     |        |     |           |         |      |     |     |
| --- | --- | ------ | --- | --------- | ------- | ---- | --- | --- |
|     |     | Ocean  |     |   SEAARE  | OBJNAM  |   A  | 8   |     |
| 2   |     |        |     | SEAARE    | OBJNAM  |   A  | 8   |     |
|     |     | Sea    |     |           |         |      |     |     |
G.
| 3   |     | Gulf  |     |   SEAARE  | OBJNAM  |   A   | 8   |     |
| --- | --- | ----- | --- | --------- | ------- | ----- | --- | --- |
| 4   | B.  |       |     | SEAARE    | CATSEA  | 5  A  | 8   |     |
|     |     | Bay   |     |           |         |       |     |     |
OBJNAM
| 5   | Fj.  |                    |     | SEAARE    | OBJNAM  |   A    | 8      |     |
| --- | ---- | ------------------ | --- | --------- | ------- | ------ | ------ | --- |
|     |      | Fjord              |     |           |         |        |        |     |
| 6   | L.   |                    |     |           |         |        |        |     |
|     |      | Lake, Loch, Lough  |     |   SEAARE  | CATSEA  | 52  A  | 4.7.8  |     |
OBJNAM
| 7   | Cr.  |        |     | SEAARE  | CATSEA  | 53?  A  | 4.7.6  |     |
| --- | ---- | ------ | --- | ------- | ------- | ------- | ------ | --- |
|     |      | Creek  |     |         |         |         |        |     |
OBJNAM
| 8   | Lag.   |          |     | SEAARE    | OBJNAM  |   A  | 8   |     |
| --- | ------ | -------- | --- | --------- | ------- | ---- | --- | --- |
|     |        | Lagoon   |     |           |         |      |     |     |
| 9   | C.     |          |     |           |         |      |     |     |
|     |        | Cove     |     |   SEAARE  | OBJNAM  |   A  | 8   |     |
| 10  | Int.   |          |     | SEAARE    | OBJNAM  |   A  | 8   |     |
|     |        | Inlet    |     |           |         |      |     |     |
| 11  | Str.   | Strait   |     |   SEAARE  | OBJNAM  |   A  | 8   |     |
| 12  | Sd.    |          |     | SEAARE    | OBJNAM  |   A  | 8   |     |
|     |        | Sound    |     |           |         |      |     |     |
| 13  | Pass.  |          |     | SEAARE    | CATSEA  |   A  | 8   |     |
|     |        | Passage  |     |           |         |      |     |     |
OBJNAM
| 14  | Chan.  | Channel  |     |   SEAARE  | CATSEA  |   A  | 8   |     |
| --- | ------ | -------- | --- | --------- | ------- | ---- | --- | --- |
OBJNAM
| 15  | Nrs.  |          |     | SEAARE  | CATSEA  | 12  A  | 8   |     |
| --- | ----- | -------- | --- | ------- | ------- | ------ | --- | --- |
|     |       | Narrows  |     |         |         |        |     |     |
OBJNAM
| 16  | Ent.  |                   |     | SEAARE    | OBJNAM  |   A  | 8   |     |
| --- | ----- | ----------------- | --- | --------- | ------- | ---- | --- | --- |
|     |       | Entrance          |     |           |         |      |     |     |
| 17  | Est.  | Estuary           |     |   SEAARE  | OBJNAM  |   A  | 8   |     |
| 18  |       |                   |     | SEAARE    | OBJNAM  |   A  | 8   |     |
|     |       | Delta             |     |           |         |      |     |     |
| 19  |       |                   |     | SEAARE    | OBJNAM  |   A  | 8   |     |
|     |       | Mouth             |     |           |         |      |     |     |
| 20  | Rds.  |                   |     |           |         |      |     |     |
|     |       | Roads, Roadstead  |     |   SEAARE  | OBJNAM  |   A  | 8   |     |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 21  | Anch.  |            |     |           |         |       |        |     |
| --- | ------ | ---------- | --- | --------- | ------- | ----- | ------ | --- |
|     |        | Anchorage  |     |   ACHARE  | CATACH  |   A   | 9.2.1  |     |
|     |        |            |     |           |         |       |        |     |
| 22  | App.   |            |     | SEAARE    | OBJNAM  |   A   | 8      |     |
|     |        | Approach   |     |           |         |       |        |     |
| 23  | Bk.    | Bank       |     |   SEAARE  | CATSEA  | 3  A  | 8      |     |
OBJNAM
| 25  | Sh.  |        |     |           |         |        |     |     |
| --- | ---- | ------ | --- | --------- | ------- | ------ | --- | --- |
|     |      | Shoal  |     |   SEAARE  | CATSEA  | 13  A  | 8   |     |
OBJNAM
| 26  | Rf.  |       |     | SEAARE  | CATSEA  | 9  A  | 8   |     |
| --- | ---- | ----- | --- | ------- | ------- | ----- | --- | --- |
|     |      | Reef  |     |         |         |       |     |     |
OBJNAM
| 27  | Rk.  | Sunken Rock  |     |   UWTROC  | WATLEV  | 3  P  | 6.1.2  |     |
| --- | ---- | ------------ | --- | --------- | ------- | ----- | ------ | --- |

VALSOU
| 28  | Le.  |        |     |           |         |        |     |     |
| --- | ---- | ------ | --- | --------- | ------- | ------ | --- | --- |
|     |      | Ledge  |     |   SEAARE  | CATSEA  | 10  A  | 8   |     |
OBJNAM
| 29  |     |           |     | SEAARE  | CATSEA  | 17  A  | 8   |     |
| --- | --- | --------- | --- | ------- | ------- | ------ | --- | --- |
|     |     | Pinnacle  |     |         |         |        |     |     |
OBJNAM
| 30  |     |        |     | SEAARE  | CATSEA  | 15  A  | 8   |     |
| --- | --- | ------ | --- | ------- | ------- | ------ | --- | --- |
|     |     | Ridge  |     |         |         |        |     |     |
OBJNAM

| 31  |     | Rise  |     |   SEAARE  | CATSEA  | 43  A  | 8   |     |
| --- | --- | ----- | --- | --------- | ------- | ------ | --- | --- |
OBJNAM
| 32  | Mtn.  |           |     | SEAARE  | CATSEA  | 40  A  | 8   |     |
| --- | ----- | --------- | --- | ------- | ------- | ------ | --- | --- |
|     |       | Mountain  |     |         |         |        |     |     |
OBJNAM
| 33  | SMt  |           |     | SEAARE  | CATSEA  | 16  A  | 8   |     |
| --- | ---- | --------- | --- | ------- | ------- | ------ | --- | --- |
|     |      | Seamount  |     |         |         |        |     |     |
OBJNAM
| 34  |     | Seamount chain  |     |   SEAARE  | CATSEA  | 45  A  | 8   |     |
| --- | --- | --------------- | --- | --------- | ------- | ------ | --- | --- |
OBJNAM
| 35  | Pk.  |       |     |           |         |        |     |     |
| --- | ---- | ----- | --- | --------- | ------- | ------ | --- | --- |
|     |      | Peak  |     |   SEAARE  | CATSEA  | 41  A  | 8   |     |
OBJNAM
| 36  |     |        |     | SEAARE  | CATSEA  | 14  A  | 8   |     |
| --- | --- | ------ | --- | ------- | ------- | ------ | --- | --- |
|     |     | Knoll  |     |         |         |        |     |     |
OBJNAM
| 37  |     | Abysall hill  |     |   SEAARE  | CATSEA  | 24  A  | 8   |     |
| --- | --- | ------------- | --- | --------- | ------- | ------ | --- | --- |

OBJNAM
| 38  |     |             |     |           |         |        |     |     |
| --- | --- | ----------- | --- | --------- | ------- | ------ | --- | --- |
|     |     | Tablemount  |     |   SEAARE  | CATSEA  | 34  A  | 8   |     |
OBJNAM
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
| 39  |     |          |     |           |         |        |     |     |
| --- | --- | -------- | --- | --------- | ------- | ------ | --- | --- |
|     |     | Plateau  |     |   SEAARE  | CATSEA  | 19  A  | 8   |     |
OBJNAM
| 40  |     |          |     | SEAARE  | CATSEA  | 49  A  | 8   |     |
| --- | --- | -------- | --- | ------- | ------- | ------ | --- | --- |
|     |     | Terrace  |     |         |         |        |     |     |
OBJNAM
| 41  |     | Spur  |     |   SEAARE  | CATSEA  | 20  A  | 8   |     |
| --- | --- | ----- | --- | --------- | ------- | ------ | --- | --- |

OBJNAM
| 42  |     |                    |     |           |         |        |     |     |
| --- | --- | ------------------ | --- | --------- | ------- | ------ | --- | --- |
|     |     | Continental shelf  |     |   SEAARE  | CATSEA  | 21  A  | 8   |     |
OBJNAM
| 43  |     |             |     | SEAARE  | CATSEA  | 46  A  | 8   |     |
| --- | --- | ----------- | --- | ------- | ------- | ------ | --- | --- |
|     |     | Shelf edge  |     |         |         |        |     |     |
OBJNAM
| 44  |     |        |     | SEAARE  | CATSEA  | 48  A  | 8   |     |
| --- | --- | ------ | --- | ------- | ------- | ------ | --- | --- |
|     |     | Slope  |     |         |         |        |     |     |
OBJNAM

| 45  |     | Continental slope  |     |   SEAARE  | CATSEA  | 48  A  | 8   |     |
| --- | --- | ------------------ | --- | --------- | ------- | ------ | --- | --- |
OBJNAM
| 46  |     |                   |     | SEAARE  | CATSEA  | 29  A  | 8   |     |
| --- | --- | ----------------- | --- | ------- | ------- | ------ | --- | --- |
|     |     | Continental rise  |     |         |         |        |     |     |
OBJNAM
| 47  |     |              |     | SEAARE  | CATSEA  | 27  A  | 8   |     |
| --- | --- | ------------ | --- | ------- | ------- | ------ | --- | --- |
|     |     | Continental  |     |         |         |        |     |     |
|     |     | borderland   |     |         | OBJNAM  |        |     |     |
| 48  |     |              |     | SEAARE  | CATSEA  | 7  A   | 8   |     |
|     |     | Basin        |     |         |         |        |     |     |
OBJNAM
| 49  |     |                |     |           |         |        |     |     |
| --- | --- | -------------- | --- | --------- | ------- | ------ | --- | --- |
|     |     | Abyssal plain  |     |   SEAARE  | CATSEA  | 18  A  | 8   |     |
OBJNAM
| 50  |     |       |     | SEAARE  | CATSEA  | 36  A  | 8   |     |
| --- | --- | ----- | --- | ------- | ------- | ------ | --- | --- |
|     |     | Hole  |     |         |         |        |     |     |
OBJNAM
| 51  |     |         |     | SEAARE  | CATSEA  | 6  A  | 8   |     |
| --- | --- | ------- | --- | ------- | ------- | ----- | --- | --- |
|     |     | Trench  |     |         |         |       |     |     |
OBJNAM
| 52  |     | Trough  |     |   SEAARE  | CATSEA  | 22  A  | 8   |     |
| --- | --- | ------- | --- | --------- | ------- | ------ | --- | --- |
OBJNAM
| 53  |     |         |     |           |         |        |     |     |
| --- | --- | ------- | --- | --------- | ------- | ------ | --- | --- |
|     |     | Valley  |     |   SEAARE  | CATSEA  | 50  A  | 8   |     |
OBJNAM
| 54  |     |                |     | SEAARE  | CATSEA  | 38  A  | 8   |     |
| --- | --- | -------------- | --- | ------- | ------- | ------ | --- | --- |
|     |     | Median Valley  |     |         |         |        |     |     |
OBJNAM
| 55  |     | Canyon  |     |   SEAARE  | CATSEA  | 11  A  | 8   |     |
| --- | --- | ------- | --- | --------- | ------- | ------ | --- | --- |

OBJNAM
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 56  |     |             |     |           |         |     |       |     |
| --- | --- | ----------- | --- | --------- | ------- | --- | ----- | --- |
|     |     | Seachannel  |     |   SEAARE  | CATSEA  | 44  | A  8  |     |
OBJNAM
| 57  |     |                 |     | SEAARE  | CATSEA  | 39  | A  8  |     |
| --- | --- | --------------- | --- | ------- | ------- | --- | ----- | --- |
|     |     | Moat, Sea moat  |     |         |         |     |       |     |
OBJNAM
| 58  |     | Fan  |     |   SEAARE  | CATSEA  | 31  | A  8  |     |
| --- | --- | ---- | --- | --------- | ------- | --- | ----- | --- |

OBJNAM
| 59  |     |        |     |           |         |     |       |     |
| --- | --- | ------ | --- | --------- | ------- | --- | ----- | --- |
|     |     | Apron  |     |   SEAARE  | CATSEA  | 25  | A  8  |     |
OBJNAM
| 60  |     |                |     | SEAARE  | CATSEA  | 32  | A  8  |     |
| --- | --- | -------------- | --- | ------- | ------- | --- | ----- | --- |
|     |     | Fracture zone  |     |         |         |     |       |     |
OBJNAM
| 61  |     |                    |     | SEAARE  | CATSEA  | 30  | A  8  |     |
| --- | --- | ------------------ | --- | ------- | ------- | --- | ----- | --- |
|     |     | Scarp, Escarpment  |     |         |         |     |       |     |
OBJNAM

| 62  |     | Sill  |     |   SEAARE  | CATSEA  | 47  | A  8  |     |
| --- | --- | ----- | --- | --------- | ------- | --- | ----- | --- |
OBJNAM
| 63  |     |      |     | SEAARE  | CATSEA  | 33  | A  8  |     |
| --- | --- | ---- | --- | ------- | ------- | --- | ----- | --- |
|     |     | Cap  |     |         |         |     |       |     |
OBJNAM
| 64  |     |         |     | SEAARE  | CATSEA  | 23  | A  8  |     |
| --- | --- | ------- | --- | ------- | ------- | --- | ----- | --- |
|     |     | Saddle  |     |         |         |     |       |     |
OBJNAM
| 65  |     | Levee  |     |   SEAARE  | CATSEA  | 37  | A  8  |     |
| --- | --- | ------ | --- | --------- | ------- | --- | ----- | --- |
OBJNAM
| 66  |     |           |     |           |         |     |       |     |
| --- | --- | --------- | --- | --------- | ------- | --- | ----- | --- |
|     |     | Province  |     |   SEAARE  | CATSEA  | 42  | A  8  |     |
OBJNAM
| 67  |     |                       |     |           | TIDEWY  |     | L/A  7.2.4  |     |
| --- | --- | --------------------- | --- | --------- | ------- | --- | ----------- | --- |
|     |     | Tideway, Tidal gully  |     |           |         |     |             |     |
| 68  |     | Sidearm               |     |   SEAARE  | OBJNAM  |     | A  8        |     |

Other Terms
| 80  |     |            |     |     |   CONDTN  | 5           |     |     |
| --- | --- | ---------- | --- | --- | --------- | ----------- | --- | --- |
|     |     | projected  |     |     |           |             |     |     |
| 81  |     |            |     |     |           | STATUS  12  |     |     |
|     |     | lighted    |     |     |           |             |     |     |
| 82  |     |            |     |     |           |             |     |     |
|     |     | buoyed     |     |     |           | INFORM      |     |     |
| 83  |     |            |     |     |           | INFORM      |     |     |
|     |     | marked     |     |     |           |             |     |     |
| 84  |     | ancient    |     |     |           | INFORM      |     |     |
| 85  |     |            |     |     |           | INFORM      |     |     |
|     |     | distant    |     |     |           |             |     |     |
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
| 86  |     |              |     |     |           |      |     |     |
| --- | --- | ------------ | --- | --- | --------- | ---- | --- | --- |
|     |     | lesser       |     |     |   INFORM  |      |     |     |
| 87  |     |              |     |     |   INFORM  |      |     |     |
|     |     | closed       |     |     |           |      |     |     |
| 88  |     |              |     |     |           |      |     |     |
|     |     | partly       |     |     |   INFORM  |      |     |     |
| 89  |     |              |     |     |   QUAPOS  | 4    |     |     |
|     |     | approximate  |     |     |           |      |     |     |
| 90  |     | submerged    |     |     |   WATLEV  | 3    |     |     |

| 91  |     |               |     |     |           |      |     |     |
| --- | --- | ------------- | --- | --- | --------- | ---- | --- | --- |
|     |     | shoaled       |     |     |   INFORM  |      |     |     |
| 92  |     |               |     |     |   INFORM  |      |     |     |
|     |     | experimental  |     |     |           |      |     |     |
| 93  |     | destroyed     |     |     |   CONDTN  | 2    |     |     |

Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
IP Lights

Light Structures, Major Floating Lights       Beacons   IQ
| 1   |     |                      |     |         | LIGHTS  COLOUR  |         | P  12.8   |     |
| --- | --- | -------------------- | --- | ------- | --------------- | ------- | --------- | --- |
|     |     | Major light, minor   |     |         |                 |         |           |     |
|     |     | lights, light house  |     |         |                 | LITCHR  |           |     |
|     |     |                      |     | LNDMRK  | CATLMK          |         |   4.8.15  |     |
CONVIS
| 2   |     |                   |     |         | LIGHTS  COLOUR  |         | P  12.8    |     |
| --- | --- | ----------------- | --- | ------- | --------------- | ------- | ---------- | --- |
|     |     | Lighted offshore  |     |         |                 |         |            |     |
|     |     | platform          |     |         |                 | LITCHR  |            |     |
|     |     |                   |     |         | OFSPLF          |         |   11.7.2   |     |
| 3   |     | Lighted beacon    |     |         | LIGHTS  COLOUR  |         | P  12.8    |     |
|     |     | tower             |     |         |                 | LITCHR  |            |     |
|     |     |                   |     | BCNXXX  | BCNSHP          | 3       | P  12.3.1  |     |
COLOUR
|     |     |                  |     | TOPMAR  | TOPSHP          |     | P  12.6  |     |
| --- | --- | ---------------- | --- | ------- | --------------- | --- | -------- | --- |
| 4   |     |                  |     |         | LIGHTS  COLOUR  |     | P  12.8  |     |
|     |     | Lighted beacons  |     |         |                 |     |          |     |
LITCHR
|     |     |     |     | BCNXXX  | BCNSHP  |     | P  12.3.1  |     |
| --- | --- | --- | --- | ------- | ------- | --- | ---------- | --- |
COLOUR
|     |     |                   |     | TOPMAR  | TOPSHP          |     | P  12.6  |     |
| --- | --- | ----------------- | --- | ------- | --------------- | --- | -------- | --- |
| 5   |     |                   |     |         |                 |     |          |     |
|     |     | Buoyant beacons   |     |         | LIGHTS  COLOUR  |     | P  12.8  |     |
LITCHR
|     |     |     |     | BCNXXX  | BCNSHP  | 7   | P  12.3.1  |     |
| --- | --- | --- | --- | ------- | ------- | --- | ---------- | --- |
COLOUR
|     |     |                       |     | TOPMAR  | TOPSHP          |     | P  12.6  |     |
| --- | --- | --------------------- | --- | ------- | --------------- | --- | -------- | --- |
| 6   |     | Major floating light  |     |         | LIGHTS  COLOUR  |     | P  12.8  |     |

|     |     | (light-vessel, major  |     |     |     | LITCHR  |     |     |
| --- | --- | --------------------- | --- | --- | --- | ------- | --- | --- |
light-float, LANBY
|     |     |     |     |     | LITFLT  COLOUR  |     | P  12.4.2  |     |
| --- | --- | --- | --- | --- | --------------- | --- | ---------- | --- |
|     |     |     |     |     | LITVES  COLOUR  |     | P          |     |

Light Characters                 Light Characters on Light Buoys   IQ
|     | Abbreviation  | Class of light  |     |     |     |     |     |     |
| --- | ------------- | --------------- | --- | --- | --- | --- | --- | --- |
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
| 10.1  | F        |                   |     |     |           |         |        |         |     |
| ----- | -------- | ----------------- | --- | --- | --------- | ------- | ------ | ------- | --- |
|       |          | Fixed             |     |     |   LIGHTS  | LITCHR  | 1  P   | 12.8.3  |     |
| 10.2  |          |                   |     |     | LIGHTS    | LITCHR  | 8  P   | 12.8.3  |     |
|       | Oc       | Single occulting  |     |     |           |         |        |         |     |
|       |          |                   |     |     |           | SIGGRP  | ()     |         |     |
|       |          |                   |     |     | LIGHTS    | LITCHR  | 8  P   | 12.8.3  |     |
|       | Oc(2)    | Group occulting   |     |     |           |         |        |         |     |
|       |          |                   |     |     |           | SIGGRP  | (2)    |         |     |
|       | Oc(2+3)  | Composite group   |     |     |   LIGHTS  | LITCHR  | 8  P   | 12.8.3  |     |
|       |          |                   |     |     |           | SIGGRP  | (2+3)  |         |     |
occulting
| 10.3  | Iso      |                      |     |     | LIGHTS    | LITCHR  | 7  P   | 12.8.3  |     |
| ----- | -------- | -------------------- | --- | --- | --------- | ------- | ------ | ------- | --- |
|       |          | Isophase             |     |     |           |         |        |         |     |
|       |          |                      |     |     |           | SIGGRP  | ()     |         |     |
| 10.4  |          |                      |     |     | LIGHTS    | LITCHR  | 2  P   | 12.8.3  |     |
|       | Fl       | Single flashing      |     |     |           |         |        |         |     |
|       |          |                      |     |     |           | SIGGRP  | (1)    |         |     |
|       | Fl(3)    | Group flashing       |     |     |   LIGHTS  | LITCHR  | 2  P   | 12.8.3  |     |
|       |          |                      |     |     |           | SIGGRP  | (3)    |         |     |
|       |          |                      |     |     | LIGHTS    | LITCHR  | 2  P   | 12.8.3  |     |
|       | Fl(2+1)  | Composite group      |     |     |           |         |        |         |     |
|       |          | flashing             |     |     |           | SIGGRP  | (2+1)  |         |     |
| 10.5  | LFl      |                      |     |     | LIGHTS    | LITCHR  | 3  P   | 12.8.3  |     |
|       |          | Long flashing(flash  |     |     |           |         |        |         |     |
|       |          | 2sec or longer)      |     |     |           | SIGGRP  | ()     |         |     |
| 10.6  |          |                      |     |     | LIGHTS    | LITCHR  | 4  P   | 12.8.3  |     |
|       | Q        | Continuous quick     |     |     |           |         |        |         |     |
|       |          |                      |     |     |           | SIGGRP  | ()     |         |     |
|       |          | Group quick          |     |     |   LIGHTS  | LITCHR  | 4  P   | 12.8.3  |     |
|       | Q(3)     |                      |     |     |           |         |        |         |     |
|       |          |                      |     |     |           | SIGGRP  | (3)    |         |     |
|       | IQ       | Interrupted quick    |     |     |   LIGHTS  | LITCHR  | 9  P   | 12.8.3  |     |
|       |          |                      |     |     |           | SIGGRP  | ()     |         |     |
| 10.7  |          |                      |     |     | LIGHTS    | LITCHR  | 5  P   | 12.8.3  |     |
|       | VQ       | Continuous very      |     |     |           |         |        |         |     |
|       |          | quick                |     |     |           | SIGGRP  | (1)    |         |     |
|       |          |                      |     |     | LIGHTS    | LITCHR  | 5  P   | 12.8.3  |     |
|       | VQ(3)    | Group very quick     |     |     |           |         |        |         |     |
|       |          |                      |     |     |           | SIGGRP  | (3)    |         |     |
|       |          | Interrupted very     |     |     |   LIGHTS  | LITCHR  | 10  P  | 12.8.3  |     |
|       | IVQ      |                      |     |     |           |         |        |         |     |
|       |          | quick                |     |     |           | SIGGRP  | ()     |         |     |
10.8  UQ  Continuous ultra      LIGHTS  LITCHR  6  P  12.8.3
|     |      | quick              |     |     |           | SIGGRP  | ()     |         |     |
| --- | ---- | ------------------ | --- | --- | --------- | ------- | ------ | ------- | --- |
|     | IUQ  | Interrupted ultra  |     |     |   LIGHTS  | LITCHR  | 11  P  | 12.8.3  |     |
|     |      | quick              |     |     |           | SIGGRP  | ()     |         |     |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 10.9   | Mo(K)  |                     |     |     |           |         |        |         |     |
| ------ | ------ | ------------------- | --- | --- | --------- | ------- | ------ | ------- | --- |
|        |        | Morse Code          |     |     |   LIGHTS  | LITCHR  | 12  P  | 12.8.3  |     |
|        |        |                     |     |     |           | SIGGRP  | (K)    |         |     |
| 10.10  | FFl    |                     |     |     | LIGHTS    | LITCHR  | 13  P  | 12.8.3  |     |
|        |        | Fixed and flashing  |     |     |           |         |        |         |     |
|        |        |                     |     |     |           | SIGGRP  | ()(1)  |         |     |
10.11  Al.WR  Alternating    LIGHTS  LITCHR  28  P  12.8.3

|     |     |     |     |     |     | SIGGRP  | ()  |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- | --- |

Colour of Lights
| 11.1  | W   |     |     |     |     |     |     |     |     |
| ----- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
White (may be omitted)      LIGHTS  COLOUR  1  P  12.8.1
| 11.2  | R   | Red     |     |     | LIGHTS    | COLOUR  | 3  P   | 12.8.1  |     |
| ----- | --- | ------- | --- | --- | --------- | ------- | ------ | ------- | --- |
|       |     |         |     |     |           |         |        |         |     |
| 11.3  | G   | Green   |     |     |   LIGHTS  | COLOUR  | 4  P   | 12.8.1  |     |
| 11.4  | Bu  |         |     |     | LIGHTS    | COLOUR  | 5  P   | 12.8.1  |     |
|       |     | Blue    |     |     |           |         |        |         |     |
| 11.5  | Vi  | Violet  |     |     | LIGHTS    | COLOUR  | 10  P  | 12.8.1  |     |
|       |     |         |     |     |           |         |        |         |     |
| 11.6  | Y   |         |     |     |           |         |        |         |     |
|       |     | Yellow  |     |     |   LIGHTS  | COLOUR  | 6  P   | 12.8.1  |     |
| 11.7  | Or  | Orange  |     |     | LIGHTS    | COLOUR  | 11  P  | 12.8.1  |     |
|       |     |         |     |     |           |         |        |         |     |
| 11.8  | Am  | Amber   |     |     |   LIGHTS  | COLOUR  | 9  P   | 12.8.1  |     |

Period
| 12  | 90s  |                    |     |     | LIGHTS  | SIGPER  |   P  | 12.8.1  |     |
| --- | ---- | ------------------ | --- | --- | ------- | ------- | ---- | ------- | --- |
|     |      | Period in seconds  |     |     |         |         |      |         |     |

Elevation
| 13  | 12m  |                     |     |     | LIGHTS  | HEIGHT  |   P  | 12.8.4  |     |
| --- | ---- | ------------------- | --- | --- | ------- | ------- | ---- | ------- | --- |
|     |      | Elevation of light  |     |     |         |         |      |         |     |
|     |      | given in metres     |     |     |         |         |      |         |     |

Range
14  15/M  Light with single      LIGHTS  VALNMR      12.8.1
range
|     | 15/10M  | Light with two  |     |     |   LIGHTS  | VALNMR  |     |     |     |
| --- | ------- | --------------- | --- | --- | --------- | ------- | --- | --- | --- |
different ranges
|     | 15-7M  | Light with three or  |     |     |   LIGHTS  | VALNMR  |     |     |     |
| --- | ------ | -------------------- | --- | --- | --------- | ------- | --- | --- | --- |
more ranges
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

Disposition
| 15  | (hor)  |                        |     | LIGHTS  | CATLIT  | 19  P  | 12.8.1  |     |
| --- | ------ | ---------------------- | --- | ------- | ------- | ------ | ------- | --- |
|     |        | horizontally disposed  |     |         |         |        |         |     |
MLTYLT
|     | (vert)  |                        |     | LIGHTS  | CATLIT  | 20  P  | 12.8.1  |     |
| --- | ------- | ---------------------- | --- | ------- | ------- | ------ | ------- | --- |
|     |         | horizontally disposed  |     |         |         |        |         |     |
MLTYLT

Example of a full Light Description
16  Fl(3)WRG. 15s13m7-5M        LIGHTS  CATLIT  1  P  12.8.1
|     |     |     |     | X 3  | LITCHR  | 2   |     |     |
| --- | --- | --- | --- | ---- | ------- | --- | --- | --- |
COLOUR
1&3&4
|     |     |     |     |     | SIGPER  | 15   |     |     |
| --- | --- | --- | --- | --- | ------- | ---- | --- | --- |
|     |     |     |     |     | SIGGRP  | (3)  |     |     |
|     |     |     |     |     | HEIGHT  | 13   |     |     |
|     |     |     |     |     | VALNMR  |      |     |     |
SECTR1
SECTR2

Lights marking Fairways       Note: Quoted bearings are always from seaward

Leading Lights and Lights in line

20.1    Leading lights with    LIGHTS  CATLIT    P  12.8.6
|     |     | leading line and arcs  |     | X 2  | SECTR1  |     |     |     |
| --- | --- | ---------------------- | --- | ---- | ------- | --- | --- | --- |
|     |     | of visibility          |     |      | SECTR2  |     |     |     |
 The bearing may
|     |     |     |     | NAVLNE  | CATNAV  | 3  L  | 10.1.1  |     |
| --- | --- | --- | --- | ------- | ------- | ----- | ------- | --- |
be shown in degrees
|     |     |     |     |     | ORIENT  |     |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |
and tenths of
|     |     | degrees  |     | RECTRC  | CATTRK  | 1  L  | 10.1.1  |     |
| --- | --- | -------- | --- | ------- | ------- | ----- | ------- | --- |
|     |     |          |     |         | ORIENT  |       |         |     |
|     |     |          |     |         | TRAFIC  | 4     |         |     |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 20.2  |     |                 |     |     |         |         |       |              |     |
| ----- | --- | --------------- | --- | --- | ------- | ------- | ----- | ------------ | --- |
|       |     | Leading lights  |     |     | LIGHTS  | CATLIT  | 4,12  | P  12.8.6.4  |     |
|       |     |                 |     |     | X 2     |         | 4,13  |              |     |
|       |     |                 |     |     |         | LITCHR  | 8     |              |     |
|       |     |                 |     |     |         | COLOUR  | 1,3   |              |     |
|       |     |                 |     |     |         | SIGPER  | 4     |              |     |
|       |     |                 |     |     |         | SIGGRP  | (1)   |              |     |
VALNMR
|       |                      |                       |     |     | NAVLNE  | CATNAV  | 3      | L  10.1.1    |     |
| ----- | -------------------- | --------------------- | --- | --- | ------- | ------- | ------ | ------------ | --- |
|       |                      |                       |     |     |         | ORIENT  |        |              |     |
|       |                      |                       |     |     | RECTRC  | CATTRK  | 1      | L  10.1.1    |     |
|       |                      |                       |     |     |         | ORIENT  |        |              |     |
|       |                      |                       |     |     |         | TRAFIC  | 4      |              |     |
| 20.3  |                      |                       |     |     | LIGHTS  | CATLIT  | 4,12   | P  12.8.6.4  |     |
|       |                      | Leading lights on     |     |     |         |         |        |              |     |
|       |                      | small scale charts    |     |     | X 2     |         | 4,13   |              |     |
|       |                      |                       |     |     |         | LITCHR  | 8      |              |     |
|       |                      |                       |     |     |         | COLOUR  | 1,3    |              |     |
|       |                      |                       |     |     |         | SIGPER  |        |              |     |
|       |                      |                       |     |     |         | SIGGRP  | ()     |              |     |
| 21    |                      |                       |     |     |         |         |        |              |     |
|       |                      | Lights in line,       |     |     | LIGHTS  | CATLIT  | 4,12   | P  12.8.6.4  |     |
|       |                      | marking the sides of  |     |     | X 4     |         | 4,13   |              |     |
|       |                      | a channel             |     |     |         | LITCHR  | 2      |              |     |
|       |                      |                       |     |     |         | COLOUR  | 4/3    |              |     |
|       |                      |                       |     |     |         | SIGPER  |        |              |     |
|       |                      |                       |     |     |         | SIGGRP  | ()     |              |     |
|       |                      |                       |     |     | NAVLNE  | CATNAV  | 1      | L  10.1.1    |     |
|       |                      |                       |     |     | X2      | ORIENT  |        |              |     |
|       |                      |                       |     |     |         |         |        |              |     |
| 22    |                      |                       |     |     | LIGHTS  | CATLIT  | 13/15  | P  12.8.6.4  |     |
|       | Rear Lt or Upper Lt  | Rear or Upper Lt      |     |     |         |         |        |              |     |
23
Frontr Lt or Lower Lt  Front or lower light      LIGHTS  CATLIT  12/14  P  12.8.6.4

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
Direction Lights
|       |     | Direction light with  |     |     |                 |       |           |     |
| ----- | --- | --------------------- | --- | --- | --------------- | ----- | --------- | --- |
| 30.1  |     |                       |     |     | LIGHTS  CATLIT  | 1  P  | 12.8.6.5  |     |
narrow sector
ORIENT
SECTR1
SECTR2
LITCHR
COLOUR
SIGPER
SIGGRP
LITVIS
HEIGHT
VALNMR
| 30.2  |     |     |     |     |     |     |     |     |
| ----- | --- | --- | --- | --- | --- | --- | --- | --- |
Direction light with      LIGHTS  CATLIT  1  P  12.8.6.5
|     |     | course to be followed  |     |     | X2  ORIENT  |     |     |     |
| --- | --- | ---------------------- | --- | --- | ----------- | --- | --- | --- |
 LITCHR
COLOUR
SIGPER
SIGGRP
VALNMR
|     |     |     |     |     | RECTRC  CATTRK  | 1  L  | 10.1.1  |     |
| --- | --- | --- | --- | --- | --------------- | ----- | ------- | --- |
|     |     |     |     |     | X2  ORIENT      |       |         |     |
TRAFIC  4
|       |     |     |     |     | NAVLNE  CATNAV  | 3  L  | 10.1.1  |     |
| ----- | --- | --- | --- | --- | --------------- | ----- | ------- | --- |
|       |     |     |     |     |   ORIENT        |       |         |     |
| 30.3  |     |     |     |     |                 |       |         |     |
Direction light with      LIGHTS  CATLIT  1  P  12.8.6.5
|     |     | narrow fairway  |     |     | X5  ORIENT  |     |     |     |
| --- | --- | --------------- | --- | --- | ----------- | --- | --- | --- |
SECTR1
SECTR2
LITCHR
COLOUR
SIGPER
VALNMR

| 31  |     | Moiré effect light  |     |     | LIGHTS  CATLIT  | 16  P  |     |     |
| --- | --- | ------------------- | --- | --- | --------------- | ------ | --- | --- |

|     |     |     |     |     | ORIENT  |     | 12.8.6.6  |     |
| --- | --- | --- | --- | --- | ------- | --- | --------- | --- |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs

Sector Lights
| 40  |     |                  |     |     | LIGHTS  | CATLIT    | P  12.8.6.1  |     |
| --- | --- | ---------------- | --- | --- | ------- | --------- | ------------ | --- |
|     |     | Sector light on  |     |     |         |           |              |     |
|     |     | standard charts  |     |     | X3      | ORIENT    |              |     |
SECTR1
SECTR2
LITCHR
COLOUR
SIGPER
SIGGRP
HEIGHT
VALNMR
| 41  |     |                       |     |     | LIGHTS  | CATLIT    | P  12.8.6.1  |     |
| --- | --- | --------------------- | --- | --- | ------- | --------- | ------------ | --- |
|     |     | Sector light on       |     |     |         |           |              |     |
|     |     | standard charts, the  |     |     | X3      | SECTR1    | 12.8.6.3     |     |
|     |     | white sector limits   |     |     |         | SECTR2    |              |     |
marking the sides of
LITCHR
the fairway
COLOUR
SIGPER
SIGGRP
|     |     |     |     |     | FAIRWY  |     | A  10.4  |     |
| --- | --- | --- | --- | --- | ------- | --- | -------- | --- |
| 42  |     |     |     |     |         |     |          |     |
Main light visible all-     LIGHTS  CATLIT  10  P  12.8.6.1
|     |     |     |     |     | X2  | SECTR1  |     |     |
| --- | --- | --- | --- | --- | --- | ------- | --- | --- |
round with red
|     |     | subsidiary light seen  |     |     |     | SECTR2  |     |     |
| --- | --- | ---------------------- | --- | --- | --- | ------- | --- | --- |
over danger
LITCHR
COLOUR
SIGPER
SIGGRP
HEIGHT
VALNMR
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
43 All round light with LIGHTS LITVIS 7/8 P 12.8.6.2
obscured sector SECTR1
SECTR2
LITCHR
COLOUR
SIGPER
SIGGRP
HEIGHT
VALNMR
44 Light with arc of No
visibility deliberately Object is
restricted encoded
for this
sector
45 Light with faint sector LIGHTS LITVIS 3 P 12.8.6.2
SECTR1
SECTR2
LITCHR
COLOUR
SIGPER
SIGGRP
HEIGHT
46 Light with intensified LIGHTS V L A I L T N V M IS R 4 P 12.8.6.2
sector SECTR1
SECTR2
LITCHR
COLOUR
SIGPER
SIGGRP
Edition 1.0 November 2000 S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
Lights with limited Times of Exhibition
50    Lights exhibited only      LIGHTS  STATUS    P  12.8.5.3
|     |     | when needed (for  |     |     |         | 2/8  |     |     |
| --- | --- | ----------------- | --- | --- | ------- | ---- | --- | --- |
|     |     | fishing vessels,  |     |     | LITCHR  |      |     |     |
ferries) and some
COLOUR
private lights
SIGPER
SIGGRP
| 51  |     |                      |     |           |         |       |           |     |
| --- | --- | -------------------- | --- | --------- | ------- | ----- | --------- | --- |
|     |     | Daytime light        |     |   LIGHTS  | EXCLIT  | 2  P  |           |     |
|     |     | (charted only where  |     |           | LITCHR  |       | 12.8.5.4  |     |
|     |     | the character shown  |     |           | COLOUR  |       |           |     |
by day differs from
HEIGHT
that shown at night)
VALNMR
|     |     |                       |     | LIGHTS  | EXCLIT  | 4  P    |           |     |
| --- | --- | --------------------- | --- | ------- | ------- | ------- | --------- | --- |
| 52  |     |                       |     | LIGHTS  | EXCLIT  | 2/4  P  | 12.8.5.5  |     |
|     |     | Fog light (exhibited  |     |         |         |         |           |     |
|     |     | only in fog, or       |     |         | LITCHR  |         |           |     |
|     |     | character changes in  |     |         | COLOUR  |         |           |     |
fog
SIGPER
SIGGRP
HEIGHT
VALNMR
STATUS
|     |     |     |     | LIGHTS  | EXCLIT  | 3  P  |     |     |
| --- | --- | --- | --- | ------- | ------- | ----- | --- | --- |
|     |     |     |     |         | LITCHR  | 2     |     |     |
COLOUR
SIGPER
SIGGRP
| 53    |     |                   |     | LIGHTS  | STATUS  | 17  P  | 12.8.5.2  |     |
| ----- | --- | ----------------- | --- | ------- | ------- | ------ | --------- | --- |
|       |     | Unwatched         |     |         |         |        |           |     |
|       |     | (unmanned) light  |     |         |         |        |           |     |
with no standby or

emergency
#
arrangements
54  #                 (temp)  Temporary      LIGHTS  STATUS  7  P  12.8.5.2
55  #                (exting)  LIGHTS  STATUS  11  P  12.8.5.2
|     |     | Extinguished  |     |     |     |     |     |     |
| --- | --- | ------------- | --- | --- | --- | --- | --- | --- |
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

Special Lights        Flare Stack (at Sea)   IL          Flare Stack (at Land)   IE       Signal Stations   IT
| 60  |     |             |     | LIGHTS  | CATLIT  | 5  P  | 12.8.7  |     |
| --- | --- | ----------- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Aero light  |     |         |         |       |         |     |
LITCHR
COLOUR
SIGPER
SIGGRP
VALNMR
| 61.1  |     |                        |     | LIGHTS  | CATLIT  | 6  P  | 12.8.7  |     |
| ----- | --- | ---------------------- | --- | ------- | ------- | ----- | ------- | --- |
|       |     | Air obstruction light  |     |         |         |       |         |     |
|       |     | of high intensity      |     |         | LITVIS  | 1     |         |     |
LITCHR
COLOUR
HEIGHT
VALNMR
|     |     |     |     | LNDMRK  |         | 7  P  | 4.8.15  |     |
| --- | --- | --- | --- | ------- | ------- | ----- | ------- | --- |
|     |     |     |     |         | CATLMK  |       |         |     |
|     |     |     |     |         | CONVIS  |       |         |     |
|     |     |     |     |         | FUNCTN  | 31    |         |     |
HEIGHT
| 61.2  |     |                        |     | LIGHTS  | CATLIT  | 6  P  | 12.8.7  |     |
| ----- | --- | ---------------------- | --- | ------- | ------- | ----- | ------- | --- |
|       |     | Air obstruction light  |     |         |         |       |         |     |
|       |     | of low intensity       |     |         | LITVIS  | 2     |         |     |
|       |     |                        |     |         | COLOUR  | 3     |         |     |
|       |     |                        |     | LNDMRK  |         | 7  P  | 4.8.15  |     |
|       |     |                        |     |         | CATLMK  |       |         |     |

CONVIS
|     |             |                          |     |             | HEIGHT  |       |         |     |
| --- | ----------- | ------------------------ | --- | ----------- | ------- | ----- | ------- | --- |
| 62  | Fog Det Lt  |                          |     | LIGHTS      | CATLIT  | 7  P  | 12.8.7  |     |
|     |             | Fog detector light       |     |             |         |       |         |     |
| 63  |             |                          |     | Any object  | STATUS  | 12    |         |     |
|     |             | Floodlit, floodlighting  |     |             |         |       |         |     |
of a structure
| 64  |     |              |     | LIGHTS  | CATLIT  | 9  P  | 12.8.7  |     |
| --- | --- | ------------ | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Strip light  |     |         |         |       |         |     |
X3  LITCHR
COLOUR
SIGPER
SIGGRP
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
65
| Private light other  |     |   LIGHTS  | CATLIT  |   P  |           |     |
| -------------------- | --- | --------- | ------- | ---- | --------- | --- |
| than one exhibited   |     |           | STATUS  | 8    | 12.8.5.3  |     |
#
| occasionally  |     |     | LITCHR  |     |     |     |
| ------------- | --- | --- | ------- | --- | --- | --- |
COLOUR

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
IQ Buoys, Beacons

Buoys and Beacons        IALA Maritime Buoyage System, which includes Beacons   IQ 130

General
| 1   |     |                      |     |     |     |     |     |     |
| --- | --- | -------------------- | --- | --- | --- | --- | --- | --- |
|     |     | Position of buoy or  |     |     |     |     |     |     |
beacon

Colours of Buoys and Beacon Topmarks
| 2   |     |                       |     | TOPMAR  | TOPSHP  |      |   12.4.1   |     |
| --- | --- | --------------------- | --- | ------- | ------- | ---- | ---------- | --- |
|     |     | Single colour: green  |     |         |         |      |            |     |
|     |     | (G) and black (B)     |     |         | COLOUR  | 4/2  | 2.4        |     |
|     |     |                       |     | BCNXXX  |         |      |            |     |
BOYXXX
| 3   |     |                       |     |         |   COLOUR  |     |   12.4.1   |     |
| --- | --- | --------------------- | --- | ------- | --------- | --- | ---------- | --- |
|     |     | Single colour other   |     |         |           |     |            |     |
|     |     | than green and black  |     | TOPMAR  | TOPSHP    |     | 2.4        |     |
|     |     |                       |     | BCNXXX  |           |     |            |     |
BOYXXX
| 4   |     |                        |     | TOPMAR  | COLPAT    | 1   | P  12.4.1   |     |
| --- | --- | ---------------------- | --- | ------- | --------- | --- | ----------- | --- |
|     |     | Multiple colours in    |     |         |           |     |             |     |
|     |     | horizontal bands: the  |     |         |   TOPSHP  |     | 2.4         |     |
colour sequence is
  COLOUR
from top to bottom
|     |     |     |     | BCNXXX  | COLPAT  |     | P   |     |
| --- | --- | --- | --- | ------- | ------- | --- | --- | --- |
|     |     |     |     | BOYXXX  | COLOUR  |     |     |     |
| 5   |     |     |     |         |         |     |     |     |
Multiple colours in      TOPMAR  TOPSHP  3,1  P  12.4.1
|     |     | vertical or diagonal  |     |     |   COLPAT  | 2/3   | 2.4  |     |
| --- | --- | --------------------- | --- | --- | --------- | ----- | ---- | --- |
|     |     | stripes; the darker   |     |     | COLOUR    |       |      |     |
colour is given first.
|     |     |     |     | BCNXXX  | COLPAT  | 2    | P   |     |
| --- | --- | --- | --- | ------- | ------- | ---- | --- | --- |
|     |     |     |     | BOYXXX  | COLOUR  | 3,1  |     |     |

| 6   |     |                         |     |     |         |     |          |     |
| --- | --- | ----------------------- | --- | --- | ------- | --- | -------- | --- |
|     |     | Retroflecting material  |     |     | RETRFL  |     | P  12.7  |     |

Lighted Marks        Marks with Fog Signals   IR
7,8  Encode as appropriate as shown in section IP

Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
Topmarks and Radar Reflectors     For application of Topmarks within the IALA System   IQ 130    Radar reflector   IS
9    IALA System topmarks    TOPMAR  TOPSHP  13/14/ P  12.6

|     |     | (beacon topmarks  |     |     |     | 11/10/ |     |     |
| --- | --- | ----------------- | --- | --- | --- | ------ | --- | --- |
4/3/5/5
shown upright)
/17/1/7
| 10  |     |                          |     | BCNXXX  | CONRAD  | 3  P  | 12.12  |     |
| --- | --- | ------------------------ | --- | ------- | ------- | ----- | ------ | --- |
|     |     | Beacon with topmark,     |     |         |         |       |        |     |
|     |     | colour, radar reflector  |     |         | COLOUR  |       |        |     |
and designation
BCNSHP
OBJNAM
|     |     |     |     | TOPMAR  | TOPSHP  |   P  | 12.6  |     |
| --- | --- | --- | --- | ------- | ------- | ---- | ----- | --- |
COLOUR
| 11  |     |                          |     | BOYXXX  | CONRAD  | 3  P  | 12.12   |     |
| --- | --- | ------------------------ | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Buoy with topmark,       |     |         |         |       |         |     |
|     |     | colour, radar reflector  |     |         | COLOUR  |       | 12.4.1  |     |
and designation
BOYSHP
OBJNAM
|     |     |     |     | TOPMAR  | TOPSHP  |   P  | 12.6  |     |
| --- | --- | --- | --- | ------- | ------- | ---- | ----- | --- |
COLOUR

Buoys         Features Common to Buoys and Beacons   IQ 1-11

Shapes
| 20  |     | Conical buoy, nun  |     |   BOYXXX  | BOYSHP  | 1  P  | 12.4.1  |     |
| --- | --- | ------------------ | --- | --------- | ------- | ----- | ------- | --- |

|     |     | buoy, ogival buoy  |     |     | COLOUR  |     |     |     |
| --- | --- | ------------------ | --- | --- | ------- | --- | --- | --- |

| 21  |     |                        |     | BOYXXX  | BOYSHP  | 2  P  | 12.4.1  |     |
| --- | --- | ---------------------- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Can buoy, cylindrical  |     |         |         |       |         |     |
|     |     | buoy                   |     |         | COLOUR  |       |         |     |

| 22  |     | Spherical buoy  |     |   BOYXXX  | BOYSHP  | 3  P  | 12.4.1  |     |
| --- | --- | --------------- | --- | --------- | ------- | ----- | ------- | --- |

COLOUR

| 23  |     |              |     | BOYXXX  | BOYSHP  | 4  P  | 12.4.1  |     |
| --- | --- | ------------ | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Pillar buoy  |     |         |         |       |         |     |
COLOUR

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
| 24  |     |                     |     |           |         |       |         |     |
| --- | --- | ------------------- | --- | --------- | ------- | ----- | ------- | --- |
|     |     | Spar buoy, spindle  |     |   BOYXXX  | BOYSHP  | 5  P  | 12.4.1  |     |
|     |     | buoy                |     |           | COLOUR  |       |         |     |

| 25  |     |                        |     | BOYXXX  | BOYSHP  | 6  P  | 12.4.1  |     |
| --- | --- | ---------------------- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Barrel buoy, tun buoy  |     |         |         |       |         |     |
COLOUR

| 26  |     |            |     |           |         |       |         |     |
| --- | --- | ---------- | --- | --------- | ------- | ----- | ------- | --- |
|     |     | Superbuoy  |     |   BOYXXX  | BOYSHP  | 7  P  | 12.4.1  |     |
COLOUR

Minor Light Floats
| 30  |     |                         |     | LITFLT  | COLOUR  | 4  P  | 12.4.2  |     |
| --- | --- | ----------------------- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Light float (IALA)      |     |         |         |       |         |     |
|     |     |                         |     |         | OBJNAM  |       |         |     |
|     |     |                         |     |         | INFORM  | NB    |         |     |
|     |     |                         |     | TOPMAR  | TOPSHP  |   P   | 12.6    |     |
|     |     |                         |     |         | COLOUR  | 4     |         |     |
| 31  |     |                         |     | LITFLT  | COLOUR  |   P   | 12.4.2  |     |
|     |     | Light float (non IALA)  |     |         |         |       |         |     |
|     |     |                         |     |         | INFORM  | NB    |         |     |

IQ 30/31 INFORM must be used to indicate the IALA status. MARSYS is not a legitimate attribute of LITFLT at present and will not be included until the next NE of the
standard.

Mooring Buoys
| 40  |     |               |     | MORFAC  | CATMOR  | 7  P  | 9.2.4  |     |
| --- | --- | ------------- | --- | ------- | ------- | ----- | ------ | --- |
|     |     | Mooring buoy  |     |         |         |       |        |     |
BOYSHP
| 41  |     |                       |     | MORFAC  | CATMOR  | 7  P  | 9.2.4   |     |
| --- | --- | --------------------- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Lighted mooring buoy  |     |         |         |       |         |     |
|     |     |                       |     |         | BOYSHP  | 6     |         |     |
|     |     |                       |     | LIGHTS  | CATLIT  |   P   | 12.8.1  |     |
|     |     |                       |     |         | LITCHR  |       |         |     |
COLOUR

|     |     |     |     |     | SIGPER  |     |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |
|     |     |     |     |     | SIGGRP  |     |     |     |

Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs

42    Trot, mooring buoy      MORFAC  CATMOR  7  P  9.2.5
|     |     | with ground tackle and  |     |     | X6  BOYSHP  | 6   |     |     |
| --- | --- | ----------------------- | --- | --- | ----------- | --- | --- | --- |
berth numbers
|     |     |     |     |     | MORFAC  CATMOR  | 6  L  | 4.6.7.1  |     |
| --- | --- | --- | --- | --- | --------------- | ----- | -------- | --- |

#
|     |     |     |     |     | OBSTRN  CATOBS  | 9  P  | 6.2.2  |     |
| --- | --- | --- | --- | --- | --------------- | ----- | ------ | --- |
WATLEV
VALSOU
|     |     |     |     |     | CBLSUB  CATCBL  | 6  L  | 11.5.1  |     |
| --- | --- | --- | --- | --- | --------------- | ----- | ------- | --- |
|     |     |     |     |     | X6              |       | 9.2.5   |     |
|     |     |     |     |     | BERTHS  OBJNAM  |   P   | 4.6.2   |     |
|     |     |     |     |     | X2              |       | 9.2.5   |     |
43    Mooring buoy with      MORFAC  CATMOR  7  P  9.2.4
|     |     | communications  |     |     | CBLSUB  CATCBL  |   L  | 11.5.1  |     |
| --- | --- | --------------- | --- | --- | --------------- | ---- | ------- | --- |

| 44  |     | Numerous moorings  |     |     | ACHARE  CATACH  | 8  A  | 9.2.1  |     |
| --- | --- | ------------------ | --- | --- | --------------- | ----- | ------ | --- |

Special Purpose Buoys
| 50  |     |                     |     |     | BOYSPP  CATSPM  | 1  P  | 12.4.1  |     |
| --- | --- | ------------------- | --- | --- | --------------- | ----- | ------- | --- |
|     |     | Firing danger area  |     |     |                 |       |         |     |
|     |     | (Danger Zone) buoy  |     |     | BOYSHP          | 3     |         |     |
COLOUR  6
|     |     |     |     |     | TOPMAR  TOPSHP  | 7  P  | 12.6  |     |
| --- | --- | --- | --- | --- | --------------- | ----- | ----- | --- |
COLOUR  6
| 51  |     |         |     |     | BOYSPP  CATSPM  | 2  P  | 12.4.1  |     |
| --- | --- | ------- | --- | --- | --------------- | ----- | ------- | --- |
|     |     | Target  |     |     |                 |       |         |     |
BOYSHP  3
COLOUR  6
|     |     |     |     |     | TOPMAR  TOPSHP  | 7  P  | 12.6  |     |
| --- | --- | --- | --- | --- | --------------- | ----- | ----- | --- |
COLOUR  6
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
| 52  |     |              |     |           |         |       |         |     |
| --- | --- | ------------ | --- | --------- | ------- | ----- | ------- | --- |
|     |     | Marker Ship  |     |   BOYSPP  | CATSPM  | 3  P  | 12.4.1  |     |
|     |     |              |     |           | BOYSHP  | 3     |         |     |
|     |     |              |     |           | COLOUR  | 6     |         |     |
|     |     |              |     | TOPMAR    | TOPSHP  | 7  P  | 12.6    |     |
|     |     |              |     |           | COLOUR  | 6     |         |     |
| 53  |     |              |     |           |         |       |         |     |
|     |     | Barge        |     |   BOYSPP  | CATSPM  | 5  P  | 12.4.1  |     |
|     |     |              |     |           | BOYSHP  | 3     |         |     |
|     |     |              |     |           | COLOUR  | 6     |         |     |
|     |     |              |     | TOPMAR    | TOPSHP  | 7  P  | 12.6    |     |

|     |     |                   |     |           | COLOUR  | 6     |         |     |
| --- | --- | ----------------- | --- | --------- | ------- | ----- | ------- | --- |
| 54  |     | Degaussing Range  |     |   BOYSPP  | CATSPM  | 4  P  | 12.4.1  |     |

|     |     | buoy  |     |         | BOYSHP  | 3     |       |     |
| --- | --- | ----- | --- | ------- | ------- | ----- | ----- | --- |
|     |     |       |     |         | COLOUR  | 6     |       |     |
|     |     |       |     | TOPMAR  | TOPSHP  | 7  P  | 12.6  |     |

|     |     |                       |     |           | COLOUR  | 6     |         |     |
| --- | --- | --------------------- | --- | --------- | ------- | ----- | ------- | --- |
| 55  |     |                       |     | BOYSPP    | CATSPM  | 6  P  | 12.4.1  |     |
|     |     | Cable buoy            |     |           |         |       |         |     |
|     |     |                       |     |           | BOYSHP  | 3     |         |     |
|     |     |                       |     |           | COLOUR  | 6     |         |     |
|     |     |                       |     | TOPMAR    | TOPSHP  | 7  P  | 12.6    |     |
|     |     |                       |     |           | COLOUR  | 6     |         |     |
| 56  |     |                       |     | BOYSPP    | CATSPM  | 7  P  | 12.4.1  |     |
|     |     | Spoil ground buoy     |     |           |         |       |         |     |
|     |     |                       |     |           | BOYSHP  | 3     |         |     |
|     |     |                       |     |           | COLOUR  | 6     |         |     |
|     |     |                       |     | TOPMAR    | TOPSHP  | 7  P  | 12.6    |     |
|     |     |                       |     |           | COLOUR  | 6     |         |     |
| 57  |     |                       |     |           |         |       |         |     |
|     |     | Buoy marking outfall  |     |   BOYSPP  | CATSPM  | 8  P  | 12.4.1  |     |
|     |     |                       |     |           | BOYSHP  | 3     |         |     |
|     |     |                       |     |           | COLOUR  | 6     |         |     |
|     |     |                       |     | TOPMAR    | TOPSHP  | 7  P  | 12.6    |     |
|     |     |                       |     |           | COLOUR  | 6     |         |     |

58  Data collection buoy      BOYSPP  CATSPM  9  P  12.4.1
|     |     |     |     |     | BOYSHP  | 7   |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |
COLOUR
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 59  |     |                      |     |           |          |        |         |     |
| --- | --- | -------------------- | --- | --------- | -------- | ------ | ------- | --- |
|     |     | Buoy marking wave    |     |   BOYSPP  | CA TSPM  | 10  P  | 12.4.1  |     |
|     |     | recorder or current  |     |           | BOYSHP   | 3      |         |     |
|     |     | meter                |     |           | COLOUR   | 6      |         |     |
|     |     |                      |     | TOPMAR    | TOPSHP   | 7  P   | 12.6    |     |
|     |     |                      |     |           | COLOUR   | 6      |         |     |
| 60  |     |                      |     |           |          |        |         |     |
|     |     | Seaplane anchorage   |     |   BOYSPP  | CATSPM   | 11  P  | 12.4.1  |     |
|     |     | buoy                 |     |           | BOYSHP   |        |         |     |
COLOUR
| 61  |     |                       |     | BOYSPP  | CATSPM  | 19  P  | 12.4.1  |     |
| --- | --- | --------------------- | --- | ------- | ------- | ------ | ------- | --- |
|     |     | Buoy marking traffic  |     |         |         |        |         |     |
|     |     | separation scheme     |     |         | BOYSHP  |        |         |     |
COLOUR

| 62  |     | Buoy marking     |     |   BOYSPP  | CATSPM  | 12  P  | 12.4.1  |     |
| --- | --- | ---------------- | --- | --------- | ------- | ------ | ------- | --- |
|     |     | recreation zone  |     |           | BOYSHP  | 3      |         |     |
|     |     |                  |     |           | COLOUR  | 6      |         |     |
|     |     |                  |     | TOPMAR    | TOPSHP  | 7  P   | 12.6    |     |

|     |     |     |     |     | COLOUR  | 6   |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |

Seasonal  Buoys
| 70  |     | Buoy privately  |     |   BOYXXX  | STATUS  | 8  P  | 12.4.1  |     |
| --- | --- | --------------- | --- | --------- | ------- | ----- | ------- | --- |

|     |     | maintained  |     |         | BOYSHP  | 3     |       |     |
| --- | --- | ----------- | --- | ------- | ------- | ----- | ----- | --- |
|     |     |             |     |         | COLOUR  | 6     |       |     |
|     |     |             |     | TOPMAR  | TOPSHP  | 7  P  | 12.6  |     |

|     |     |                |     |         | COLOUR  | 6        |         |     |
| --- | --- | -------------- | --- | ------- | ------- | -------- | ------- | --- |
| 71  |     |                |     | BOYXXX  | PERSTA  | --04  P  | 12.4.1  |     |
|     |     | Seasonal buoy  |     |         |         |          |         |     |
|     |     |                |     |         | PEREND  | --10     |         |     |
|     |     |                |     |         | STATUS  | 5        |         |     |
|     |     |                |     |         | BOYSHP  | 3        |         |     |
|     |     |                |     |         | COLOUR  | 6        |         |     |
|     |     |                |     | TOPMAR  | TOPSHP  | 7  P     | 12.6    |     |

|     |     |     |     |     | COLOUR  | 6   |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
Beacons         Lighted Beacons   IP       Features Common to Beacons and Buoys   IQ 1-11
General
| 80  |     |     |                     |     | BCNXXX  | BCNSHP  |   P  | 12.3.1  |     |
| --- | --- | --- | ------------------- | --- | ------- | ------- | ---- | ------- | --- |
|     |     |     | Beacon in general,  |     |         |         |      |         |     |
|     |     |     | characteristics     |     |         | COLOUR  |      |         |     |
unknown or chart

scale too small to
show
| 81  |     |                         |                      |     | BCNXXX  | BCNSHP  |   P  | 12.3.1  |     |
| --- | --- | ----------------------- | -------------------- | --- | ------- | ------- | ---- | ------- | --- |
|     |     | Beacon with colour, no  |                      |     |         |         |      |         |     |
|     |     |                         | distinctive topmark  |     |         | COLOUR  |      |         |     |

| 82  |     |     |                     |     | BCNXXX  | BCNSHP  |   P  | 12.3.1  |     |
| --- | --- | --- | ------------------- | --- | ------- | ------- | ---- | ------- | --- |
|     |     |     | Beacon with colour  |     |         |         |      |         |     |
|     |     |     | and  topmark        |     |         | COLOUR  |      |         |     |

|     |     |     |     |     | TOPMAR  | TOPSHP  |   P  | 12.6  |     |
| --- | --- | --- | --- | --- | ------- | ------- | ---- | ----- | --- |
COLOUR
| 83  |     |                      |     |     |           |         |      |         |     |
| --- | --- | -------------------- | --- | --- | --------- | ------- | ---- | ------- | --- |
|     |     | Beacon on submerged  |     |     |   BCNISD  | BCNSHP  |   P  | 12.3.1  |     |
#
|     |     |     | rock  |     |     | COLOUR  |     |     |     |
| --- | --- | --- | ----- | --- | --- | ------- | --- | --- | --- |

Minor Impermanent Marks         Minor Pile   IF
| 90  |     |     |              |     |           |         |       |         |     |
| --- | --- | --- | ------------ | --- | --------- | ------- | ----- | ------- | --- |
|     |     |     | Stake, pole  |     |   BCNXXX  | BCNSHP  | 1  P  | 12.3.1  |     |
COLOUR
| 91  | PORT   STARBOARD  |     |              |     | BCNLAT  | BCNSHP  | 1    | 12.3.1  |     |
| --- | ----------------- | --- | ------------ | --- | ------- | ------- | ---- | ------- | --- |
|     |                   |     | Perch stake  |     |         |         |      |         |     |
|     |                   |     |              |     |         | CATLAM  | 1/2  |         |     |
COLOUR
| 92  |     |     | Withy  |     |   BCNLAT  | BCNSHP  | 2    | 12.3.1  |     |
| --- | --- | --- | ------ | --- | --------- | ------- | ---- | ------- | --- |
|     |     |     |        |     |           | CATLAM  | 1/2  |         |     |
COLOUR

Minor Marks, usually on Land      Landmarks   IE
| 100  |     |     |        |     | BCNSPP  | BCNSHP  | 6  P  |         |     |
| ---- | --- | --- | ------ | --- | ------- | ------- | ----- | ------- | --- |
|      |     |     | Cairn  |     |         |         |       |         |     |
|      |     |     |        |     |         | CATSPM  |       | 12.3.1  |     |
COLOUR
|     |     |     |     |     | LNDMRK  | CATLMK  | 1  P  | 4.8.15  |     |
| --- | --- | --- | --- | --- | ------- | ------- | ----- | ------- | --- |
|     |     |     |     |     |         | CONVIS  |       |         |     |
                                                                      S-57 Appendix B.1 - Annex D
Edition 1.0                                                                                                           November 2000

INT1 to S-57/52 for ENCs
| 101  |     |                    |     |           |         |      |         |     |
| ---- | --- | ------------------ | --- | --------- | ------- | ---- | ------- | --- |
|      |     | Coloured or white  |     |   DAYMAR  | TOPSHP  |   P  | 12.3.1  |     |
|      |     | mark               |     |           | NATCON  | 9    | 12.3.3  |     |
COLOUR
| 102.1    |     |                        |     | DAYMAR  | TOPSHP  | 24  P  | 12.3.1  |     |
| -------- | --- | ---------------------- | --- | ------- | ------- | ------ | ------- | --- |
|          |     | Coloured topmark with  |     |         |         |        |         |     |
|          |     | function of beacon     |     |         | NATCON  | 9      | 12.3.3  |     |
#
COLOUR
| 102.2  |     |                      |     |           |         |       |         |     |
| ------ | --- | -------------------- | --- | --------- | ------- | ----- | ------- | --- |
|        |     | Painted board with   |     |   DAYMAR  | TOPSHP  | 6  P  |         |     |
|        |     | function of leading  |     |           | CATSPM  | 16    | 12.3.3  |     |
|        |     | beacon               |     |           | NATCON  | 9     |         |     |
COLOUR

Beacon Towers
| 110    |     | Beacon tower  |     |   BCNXXX  | BCNSHP  | 3  P  | 12.3.1  |     |
| ------ | --- | ------------- | --- | --------- | ------- | ----- | ------- | --- |
|        |     |               |     |           | COLOUR  |       |         |     |

| 111  #  |     |                 |     | BCNXXX  | BCNSHP  | 4  P  | 12.3.1  |     |
| ------- | --- | --------------- | --- | ------- | ------- | ----- | ------- | --- |
|         |     | Lattice beacon  |     |         |         |       |         |     |
COLOUR

Special Purpose Beacons       Leading Lines, Clearing Lines   IM
| 120  |     |                  |     | BCNSPP  | BCNSHP  |   P  | 12.3.1  |     |
| ---- | --- | ---------------- | --- | ------- | ------- | ---- | ------- | --- |
|      |     | Leading beacons  |     |         |         |      |         |     |
|      |     |                  |     |         | CATSPM  | 16   |         |     |
COLOUR
| 121  |     |                           |     | BCNSPP  | BCNSHP  |   P  | 12.3.1  |     |
| ---- | --- | ------------------------- | --- | ------- | ------- | ---- | ------- | --- |
|      |     | Beacon marking a          |     |         |         |      |         |     |
|      |     | clearing line or transit  |     |         | CATSPM  | 41   |         |     |
COLOUR
| 122  |     |                    |     | BCNSPP  | BCNSHP  |   P  | 12.3.1  |     |
| ---- | --- | ------------------ | --- | ------- | ------- | ---- | ------- | --- |
|      |     | Beacon marking     |     |         |         |      |         |     |
|      |     | measured distance  |     |         | CATSPM  | 17   | 10.1.3  |     |
COLOUR
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
| 123  |     |     |                       |     |           |         |      |         |     |
| ---- | --- | --- | --------------------- | --- | --------- | ------- | ---- | ------- | --- |
|      |     |     | Cable landing beacon  |     |   BCNSPP  | BCNSHP  |   P  | 12.3.1  |     |
|      |     |     |                       |     |           | CATSPM  | 6    |         |     |
COLOUR
| 124  |     |     | Refuge beacon  |     | BCNSPP  | BCNSHP  |   P  | 12.3.1  |     |
| ---- | --- | --- | -------------- | --- | ------- | ------- | ---- | ------- | --- |
|      |     |     |                |     |         |         |      |         |     |
|      |     |     |                |     |         | CATSPM  | 44   |         |     |
COLOUR
| 125  |     |     |                       |     |           |         |      |         |     |
| ---- | --- | --- | --------------------- | --- | --------- | ------- | ---- | ------- | --- |
|      |     |     | Firing practice area  |     |   BCNSPP  | BCNSHP  |   P  | 12.3.1  |     |
|      |     |     | beacons               |     |           | CATSPM  | 1    |         |     |
COLOUR
| 126  |     |     |               |     | BCNSPP  | BCNSHP  |   P  | 12.3.1  |     |
| ---- | --- | --- | ------------- | --- | ------- | ------- | ---- | ------- | --- |
|      |     |     | Notice board  |     |         |         |      |         |     |
|      |     |     |               |     |         | CATSPM  | 18   |         |     |
COLOUR

130        IALA Maritime Buoyage Systems

Lateral marks
|        |     |     |                      |     |          |         |         |       |     |
| ------ | --- | --- | -------------------- | --- | -------- | ------- | ------- | ----- | --- |
| 130.1  |     |     |                      |     | BOYXXX/  | MARSYS  | 1/2  A  | 12.2  |     |
|        |     |     | IALA buoyage system  |     |          |         |         |       |     |
BCNXXX
M_NSYS
130.2    Symbol showing                           M_NSYS  ORIENT    A  12.2
|     |     |     | D i r e c tion of buoyage  |     |     |     |     |     |     |
| --- | --- | --- | -------------------------- | --- | --- | --- | --- | --- | --- |
direction of buoyage
where not obvious
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs

130.3      Cardinal Marks
| UNLIT MARKS  |     |             | LIGHTED MARKS  |     |     |     |      |         |     |
| ------------ | --- | ----------- | -------------- | --- | --- | --- | ---- | ------- | --- |
|              |     |             |                |     |     |     |      |         |     |
|              |     | North Mark  |                |     |     |     |   P  | 12.4.1  |     |

|     |     |     |     |     | XXXCAR  | XXXSHP  |         |       |     |
| --- | --- | --- | --- | --- | ------- | ------- | ------- | ----- | --- |
|     |     |     |     |     |         | CATCAM  | 1       |       |     |
|     |     |     |     |     |         | COLOUR  | 2,6     |       |     |
|     |     |     |     |     |         | COLPAT  | 1       |       |     |
|     |     |     |     |     | TOPMAR  | TOPSHP  | 13  P   | 12.6  |     |
|     |     |     |     |     |         | COLOUR  | 2       |       |     |
|     |     |     |     |     | LIGHTS  | LITCHR  | 4/5  P  | 12.8  |     |
|     |     |     |     |     |         | COLOUR  | 1       |       |     |
|     |     |     |     |     |         | SIGPER  |         |       |     |
|     |     |     |     |     |         | SIGGRP  | (1)     |       |     |
S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

|     |     | East Mark   |     |           |   XXXSHP  |               | P  12.4.1  |     |
| --- | --- | ----------- | --- | --------- | --------- | ------------- | ---------- | --- |
|     |     |             |     | XXXCAR    |           |               |            |     |
|     |     |             |     |           | CATCAM    | 2             |            |     |
|     |     |             |     |           | COLOUR    | 2,6,2         |            |     |
|     |     |             |     |           | COLPAT    | 1             |            |     |
|     |     |             |     | TOPMAR    | TOPSHP    | 11            | P  12.6    |     |
|     |     |             |     |           | COLOUR    | 2             |            |     |
|     |     |             |     |           | LIGHTS    | LITCHR  4/5   | P  12.8    |     |
|     |     |             |     |           |   COLOUR  | 1             |            |     |
|     |     |             |     |           |           | SIGPER  5/10  |            |     |
|     |     |             |     |           |           | SIGGRP  (3)   |            |     |
|     |     | South Mark  |     |   XXXCAR  | XXXSHP    |               | P  12.4.1  |     |

|     |     |            |     |         |           |                 |            |     |
| --- | --- | ---------- | --- | ------- | --------- | --------------- | ---------- | --- |
|     |     |            |     |         | CATCAM    | 3               |            |     |
|     |     |            |     |         | COLOUR    | 6,2             |            |     |
|     |     |            |     |         | COLPAT    | 1               |            |     |
|     |     |            |     | TOPMAR  | TOPSHP    | 14              | P  12.6    |     |
|     |     |            |     |         | COLOUR    | 2               |            |     |
|     |     |            |     |         | LIGHTS    | LITCHR  26/25   | P  12.8    |     |
|     |     |            |     |         |   COLOUR  | 1               |            |     |
|     |     |            |     |         |           | SIGPER  10/15   |            |     |
|     |     |            |     |         |           | SIGGRP  (6)(1)  |            |     |
|     |     |            |     |         |           |                 | P  12.4.1  |     |
|     |     | West Mark  |     |         |           |                 |            |     |
|     |     |            |     | XXXCAR  | XXXSHP    |                 |            |     |
|     |     |            |     |         | CATCAM    | 4               |            |     |
|     |     |            |     |         | COLOUR    | 6,2,6           |            |     |
|     |     |            |     |         | COLPAT    | 1               |            |     |
|     |     |            |     | TOPMAR  | TOPSHP    | 10              | P  12.6    |     |
|     |     |            |     |         | COLOUR    | 2               |            |     |
|     |     |            |     |         | LIGHTS    | LITCHR  5/4     | P  12.8    |     |
|     |     |            |     |         |   COLOUR  | 1               |            |     |
SIGPER
10/15
|     |     |     |     |     |     | SIGGRP  (9)  |     |     |
| --- | --- | --- | --- | --- | --- | ------------ | --- | --- |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
130.4
| Isolated Danger Marks  |     |     |         |         | 4/5    | P  12.4.1  |     |
| ---------------------- | --- | --- | ------- | ------- | ------ | ---------- | --- |
|                        |     |     | XXXISD  | XXXSHP  | 2,3.2  |            |     |
|                        |     |     |         | COLOUR  | 1      |            |     |
COLPAT
|     |     |     | TOPMAR  | TOPSHP  | 4   | P  12.6  |     |
| --- | --- | --- | ------- | ------- | --- | -------- | --- |
|     |     |     |         | COLOUR  | 2   |          |     |
130.5
| Safe Water Marks  |     |     |         |         | 3/4/5  | P  12.4.1  |     |
| ----------------- | --- | --- | ------- | ------- | ------ | ---------- | --- |
|                   |     |     | XXXSAW  | XXXSHP  | 3,1    |            |     |
|                   |     |     |         | COLOUR  | 2      |            |     |
COLPAT
|     |     |     | TOPMAR  | TOPSHP  | 3   | P  12.6  |     |
| --- | --- | --- | ------- | ------- | --- | -------- | --- |
|     |     |     |         | COLOUR  | 3   |          |     |
130.6
| Special Marks  |     |     |         |         |     |          |     |
| -------------- | --- | --- | ------- | ------- | --- | -------- | --- |
|                |     |     | XXXSPP  | XXXSHP  |     |          |     |
|                |     |     |         | CATSPM  |     |          |     |
|                |     |     |         | COLOUR  | 6   |          |     |
|                |     |     | TOPMAR  | TOPSHP  |     | P  12.6  |     |
COLOUR

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

IR Fog Signals

General       Fog Detector Light    IP        Fog Light   IP
| 1   |     |                          |     |           |         |      |       |     |
| --- | --- | ------------------------ | --- | --------- | ------- | ---- | ----- | --- |
|     |     | Position of fog signal:  |     |   FOGSIG  | CATFOG  |   P  | 12.5  |     |

Type of fog signal not
given

Types of Fog Signals, with Abbreviations
| 10  | Explos  | Explosive  |     |   FOGSIG  | CATFOG  | 1  P  | 12.5  |     |
| --- | ------- | ---------- | --- | --------- | ------- | ----- | ----- | --- |

| 11  | Dia    |             |     | FOGSIG    | CATFOG  | 2  P   | 12.5  |     |
| --- | ------ | ----------- | --- | --------- | ------- | ------ | ----- | --- |
|     |        | Diaphone    |     |           |         |        |       |     |
| 12  | Siren  |             |     | FOGSIG    | CATFOG  | 3  P   | 12.5  |     |
|     |        | Siren       |     |           |         |        |       |     |
| 13  |        |             |     |           |         |        |       |     |
|     |        | Horn        |     |   FOGSIG  | CATFOG  | 10  P  | 12.5  |     |
|     |        |             |     | FOGSIG    | CATFOG  | 4  P   | 12.5  |     |
|     |        | Nautophone  |     |           |         |        |       |     |
Horn
|     |       | Reed     |     |   FOGSIG  | CATFOG  | 5  P  | 12.5  |     |
| --- | ----- | -------- | --- | --------- | ------- | ----- | ----- | --- |
|     |       |          |     | FOGSIG    | CATFOG  | 6  P  | 12.5  |     |
|     |       | tyfon    |     |           |         |       |       |     |
| 14  | Bell  |          |     | FOGSIG    | CATFOG  | 7  P  | 12.5  |     |
|     |       | Bell     |     |           |         |       |       |     |
| 15  | Whis  |          |     |           |         |       |       |     |
|     |       | Whistle  |     |   FOGSIG  | CATFOG  | 8  P  | 12.5  |     |
| 16  | Gong  |          |     | FOGSIG    | CATFOG  | 9  P  | 12.5  |     |
|     |       | Gong     |     |           |         |       |       |     |

Examples of Fog Signal Descriptions
| 20  |     | Siren  |     |   FOGSIG  | CATFOG  | 3  P  | 12.5  |     |
| --- | --- | ------ | --- | --------- | ------- | ----- | ----- | --- |

|     |     |     |     |     | SIGPER  |     |     |     |
| --- | --- | --- | --- | --- | ------- | --- | --- | --- |
SIGGRP
SIGGEN
|     |     |     |     | LIGHTS  | LITCHR  |   P  | 12.8  |     |
| --- | --- | --- | --- | ------- | ------- | ---- | ----- | --- |
COLOUR
SIGPER
SIGGRP
HEIGHT
VALNMR
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 21  |     |                     |     |     |         |         |        |       |     |
| --- | --- | ------------------- | --- | --- | ------- | ------- | ------ | ----- | --- |
|     |     | Wave actuated bell  |     |     | FOGSIG  | CATFOG  | 7  P   | 12.5  |     |
|     |     | buoy                |     |     |         | SIGGEN  | 2      |       |     |
| 22  |     |                     |     |     | FOGSIG  | CATFOG  | 10  P  | 12.5  |     |
|     |     | Horn with whistle   |     |     |         |         |        |       |     |
|     |     |                     |     |     |         | SIGPER  |        |       |     |
|     |     |                     |     |     |         | SIGGRP  | 1      |       |     |
SIGGEN
|     |     |     |     |     | FOGSIG  | CATFOG  | 8  P  | 12.5  |     |
| --- | --- | --- | --- | --- | ------- | ------- | ----- | ----- | --- |
|     |     |     |     |     |         | SIGGEN  | 2     |       |     |
|     |     |     |     |     | LIGHTS  | LITCHR  |   P   | 12.8  |     |
|     |     |     |     |     |         |         |       |       |     |
COLOUR
SIGPER
SIGGRP

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs

IS Radar, Radio, Electronic Position-Fixing Systems

Radar       Radar Structures Forming Landmarks    IE       Radar Surveillance Systems   IM
| 1    |     |                      |     |           |         |         |          |     |
| ---- | --- | -------------------- | --- | --------- | ------- | ------- | -------- | --- |
|      |     | Coast radar station  |     |   RADSTA  | CATRAS  | 2  P    | 12.11.3  |     |
| 2    |     |                      |     | RTPBCN    | CATRTB  | 1  P    | 12.10    |     |
|      |     | Ramark               |     |           |         |         |          |     |
| 3.1  |     |                      |     | RTPBCN    | CATRTB  | P       | 12.10    |     |
|      |     | Radar transponder    |     |           |         | 2       |          |     |
|      |     | beacon (3cm)         |     |           | RADWAL  | 0.03-X  |          |     |
|      |     |                      |     |           | SIGGRP  |         |          |     |
3.2    Radar transponder      RTPBCN  CATRTB  2  P  12.10
|     |     | beacon (10cm)  |     |     | RADWAL  | 0.10-S  |     |     |
| --- | --- | -------------- | --- | --- | ------- | ------- | --- | --- |
SIGGRP

| 3.3  |     |                    |     | RTPBCN  | CATRTB  | P     | 12.10  |     |
| ---- | --- | ------------------ | --- | ------- | ------- | ----- | ------ | --- |
|      |     | Radar transponder  |     |         |         | 2     |        |     |
|      |     | beacon (3 & 10cm)  |     |         | RADWAL  | 0.03- |        |     |
X,

0.10-S
SIGGRP

| 3.4  |     |                        |     | RTPBCN  | CATRTB  | 1  P  | 12.10  |     |
| ---- | --- | ---------------------- | --- | ------- | ------- | ----- | ------ | --- |
|      |     | Radar transponder      |     |         |         |       |        |     |
|      |     | beacon with sector of  |     |         | SIGGRP  |       |        |     |
obscured reception
SECTR1
SECTR2
|     |     |                        |     | RTPBCN  | CATRTB  | 2  P  | 12.10  |     |
| --- | --- | ---------------------- | --- | ------- | ------- | ----- | ------ | --- |
|     |     | Radar transponder      |     |         |         |       |        |     |
|     |     | beacon with sector of  |     |         | SIGGRP  |       |        |     |
|     |     | reception              |     |         | SECTR1  |       |        |     |
SECTR2
| 3.5  |     |                |     |           |         |       |        |     |
| ---- | --- | -------------- | --- | --------- | ------- | ----- | ------ | --- |
|      |     | Leading radar  |     |   RTPBCN  | CATRTB  | 3  P  | 12.10  |     |
transponder beacons
| 3.6  |     |                      |     |           |         |       |        |     |
| ---- | --- | -------------------- | --- | --------- | ------- | ----- | ------ | --- |
|      |     | Floating marks with  |     |   RTPBCN  | CATRTB  | 2  P  | 12.10  |     |
radar transponder
beacon
| 4   |     | Radar reflector  |     |     |   CONRAD  | 3  P  | 12.12  |     |
| --- | --- | ---------------- | --- | --- | --------- | ----- | ------ | --- |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 5   |     |                    |     |     |           |       |        |     |
| --- | --- | ------------------ | --- | --- | --------- | ----- | ------ | --- |
|     |     | Radar conspicuous  |     |     |   CONRAD  | 1  P  | 12.12  |     |
feature

Radio       Radio Structures Forming Landmarks    IE       Radio Reporting (Calling-in or Way) Points   IM
| 10  |     |                         |     | RDOSTA  | CATROS  | 1  P  | 12.9.1  |     |
| --- | --- | ----------------------- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Non-directional radio   |     |         |         |       |         |     |
|     |     | beacon                  |     |         | OBJNAM  |       |         |     |
| 11  |     |                         |     | RDOSTA  | CATROS  | 2  P  | 12.9.1  |     |
|     |     | Directional radio       |     |         |         |       |         |     |
|     |     | beacon                  |     |         | ORIENT  |       |         |     |
| 12  |     |                         |     | RDOSTA  | CATROS  | 3  P  | 12.9.1  |     |
|     |     | Rotating pattern radio  |     |         |         |       |         |     |
beacon
| 13  |     |                |     | RDOSTA  | CATROS  | 4  P  | 12.9.1  |     |
| --- | --- | -------------- | --- | ------- | ------- | ----- | ------- | --- |
|     |     | Consol beacon  |     |         |         |       |         |     |
| 14  |     |                |     |         |         |       |         |     |
Radio direction-finding      RDOSTA  CATROS  5  P  12.9.3
CALSGN
station
| 15  |     |                      |     |           |         |       |         |     |
| --- | --- | -------------------- | --- | --------- | ------- | ----- | ------- | --- |
|     |     | Coast radio station  |     |   RDOSTA  | CATROS  | 6  P  | 12.9.4  |     |
| 16  |     |                      |     | RDOSTA    | CATROS  | 7  P  | 12.9.2  |     |
|     |     | Aeronautical         |     |           |         |       |         |     |
|     |     | radiobeacon          |     |           | CALSGN  |       |         |     |

Electronic Position-Fixing Systems
  Not applicable for ENC

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
IT Services

Pilotage
| 1.1  |     |                       |     |     | PILBOP  | CATPIL    | P/A  13.1.2  |     |
| ---- | --- | --------------------- | --- | --- | ------- | --------- | ------------ | --- |
|      |     | Pilot boarding place  |     |     |         |           |              |     |
1.2    Pilot boarding place,    PILBOP  CATPIL  1  P/A  13.1.2

|     |     | with name  |     |     |     |     |     |     |
| --- | --- | ---------- | --- | --- | --- | --- | --- | --- |
OBJNAM
| 1.3  |     |                        |     |     | PILBOP  | CATPIL    | P/A  13.1.2  |     |
| ---- | --- | ---------------------- | --- | --- | ------- | --------- | ------------ | --- |
|      |     | Pilot boarding place,  |     |     |         |           |              |     |
|      |     | with note              |     |     |         | INFORM    |              |     |
TXTDSC
1.4    Pilot transferred by    PILBOP  CATPIL  2  P/A  13.1.2

helicopter
| 2   |     | Pilot look-out           |     |   LNDMRK  | FUNCTN          | 12              | P  13.1.1  |     |
| --- | --- | ------------------------ | --- | --------- | --------------- | --------------- | ---------- | --- |
|     |     |                          |     |           | BUISGL  FUNCTN  | 12              | P          |     |
| 3   |     |                          |     | LNDMRK    | FUNCTN          | 11              | P  13.1.1  |     |
|     |     | Pilot office             |     |           |                 |                 |            |     |
|     |     |                          |     |           | BUISGL  FUNCTN  | 11              | P          |     |
| 4   |     |                          |     |           |                 | INFORM  Pilots  |            |     |
|     |     | Port with pilot service  |     |           |                 |                 |            |     |

Coastguard, Rescue
| 10  |     |                     |     | CGUSTA  |     |     | P  13.2  |     |
| --- | --- | ------------------- | --- | ------- | --- | --- | -------- | --- |
|     |     | Coastguard station  |     |         |     |     |          |     |
| 11  |     |                     |     | RSCSTA  |     |     | P  13.3  |     |
|     |     | Coastguard station  |     |         |     |     |          |     |
with Rescue station
|     |     |     |     | CGUSTA  |     |     | P  13.2  |     |
| --- | --- | --- | --- | ------- | --- | --- | -------- | --- |

| 12  |     |                  |     |           |         |     |          |     |
| --- | --- | ---------------- | --- | --------- | ------- | --- | -------- | --- |
|     |     | Rescue station;  |     |   RSCSTA  | CATRSC  |     | P  13.3  |     |
1
Lifeboat station;
|     |     | Rocket station       |     |         |         | 2   |          |     |
| --- | --- | -------------------- | --- | ------- | ------- | --- | -------- | --- |
| 13  |     |                      |     | RSCSTA  | CATRSC  | 6   | P  13.3  |     |
|     |     | Lifeboat lying at a  |     |         |         |     |          |     |
mooring
| 14  |     |             |     | RSCSTA  | CATRSC  | 4   | P  13.3  |     |
| --- | --- | ----------- | --- | ------- | ------- | --- | -------- | --- |
|     |     | Refuge for  |     |         |         |     |          |     |
shipwrecked mariners

Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs

Stations
| 20  |     |                    |     |     | SISTAW  | CATSIW    P   | 13.4  |     |
| --- | --- | ------------------ | --- | --- | ------- | ------------- | ----- | --- |
|     |     | Signal station in  |     |     |         |               |       |     |
|     |     | general            |     |     |         |               |       |     |
| 21  |     |                    |     |     | SISTAT  | CATSIT  3  P  | 13.4  |     |
|     |     | Signal station,    |     |     |         |               |       |     |
International Port
Traffic Signals
| 22  |     |                         |     |     | SISTAT  | CATSIT  2  P  | 13.4  |     |
| --- | --- | ----------------------- | --- | --- | ------- | ------------- | ----- | --- |
|     |     | Traffic signal station  |     |     |         |               |       |     |
23    Port control signal      SISTAT  CATSIT  1  P  13.4
station
| 24    |     |                        |     |     |         |               |       |     |
| ----- | --- | ---------------------- | --- | --- | ------- | ------------- | ----- | --- |
|       |     | Lock signal station    |     |     | SISTAT  | CATSIT  6  P  | 13.4  |     |
| 25.1  |     |                        |     |     | SISTAT  | CATSIT  8  P  | 13.4  |     |
|       |     | Bridge passage signal  |     |     |         |               |       |     |
station
| 25.2  |     |                          |     |     | SISTAT  | CATSIT  8  P  | 13.4  |     |
| ----- | --- | ------------------------ | --- | --- | ------- | ------------- | ----- | --- |
|       |     | Bridge lights including  |     |     |         |               |       |     |
traffic signals
| 26  |     |                          |     |     | SISTAW   | CATSIW  5  P  | 13.4  |     |
| --- | --- | ------------------------ | --- | --- | -------- | ------------- | ----- | --- |
|     |     | Distress signal station  |     |     |          |               |       |     |
| 27  |     |                          |     |     |          |               |       |     |
|     |     | Telegraph station        |     |     | SISTAT/  | CATSIT    P   | 13.4  |     |
SISTAW  CATSIW
| 28       |     |                         |     |     | SISTAW  | CATSIW  7  P   | 13.4  |     |
| -------- | --- | ----------------------- | --- | --- | ------- | -------------- | ----- | --- |
|          |     | Storm signal station    |     |     |         |                |       |     |
| 29       |     |                         |     |     | SISTAW  | CATSIW  6  P   | 13.4  |     |
|          |     | Weather signal station  |     |     |         |                |       |     |
| 30       |     |                         |     |     |         |                |       |     |
|          |     | Ice signal station      |     |     | SISTAW  | CATSIW  8  P   | 13.4  |     |
| 31       |     |                         |     |     | SISTAW  | CATSIW  9  P   | 13.4  |     |
|          |     | Time signal station     |     |     |         |                |       |     |
| 32.1  #  |     | Tide scale              |     |     | SISTAW  | CATSIW  13  P  | 13.4  |     |
| 32.2     |     |                         |     |     | SISTAW  | CATSIW  12  P  | 13.4  |     |
|          |     | Tide gauge              |     |     |         |                |       |     |
| 33       |     |                         |     |     | SISTAW  | CATSIW  10  P  | 13.4  |     |
|          |     | Tide signal station     |     |     |         |                |       |     |
| 34       |     |                         |     |     |         |                |       |     |
|          |     | Tidal stream signal     |     |     | SISTAW  | CATSIW  11  P  | 13.4  |     |
station
| 35  |     |                         |     |     |         |                 |       |     |
| --- | --- | ----------------------- | --- | --- | ------- | --------------- | ----- | --- |
|     |     | Danger signal station   |     |     | SISTAW  | CATSIW  1  P    | 13.4  |     |
| 36  |     |                         |     |     | SISTAW  | CATSIW  1/4  P  | 13.4  |     |
|     |     | Firing practice signal  |     |     |         |                 |       |     |
station

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
IU Small Craft Facilities

Small Craft Facilities     Transport Features, Bridges   ID      Public Buildings Cranes   IF    Pilots, Coastguards, Rescue, Signal Stations   IT
| 1.1  |     |                        |     | HRBFAC    | CATHAF  | 5  P/A  | 4.6.1  |     |
| ---- | --- | ---------------------- | --- | --------- | ------- | ------- | ------ | --- |
|      |     | Yacht harbour, marina  |     |           |         |         |        |     |
| 1.2  |     |                        |     | HRBFAC    | CATHAF  | 5  P/A  | 4.6.1  |     |
|      |     | Yacht berths without   |     |           |         |         |        |     |
|      |     | facilities             |     |           | INFORM  |         |        |     |
| 2    |     | Visitors’ berth        |     |   SMCFAC  | CATSCF  | 1  P/A  | 4.6.5  |     |

| 3   |     |                    |     | SMCFAC  | CATSCF  | 29  P/A  | 4.6.5  |     |
| --- | --- | ------------------ | --- | ------- | ------- | -------- | ------ | --- |
|     |     | Visitors’ mooring  |     |         |         |          |        |     |
4    Yacht club, Sailing      SMCFAC  CATSCF  2  P/A  4.6.5
club
| 5   |     |                         |     |           |         |          |        |     |
| --- | --- | ----------------------- | --- | --------- | ------- | -------- | ------ | --- |
|     |     | Public slipway          |     |   SMCFAC  | CATSCF  | 28  P/A  | 4.6.5  |     |
|     |     |                         |     | SLCONS    | CATSLC  | 13  L    |        |     |
| 6   |     |                         |     | SMCFAC    | CATSCF  | 3  P/A   | 4.6.5  |     |
|     |     | Boat hoist              |     |           |         |          |        |     |
| 7   |     |                         |     | SMCFAC    | CATSCF  | 28  P/A  | 4.6.5  |     |
|     |     | Public landing, Steps,  |     |           |         |          |        |     |
Ladder
| 8   |     |                    |     | SMCFAC    | CATSCF  | 4  P/A  | 4.6.5  |     |
| --- | --- | ------------------ | --- | --------- | ------- | ------- | ------ | --- |
|     |     | Sailmaker          |     |           |         |         |        |     |
| 9   |     |                    |     |           |         |         |        |     |
|     |     | Boatyard           |     |   SMCFAC  | CATSCF  | 5  P/A  | 4.6.5  |     |
| 10  |     |                    |     | SMCFAC    | CATSCF  | 6  P/A  | 4.6.5  |     |
|     |     | Public House, Inn  |     |           |         |         |        |     |
| 11  |     | Restaurant         |     |   SMCFAC  | CATSCF  | 7  P/A  | 4.6.5  |     |
| 12  |     |                    |     | SMCFAC    | CATSCF  | 8  P/A  | 4.6.5  |     |
|     |     | Chandler           |     |           |         |         |        |     |
| 13  |     | Provisions         |     |   SMCFAC  | CATSCF  | 9  P/A  | 4.6.5  |     |

| 14  |     |                  |     |           |         |          |         |     |
| --- | --- | ---------------- | --- | --------- | ------- | -------- | ------- | --- |
|     |     | Bank, Bureau de  |     |   BUISGL  | FUNCTN  | 13  P/A  | 4.8.15  |     |
change
| 15  |     |                    |     | SMCFAC  | CATSCF  | 10  P/A  | 4.6.5  |     |
| --- | --- | ------------------ | --- | ------- | ------- | -------- | ------ | --- |
|     |     | Physician, Doctor  |     |         |         |          |        |     |
16    Pharmacy, Chemist      SMCFAC  CATSCF  11  P/A  4.6.5
| 17  |     |                        |     | SMCFAC  | CATSCF  | 12  P/A  | 4.6.5  |     |
| --- | --- | ---------------------- | --- | ------- | ------- | -------- | ------ | --- |
|     |     | Water tap              |     |         |         |          |        |     |
| 18  |     |                        |     | SMCFAC  | CATSCF  | 13  P/A  | 4.6.5  |     |
|     |     | Fuel station (Petrol,  |     |         |         |          |        |     |
Diesel)
| 19  |     | Electricity  |     |   SMCFAC  | CATSCF  | 14  P/A  | 4.6.5  |     |
| --- | --- | ------------ | --- | --------- | ------- | -------- | ------ | --- |

| 20  |     |             |     | SMCFAC  | CATSCF  | 15  P/A  | 4.6.5  |     |
| --- | --- | ----------- | --- | ------- | ------- | -------- | ------ | --- |
|     |     | Bottle gas  |     |         |         |          |        |     |
| 21  |     |             |     | SMCFAC  | CATSCF  | 16  P/A  | 4.6.5  |     |
|     |     | Showers     |     |         |         |          |        |     |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
| 22  |     |             |     |           |         |          |        |     |
| --- | --- | ----------- | --- | --------- | ------- | -------- | ------ | --- |
|     |     | Laundrette  |     |   SMCFAC  | CATSCF  | 17  P/A  | 4.6.5  |     |
23    Public toilets      SMCFAC  CATSCF  18  P/A  4.6.5
| 24  |     | Post box  |     |   SMCFAC  | CATSCF  | 19  P  | 4.6.5  |     |
| --- | --- | --------- | --- | --------- | ------- | ------ | ------ | --- |

| 25  |     | Public Telephone  |     |   SMCFAC  | CATSCF  | 20  P    | 4.6.5  |     |
| --- | --- | ----------------- | --- | --------- | ------- | -------- | ------ | --- |
|     |     |                   |     |           |         |          |        |     |
| 26  |     |                   |     |           |         |          |        |     |
|     |     | Refuse bin        |     |   SMCFAC  | CATSCF  | 21  P    | 4.6.5  |     |
| 27  |     | Public car park   |     |   SMCFAC  | CATSCF  | 22  P/A  | 4.6.5  |     |

28    Parking for boats and    SMCFAC  CATSCF  23  P/A  4.6.5
|     |     |     |     |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
trailers
| 29  |     | Caravan site  |     |   SMCFAC  | CATSCF  | 24  P/A  | 4.6.5  |     |
| --- | --- | ------------- | --- | --------- | ------- | -------- | ------ | --- |

| 30  |     |               |     |           |         |          |         |     |
| --- | --- | ------------- | --- | --------- | ------- | -------- | ------- | --- |
|     |     | Camping site  |     |   SMCFAC  | CATSCF  | 25  P/A  | 4.6.5   |     |
| 31  |     | Water police  |     |   BUISGL  | FUNCTN  | 10  P/A  | 4.8.15  |     |

S-57 Appendix B.1 - Annex D                                                                             November 2000                                                                                                        Edition 1.0

INT1 to S-57/52 for ENCs
Index

Abyssal hill………..…………………...IO 37  Bascule  bridge……………………….ID  Bird sanctuary ………………………….IN  Brick kiln, works………………………..IG 81
Abyssal plain……………………………IO 49  23.4  22  Bridges………………………………ID 22-24
Aerial, dish ……………………………...IE 31  Baseline,  Territorial  Sea……………….IN  Black…………………………………….. IQ    suspension………………..IG 114
Aerial cableway…………………….…..ID 25  42  2    lights, traffic signals ………..IT 25
Aero light ……………………………….IP 60  Basin………………..IF 27-28, IG 134,  Blockhouse……………………………IE  Broken …………………………………..IJ 33
Aeronautical radiobeacon…..…………IS 16  IO 48  34.2  Buddhist temple………………………...IE 16
Airfield, airport…………..………………ID 17  Battery…………………………………IE  Blue……………………………. ……..IP  Building…………………………ID 1-8, IG 60
Air obstruction light…………….………IP 61  34.3  11.4    harbour…………………….IG 148
Air traffic………….………………IG 116-118  Bay………………………………………..IO  Board, painted………………………IQ  slip………………………….IG 171

Alternating light……………………..IP 10.11  4  102.2    yard………………………...IG 172
Amber………………………………….IP 11.8    Boarding place, pilot……………………..IT  Bunker station………………………...IG 174
Anchor berth ……………………………IN 11  Beacon…………………..IQ 1-11, IQ 80- 1  Buoys…………………………………IQ 1 -71
|     | 126  | Boat  |     |
| --- | ---- | ----- | --- |
Anchorage………………………IN 10, IO 21  Buoy dump, yard……………………..IG 173
|     | buoyant, resilient…………….IP  | harbour………………..……IU  |     |
| --- | --------------------------- | -------------------- | --- |
Anchorage area………………………..IN 10    Buoyant beacon………………………….IP 5
Anchoring prohibited ………………….IN 20  5  1.1  Buoyed ………………………………….IO 82
Ancient…………………………………..IO 84  Consol……………………….IS    hoist, lift……………...IG 131,  Buried pipe, pipeline……………………IL 42
|                                | 13  | IU 6  |                             |
| ------------------------------ | --- | ----- | --------------------------- |
| Annual change………………………….IB 66  |     |       | Bushes…………………………………..IG 37  |
Anomaly, local magnetic………………IB 82    lighted………………………...IP    park…………………………..IU
| Approach………………………………..IO 22  | 4   | 28  |     |
| ---------------------------- | --- | --- | --- |
Beacon (contd)   yard……………………………IU  Cable  buoy………………………….IQ 55
Approximate…………………………….IO 89
|     |   radar………………….…….IS 2- | 9   |   ferry …………………………IM 51  |
| --- | ----------------------- | --- | ------------------------ |
depth contour…………..……II 31
|                            | 3   |                          | landing beacon……………IQ 123  |
| -------------------------- | --- | ------------------------ | -------------------------- |
| height contour………………IC 12  |     | Bollard………………………………… IG  |                            |
Apron……………………………………IO 59    radio………………….….IS 10- 181    overhead…………………….ID 27
|     | 16  | Boom ………………………………….IG  |   submarine ………………IL 30-32  |
| --- | --- | ---------------------- | --------------------------- |
Archipelago…………………………….. IG 5
|                                   |   tower…………………IP 3, IQ  |      | Cableway (aerial)………………………ID 25  |
| --------------------------------- | ----------------------- | ---- | -------------------------------- |
| Area to be avoided…………………….IM 29  |                         | 178  |                                  |
|                                   | 110                     |      | Cairn……………………………………IQ 100        |
Area, restricted……………………...….IN 20  Borderland, continental………………..IO
Artificial features……………………….IF 1-6  Bell……………………………………….IR  47  Caisson………………………………….IF 42
|     | 14  |     | Calcareous ……………………………...IJ 38  |
| --- | --- | --- | ------------------------------- |
Artificial island…………………………..IL 15  Bottle gas ……………………………….IU
|                                        | Benchmark………………………………IB  |     | Calling-in point …………………………IM 40  |
| -------------------------------------- | ------------------------ | --- | --------------------------------- |
| Astronomical tides…………….IH 2-3, IH 20  |                          | 20  |                                   |
Atoll……………………………………….IG 6  23  Boulder………………………………….IG  Calvary…………………………………..IE 12
Automatic fog signal……………….IR 20-22  Berth  28  Camping site …………………………...IU 30
|     |   Anchor……………………….IN  |     | Canal…………………………...IF 40, IG 132  |
| --- | --------------------- | --- | -------------------------------- |
Avenue…………………………………IG 111  Boundary, international……………IN 40-
11
Awash, rock……………………………..IK 12  41  Canal distance mark…………………...IF 40
designation………………….IF
|     |     | Boundary mark…………………………IB  | Can buoy ……………………………….IQ 21  |
| --- | --- | -------------------------- | ---------------------------- |
Bank……………………………..IO 23, IU 14  19  Canyon …………………………………IO 55
24
Barge buoy……………………………..IQ 53    visitors'………………………..IU  Cap………………………………………IO 63
Breakers ………………………………..IK
2
Barrage, flood…………………………..IF 43  17  Cape ……………………………………...IG 7
Barrel buoy……………………………...IQ 25    yacht………………………...IU  Caravan site ……………………………IU 29
Breakwater……………………………….IF
Barrier, floating………………………...G 178  1.2  Cardinal marks……………………...IQ 130.3
4
Barrier, tidal……………………………IG 130  Cargo transhipment area……………...IN 64
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
Car park ………………………………...IU 27  Commercial port ……………………..IG    line……………………IC 10, IC  Dam…………………………………...…IF 44
| Castle………………………….IE 34.2, IG 64  |     | 147  | 12  | Danger  |     |     |     |
| -------------------------------- | --- | ---- | --- | ------- | --- | --- | --- |
Casuarina……………………………..IC 31.6  Compass rose ………………………….IB  Control points …………………………..IB    area beacon ………………IQ 125
area/zone buoy…………….IQ 50
| Cathedral ……………………………….IG 75  |     | 70  | 20  |     |     |     |     |
| ----------------------------- | --- | --- | --- | --- | --- | --- | --- |
Causeway ………………………………..IF 3  Composite light ………………………...IP  Convent…………………………………IG    firing area …………………..IN 30
Cay………………………………………..IG 3  10  76    isolated marks…………..IQ 130.4
  line……………………………..IK 1
Cement works…………………………..IG 82  Conical buoy …………………………...IQ  Conveyor ……………………………..IG
reported……………………...II 3-4
| Cemetery ………………………………..IE 19  |     | 20  | 182  |     |     |     |     |
| ----------------------------- | --- | --- | ---- | --- | --- | --- | --- |
Chandler ………………………………..IU 12  Conifer ………………………………..IC  Cooling water intake/outfall …………IG    signal station………………..IT 35
Channel…………………………………IO 14  31.3  177  Dangerous wreck………………………IK 28
Data collection buoy …………………..IQ 58
  dredged …………………..Il 21-23  Coniferous woodland …………………IG  Coral………………………IJ 10, IJ 22, IK
|   maintained……………………Il 23  |     | 39  | 16  | Datum  |     |     |     |
| -------------------------- | --- | --- | --- | ------ | --- | --- | --- |
Chart…………………..IH 1, IH 20
Chapel …………………………………..IE 11  Consol beacon …………………………IS  Cove ……………………………………...IO
  land survey……………IH 7, IH 20
| Characters, Light……………………….IP 10  |     | 13  | 9   |     |     |     |     |
| --------------------------------- | --- | --- | --- | --- | --- | --- | --- |
  Ordnance ………………….. IH 20
Chart  Conspicuous landmark …………………IE  Crane…………………………………….IF
Daytime light…………………………….IP 51
|   Datum…………………IH 1, IH 20  |     | 2   | 53  |     |     |     |     |
| -------------------------- | --- | --- | --- | --- | --- | --- | --- |
Chemical pipeline…………………….IL 40.1  Deciduous tree ………………………IC 31.1
|     |     | Conspicuous, on radar …………………IS  | Creek……………………………………...IO  |     |     |     |     |
| --- | --- | -------------------------------- | ------------------------- | --- | --- | --- | --- |
Chemical dumping ground…………….IN 24  Deciduous woodland ………………….IG 38
|     |     | 5   | 7   |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- |
Deep water (DW)
Chemist………………………………….IU 16  Construction works …………………….IF  Cross…………………………………….IE
  Anchorage………………..IN 12.4
| Chimney…………………………………IE 22  |     | 32  | 12  |     |     |     |     |
| -------------------------- | --- | --- | --- | --- | --- | --- | --- |
harbour ……………………IG 142
| Church …………………………………..IE 10  |     |                                |                                            |     |     |     |     |
| ---------------------------- | --- | ------------------------------ | ------------------------------------------ | --- | --- | --- | --- |
|                              |     | Container crane ……………………...IF  | Crossing gates, traffic separation …...IM  |     |     |     |     |
  route …………….…………..IM c
| City ……………………………………...IG 50  |     | 53.2  | 22  |     |     |     |     |
| ---------------------------- | --- | ----- | --- | --- | --- | --- | --- |
Degaussing range ……….…………….IN 25
Clay ……………………………………….IJ 3  Container harbour ……………………IG  Crossing, traffic separation …………..IM
Buoy…………………………IQ 54
| Clearance                   |            |                              |           |                             |             |             |           |
| --------------------------- | ---------- | ---------------------------- | --------- | --------------------------- | ----------- | ----------- | --------- |
|                             |            | 152                          | 23        |                             |             |             |           |
|   horizontal ……………………ID 21  |            |                              |           | Delta……………………………………..IO 18  |             |             |           |
|                             |            | Contiguous Zone ………………………IN  | Cultural  |                             |             |             |           |
|                             | .I D   2 6 |                              |           | D e p t h s ……              | … … … … … … | … … … … … … | … . .I I  |
  s a fe  o v e rh ea d  … … … … … ..   4 4 f e a tu r e s … … … … … … … … … … … ID
|     |     |     |     | D e p t h   |     |     |     |
| --- | --- | --- | --- | ----------- | --- | --- | --- |
  ve r tic a l  … … … … . .ID  2 0 , I D   2 2 - 2 8  C u p o l a ,  ch ur ch  … … … … … … … … … IE
|                                       |     | C on tinental          |       |     | co nt ou rs … … …       | … … … … … ... | II  3 0   |
| ------------------------------------- | --- | ---------------------- | ----- | --- | ----------------------- | ------------- | --------- |
| Cleared platform, site ………………….IL 22  |     | Borderland ………………….IO  | 10.4  |     |                         |               |           |
|                                       |     |                        |       |     | minimum………………….IM 27.2  |               |           |
Clearing line ……………………………..IM 2
|     |     | 47  | Current………………………………IH 42- |     | swept……………………II 24, IK 2  |     |     |
| --- | --- | --- | ------------------------- | --- | ------------------------- | --- | --- |
Clearing line beacons ……………….IQ 121  Continental (contd)  43  Derrick, oil ………………………………IL 10
Cliffs ………………………………………IC 3    rise  ………………………….IO  meter buoy …………………IQ
|                                 |     |                         |     | Designation of beacon or buoy...…IQ 10-11     |     |     |     |
| ------------------------------- | --- | ----------------------- | --- | --------------------------------------------- | --- | --- | --- |
| Closed …………………………………..IO 87     |     | 46                      |     |                                               |     |     |     |
|                                 |     |                         | 59  | Designation of berth …...lF 19, IN 11, IQ 42  |     |     |     |
| Coal harbour ………………………….IG 154  |     | shelf………………….IN 46, IO  |     |                                               |     |     |     |
  Custom office……………………………IF  Designation of reporting point ………..lM 40
| Coarse …………………………………..IJ 32  |     | 42  | 61  |     |     |     |     |
| ---------------------------- | --- | --- | --- | --- | --- | --- | --- |
Destroyed……………………………….lO 93
Coastguard station ………………...IT 10-11    slope ………………………..IO  Customs harbour……………………….lG 14
Detector light …………………………...IP 62
Coastline ………………………………IC 1-8  45  Customs limit……………………………IN
Development area ………………………IL 4
| Coast radar station ……………………...IS 1  |     | Continuous flashing light  …………….IP  | 48  |     |     |     |     |
| ------------------------------------ | --- | ------------------------------------ | --- | --- | --- | --- | --- |
Deviation dolphin……………………….IF 21
Coast radio station, QTG service  …...IS 15  10  Cut……………………………………….IG
Diagonal colour stripes………………….IQ 5
| Cobbles …………………………………...IJ 8  |     | Contour  | 32  |     |     |     |     |
| ----------------------------- | --- | -------- | --- | --- | --- | --- | --- |
Diaphone ……………………………….IR 11
  depth ……………………..II 30-
Coldstore ……………………………….IG 86  Cutting…………………………………...ID  Diffuser ………………………………….IL 43
31
| Colour of beacon, buoy ……………...IQ 2-5  |     |     | 14  |     |     |     |     |
| -------------------------------------- | --- | --- | --- | --- | --- | --- | --- |
Direction-finding station………………..lS 14
Colour of lights …………………………IP 11    drying…………………………II  Cylindrical buoy ………………………..IQ
Direction lights …………………………IP 30
| Coloured mark ………………………..IQ 101  |     | 30  | 21  |     |     |     |     |
| -------------------------------- | --- | --- | --- | --- | --- | --- | --- |
Direction of buoyage ………………lQ 130.2
| Column ………………………….IE 24, IG 66  |     |     |     |     |     |     |     |
| ------------------------------- | --- | --- | --- | --- | --- | --- | --- |
Directional radiobeacon……………….IS 11
Page 91 of 69

INT1 to S-57/52 for ENCs
Discharge pipe …………………………IL 41  Exclusive Economic Zone…………….IN  Fine……………………………………….IJ  Flare stack……………………….IE 23, IL 11
Dish aerial……………………………….IE 31  47  30  Flashing light………………………….IP 10.4
Disposition of lights…………………….IP 15  Exercise area, submarine ……….……IN  Firing danger area …………………….IN  Flat coast ………………………………...IC 5
| Distance mark, canal …………………..lF 40  | 33  | 30  | Floating  |
| ------------------------------------ | --- | --- | --------- |
Distant…………………………………...IO 85  Existence doubtful ……………….………II    Beacon…………………….IQ    barrier ……………………..IG 178
Distress signal station………………….IT 26  1  125    dock………………………….IF 26
  lights………………………..IP 6-8
| Disused                | Experimental……………………….…..IO  |   Buoy…………………………IQ  |                                 |
| ---------------------- | ---------------------------- | ------------------- | ------------------------------- |
| cable …………………………IL 32  |                              |                     | Flood barrage…………………………..IF 43  |
|                        | 92                           | 50                  |                                 |
  pipeline………………………IL 44  Explosive  Firing practice signal station ………….IT  Flood tide stream………………………IH 40
  platform………………………IL 14    anchorage area…….…….IN  36  Floodlight ……………………………….IG 70
| Dock .  | 12.7  |     | Floodlit structure ……………………….IP 63  |
| ------- | ----- | --- | ----------------------------------- |
Fish
  dry, graving………………….IF 25
|                               |   dumping ground….………...IN  |   cages, farm ………………IK  | Fog                          |
| ----------------------------- | --------------------------- | ----------------------- | ---------------------------- |
| floating, wet …………….IF 26-27  |                             |                         | detector light………………..IP 62  |
|                               | 23                          | 48.1                    |                              |
Doctor …………………………………..IU 15  fog signals ……….…………IR  haven ……………………….IK    Iight ………………………….IP 52
|                              |                                |                              |                                |
| ---------------------------- | ------------------------------ | ---------------------------- | ------------------------------ |
| Dolphin………………………………IF 20-21  |                                |                              |   Signals…………………………..IR        |
|                              | 10                             | 46                           |                                |
| Dome………………………………….IE 30.4    |                                |                              | Footbridge …………………………….IG 115  |
|                              | Extinguished light………………………IP  |   trap, weir ……………..IK 44.2- |                                |
| Doubtful                     | 55                             | 45                           | Form lines ………………………………IC 13   |
  depth …………………………..II 2    Fishery limit…………………………..…IN  Fort ………………………………………IE 34
  existence ………………………II 1  Factory………………………………….IG  Foul ……………………………………...IK 31
45
Draw bridge…………………………..ID 23.6
|     | 80  | Fishing  | Fracture zone…………………………..IO 60  |
| --- | --- | -------- | ------------------------------- |
Dredged area, channel……………..II 20-23
Faint sector ……………………………..IP    harbour ….…………………..IF  Free port ……………………………...IG 143
Dredging area…………………………..IN 63  45  10  Front light………………………………..IP 23
Dry dock ………………………………...IF 25  Fairway, safety…………………………..IM f  light ………………………….IP  Fuel station ……………………………..IU 18

Drying contour …………………………..II 30
|     | Fairway, lights marking…………….IP 20- | 50  |     |
| --- | ----------------------------------- | --- | --- |
Drying heights …………………………..II 15  41    prohibited ……………………IN  Game preserve…..……………………IN 22
Dumping ground …………………...IN 23-24  Fan………………………………………IO  21  Gas
| Dunes……………………………………..IC 8  |                         |                        |   Bottle………………………...IU 20  |
| -------------------------- | ----------------------- | ---------------------- | -------------------------- |
|                            | 58                      |   stakes……………………...IK  |                            |
| Dyke ………………………………………IF 1   |                         |                        |   Pipeline……………………IL 40.1  |
|                            | Farm …………………………………….IG  | 44.1                   |                            |
works ……………………….IG 90
|     | 53  | village ……………………….IG  |     |
| --- | --- | --------------------- | --- |

East cardinal mark ………….…...IQ 130.3  Gasfield name……………………………IL 1
|     | Farm, fish, marine ……………………..IK  | 52  |     |
| --- | -------------------------------- | --- | --- |
Ebb tide stream…………………………IH 41  Geographical positions ……………..IB 1-16
|                              | 48                            | Fixed                  |                               |
| ---------------------------- | ----------------------------- | ---------------------- | ----------------------------- |
| Eddies …………………………………..IH 45  |                               |                        | Glacier…………………………………...IC 25  |
|                              | Fast ice, limit………………………….IN  |   Bridge………………………..ID  |                               |
Electricity ……………………………….IU 19  60.1  22  Gong …………………………………….IR 16
Electric works ………………………….IG 89  & flashing light…………..IP  Gorge……………………………………IG 33
|     | Ferry…………………………………IM 50- |     |     |
| --- | ------------------------ | --- | --- |
Electronic position-fixing systems………..IS  Grain harbour…………………………IG 151
|     | 51  | 10.10  |     |
| --- | --- | ------ | --- |
Elevation of light………………..IH 20, IP 13  Grassland……………………………….IG 35
|     | harbour ……………………IG  |   light ………………………..IP  |     |
| --- | ------------------- | ---------------------- | --- |

Embankment …………………………...ID 15  10.1  Gravel ……………………………………..IJ 6
155
Entrance………………………………...IO 16  Graving dock …………………………...IF 25
|     |   light ………………………….IP  |   point …………………………IB  |     |
| --- | ---------------------- | --------------------- | --- |
Entry prohibited area…………………..IN 31  Green………………………….. IP 11.3, IQ 2
|     | 50  | 22  |     |
| --- | --- | --- | --- |
Escarpment …………………………….IO 61  Fjord………………………………………IO  Greenhouse ……………………………IG 84
  terminal, RoRo……………...IF
Established direction of traffic flow…..IM 10  5  Gridiron ………………………………….IF 24
50
| Estuary…………………………………..IO 17  |                        |                                 | Ground tackle…………………………..IQ 42  |
| ---------------------------- | ---------------------- | ------------------------------- | ------------------------------- |
|                              | Filao……………………………………IC  | Flagpole, flagstaff………………………IE  |                                 |
Eucalypt ………………………………IC 31.8  31.7  27  Group light………………………………IP 10
| Evergreen …………………………….IC 31.2     |     |     | Groyne ……………………………………IF 6  |
| --------------------------------- | --- | --- | -------------------------- |
| Exchange office………………………...IU 14  |     |     | Gulf ……………………………………….IO 3  |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
Gully, tidal ………………………………IO 67  Inshore Traffic Zone …………………..IM  Lagoon……………………………IG 13, IO    beacons   …………………IQ 120
| Gun………………………………………IR 10  |     |     | 14  | 8   |     | lights  ………………………..IP 20  |     |     |
| ------------------------ | --- | --- | --- | --- | --- | ------------------------- | --- | --- |
  Installations,  Lake……………………………….IC 23, IO  line  …………………………..IM 1

| Harbour……………………..…….…..IG 138  |     |     | offshore………………………IL  |     |                                           |     |     |     |
| ------------------------------ | --- | --- | -------------------- | --- | ----------------------------------------- | --- | --- | --- |
|                                |     |     |                      | 6   | Least depth in narrow channel ………..II 12  |     |     |     |
Installations,
  Installations…………. IG 170-187  LANBY…………………………….IP 8, IQ  Ledge  …………………………………..IO 28
harbour………………………IG
|                             | Iimit ………………………….IN 49        |     |                            | 26                                 | Lesser …………………………………..IO 86       |     |     |     |
| --------------------------- | ----------------------------- | --- | -------------------------- | ---------------------------------- | --------------------------------- | --- | --- | --- |
|                             | Master's Office ……………..IF 60  |     | Institute………………………………….IG  |                                    |                                   |     |     |     |
|                             |                               |     |                            | Land survey datum ……………..IH 7, IH  | Levee……………………………...IF 1, IO 65    |     |     |     |
| Hard ……………………………………..IJ 39  |                               |     | 74                         |                                    |                                   |     |     |     |
|                             |                               |     |                            | 20                                 | Lifeboat mooring ……………………….IT 13  |     |     |     |
Intake…………………………IG 177, IL
Haven………………………………….IG 139  Landing…………………………………..IF  Lifeboat station …………………………IT 12
Head, headland …………………………IG 8  41.1  17  Lifting bridge………………………….ID 23.3
| Headway ……………………………ID 20-28  |     |     | Intensified sector ………………………IP  |                            |                            |     |     |     |
| ---------------------------- | --- | --- | ------------------------------- | -------------------------- | -------------------------- | --- | --- | --- |
|                              |     |     |                                 |   area (seaplane) ……………IN  | Lights………………………………………..IP  |     |     |     |
46
Health Office………………………….IF 62.1  13    character ……………………IP 10
Intermittent river ……………………….IC
Height……………………….IC 10-14, IE 4-5  beacon (cable)……………IQ  colour  ……………………….IP 11
|                                       |     |     |     |      |     |                            |     |     |
| ------------------------------------- | --- | --- | --- | ---- | --- | -------------------------- | --- | --- |
| Helicopter landing site……………….IG 118  |     |     | 21  |      |     |                            |     |     |
|                                       |     |     |     | 123  |     | description…………………..IP 16  |     |     |
International
High Water …………………………..IH 5-20    lights……………………….IG    direction………………….IP 30-31
  boundary ….…………….IN 40-
Highest Astronomical Tide……...IH 3, IH 20  117  disposition  ………………….IP 15
|     |     |     | 41  |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
Hill ……………………………………….IG 27
|     |     |     |     |     |     | elevation …………………….IP 13  |     |     |
| --- | --- | --- | --- | --- | --- | ------------------------- | --- | --- |
Hillocks …………………………………..IC 4  Interrupted light…………………………IP  public………………………….IU
|     |     |     |     |     |     | in line………………………...IP 21  |     |     |
| --- | --- | --- | --- | --- | --- | ------------------------- | --- | --- |
10
Historic wreck ………………………….IN 26  7    landing   …………………..IG 117
Intertidal area………………………..IJ 20-
Hoist………………………………………IU 6    site (helicopter)……………IG  leading   …………………….IP 20
|     |     |     | 22  |     |     |     |     |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
Hole …………………………………….IO 50  118  stairs, steps …………..IF 18, IU
  major floating  …………….IP 6-8
Island, islet……………………………..IG 1-
| Horizontal  |                           |     |                                 | 7                          |     | marking fairways  ………IP 20-41  |     |     |
| ----------- | ------------------------- | --- | ------------------------------- | -------------------------- | --- | ------------------------------ | --- | --- |
|             | clearance …………………..ID 21  |     | 2                               |                            |     |                                |     |     |
|             |                           |     |                                 |                            |     | moire  effect  ……………….IP 31    |     |     |
|             |                           |     |      Artificial……………………………….IL  |                            |     |                                |     |     |
|             | colour bands…………………IQ 4   |     |                                 | Landmarks……………………………ID 8,  |     |                                |     |     |
  period  ………………………IP 12
|     | lights…………………………IP 15  |     | 15  | IE  |     |     |     |     |
| --- | ---------------------- | --- | --- | --- | --- | --- | --- | --- |
  range ………………………..IP 14
Horn ……….…………………………….IR 13  Isogonal …………………………………IB  Lane, submarine transit  ……………...IN
sector  ………………..….IP 40-42
|                                |     |     | 71  |     |     |     |     |     |
| ------------------------------ | --- | --- | --- | --- | --- | --- | --- | --- |
| Hospital ……………………………….IF 62.2  |     |     |     | 33  |     |     |     |     |
Isolated danger mark ……………...IQ    special …………………..IP 60-65
Hotel……………………………..IG 96, IG 98  Large Automatic Navigational Buoy……….
  structure ……………………IP 1-5
| House……………………………………IG 61  |     |     | 130.4  | …………………………………………...IQ  |     |     |     |     |
| ------------------------- | --- | --- | ------ | ---------------------- | --- | --- | --- | --- |
  times of exhibition. ….....IP 50-52
|                              |                        |                      | Isophase light…………………………IP  | 26                                 |                                |                |                    |          |
| ---------------------------- | ---------------------- | -------------------- | --------------------------- | ---------------------------------- | ------------------------------ | -------------- | ------------------ | -------- |
| Hulk ……………………………………...IF 34  |                        |                      |                             |                                    | Light-float…………………………………IP 7   |                |                    |          |
|                              |                        |                      | 10.3                        | Lateral marks (IALA System))…….IQ  |                                |                |                    |          |
| Hut……………………………………….IG 62     |                        |                      |                             |                                    | Light-vessel ………………………………IP 6  |                |                    |          |
| H y d ro gr a                | p h i c  te rm s … … … | … … … … … … . .I O   |                             | 1 3 0 . 1                          |                                |                |                    |          |
|                              |                        |                      |                             |                                    | L ighted……                     | … … … … … …    | … … … … … . . I O  |  8 1     |
|                              |                        |                      | J e t ty…………..…………………………IF  | L a t tic e                        |                                |                |                    |          |
|                              |                        |                      |                             |                                    |                                | be ac on … … … | .… … … … … . . I P |  3 - 4   |
|                              |                        | .IQ   1 3 0          | 1 4                         |   beacon  ……………………IQ               |                                |                |                    |          |
IA L A  M a r i ti m e  B u o ya ge  S ys te m  ..   marks ………………………IQ 7-8
|     |     |     | Josshouse……………………………….IE  | 111  |     |     |     |     |
| --- | --- | --- | ------------------------- | ---- | --- | --- | --- | --- |
Ice front, limits…………………………..IN 60
|                                     |     |     | 15  | tower…………………………IG  |     | mooring buoy  ……………..IQ 41  |     |     |
| ----------------------------------- | --- | --- | --- | ------------------ | --- | --------------------------- | --- | --- |
| Ice signal station………………………..IT 30  |     |     |     |                    |     |                             |     |     |
  offshore platform ……………IP 2
| Illuminated………………………………IP 63  |     |     |     | 68  |     |     |     |     |
| ----------------------------- | --- | --- | --- | --- | --- | --- | --- | --- |
Lighter Aboard Ship (LASH) ………..IG 184
|     |     |     | Kelp ……………………………………..IJ  | Laundrette………………………………IU  |     |     |     |     |
| --- | --- | --- | ------------------------ | ------------------------- | --- | --- | --- | --- |
In line……………………….IM 1-2, IP 20-21  ighthouse ……………………………….IP 1
|                 |                             |                          | 1 3                   | 2 2 | L   |     |     |     |
| --------------- | --------------------------- | ------------------------ | --------------------- | --- | --- | --- | --- | --- |
| I n a d e q u a | te l y   s u rv ey e d a re | a  … … … … . . II  2 5   |                       |     |     |     |     |     |
|                 |                             |                          | ll……………………………………..IO  |     |     |     |     |     |
I n ci n e ra t io n   a r e a … … … … … … … … … . I N   6 5   Kn o La v a……..………………………………IC
36
| Industrial harbour …………………….IG 146  |     |     |     | 26                          |                            |     |     |     |
| ----------------------------------- | --- | --- | --- | --------------------------- | -------------------------- | --- | --- | --- |
|                                     |     |     |     | Layered bottom……………………….IJ  | Limits………………………………………..IN  |     |     |     |
Inlet………………………………………IO 10
Ladder …….……………………………..IU
I n n … … … … … … … … … … … … … … … . .IU  1 0   1 2 . 1     d a n g e r  li n e … … … …… … … … I K  1
7
.I G  1 4 0 Le a d i ng    d re d g e d   a re a… … … …… . II  2 0 -2 3
| I n n er  ha rb | ou r… … … … … … | … … … …   |     |     |     |     |     |     |
| --------------- | --------------- | --------- | --- | --- | --- | --- | --- | --- |
  gasfield, oilfield…………….IL 3-4
Page 93 of 69

INT1 to S-57/52 for ENCs
  restricted area..…..IN 2, IN 20-26  Mast …………………………………….IG  Moat …………………………………….IO  Mouth……………………………………IO 19
  routeing measure  …………IM 15  67  57  Mud  ……………………………………….IJ 2
unsurveyed area…………….II 25  mooring …………………….IG  Moir# effect light ……………………….IP  Multi-storey building …………………..IG 63
|                                          |     |     |     |     |
| ---------------------------------------- | --- | --- | --- | --- |
| Liquified Natural Gas (LNG) ………..IG 185  |     | 69  | 31  |     |
Liquified Petroleum Gas (LPG)……..IG 186    radar……………………….lE 30.1  Mole ……………………………………..IF  Named anchorage area …………..IN 12.3
Local Magnetic Anomaly………………IB 82    radio, television……………..lE 28  12  Narrows…………………………………IO 15
|                             |     |   wreck………………………...IK  |                           | National limits …………….……….IN 40-49  |
| --------------------------- | --- | ---------------------- | ------------------------- | ----------------------------------- |
| Loch……………………………………….IO 6    |     |                        | Monastery ………………………………IG  |                                     |
|                             |     | 25                     |                           | Natural features……………………………IC       |
| Lock ……………………………………..IF 41  |     |                        | 76                        |                                     |
Maximum draught on track …………….IM
  signal station………………..IT 24  Monument ………………………………IE  Natural inland features…………….IG 20-39
Log pond ……………………………….IN 61  6  24  Nature reserve …………………………IN 22
Mean Sea Level………………….IH 6, IH  Nature of the seabed………………………IJ
Long-flashing light……………………IP 10.5  Moored storage tanker ………………..IL
20
Look-out, pilot  …………………………...IT 2  17  Nautophone…………………………….IR 13
Mean Tide Level………………………..IH
Look-out station ……………………….IG 77  Mooring………………………………….lL  Naval port …………………………….IG 145
|                            |     | 20  |     | Naval College  …………………………IG 79  |
| -------------------------- | --- | --- | --- | ------------------------------- |
| Lough …………………………………….IO 6  |     |     | 12  |                                 |
Measured distance…………………...IQ
Low Water…………………………….IH 4-20    berth number…………….…lQ  Navigation school………………………IG 78
122
Lower light………………………………IP 23  42  Neap tides ………………………….IH 10-20
|     |     | Median valley…………………………...IO  |     | Nets, tunny……………………………IK 44.2  |
| --- | --- | ----------------------------- | --- | ------------------------------ |
Lowest Astronomical Tide………IH 2, IH 20    ground tackle………….……lQ
|     |     | 54  |     | Nipa palm……………………………..IC 31.5  |
| --- | --- | --- | --- | ------------------------------ |
|     |     |     | 42  |                                |
Medium ………………………………….IJ
Machine house ……………………….IG 93    lifeboat………………….……lT  No bottom found…………………………Il 13
| Magnetic     |     | 31  | 13  | Non-dangerous wreck  ………………..IK 29  |
| ------------ | --- | --- | --- | ----------------------------------- |
  Anomaly……………………..IB 82  Military practice area……………….IN 30- Non-directional radiobeacon…………..lS 10
  mast…………………….……lG
|   Compass………………...IB 60-82  |     | 34                      |     |                                     |
| --------------------------- | --- | ----------------------- | --- | ----------------------------------- |
|                             |     |                         | 69  | Non-tidal basin  ………………………...IF 27  |
| Variation……………………..IB 60    |     | Mill……………………………………….IG  |     |                                     |
  numerous……………….…..IQ  North cardinal mark ………………..IQ 130.3

Maintained channel …………………….II 23  83  Notice board ………………………….IQ 126
44
Major floating light …………………….IP 6-8  Minaret ………………………………….IE  Number, anchorage,
  trot …………………….…….lQ
Major light…………………………………IP 1  17      berth……..………..IF 19, IN 11-12, lQ
42
| Mangrove………………………………..IC 32  |     | Mine……………………………………...IE  |     | 42  |
| ---------------------------- | --- | ------------------------ | --- | --- |
  visitors' ……………….………IU
Marabout………………………………...IE 18  36  Numerous moorings…………………...IQ 44
3
Marina…………………………………..IU 1.1  Mine-laying practice area …………….IN  Nun buoy ……………………………….IQ 20
Mooring buoy ………………….……….IQ
|   facilities………………….…..IU 32   |     |     |     |     |
| ------------------------------ | --- | --- | --- | --- |
|                                |     | 32  | 40  |     |
| Marine farm……………………………..IK 48  |     |     |     |     |
Minefield ………………………………..IN
|                                  |     |     |   lighted……………….………IQ  | Obelisk………………………………….IE 24  |
| -------------------------------- | --- | --- | ---------------------- | --------------------------- |
| Maritime limit……………………………..IN 1  |     | 34  |                        |                             |
41
Marks  Minimum depth on route ……………….IM  Obscured sector………………………..IP 43
tanker……….………..IL 16, IQ
  cardinal ………………….IQ 130.3    Observation platform ………………….IL 13
c
  coloured   ………………....IQ 101  26  Observation spot ………………………IB 21
Minor
isolated danger…………IQ 130.4    telephonic…………………...IQ  Observatory…………………………….IG 73
|     |     |   light ……………………………IP  |     |     |
| --- | --- | ---------------------- | --- | --- |
43
  lateral ……………………IQ 130.1  1  Obstruction………………………….IK 40-48
  lighted ……………………...IQ 7-8  Morse Code light……………………..IP  Obstruction light, air …………………...IP 61
  marks ………………….IQ 90-
minor …………………..IQ 90-102  10.9  Occasional light ………………………..IP 50
|     |     | 102  |     |     |
| --- | --- | ---- | --- | --- |
Mosque …………………………………IE
  safe water……………….IQ 130.5    post, pile……………………..IF  Occulting light………………………...IP 10.2
17
|   special …………………..IQ 130.6  |     | 22  |     | Ocean……………………………………..IO 1  |
| ---------------------------- | --- | --- | --- | -------------------------- |
white ……………………….IQ 101  Motorway ……………………………….ID  current ………………………IH 43
|                                   |     | Mixed bottom………………………….IJ  |                                  |                                |
| --------------------------------- | --- | -------------------------- | -------------------------------- | ------------------------------ |
|                                   |     |                            | 10                               | Ocean Data Acquisition System  |
| Marked ………………………………….IO 83        |     | 12.2                       |                                  |                                |
|                                   |     |                            | Mount, Mountain……………….IG 23, IO  | (ODAS) buoy……….IQ 26, IQ 58    |
| Marker Ship buoy ……………………..IQ 52  |     |                            |                                  |                                |
| Marsh …………………………………...IC 33       |     |                            | 32                               | Office ……………………………………IG 72     |
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
  Custom………………………IF 61  Perch  …………………………………..IQ    land………….………………ID    tidal data ……………IH 30, IH 46
  Harbour Master's……….…..IF 60  91  29  Position-fixing systems…………………….IS
Health………………….…..IF 62.1  Period of light  ………………………….IP  overhead ……………………ID  Post  ……………………………………..IF 22
|                            |     |     |                          |
| -------------------------- | --- | --- | ------------------------ |
|   pilot …………………….……..IT 3  | 12  | 28  |   box …………………………..IU 24  |
Offshore installations……………….……..IL  Pharmacy ………………………………IU  Plateau………………………….IG 30, IO    Office…………………………IF 63
Offshore platform, lighted ………….….IP 2  16  39  submerged ………………….IK 43

Offshore position, tidal levels  ……….IH 30  Physician ……………………………….IU  Platform….IL 2, IL 10,, IL 13-14, IL  Power
cable …………………………IL 31
| Ogival buoy……………………………..lQ 20  | 15  | 22, IP 2  |     |
| ------------------------------ | --- | --------- | --- |
Oil  Pictorial symbols………………………IE  Point………………………………………IG    transmission line …………..ID 26
  barrier ……………………….IF 29  3.1  9     station ………………………IG 88
derrick ……………………….lL 10  Practice area (military) ………….…IN 30-34
|     | Pier ………………………………………IF  |   fixed …………………………IB  |     |
| --- | ----------------------- | --------------------- | --- |
  harbour ……………………IG 149  14  22  Precautionary area …………...IM 16, IM 24
  pipeline…………………….IL 40.1  promenade ………………….IF  radio reporting ……………..IM  Preferred channel buoy …………...IQ 130.1
|     |     |     |     |
| --- | --- | --- | --- |
Oilfield …………………………………..lL 1-4  Private buoy ……………………………IQ 70
|     | 15  | 40  |     |
| --- | --- | --- | --- |
  name…………………………..IL 1    ruined………………………IF  Pole……………………………………...IQ  Private light……………………...IP 50, IP 65
Oily wastes, reception facilities …….IG 175  33.2  90  Production platform ……………………IL 10
One-way track ………………….IM 5.1, IM d  Production well………………………….IL 20
|                                  | Pile, piling……………………...IF 22, IG  | Police  …………………………………IG  |              |
| -------------------------------- | --------------------------------- | ------------------------ | ------------ |
| Opening bridge ………………………ID 23.1  |                                   |                          | Prohibited   |
|                                  | 179                               | 156                      |              |
anchoring …………………..IN 20
Orange………………………………...IP 11.7    row of………………………IG    water …………...…………...IU
Ordnance Datum……………………….IH 20  180  31    area………………………….IN 31
| Ore harbour……………………………IG 150  |                        |                          |   fishing ……………………….IN 21  |
| ----------------------------- | ---------------------- | ------------------------ | -------------------------- |
|                               |   submerged …………………IK  | Pontoon …………………………………IF  |                            |
Projected ……………………………….IO 80
| Outer harbour…………………………IG 141  | 43  | 16  |     |
| ------------------------------ | --- | --- | --- |
Promenade pier ………………………..IF 15
Outfall………………………………….IL 41.1  Pillar …………………………………….IE  Bridge……………………..ID

| Buoy…………………………IQ 57  |     |       | Promontory……………………………..IG 20  |
| -------------------- | --- | ----- | ----------------------------- |
|                      | 24  | 23.5  |                               |
Province ………………………………..IO 66
|   cooling water ……………..IG 177  |   Buoy…………………………IQ  |     |     |
| ------------------------------ | ------------------- | --- | --- |
Provisions ………………………………IU 13
| Overfalls…………………………………IH 44  | 23                        |                        |                              |
| ---------------------------- | ------------------------- | ---------------------- | ---------------------------- |
|                              |                           |                        | Public                       |
|                              | Pilot                     | Ports…………………………………………I | buildings ………………...IF 60-63  |
| Overhead                     | boarding place…………….….IT  |                        |                              |

|   cable ………………………...ID 27  |     | F   |   inn …………………………...IU 10  |
| -------------------------- | --- | --- | ------------------------- |
1
  pipe…………………………..ID 28    types ………………………IG    Ianding ……………………….IU 7
  cruising vessel position….…..IT
| transporter…………………..lD 25  |     | 130  | telephone …………………..IU 25  |
| -------------------------- | --- | ---- | ------------------------- |
|                            | 1   |      |                           |
    control signal station……….IT    toilets …….………………….IU 23
  helicopter transfer………….IT
Pack ice, limit……………………….IN 60.2  23  Pump house ……………………………IG 93
1.4
Paddyfield ………………………………IG 36  Position …………………………………IB  Pylon  …………………………………...ID 26
look-out………………………..IT
| Pagoda…………………………………..IE 14       |     |                           |                             |
| -------------------------------- | --- | ------------------------- | --------------------------- |
|                                  |     | 22                        | Pyramid………………………………….IG 65  |
| Painted board……………………….IQ 102.2  | 2   |                           |                             |
|                                  |     |   approximate ………………….IB  |                             |
  office………………………...IT 2-
| Palm…………………………………...IC 31.4  |     | 7   | QTG service……………………………IS 15  |
| ---------------------------- | --- | --- | ---------------------------- |
3
Parking, boat, car ………………….IU 27-28  Qualifying terms, seabed ………….IJ 30-39
|     | Pilotage ………………………………...IT 1- |   beacon, buoy………………...IQ  |     |
| --- | ----------------------------- | -------------------------- | --- |
Partly ……………………………………IO 88  Quarantine anchorage ……………...IN 12.8
1
4
Passage…………………………………IO 13    doubtful……………………….IB 8  Quarantine building ………………….IF 62.1
Pinnacle ………………………………...IO
Patent slip……………………………….IF 23    fog signal …………………….IR  Quarry …………………………………..IE 35
29
| Path ……………………………………..ID 12  |     | 1   | Quay …………………………………….IF 13  |
| --------------------------- | --- | --- | -------------------------- |
Pipe, pipeline ……………………….IL 40-
Peak……………………………..IG 25, IO 35    pilot cruising vessel  …………IT  Quick light……………………………..IP 10.6
44
| Pebbles……………………………………IJ 7   |     | 1   |                           |
| --------------------------- | --- | --- | ------------------------- |
| Peninsula…………………………………IG 4  |     |     | Races……………………………………IH 44  |
Page 95 of 69

INT1 to S-57/52 for ENCs
Racon……………………………………..IS 3  Reporting, Radio ………………………IM  Roundabout, traffic separation ………..IM  Scarp  …………………………………..IO 61
Radar……………………………………IS 1-5  40  a  School…………………………………..IG 78
beacon, transponder ……..IS 2-3  Rescue station ……………………..IT 11- Route………………………………..IM 27-28  Scrubbing grid …………………………IF 24

Routeing measures …………………IM
|   conspicuous ………………….IS 5  | 12  |     | Sea……………………………………….IO 2  |
| --------------------------- | --- | --- | ------------------------ |
  dome, mast, scanner,   Research platform……………………...IL  10-f    ice limit……………………IN 60.2
tower………………………... IE 30  13  Row of piles …………………………..IG  moat…………………………IO 57

|     |                                 | 180  |           |
| --- | ------------------------------- | ---- | --------- |
|     | Reserve fog signal …………………….IR  |      | Seaplane  |
Radar (contd)  Ruin………………………………..ID 8, IF  anchorage …………….……IN 14
|     | 22  |     |     |
| --- | --- | --- | --- |
  range ………………………..IM 31
Reserved anchorage area…………..IN  33    anchorage buoy……………IQ 60
  reference line……………….IM 32
|     | 12.9  | Runway ……………………………….IG  |   landing area………………...IN 13  |
| --- | ----- | ----------------------- | ----------------------------- |
reflector……………IQ 10-11, IS 4
|     |     | 116  | Seabed, types of …………………….IJ 1-15  |
| --- | --- | ---- | ---------------------------------- |
Reservoir ……………………………...IG
  station ………………………...IS 1
|                                     | 135  |     | Seachannel …………………………….IO 56  |
| ----------------------------------- | ---- | --- | ----------------------------- |
|   surveillance system …...IM 30-32  |      |     |                               |
Resilient beacon ………………………...IP  Seal sanctuary …………………………IN 22
| Radio…………………………………IS 10-16  |     | IO 64    |                                |
| --------------------------- | --- | -------- | ------------------------------ |
|                             |     | Saddle   | Seamount …………………………..IO 33-34  |
5
  direction-finding station …...IS 14  Safe overhead clearance……………...ID
Restaurant  ……………………………..IU  Seasonal sea ice limit……………….IN 60.2
|   mast, tower ……………..IE 28-29  |     | 26  |                                |
| ------------------------------ | --- | --- | ------------------------------ |
|                                | 11  |     | Seasonal buoy………………………….IQ 71  |
reporting line, point………...IM 40  Safe water marks ………………….IQ
|     |     |     | Seawall……………………………………IF 2  |
| --- | --- | --- | -------------------------- |
Restricted area…………………..IM 14, IN
|   station, QTG service……….IS 15  |     | 130.5  |     |
| -------------------------------- | --- | ------ | --- |
2
Radiobeacon………………………..IS 10-16  Safety fairway ……………………………IM
Restricted light sector………………….IP
| Rail Traffic ……………………….IG 110-118  |     | f   | Sector  |
| ---------------------------------- | --- | --- | ------- |
44
Railway, railway station ……………….ID 13  Safety zone……………………………….IL    faint ………………………….IP 45
Retroreflecting material ………………...IQ  intensified …………………..IP 46
| Ramark …………………………………...IS 2  |     | 3   |     |
| ---------------------------- | --- | --- | --- |
6
Ramp ……………………………………IF 23  Sailing club ………………………………IU    Iights……………………..IP 40-41
Ridge…………………………….IG 22, IO
| Range……………………………………IG 21     |                              | 4                         |   obscured ……………………IP 43        |
| ---------------------------- | ---------------------------- | ------------------------- | ------------------------------- |
|                              | 30                           |                           |   restricted…………………….IP 44      |
| Rapids …………………………………..IC 22  |                              | Sailmaker…………………………………IU  |                                 |
|                              | Rise……………………………...IO 31, IO  |                           | Separation line…………………………IM 12  |
| Ravine …………………………………..IG 32  |                              | 8                         |                                 |
46
Rear light ……………………………….IP 22  Sailors' home…………………………...IG  Separation zone………………………..IM 13
Reception facilities, oily wastes …….IG 175  River…………………………………IC 20- 97  Settlements………………...ID 1-8, IG 50-54
|     | 21  |     | Sewage works …………………………IG 92  |
| --- | --- | --- | ----------------------------- |
Reclamation area……………..IF 31, IG 136  Saint……………………………………..IG
Road ………………………………...ID 10-
| Recommended  |     | 54  | Sewer…………………………………….IL 41  |
| ------------ | --- | --- | -------------------------- |
12
  anchorage ………………….IN 10  Salt pans ……………………………….IC  Shapes, Buoy ……………………...IQ 20-26
direction of traffic flow …….IM 11 Road traffic ……………………...IG 110- Shed, transit…………………………….IF 51
|                            |                                | 24                                  |                                |
| -------------------------- | ------------------------------ | ----------------------------------- | ------------------------------ |
|   route ………………………IM 28.1   | 118                            |                                     |                                |
|                            |                                | Saltings, saltmarsh…………….IC 33, IG  | Sheerlegs……………………………..IF 53.3  |
| track………………….IM 3-4, IM 6  | Roads, roadstead  …………………….IO  |                                     |                                |
|                            |                                | 12                                  | Shelf…………………………………IO 42-43     |
Recreation zone buoy ………………...IQ 62  20  Shellfish bed…………………………….IK 47
Sand ………………………………………IJ
Red…………………………..….IP 11.2, IQ 3  Rock……………………IG 11, IJ 9, IK  Shells ……………………………………IJ 11
1
| Reed …………………………………….IR 13  | 10-15  |                                   |                                |
| -------------------------- | ------ | --------------------------------- | ------------------------------ |
|                            |        | Sandhills, Sand dunes…………………..IC  | Shingly shore…………………………….IC 7  |
Reef………………………IJ 22, IK 16, IO 26    Area…………………………..IJ  Shinto shrine……………………………IE 15
8
Refinery…………………………………IG 87  21  Ship lift…………………………………IG 131
Sandwaves………………………………IJ
| Reflector, radar………………IQ 10-11, IS 4   | Rocket station ………………………….IT  |                              |                             |
| -------------------------------------- | ----------------------------- | ---------------------------- | --------------------------- |
|                                        |                               | 14                           | Shoal…………………………………….IO 25   |
| Refrigerated storage house…………..IG 86  | 12                            |                              |                             |
|                                        |                               | Sandy shore……………………………...IC  | Shoaled………………………………….IO 91  |
Refuge beacon………………..IQ 124, IT 14  Roll-on, Roll-off (RoRo) ferry terminal..IF  Shore, shoreline ………………………IC 1-8
6
| Refuse bin ……………………………...IU 26  | 50                                     |                              |                            |
| ------------------------------- | -------------------------------------- | ---------------------------- | -------------------------- |
|                                 |                                        | Scanner, radar………………………..IE  | Showers…………………………………IU 21  |
| Relief…………………………………IC 10-14     | Rotating pattern radiobeacon ………...IS  |                              |                            |
|                                 |                                        | 30.3                         | Sidearm…………………………………IO 68  |
Reported depth ………………………...Il 3-4  12  Signal station…………….IT 20-31, IT 33-36
Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
Sill ……………………………………….IO 62  Steep coast……………………………….IC  Subsidiary light ………………………...IP     Line…………………………..ID 27
Silo ………………………………………IE 33  3  42     Office………………………...IG 95
Silt …………………………………………IJ 4  Steps………………………………IF 18, IU  Summit ………………………………….IG  station ……………………….IT 27

Single Buoy Mooring (SBM)…..IL 16, IQ 26  7  24  Telephone line …………………………ID 27
Single Point Mooring (SPM) ………….IL 12  Sticky …………………………………….IJ  Sunken rock…………………………….IO  Telephone, public………………………IU 25
Siren…………….……………………….IR 12  34  27  Telephonic mooring buoy……………..IQ 43
Sketches ……………………………….IE 3.2  Stiff……………………………………….IJ  Superbuoy………...IL 16, IP 8, IQ 26, IQ  Television mast, tower …………….IE 28-29
Slip …………………………………….IG 171  36  58  Temple ………………………………IE 13-16
Slipway…………………………….IF 23, IU 5  Stones …………………………………….IJ  Supply pipeline………………………….IL  Temporary light…………………………IP 54
Slope………………………………...IO 44-45  5  40  Terminal ………………………………IG 170
Sluice ………………………………….IG 133  Stony shore………………………………IC  Surveyed coastline ……………………..IC  Terrace………………………………….IO 40
Small craft facilities………………………..IU  7  1  Territorial Sea limit……………………..IN 43
Snags……………………………………IK 43  Storage tanker…………………IG 183, IL  Suspended well…………………………IL  Territorial Sea, straight baseline ……..IN 42
| Soft ………………………………………IJ 35  | 17  | 21  | Tidal  |
| -------------------------- | --- | --- | ------ |
Sound……………………………………IO 12  Storehouse …………………………….IG  Suspension bridge……………………IG    barrier……………………...IG 130
Soundings……………………………..II 1-24  85  114       basin ………………………...IF 28
  gully …………………………IO 67
South cardinal mark ……………….IQ 130.3  Storm signal station ……………………IT  Swamp…………………………………..IC
harbour ……………………...IF 28
| Spa hotel ……………………………….IG 98  | 28  | 33  |     |
| ----------------------------- | --- | --- | --- |
Spar buoy……………………………….IQ 24  Strait……………………………………..IO  Swept area, depth………………...II 24, IK  Tidal level……………………………..IH 1-30
Special lights ……………………….IP 60-65  11  2    station, offshore ……………IH 30
table …………………………IH 30
Special marks……………………….IQ 130.6  Stranded wreck……………..IK 20-21, IK  Swing bridge …………………………ID
Special purpose buoys…………….IQ 50-62  24  23.2  Tidal stream ……………………………IH 31
Special purpose beacons………IQ 120-126  Stream ………………………………….IC  Swinging circle……………………….IN    ebb, flood………………..IH 40-41
station ……………………….IH 46
| Spherical buoy …………………………IQ 22  | 20  | 11.2  |     |
| ------------------------------- | --- | ----- | --- |
Spindle buoy …………………………...IQ 24  Street……………….……………ID 7, IG      signal station………………..IT 34
Spire…………………………………...IE 10.3  110  Table-land ……………………………..IG    table  ………………………..IH 30
|                                |                               | 29                         | Tide                          |
| ------------------------------ | ----------------------------- | -------------------------- | ----------------------------- |
| Spit ……………………………………...IG 10    | Strip light ………….…………………….IP  |                            |                               |
|                                |                               | Tablemount …………………………….IO  |   gauge, scale………………...IT 32  |
| Spoil ground ……………………………IN 62  | 64                            |                            |                               |
rips…………………………...IH 44
|   Buoy…………………………IQ 56  | Stumps …………….……………………IK  | 38  |     |
| ---------------------- | ------------------------ | --- | --- |
Spot heights……………..…………IC 11-13  43  Tank ……………………………………..IE    signal station  ………………IT 33
|                                   |            | 32  | Tideway…………………………………IO 67  |
| --------------------------------- | ---------- | --- | -------------------------- |
| Spring, seabed …………..…………….IJ 15  | Submarine  |     |                            |
Timber harbour ………………………IG 153
| Spring tides ……………..…………..IH 8-20  |   cable ……………………..IL 30- | Tanker                  |                                |
| ---------------------------------- | ------------------------ | ----------------------- | ------------------------------ |
|                                    |                          | anchorage area…………..IN  | Timber yard……………………………..IF 52  |
| Spur …………………..………………..IO 41        | 32                       |                         |                                |
exercise area ………………IN  12.5  Time signal station……………………..IT 31
| Stake…………………...….IK 44.1, IQ 90-91  |                           |                                   |                             |
| ----------------------------------- | ------------------------- | --------------------------------- | --------------------------- |
|                                     |                           |   cleaning facilities …………IG 176  | Toilets……………………………………IU 23  |
| Stations                            | 33                        |                                   |                             |
|                                     |                           |   mooring buoy………..IL 16, IQ      | Topmark IQ 9-11, IQ 102.1   |
|   Bunker………………..……IG 174            |   pipeline…………………..IL 40- |                                   |                             |
26
  Coastguard……………...IT 10-11  44  Topographic terms…………………………IG
coast radar……………IM 30, IS 1  transit lane…………………..IN    storage……………..IG 183, IL  Tower…………………………………….IE 20
|                                 |     |     |                           |
| ------------------------------- | --- | --- | ------------------------- |
|                                 |     | 17  | Beacon………………IP 3, IQ 110  |
|   coast radio ……………...IS 14-15  | 33  |     |                           |
Tap, water……………………………….IU
  fuel…………………………...IU 18  Submerged……………………………..IO    Church…………………….IE 10.2
17
| lookout ……………………...IG 77  | 90  |     |   lattice ……………………….IG 68  |
| ------------------------- | --- | --- | -------------------------- |

|                           |                            | Target buoy……………………………..IQ  |   radar ………………………IE 30.2       |
| ------------------------- | -------------------------- | --------------------------- | ------------------------------ |
|   railway ………………………ID 13  |   rock, beacon on ……………IQ  |                             |                                |
|                           |                            | 51                          | radio, television …………….IE 29  |
|   rescue……………………IT 11-13  | 83                         |                             |                                |
Telegraph
| signal……………………..IT 20-36  |     |     |   watch ………………………..IG 77  |
| ------------------------- | --- | --- | ------------------------- |

Page 97 of 69

INT1 to S-57/52 for ENCs
  water ………………………...IE 21    Area…………………………...Il  Volcano………………………………….IG    Production…………………...IL 20
| Town ……………………………………IG 50  | 25  | 26  |     |   Suspended…………………..IL 21  |
| ------------------------- | --- | --- | --- | -------------------------- |
Town Hall ……………………………….IG 71  coastline ……………………..IC    West cardinal mark ………………..IQ 130.3

Wall, training ……………………………IF
Track……………………………ID 12, IM 1-6  2  Wet dock  ………………………………..IF 27
Trade port …………………………….IG 147    wreck ……………………IK 28- 5  Wharf…………………………………….IF 13
Traffic (road, rail, air)……………IG 110-118  30  Warehouse……………………...IF 51, IG  Whistle…………………………………..IR 15
85
Traffic flow, direction………IM 10-11, IM 26  Upper light………………………………IP  White…………………………………..IP 11.1
Watch tower…………………………….IG
Traffic Separation Scheme………..IM 10-27  22  White mark ……………………………IQ 101
  Buoy…………………………IQ 61  Urban area ………………………………ID  77  Wind signal station……………………..IT 29
Traffic signal…………………..IT 22, IT 25.2  1  Water  Windmill …………………………………IE 25
  features…………….……IC 20-
Traffic surveillance station ……………IM 30    Windmotor………………………………IE 26
25
Trailer park ………..……………………IU 28    Wire drag sweep…………………..II 24, IK 2
|                                | Valley………………………..IG 31, IO  | mill…………………………...IG  |     |                            |
| ------------------------------ | --------------------------- | -------------------- | --- | -------------------------- |
| Training wall………………………………IF 5  |                             |                      |     | Withy…………………………………….IQ 92  |
|                                | 53-54                       | 83                   |     |                            |
Tramway ……………………………...IG 112  Woodland, woods…………..IC 30, IG 38-39
|     | Variable arrow light ……………………IP  |   pipe, pipeline…………….IL 40- |     |     |
| --- | -------------------------------- | ---------------------------- | --- | --- |
Transhipment facilities……………..IF 50-53  Works………………………………..IG 81-92
|     | 31  | 41  |     |     |
| --- | --- | --- | --- | --- |
Transhipment area……………………..IN 64  Works in progress…………………..IF 30-32
|                             | Variation…………………………………IB         |   police………………………...IU  |     |                              |
| --------------------------- | -------------------------------- | ----------------------- | --- | ---------------------------- |
| Transit…………………………………….IM 2  |                                  |                         |     | Wreck………………………………..IK 20-31  |
|                             | 60                               | 31                      |     |                              |
|   Lane………………………….IN 33      |                                  |                         |     |   historic…………….…………IN 26    |
|                             | Vegetation…………………..IC 30-33, IG  | tap ………...…………………IU     |     |                              |
|   Shed………………………….IF 51      |                                  |                         |     |                              |
Transponder beacon ……………………IS 3  34  17  Yacht berth, harbour ………………….IU 1
Vertical    tower ………………………...IE  Yacht club ………………………………..IU 4
Transporter………………………….ID 24-25
|                                   |   Clearance……….ID 20, ID 22- | 21                   |     |                          |
| --------------------------------- | ---------------------------- | -------------------- | --- | ------------------------ |
| Trap, fish ………………………….IK 44.2-45  |                              |                      |     | Yard                     |
|                                   | 28                           | works ………………………..IG  |     | Building…………………….IG 172  |
| Travelling crane………………………IF 53.1  |                              |                      |     |                          |
colour stripes…………………IQ
|     |     | 91  |     |   buoy ……………………….IG 173  |
| --- | --- | --- | --- | ------------------------ |
Trees…………………………………….IC 31
|     | 5   | Waterfall…………………………………IC  |     |   timber ………………………..IF 52  |
| --- | --- | ------------------------- | --- | -------------------------- |
  height to top ………………..IC 14
|     |   lights…………………………IP  | 22  |     | Yellow…………………………...IP 11.6, IQ 3  |
| --- | --------------------- | --- | --- | --------------------------------- |
Trench …………………………………..IO 51
15
|     |     | Wave recorder  |     |     |
| --- | --- | -------------- | --- | --- |
Triangulation point……………………..IB 20  Very Large Crude Carrier (VLCC) …IG  Buoy…………………………IQ  Zone

| Trot, mooring……………………………IQ 42  |      |     |     | fracture………………………IO 60  |
| ------------------------------ | ---- | --- | --- | ----------------------- |
|                                | 187  | 59  |     |                         |
Trough…………………………………...lO 52
Very quick light  ………………………IP  Wave-actuated fog signal…………IR 21-   inshore traffic ………………IM 14
Tufa ……………………………………….IJ 1
|                                 | I0.7                    | 22                                  |     |   separation…………………..IM 13  |
| ------------------------------- | ----------------------- | ----------------------------------- | --- | --------------------------- |
| Tun buoy ……………………………….IQ 25     |                         |                                     |     |                             |
|                                 | Viaduct…………………………………IG  | Way point ……………………………….IM           |     |                             |
| Tunnel ………………….……………….ID 16     |                         |                                     |     |                             |
|                                 | 113                     | 40                                  |     |                             |
| Tunny nets …………….……………IK 44-45  |                         |                                     |     |                             |
|                                 | Views……………………………………IE   | Weather signal station ………………...IT  |     |                             |

Two-way route…….………………..IM 27-28
|                                            | 3.2                                  | 29                          |     |     |
| ------------------------------------------ | ------------------------------------ | --------------------------- | --- | --- |
| Two-way track…………………………IM 5.2              |                                      |                             |     |     |
|                                            | Village…………………………ID 4, IG 51-        | Weed…………………………………..IJ       |     |     |
| Tyfon…………………………………….IR 13                  |                                      |                             |     |     |
|                                            | 52                                   | 13.1                        |     |     |
| Types of seabed……………………...IJ 1-15          |                                      |                             |     |     |
|                                            | Violet ………………………………….IP              | Weir, fish ……………………………..IK  |     |     |
| Ultra quick light……………………..IP 10.8         | 11.5                                 | 44.2                        |     |     |
|                                            | Visitors' berth, mooring ………………IU 2- | Well………………………………IG 94, IK   |     |     |
| Under construction, reclamation …IF 30-32  |                                      |                             |     |     |
|                                            | 3                                    | 43                          |     |     |
| Underwater installations ……………IL20-23      |                                      |                             |     |     |
Underwater rock……………………IK 11-15  Volcanic …………………………………IJ    Head………………………….IL
| Unmanned, unwatched light…………..IP 53  | 37  | 23  |     |     |
| ------------------------------------- | --- | --- | --- | --- |

Unsurveyed

Edition 1.0                                                                                                           November 2000                                                                       S-57 Appendix B.1 - Annex D

INT1 to S-57/52 for ENCs
Page 99 of 69