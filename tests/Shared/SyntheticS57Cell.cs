using EncDotNet.Iso8211;

namespace EncDotNet.Testing;

/// <summary>
/// Generates deterministic, S-57-shaped ISO 8211 cells for tests and benchmarks.
/// </summary>
/// <remarks>
/// <para>
/// The repository never commits real ENC cells, so performance tests that need exact,
/// repeatable inputs use these instead. The output depends only on the
/// <see cref="SyntheticCellProfile"/>: the generator uses its own pseudo-random sequence
/// rather than <see cref="Random"/>, so the bytes are identical on every runtime and OS.
/// </para>
/// <para>
/// Records follow the S-57 Part 3 structure (DSID/DSSI, DSPM, vector records with
/// VRID/ATTV/VRPT/SG2D/SG3D, feature records with FRID/FOID/ATTF/FSPT) and are encoded with
/// the library's own <see cref="Iso8211FieldBuilder"/> and <see cref="Iso8211DocumentWriter"/>.
/// The content is plausible but not a valid chart: geometry is a random walk and attribute
/// values are random text.
/// </para>
/// </remarks>
public static class SyntheticS57Cell
{
    private const int FeatureRcnm = 100;
    private const int IsolatedNodeRcnm = 110;
    private const int ConnectedNodeRcnm = 120;
    private const int EdgeRcnm = 130;
    private const int ProducingAgency = 550;

    private const byte Insert = 1;
    private const byte Delete = 2;
    private const byte Modify = 3;

    // Common S-57 object classes per primitive (S-57 Appendix A, Chapter 1).
    private static readonly int[] PointObjectClasses = [17, 14, 75, 159, 153, 86, 90];
    private static readonly int[] LineObjectClasses = [30, 43, 122, 69, 109];
    private static readonly int[] AreaObjectClasses = [42, 71, 46, 4, 112, 27];

    // Common attribute codes (S-57 Appendix A, Chapter 2): OBJNAM, DRVAL1, DRVAL2, COLOUR,
    // CATLAM, VALSOU, SCAMIN, SORDAT, SORIND, NOBJNM, INFORM, QUASOU.
    private static readonly int[] AttributeCodes = [116, 87, 88, 75, 36, 179, 133, 147, 148, 301, 102, 125];

    private static readonly Lazy<Iso8211Record> LazyDdr = new(BuildDdr);
    private static readonly Lazy<Iso8211DataDescriptiveRecord> LazyDefinitions =
        new(() => Iso8211DataDescriptiveRecordReader.Read(LazyDdr.Value));

    /// <summary>
    /// Generates the base cell (<c>.000</c>) for a profile.
    /// </summary>
    /// <param name="profile">The cell shape to generate.</param>
    /// <returns>The encoded ISO 8211 bytes.</returns>
    public static byte[] Create(SyntheticCellProfile profile) =>
        Iso8211DocumentWriter.Write(CreateDocument(profile));

    /// <summary>
    /// Generates the base cell (<c>.000</c>) for a profile as an ISO 8211 document model.
    /// </summary>
    /// <param name="profile">The cell shape to generate.</param>
    /// <returns>The document, ready for <see cref="Iso8211DocumentWriter"/>.</returns>
    public static Iso8211Document CreateDocument(SyntheticCellProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var defs = LazyDefinitions.Value;
        var rng = new SplitMix64(profile.Seed);
        var builder = new Iso8211DocumentBuilder().AddRecord(LazyDdr.Value);
        var recordNumber = 1;

        builder.AddRecord(DataSetIdentification(defs, profile, recordNumber++, updateNumber: 0));
        builder.AddRecord(DataSetParameters(defs, recordNumber++));

        var walk = new CoordinateWalk(new SplitMix64((long)rng.NextUInt64()));

        for (int i = 1; i <= profile.IsolatedNodes; i++)
        {
            builder.AddRecord(Node(defs, recordNumber++, IsolatedNodeRcnm, i, walk.Next()));
        }

        for (int i = 1; i <= profile.SoundingNodes; i++)
        {
            builder.AddRecord(SoundingNode(defs, ref rng, recordNumber++, profile.IsolatedNodes + i, profile.SoundingsPerNode, walk));
        }

        for (int i = 1; i <= profile.ConnectedNodes; i++)
        {
            builder.AddRecord(Node(defs, recordNumber++, ConnectedNodeRcnm, i, walk.Next()));
        }

        for (int i = 1; i <= profile.Edges; i++)
        {
            builder.AddRecord(Edge(defs, ref rng, recordNumber++, i, EdgePointCount(profile, i), profile.ConnectedNodes, walk, Insert, ruinModifyRange: null));
        }

        var featureId = 1;
        for (int i = 0; i < profile.PointFeatures; i++)
        {
            builder.AddRecord(Feature(defs, ref rng, recordNumber++, featureId++, profile, primitive: 1, Insert));
        }

        for (int i = 0; i < profile.LineFeatures; i++)
        {
            builder.AddRecord(Feature(defs, ref rng, recordNumber++, featureId++, profile, primitive: 2, Insert));
        }

        for (int i = 0; i < profile.AreaFeatures; i++)
        {
            builder.AddRecord(Feature(defs, ref rng, recordNumber++, featureId++, profile, primitive: 3, Insert));
        }

        return builder.Build();
    }

    /// <summary>
    /// Generates an update cell (<c>.001</c>, <c>.002</c>, …) for a profile's base cell.
    /// </summary>
    /// <remarks>
    /// Each update modifies the attributes of about 5% of features, deletes about 1%,
    /// inserts about 1% new features, and replaces a run of coordinates in about 2% of
    /// edges using an SGCC update control field (S-57 Part 3, 8.4.3).
    /// </remarks>
    /// <param name="profile">The profile of the base cell being updated.</param>
    /// <param name="updateNumber">The update number, starting at 1.</param>
    /// <returns>The encoded ISO 8211 bytes.</returns>
    public static byte[] CreateUpdate(SyntheticCellProfile profile, int updateNumber)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentOutOfRangeException.ThrowIfLessThan(updateNumber, 1);

        var defs = LazyDefinitions.Value;
        var rng = new SplitMix64(profile.Seed * 1000L + updateNumber);
        var builder = new Iso8211DocumentBuilder().AddRecord(LazyDdr.Value);
        var recordNumber = 1;
        builder.AddRecord(DataSetIdentification(defs, profile, recordNumber++, updateNumber));

        var walk = new CoordinateWalk(new SplitMix64((long)rng.NextUInt64()));
        var edgeModifications = Math.Max(1, profile.Edges / 50);
        for (int i = 0; i < edgeModifications; i++)
        {
            // Replace a run of existing points, so the SGCC range always lies within the edge.
            var rcid = 1 + (int)rng.NextUInt32((uint)profile.Edges);
            var basePoints = EdgePointCount(profile, rcid);
            if (basePoints == 0)
            {
                continue;
            }

            var count = 1 + (int)rng.NextUInt32((uint)Math.Min(basePoints, 8));
            var index = 1 + (int)rng.NextUInt32((uint)(basePoints - count + 1));
            builder.AddRecord(Edge(defs, ref rng, recordNumber++, rcid, count, profile.ConnectedNodes, walk, Modify, ruinModifyRange: (index, count)));
        }

        var featureCount = profile.PointFeatures + profile.LineFeatures + profile.AreaFeatures;
        var modifications = Math.Max(1, featureCount / 20);
        for (int i = 0; i < modifications; i++)
        {
            var rcid = 1 + (int)rng.NextUInt32((uint)featureCount);
            builder.AddRecord(FeatureAttributeUpdate(defs, ref rng, recordNumber++, rcid));
        }

        var deletions = Math.Max(1, featureCount / 100);
        for (int i = 0; i < deletions; i++)
        {
            var rcid = 1 + (int)rng.NextUInt32((uint)featureCount);
            builder.AddRecord(FeatureDeletion(defs, recordNumber++, rcid));
        }

        var insertions = Math.Max(1, featureCount / 100);
        for (int i = 0; i < insertions; i++)
        {
            var rcid = featureCount + (updateNumber * insertions) + i + 1;
            builder.AddRecord(Feature(defs, ref rng, recordNumber++, rcid, profile, primitive: 1 + (int)rng.NextUInt32(3), Insert));
        }

        return Iso8211DocumentWriter.Write(builder.Build());
    }

    private static Iso8211Record DataSetIdentification(Iso8211DataDescriptiveRecord defs, SyntheticCellProfile profile, int recordNumber, int updateNumber)
    {
        var name = $"SY{profile.Name.ToUpperInvariant()[..Math.Min(6, profile.Name.Length)]}.{updateNumber:D3}";
        return Record(recordNumber)
            .AddField(Field(defs, "DSID").AddSubfields(
                10, 1, updateNumber == 0 ? 1 : 2, 5, name, "1", updateNumber.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "20260101", "20260101", "03.1", 1, "2.0", "2.0", 1, ProducingAgency, string.Empty))
            .AddField(Field(defs, "DSSI").AddSubfields(
                2, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0))
            .Build();
    }

    private static Iso8211Record DataSetParameters(Iso8211DataDescriptiveRecord defs, int recordNumber) =>
        Record(recordNumber)
            .AddField(Field(defs, "DSPM").AddSubfields(
                20, 1, 2, 17, 23, 20_000, 1, 1, 1, 1, 10_000_000, 10, string.Empty))
            .Build();

    private static Iso8211Record Node(Iso8211DataDescriptiveRecord defs, int recordNumber, int rcnm, int rcid, (int Y, int X) position) =>
        Record(recordNumber)
            .AddField(Field(defs, "VRID").AddSubfields(rcnm, rcid, 1, Insert))
            .AddField(Field(defs, "SG2D").AddSubfields(position.Y, position.X))
            .Build();

    private static Iso8211Record SoundingNode(Iso8211DataDescriptiveRecord defs, ref SplitMix64 rng, int recordNumber, int rcid, int soundings, CoordinateWalk walk)
    {
        var sg3d = Field(defs, "SG3D");
        for (int i = 0; i < soundings; i++)
        {
            var (y, x) = walk.Next();
            sg3d.AddSubfields(y, x, (int)rng.NextUInt32(400));
        }

        return Record(recordNumber)
            .AddField(Field(defs, "VRID").AddSubfields(IsolatedNodeRcnm, rcid, 1, Insert))
            .AddField(sg3d)
            .Build();
    }

    private static Iso8211Record Edge(
        Iso8211DataDescriptiveRecord defs,
        ref SplitMix64 rng,
        int recordNumber,
        int rcid,
        int points,
        int connectedNodes,
        CoordinateWalk walk,
        byte ruin,
        (int Index, int Count)? ruinModifyRange)
    {
        var record = Record(recordNumber)
            .AddField(Field(defs, "VRID").AddSubfields(EdgeRcnm, rcid, ruin == Insert ? 1 : 2, ruin));

        if (ruin == Insert)
        {
            if (rng.NextUInt32(10) == 0)
            {
                // QUAPOS (402) on about 10% of edges, as in real cells.
                record.AddField(Field(defs, "ATTV").AddSubfields(402, "4"));
            }

            var begin = 1 + (int)rng.NextUInt32((uint)connectedNodes);
            var end = 1 + (int)rng.NextUInt32((uint)connectedNodes);
            record.AddField(Field(defs, "VRPT").AddSubfields(
                Name(ConnectedNodeRcnm, begin), 255, 255, 1, 255,
                Name(ConnectedNodeRcnm, end), 255, 255, 2, 255));
        }

        if (ruinModifyRange is { } range)
        {
            record.AddField(Field(defs, "SGCC").AddSubfields(Modify, range.Index, range.Count));
        }

        if (points > 0)
        {
            var sg2d = Field(defs, "SG2D");
            for (int i = 0; i < points; i++)
            {
                var (y, x) = walk.Next();
                sg2d.AddSubfields(y, x);
            }

            record.AddField(sg2d);
        }

        return record.Build();
    }

    private static Iso8211Record Feature(Iso8211DataDescriptiveRecord defs, ref SplitMix64 rng, int recordNumber, int rcid, SyntheticCellProfile profile, int primitive, byte ruin)
    {
        var objectClasses = primitive switch
        {
            1 => PointObjectClasses,
            2 => LineObjectClasses,
            _ => AreaObjectClasses
        };
        var objl = objectClasses[rng.NextUInt32((uint)objectClasses.Length)];
        var group = primitive == 3 && objl is 42 or 71 ? 1 : 2;

        var record = Record(recordNumber)
            .AddField(Field(defs, "FRID").AddSubfields(FeatureRcnm, rcid, primitive, group, objl, 1, ruin))
            .AddField(Field(defs, "FOID").AddSubfields(ProducingAgency, rcid, 1));

        // 1-7 attributes, mean about 3.7, values averaging about 9 characters.
        var attributeCount = 1 + (int)rng.NextUInt32(4) + (int)rng.NextUInt32(4);
        record.AddField(Attributes(defs, ref rng, attributeCount));

        var fspt = Field(defs, "FSPT");
        if (primitive == 1)
        {
            var node = 1 + (int)rng.NextUInt32((uint)(profile.IsolatedNodes + profile.SoundingNodes));
            fspt.AddSubfields(Name(IsolatedNodeRcnm, node), 255, 255, 255);
        }
        else
        {
            var mean = primitive == 2 ? 3.0 : 15.0;
            var edges = Math.Clamp(1 + (int)(-mean * Math.Log(1 - rng.NextDouble())), 1, profile.Edges);
            for (int i = 0; i < edges; i++)
            {
                var edge = 1 + (int)rng.NextUInt32((uint)profile.Edges);
                fspt.AddSubfields(Name(EdgeRcnm, edge), 1 + (int)rng.NextUInt32(2), 1, 2);
            }
        }

        record.AddField(fspt);
        return record.Build();
    }

    private static Iso8211Record FeatureAttributeUpdate(Iso8211DataDescriptiveRecord defs, ref SplitMix64 rng, int recordNumber, int rcid) =>
        Record(recordNumber)
            .AddField(Field(defs, "FRID").AddSubfields(FeatureRcnm, rcid, 255, 255, 0, 2, Modify))
            .AddField(Field(defs, "FOID").AddSubfields(ProducingAgency, rcid, 1))
            .AddField(Attributes(defs, ref rng, 1 + (int)rng.NextUInt32(2)))
            .Build();

    private static Iso8211Record FeatureDeletion(Iso8211DataDescriptiveRecord defs, int recordNumber, int rcid) =>
        Record(recordNumber)
            .AddField(Field(defs, "FRID").AddSubfields(FeatureRcnm, rcid, 255, 255, 0, 2, Delete))
            .AddField(Field(defs, "FOID").AddSubfields(ProducingAgency, rcid, 1))
            .Build();

    private static Iso8211FieldBuilder Attributes(Iso8211DataDescriptiveRecord defs, ref SplitMix64 rng, int count)
    {
        var attf = Field(defs, "ATTF");
        for (int i = 0; i < count; i++)
        {
            var code = AttributeCodes[rng.NextUInt32((uint)AttributeCodes.Length)];
            attf.AddSubfields(code, RandomText(ref rng, 1 + (int)rng.NextUInt32(16)));
        }

        return attf;
    }

    private static string RandomText(ref SplitMix64 rng, int length)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .";
        return string.Create(length, rng.NextUInt64(), static (span, seed) =>
        {
            var local = new SplitMix64((long)seed);
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = alphabet[(int)local.NextUInt32((uint)alphabet.Length)];
            }
        });
    }

    /// <summary>
    /// Gets the number of intermediate points in an edge. Edge 1 is the profile's longest; the
    /// rest follow an exponential distribution, which matches the long tail of real coastline
    /// edges. The count depends only on the profile and record ID, so updates can target
    /// coordinate ranges that exist in the base cell.
    /// </summary>
    private static int EdgePointCount(SyntheticCellProfile profile, int rcid)
    {
        if (rcid == 1)
        {
            return profile.MaxEdgePoints;
        }

        var rng = new SplitMix64(((long)profile.Seed << 32) ^ rcid);
        return Math.Min(profile.MaxEdgePoints, (int)(-profile.MeanEdgePoints * Math.Log(1 - rng.NextDouble())));
    }

    // NAME (b15) packs RCNM (b11) and RCID (b14) little-endian (S-57 Part 3, 2.2).
    private static long Name(int rcnm, int rcid) => (long)(((ulong)(uint)rcid << 8) | (byte)rcnm);

    private static Iso8211RecordBuilder Record(int recordNumber) =>
        new Iso8211RecordBuilder().AddField("0001", BitConverter.GetBytes((ushort)recordNumber));

    private static Iso8211FieldBuilder Field(Iso8211DataDescriptiveRecord defs, string tag) =>
        new(defs.GetFieldDefinition(tag) ?? throw new InvalidOperationException($"No DDR definition for {tag}."));

    private static Iso8211Record BuildDdr()
    {
        // S-57 Part 3, Annex A: field definitions for the data structures used above.
        var definitions = new[]
        {
            Definition("0001", "ISO/IEC 8211 Record Identifier", Iso8211DataStructureCode.Elementary, Iso8211DataTypeCode.Binary, [], -1, "(b12)"),
            Definition("DSID", "Data set identification field", Iso8211DataStructureCode.Vector, Iso8211DataTypeCode.MixedDataTypes,
                ["RCNM", "RCID", "EXPP", "INTU", "DSNM", "EDTN", "UPDN", "UADT", "ISDT", "STED", "PRSP", "PSDN", "PRED", "PROF", "AGEN", "COMT"], -1,
                "(b11,b14,2b11,3A,2A(8),R(4),b11,2A,b11,b12,A)"),
            Definition("DSSI", "Data set structure information field", Iso8211DataStructureCode.Vector, Iso8211DataTypeCode.MixedDataTypes,
                ["DSTR", "AALL", "NALL", "NOMR", "NOCR", "NOGR", "NOLR", "NOIN", "NOCN", "NOED", "NOFA"], -1,
                "(3b11,8b14)"),
            Definition("DSPM", "Data set parameter field", Iso8211DataStructureCode.Vector, Iso8211DataTypeCode.MixedDataTypes,
                ["RCNM", "RCID", "HDAT", "VDAT", "SDAT", "CSCL", "DUNI", "HUNI", "PUNI", "COUN", "COMF", "SOMF", "COMT"], -1,
                "(b11,b14,3b11,b14,4b11,2b14,A)"),
            Definition("VRID", "Vector record identifier field", Iso8211DataStructureCode.Vector, Iso8211DataTypeCode.MixedDataTypes,
                ["RCNM", "RCID", "RVER", "RUIN"], -1, "(b11,b14,b12,b11)"),
            Definition("ATTV", "Vector record attribute field", Iso8211DataStructureCode.Array, Iso8211DataTypeCode.MixedDataTypes,
                ["ATTL", "ATVL"], 0, "(b12,A)"),
            Definition("VRPC", "Vector record pointer control field", Iso8211DataStructureCode.Vector, Iso8211DataTypeCode.MixedDataTypes,
                ["VPUI", "VPIX", "NVPT"], -1, "(b11,2b12)"),
            Definition("VRPT", "Vector record pointer field", Iso8211DataStructureCode.Array, Iso8211DataTypeCode.MixedDataTypes,
                ["NAME", "ORNT", "USAG", "TOPI", "MASK"], 0, "(B(40),4b11)"),
            Definition("SGCC", "Coordinate control field", Iso8211DataStructureCode.Vector, Iso8211DataTypeCode.MixedDataTypes,
                ["CCUI", "CCIX", "CCNC"], -1, "(b11,2b12)"),
            Definition("SG2D", "2-D coordinate field", Iso8211DataStructureCode.Array, Iso8211DataTypeCode.MixedDataTypes,
                ["YCOO", "XCOO"], 0, "(2b24)"),
            Definition("SG3D", "3-D coordinate (sounding array) field", Iso8211DataStructureCode.Array, Iso8211DataTypeCode.MixedDataTypes,
                ["YCOO", "XCOO", "VE3D"], 0, "(3b24)"),
            Definition("FRID", "Feature record identifier field", Iso8211DataStructureCode.Vector, Iso8211DataTypeCode.MixedDataTypes,
                ["RCNM", "RCID", "PRIM", "GRUP", "OBJL", "RVER", "RUIN"], -1, "(b11,b14,2b11,2b12,b11)"),
            Definition("FOID", "Feature object identifier field", Iso8211DataStructureCode.Vector, Iso8211DataTypeCode.MixedDataTypes,
                ["AGEN", "FIDN", "FIDS"], -1, "(b12,b14,b12)"),
            Definition("ATTF", "Feature record attribute field", Iso8211DataStructureCode.Array, Iso8211DataTypeCode.MixedDataTypes,
                ["ATTL", "ATVL"], 0, "(b12,A)"),
            Definition("NATF", "Feature record national attribute field", Iso8211DataStructureCode.Array, Iso8211DataTypeCode.MixedDataTypes,
                ["ATTL", "ATVL"], 0, "(b12,A)"),
            Definition("FFPC", "Feature record to feature object pointer control field", Iso8211DataStructureCode.Vector, Iso8211DataTypeCode.MixedDataTypes,
                ["FFUI", "FFIX", "NFPT"], -1, "(b11,2b12)"),
            Definition("FFPT", "Feature record to feature object pointer field", Iso8211DataStructureCode.Array, Iso8211DataTypeCode.MixedDataTypes,
                ["LNAM", "RIND", "COMT"], 0, "(B(64),b11,A)"),
            Definition("FSPC", "Feature record to spatial record pointer control field", Iso8211DataStructureCode.Vector, Iso8211DataTypeCode.MixedDataTypes,
                ["FSUI", "FSIX", "NSPT"], -1, "(b11,2b12)"),
            Definition("FSPT", "Feature record to spatial record pointer field", Iso8211DataStructureCode.Array, Iso8211DataTypeCode.MixedDataTypes,
                ["NAME", "ORNT", "USAG", "MASK"], 0, "(B(40),3b11)"),
        };

        return Iso8211DataDescriptiveRecordWriter.BuildDdr(definitions, fieldControlLength: 9);
    }

    private static Iso8211FieldDefinition Definition(
        string tag,
        string name,
        Iso8211DataStructureCode structure,
        Iso8211DataTypeCode type,
        string[] labels,
        int repeatingStart,
        string formats) => new()
        {
            Tag = tag,
            FieldName = name,
            DataStructureCode = structure,
            DataTypeCode = type,
            FormatControls = formats,
            RepeatingSubfieldStartIndex = repeatingStart,
            SubfieldDefinitions = [.. labels.Select((label, index) => new Iso8211SubfieldDefinition { Name = label, Index = index })]
        };

    /// <summary>
    /// A random walk of positions near 47.6°N 122.3°W in COMF 10<sup>7</sup> units.
    /// </summary>
    private sealed class CoordinateWalk(SplitMix64 rng)
    {
        private SplitMix64 _rng = rng;
        private int _y = 476_000_000;
        private int _x = -1_223_000_000;

        public (int Y, int X) Next()
        {
            _y += (int)_rng.NextUInt32(4001) - 2000;
            _x += (int)_rng.NextUInt32(4001) - 2000;
            return (_y, _x);
        }
    }

    /// <summary>
    /// SplitMix64: a small, fully specified generator whose sequence never changes across
    /// .NET versions, unlike <see cref="Random"/>.
    /// </summary>
    private struct SplitMix64(long seed)
    {
        private ulong _state = unchecked((ulong)seed);

        public ulong NextUInt64()
        {
            var z = _state += 0x9E3779B97F4A7C15UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        public uint NextUInt32(uint exclusiveMax) => (uint)(NextUInt64() % exclusiveMax);

        public double NextDouble() => (NextUInt64() >> 11) * (1.0 / (1UL << 53));
    }
}
