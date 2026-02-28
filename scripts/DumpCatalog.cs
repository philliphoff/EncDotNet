#!/usr/bin/env dotnet run

#:package Microsoft.Extensions.Logging@9.0.0
#:package Microsoft.Extensions.Logging.Console@9.0.0

#:project ../src/EncDotNet.S57/EncDotNet.S57.csproj

using System.Text;
using EncDotNet.Enc.Catalogs;
using EncDotNet.Iso8211;
using Microsoft.Extensions.Logging;

// ============================================================================
// DumpCatalog.cs — Dumps the full contents of an ENC CATALOG.031 file.
//
// Usage: dotnet run DumpCatalog.cs <path-to-CATALOG.031>
//
// Part 1: ISO 8211 layer — every record, field, and raw field data (hex).
// Part 2: S-57 layer   — parsed catalog entries with all subfields decoded.
// ============================================================================

if (args.Length < 1)
{
    Console.WriteLine("Usage: dotnet run DumpCatalog.cs <path-to-CATALOG.031>");
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

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger("DumpCatalog");

// ============================================================================
// PART 1 — ISO 8211 Raw Dump
// ============================================================================

Console.WriteLine("╔══════════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                     PART 1: ISO 8211 RAW DUMP                       ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

var iso8211Doc = Iso8211DocumentReader.ReadFromFile(filePath);

// Parse DDR for subfield-aware dumping in Part 2
Iso8211DataDescriptiveRecord? ddr = null;
if (iso8211Doc.DataDescriptiveRecord is not null)
{
    ddr = Iso8211DataDescriptiveRecordReader.Read(iso8211Doc.DataDescriptiveRecord);
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
// PART 2 — S-57 Catalog Parsed Dump
// ============================================================================

Console.WriteLine();
Console.WriteLine("╔══════════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║                  PART 2: S-57 CATALOG PARSED DUMP                   ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

var catalog = S57CatalogReader.ReadFromFile(filePath, logger);

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

// --- Catalog Entries ---
Console.WriteLine($"┌─── Catalog Entries ({catalog.Entries.Length} total) ──────────────────────────");
Console.WriteLine("│");

for (int i = 0; i < catalog.Entries.Length; i++)
{
    var entry = catalog.Entries[i];

    Console.WriteLine($"│  ┌─ Entry {i} ──────────────────────────────────────────");
    Console.WriteLine($"│  │  Record Name     : {entry.RecordName} (RCID={entry.RecordId})");
    Console.WriteLine($"│  │  File Name       : {entry.FileName}");
    Console.WriteLine($"│  │  Long File Name  : {entry.LongFileName}");
    Console.WriteLine($"│  │  Volume          : {entry.Volume}");
    Console.WriteLine($"│  │  Implementation  : {entry.Implementation}");

    if (entry.SouthernmostLatitude.HasValue || entry.WesternmostLongitude.HasValue ||
        entry.NorthernmostLatitude.HasValue || entry.EasternmostLongitude.HasValue)
    {
        Console.WriteLine($"│  │  South Latitude  : {entry.SouthernmostLatitude?.ToString("F7") ?? "(none)"}");
        Console.WriteLine($"│  │  West Longitude  : {entry.WesternmostLongitude?.ToString("F7") ?? "(none)"}");
        Console.WriteLine($"│  │  North Latitude  : {entry.NorthernmostLatitude?.ToString("F7") ?? "(none)"}");
        Console.WriteLine($"│  │  East Longitude  : {entry.EasternmostLongitude?.ToString("F7") ?? "(none)"}");
    }

    if (!string.IsNullOrEmpty(entry.CrcChecksum))
    {
        Console.WriteLine($"│  │  CRC Checksum    : {entry.CrcChecksum}");
    }

    if (!string.IsNullOrEmpty(entry.Comment))
    {
        Console.WriteLine($"│  │  Comment         : {entry.Comment}");
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
Console.WriteLine($"  Catalog Entries    : {catalog.Entries.Length}");
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
