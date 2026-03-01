# S-57 / ISO 8211 Glossary

Key abbreviations and terminology used throughout the EncDotNet codebase, drawn from the IHO S-57 and ISO/IEC 8211 standards.

## File & Record Structure

| Term | Meaning |
|------|---------|
| **DDR** | Data Descriptive Record — the ISO 8211 schema record at the start of each file |
| **CATALOG.031** | The S-57 exchange set catalog file listing all chart files |
| **CATD** | Catalogue Directory record within CATALOG.031 |

## Data Set Fields

| Term | Meaning |
|------|---------|
| **DSID** | Data Set Identification field (dataset metadata: name, edition, agency) |
| **DSSI** | Data Set Structure Information |
| **DSPM** | Data Set Parameters field (coordinate systems, scales, multiplication factors) |
| **COMF** | Coordinate Multiplication Factor — divide raw integer coordinates by this to get decimal degrees |
| **SOMF** | Sounding Multiplication Factor — divide raw integer depths by this |
| **CSCL** | Compilation Scale (e.g., 22000 for a 1:22,000 chart) |

## Feature Records

| Term | Meaning |
|------|---------|
| **FRID** | Feature Record Identifier (contains PRIM, GRUP, OBJL) |
| **FOID** | Feature Object Identifier (contains FIDN, FIDS) |
| **OBJL** | Object Label/Code (the S57ObjectCode enum value, e.g., DEPARE=42, SOUNDG=129) |
| **PRIM** | Geometric Primitive (Point=1, Line=2, Area=3, None=255) |
| **ATTF/NATF** | Feature Record Attribute Field / National Attribute Field |
| **ATTL/ATVL** | Attribute Label (numeric code) / Attribute Value (string) |
| **FSPT** | Feature-to-Spatial Record Pointer (links features to vector geometry) |
| **FFPT** | Feature-to-Feature Pointer (links related features) |

## Vector/Spatial Records

| Term | Meaning |
|------|---------|
| **VRID** | Vector Record Identifier |
| **RCNM** | Record Name Code (110=Isolated Node, 120=Connected Node, 130=Edge, 140=Face) |
| **RCID** | Record Identification Number |
| **SG2D** | 2-D Coordinate field (lat/lon as integers) |
| **SG3D** | 3-D Coordinate (Sounding) field (lat/lon/depth as integers) |
| **VRPT** | Vector Record Pointer (edge-to-node topology) |
| **ORNT** | Orientation (Forward=1, Reverse=2) |
| **USAG** | Usage Indicator (Exterior=1, Interior=2) |
| **MASK** | Masking Indicator (Mask=1, Show=2) |
| **TOPI** | Topology Indicator (Beginning=1, End=2, LeftFace=3, RightFace=4) |

## Topology Model

S-57 uses **chain-node topology**:

- **Features** reference spatial records via FSPT pointers
- **Area features** reference **Faces**, which are bounded by **Edges**
- **Edges** are polylines connecting two **Connected Nodes**
- **Isolated Nodes** represent standalone points (or sounding clusters)
- Edge orientation and usage indicators determine interior vs. exterior boundaries

## Coordinate Handling

- Raw coordinates in S-57 files are **integers**; divide by **COMF** (typically 10,000,000) to get lat/lon in decimal degrees
- Soundings are stored in SG3D fields; divide depth by **SOMF** (typically 10)
