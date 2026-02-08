#!/usr/bin/env dotnet run

#:project ../src/EncDotNet.Enc/EncDotNet.Enc.csproj

using System.Text;
using EncDotNet.Enc;
using EncDotNet.Iso8211;

// ============================================================================
// DumpChart.cs — Dumps the full contents of an ENC chart file.
//
// Usage: dotnet run DumpChart.cs <path-to-.000-file>
//
// Part 1: ISO 8211 layer — every record, field, and raw field data (hex).
// Part 2: S-57 layer   — parsed DSID, DSPM, feature records, and vector records
//                         with all subfields decoded.
// ============================================================================

if (args.Length < 1)
{
    Console.WriteLine("Usage: dotnet run DumpChart.cs <path-to-.000-file>");
    return;
}

string filePath = args[0];

if (!File.Exists(filePath))
{
    Console.WriteLine($"Error: File not found: {filePath}");
    return;
}

Console.WriteLine($"File: {filePath}");
Console.WriteLine($"Size: {new FileInfo(filePath).Length} bytes");
Console.WriteLine();

// ============================================================================
// PART 1 — ISO 8211 Raw Dump
// ============================================================================

Console.WriteLine("╔══════════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                     PART 1: ISO 8211 RAW DUMP                       ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

var iso8211Doc = Iso8211Reader.ReadFromFile(filePath);

// Parse DDR for subfield-aware dumping in Part 2
Iso8211DataDescriptiveRecord? ddr = null;
if (iso8211Doc.DataDescriptiveRecord is not null)
{
    ddr = Iso8211DdrParser.Parse(iso8211Doc.DataDescriptiveRecord);
}

for (int r = 0; r < iso8211Doc.Records.Length; r++)
{
    var record = iso8211Doc.Records[r];
    var leader = record.Leader;

    string recordType = record.IsDataDescriptiveRecord ? "DDR (Data Descriptive Record)" : "DR  (Data Record)";

    Console.WriteLine($"┌─── Record {r} ─── {recordType} ───────────────────────────────");
    Console.WriteLine($"│  Record Length     : {leader.RecordLength}");
    Console.WriteLine($"│  Interchange Level : {leader.InterchangeLevel}");
    Console.WriteLine($"│  Leader Identifier : {leader.LeaderIdentifier}");
    Console.WriteLine($"│  Version Number    : {leader.VersionNumber}");
    Console.WriteLine($"│  App Indicator     : {leader.ApplicationIndicator}");
    Console.WriteLine($"│  Field Control Len : {leader.FieldControlLength}");
    Console.WriteLine($"│  Base Address      : {leader.BaseAddressOfFieldArea}");
    Console.WriteLine($"│  Ext Char Set      : {leader.ExtendedCharacterSetIndicator}");
    Console.WriteLine($"│  Entry Map         : tag={leader.SizeOfFieldTagField} len={leader.SizeOfFieldLengthField} pos={leader.SizeOfFieldPositionField}");
    Console.WriteLine($"│  Directory Entries : {record.Directory.Length}");
    Console.WriteLine($"│  Fields           : {record.Fields.Length}");
    Console.WriteLine("│");

    // Directory entries
    if (record.Directory.Length > 0)
    {
        Console.WriteLine("│  ┌─ Directory ────────────────────────────────────────");
        foreach (var entry in record.Directory)
        {
            Console.WriteLine($"│  │  Tag: {entry.Tag,-6}  Length: {entry.Length,6}  Position: {entry.Position,6}");
        }
        Console.WriteLine("│  └────────────────────────────────────────────────────");
        Console.WriteLine("│");
    }

    // Fields with hex dump
    for (int f = 0; f < record.Fields.Length; f++)
    {
        var field = record.Fields[f];
        Console.WriteLine($"│  ┌─ Field {f}: Tag = \"{field.Tag}\"  ({field.Data.Length} bytes) ──────");

        // Print ASCII interpretation (printable chars only)
        string ascii = field.GetDataString();
        if (ascii.Length > 0 && ascii.Length <= 200)
        {
            Console.WriteLine($"│  │  ASCII: {ascii}");
        }

        // Hex dump
        DumpHex(field.Data, "│  │  ");

        Console.WriteLine("│  └────────────────────────────────────────────────────");
    }

    Console.WriteLine("└───────────────────────────────────────────────────────────────────");
    Console.WriteLine();
}

// ============================================================================
// PART 2 — S-57 Parsed Dump
// ============================================================================

Console.WriteLine();
Console.WriteLine("╔══════════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                    PART 2: S-57 PARSED DUMP                         ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

var s57Doc = S57Reader.ReadFromFile(filePath);

// --- DDR Field Definitions ---
if (ddr is not null)
{
    Console.WriteLine("┌─── DDR Field Definitions ────────────────────────────────────────");
    foreach (var fieldDef in ddr.FieldDefinitions)
    {
        Console.WriteLine($"│  Tag: {fieldDef.Tag,-6}  Name: \"{fieldDef.FieldName}\"");
        Console.WriteLine($"│    DataStructure: {fieldDef.DataStructureCode}  DataType: {fieldDef.DataTypeCode}");
        Console.WriteLine($"│    Format Controls: {fieldDef.FormatControls}");
        Console.WriteLine($"│    Repeating Group: {(fieldDef.HasRepeatingGroup ? $"Yes (from index {fieldDef.RepeatingSubfieldStartIndex})" : "No")}");

        if (!fieldDef.SubfieldDefinitions.IsDefaultOrEmpty)
        {
            Console.WriteLine("│    Subfields:");
            foreach (var sf in fieldDef.SubfieldDefinitions)
            {
                string rep = sf.IsRepeating ? " [repeating]" : "";
                Console.WriteLine($"│      [{sf.Index}] {sf.Name,-6} Format: {sf.Format}{rep}");
            }
        }
        Console.WriteLine("│");
    }
    Console.WriteLine("└──────────────────────────────────────────────────────────────────");
    Console.WriteLine();
}

// --- DSID ---
if (s57Doc.DataSetIdentification is not null)
{
    var dsid = s57Doc.DataSetIdentification;
    Console.WriteLine("┌─── Data Set Identification (DSID) ───────────────────────────────");
    Console.WriteLine($"│  Record Name       : {dsid.RecordName}");
    Console.WriteLine($"│  Intended Usage    : {dsid.IntendedUsage}");
    Console.WriteLine($"│  Data Set Name     : {dsid.DataSetName}");
    Console.WriteLine($"│  Edition Number    : {dsid.EditionNumber}");
    Console.WriteLine($"│  Update Number     : {dsid.UpdateNumber}");
    Console.WriteLine($"│  Update App Date   : {dsid.UpdateApplicationDate}");
    Console.WriteLine($"│  Issue Date        : {dsid.IssueDate}");
    Console.WriteLine($"│  S-57 Edition      : {dsid.S57EditionNumber}");
    Console.WriteLine($"│  Producing Agency  : {dsid.ProducingAgency}");
    Console.WriteLine($"│  Data Structure    : {dsid.DataStructure}");
    Console.WriteLine($"│  ATTF Lexical Level: {dsid.AttfLexicalLevel}");
    Console.WriteLine($"│  NATF Lexical Level: {dsid.NatfLexicalLevel}");
    Console.WriteLine($"│  Comment           : {dsid.Comment}");
    Console.WriteLine("└──────────────────────────────────────────────────────────────────");
    Console.WriteLine();
}

// --- DSPM ---
if (s57Doc.DataSetParameters is not null)
{
    var dspm = s57Doc.DataSetParameters;
    Console.WriteLine("┌─── Data Set Parameters (DSPM) ───────────────────────────────────");
    Console.WriteLine($"│  Record Name       : {dspm.RecordName}");
    Console.WriteLine($"│  Horizontal Datum  : {dspm.HorizontalDatum}");
    Console.WriteLine($"│  Vertical Datum    : {dspm.VerticalDatum}");
    Console.WriteLine($"│  Sounding Datum    : {dspm.SoundingDatum}");
    Console.WriteLine($"│  Compilation Scale : 1:{dspm.CompilationScale}");
    Console.WriteLine($"│  Depth Units       : {dspm.DepthUnits}");
    Console.WriteLine($"│  Height Units      : {dspm.HeightUnits}");
    Console.WriteLine($"│  Positional Units  : {dspm.PositionalUnits}");
    Console.WriteLine($"│  Coordinate Units  : {dspm.CoordinateUnits}");
    Console.WriteLine($"│  Coord Mult Factor : {dspm.CoordinateMultiplicationFactor}");
    Console.WriteLine($"│  Sound Mult Factor : {dspm.SoundingMultiplicationFactor}");
    Console.WriteLine($"│  Comment           : {dspm.Comment}");
    Console.WriteLine("└──────────────────────────────────────────────────────────────────");
    Console.WriteLine();
}

// --- Feature Records ---
Console.WriteLine($"┌─── Feature Records ({s57Doc.FeatureRecords.Length} total) ─────────────────────────");
Console.WriteLine("│");

for (int i = 0; i < s57Doc.FeatureRecords.Length; i++)
{
    var feature = s57Doc.FeatureRecords[i];

    Console.WriteLine($"│  ┌─ Feature {i} ──────────────────────────────────────────");
    Console.WriteLine($"│  │  Record Name     : {feature.RecordName}");
    Console.WriteLine($"│  │  Primitive       : {feature.Primitive}");
    Console.WriteLine($"│  │  Group           : {feature.Group}");
    Console.WriteLine($"│  │  Object Code     : {feature.ObjectCode}");
    Console.WriteLine($"│  │  Record Version  : {feature.RecordVersion}");
    Console.WriteLine($"│  │  Update Instr    : {feature.UpdateInstruction}");

    // Attributes
    if (!feature.Attributes.IsDefaultOrEmpty && feature.Attributes.Length > 0)
    {
        Console.WriteLine("│  │  Attributes (ATTF):");
        foreach (var attr in feature.Attributes)
        {
            Console.WriteLine($"│  │    ATTL={attr.AttributeCode,5}  ATVL=\"{attr.Value}\"");
        }
    }

    // National Attributes
    if (!feature.NationalAttributes.IsDefaultOrEmpty && feature.NationalAttributes.Length > 0)
    {
        Console.WriteLine("│  │  National Attributes (NATF):");
        foreach (var attr in feature.NationalAttributes)
        {
            Console.WriteLine($"│  │    ATTL={attr.AttributeCode,5}  ATVL=\"{attr.Value}\"");
        }
    }

    // Spatial Pointers
    if (!feature.SpatialPointers.IsDefaultOrEmpty && feature.SpatialPointers.Length > 0)
    {
        Console.WriteLine("│  │  Spatial Pointers (FSPT):");
        foreach (var sp in feature.SpatialPointers)
        {
            Console.WriteLine($"│  │    Name: {sp.Name}  Ornt: {sp.Orientation}  Usag: {sp.Usage}  Mask: {sp.Mask}");
        }
    }

    // Feature Pointers
    if (!feature.FeaturePointers.IsDefaultOrEmpty && feature.FeaturePointers.Length > 0)
    {
        Console.WriteLine("│  │  Feature Pointers (FFPT):");
        foreach (var fp in feature.FeaturePointers)
        {
            Console.WriteLine($"│  │    Name: {fp.Name}  Rel: {fp.Relationship}  Comment: \"{fp.Comment}\"");
        }
    }

    Console.WriteLine("│  └────────────────────────────────────────────────────────");
}

Console.WriteLine("└──────────────────────────────────────────────────────────────────");
Console.WriteLine();

// --- Vector Records ---
int comf = s57Doc.CoordinateMultiplicationFactor;
int somf = s57Doc.SoundingMultiplicationFactor;

Console.WriteLine($"┌─── Vector Records ({s57Doc.VectorRecords.Length} total) ──────────────────────────");
Console.WriteLine("│");

for (int i = 0; i < s57Doc.VectorRecords.Length; i++)
{
    var vector = s57Doc.VectorRecords[i];

    string vectorType = vector.RecordName.RecordNameCode switch
    {
        110 => "Isolated Node",
        120 => "Connected Node",
        130 => "Edge",
        140 => "Face",
        _ => $"Unknown ({vector.RecordName.RecordNameCode})"
    };

    Console.WriteLine($"│  ┌─ Vector {i}: {vectorType} ───────────────────────────────");
    Console.WriteLine($"│  │  Record Name     : {vector.RecordName}");
    Console.WriteLine($"│  │  Record Version  : {vector.RecordVersion}");
    Console.WriteLine($"│  │  Update Instr    : {vector.UpdateInstruction}");

    // Attributes
    if (!vector.Attributes.IsDefaultOrEmpty && vector.Attributes.Length > 0)
    {
        Console.WriteLine("│  │  Attributes (ATTV):");
        foreach (var attr in vector.Attributes)
        {
            Console.WriteLine($"│  │    ATTL={attr.AttributeCode,5}  ATVL=\"{attr.Value}\"");
        }
    }

    // Vector Pointers
    if (!vector.VectorPointers.IsDefaultOrEmpty && vector.VectorPointers.Length > 0)
    {
        Console.WriteLine("│  │  Vector Pointers (VRPT):");
        foreach (var vp in vector.VectorPointers)
        {
            Console.WriteLine($"│  │    Name: {vp.Name}  Ornt: {vp.Orientation}  Usag: {vp.Usage}  Topi: {vp.Topology}  Mask: {vp.Mask}");
        }
    }

    // 2D Coordinates
    if (!vector.Coordinates2D.IsDefaultOrEmpty && vector.Coordinates2D.Length > 0)
    {
        Console.WriteLine($"│  │  2D Coordinates (SG2D): {vector.Coordinates2D.Length} point(s)");
        int maxCoords = Math.Min(vector.Coordinates2D.Length, 20);
        for (int c = 0; c < maxCoords; c++)
        {
            var coord = vector.Coordinates2D[c];
            var (lon, lat) = coord.ToDecimalDegrees(comf);
            Console.WriteLine($"│  │    [{c,4}] Y={coord.Y,12} X={coord.X,12}  ({lat:F7}°, {lon:F7}°)");
        }
        if (vector.Coordinates2D.Length > maxCoords)
        {
            Console.WriteLine($"│  │    ... and {vector.Coordinates2D.Length - maxCoords} more point(s)");
        }
    }

    // 3D Soundings
    if (!vector.Soundings.IsDefaultOrEmpty && vector.Soundings.Length > 0)
    {
        Console.WriteLine($"│  │  3D Soundings (SG3D): {vector.Soundings.Length} sounding(s)");
        int maxSoundings = Math.Min(vector.Soundings.Length, 20);
        for (int s = 0; s < maxSoundings; s++)
        {
            var snd = vector.Soundings[s];
            var (lon, lat, depth) = snd.ToDecimalValues(comf, somf);
            Console.WriteLine($"│  │    [{s,4}] Y={snd.Y,12} X={snd.X,12} D={snd.Depth,8}  ({lat:F7}°, {lon:F7}°, {depth:F1}m)");
        }
        if (vector.Soundings.Length > maxSoundings)
        {
            Console.WriteLine($"│  │    ... and {vector.Soundings.Length - maxSoundings} more sounding(s)");
        }
    }

    Console.WriteLine("│  └────────────────────────────────────────────────────────");
}

Console.WriteLine("└──────────────────────────────────────────────────────────────────");
Console.WriteLine();

// --- Summary ---
Console.WriteLine("╔══════════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                           SUMMARY                                   ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════════╝");
Console.WriteLine($"  ISO 8211 Records   : {iso8211Doc.Records.Length}");
Console.WriteLine($"  DDR Fields         : {(ddr is not null ? ddr.FieldDefinitions.Length : 0)}");
Console.WriteLine($"  Feature Records    : {s57Doc.FeatureRecords.Length}");
Console.WriteLine($"  Vector Records     : {s57Doc.VectorRecords.Length}");
Console.WriteLine($"  Coord Mult Factor  : {comf}");
Console.WriteLine($"  Sound Mult Factor  : {somf}");
Console.WriteLine();

// ============================================================================
// Helper: Hex dump
// ============================================================================

static void DumpHex(byte[] data, string linePrefix)
{
    const int bytesPerLine = 16;
    int maxBytes = Math.Min(data.Length, 256);

    for (int offset = 0; offset < maxBytes; offset += bytesPerLine)
    {
        int count = Math.Min(bytesPerLine, maxBytes - offset);
        var sb = new StringBuilder();
        sb.Append(linePrefix);
        sb.Append($"{offset:X4}: ");

        // Hex part
        for (int i = 0; i < bytesPerLine; i++)
        {
            if (i < count)
            {
                sb.Append($"{data[offset + i]:X2} ");
            }
            else
            {
                sb.Append("   ");
            }
            if (i == 7) sb.Append(' ');
        }

        sb.Append(' ');

        // ASCII part
        for (int i = 0; i < count; i++)
        {
            byte b = data[offset + i];
            sb.Append(b is >= 0x20 and < 0x7F ? (char)b : '.');
        }

        Console.WriteLine(sb.ToString());
    }

    if (data.Length > maxBytes)
    {
        Console.WriteLine($"{linePrefix}... ({data.Length - maxBytes} more bytes)");
    }
}
