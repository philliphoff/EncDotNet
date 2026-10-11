using System.Globalization;
using System.Text;
using EncDotNet.Iso8211;
using EncDotNet.Testing;
using EncDotNet.UnitTests.Reference;

namespace EncDotNet.UnitTests;

/// <summary>
/// Checks that <see cref="Iso8211FieldReader"/> returns exactly what it did before its
/// performance rewrite, by comparing it with <see cref="LegacyIso8211FieldReader"/>, a frozen
/// copy of the earlier implementation, on real-shaped, truncated and hand-built fields.
/// </summary>
public class Iso8211FieldReaderEquivalenceTests
{
    private const byte UnitTerminator = 0x1F;
    private const byte FieldTerminator = 0x1E;

    [Fact]
    public void EveryFieldOfTheSyntheticCell_ReadsTheSame()
    {
        // The larger profiles have the same field shapes, only more of them.
        var profile = SyntheticCellProfile.Harbour;
        AssertCellReadsTheSame(SyntheticS57Cell.Create(profile), truncate: false);
        AssertCellReadsTheSame(SyntheticS57Cell.CreateUpdate(profile, 1), truncate: false);
    }

    [Fact]
    public void TruncatedFieldsOfTheSyntheticCell_ReadTheSame()
    {
        // Every prefix of the data, with and without a trailing field terminator, exercises
        // partial subfields, partial groups and the terminator checks.
        AssertCellReadsTheSame(SyntheticS57Cell.Create(SyntheticCellProfile.Harbour), truncate: true);
    }

    [Fact]
    public void EveryFieldOfRealCells_ReadsTheSame()
    {
        // Real cells aren't committed. Point ENCDOTNET_ENC_ROOT at a directory of them, as for
        // the benchmarks, to include them.
        var root = Environment.GetEnvironmentVariable("ENCDOTNET_ENC_ROOT");
        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
        {
            return;
        }

        foreach (var path in Directory.EnumerateFiles(root, "*.0??", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(path)[1..].All(char.IsAsciiDigit))
            {
                AssertCellReadsTheSame(File.ReadAllBytes(path), truncate: false);
            }
        }
    }

    public static TheoryData<string, string, int, string> HandBuiltLayouts => new()
    {
        // Name, descriptors ('*' starts the repeating group), repeating start, formats.
        { "all fixed, no group", "RCNM!RCID!PRIM!OBJL", -1, "b11,b14,b11,b12" },
        { "fixed group", "YCOO!XCOO", 0, "b24,b24" },
        { "fixed prefix then fixed group (S-101 C3IL)", "VCID!YCOO!XCOO!ZCOO", 1, "b11,b24,b24,b24" },
        { "variable group", "ATTL!ATVL", 0, "b12,A" },
        { "fixed prefix then variable group", "RCNM!ATTL!ATVL", 1, "b11,b12,A" },
        { "variable, no group", "DSNM!EDTN!UPDN", -1, "A,A,A" },
        { "ascii numbers", "NUM!REAL!TEXT", -1, "I,R,A" },
        { "fixed ascii", "CODE!NUM", -1, "A(3),I(4)" },
        { "signed and float", "S1!S2!S4!F4!F8", -1, "b21,b22,b24,b44,b48" },
        { "odd widths", "U3!S3!U5", -1, "b13,b23,b15" },
        { "bit string group", "NAME!ORNT", 0, "B(40),b11" },
    };

    [Theory]
    [MemberData(nameof(HandBuiltLayouts))]
    public void HandBuiltLayouts_ReadTheSame(string name, string labels, int repeatingStart, string formats)
    {
        _ = name;
        var definition = Definition(labels.Split('!'), repeatingStart, formats.Split(','));
        var random = new Random(labels.Length * 31 + formats.Length);

        foreach (var lexicalLevel in new[] { 0, 2 })
        {
            for (var sample = 0; sample < 8; sample++)
            {
                var data = RandomFieldData(definition, random, lexicalLevel);
                for (var length = 0; length <= data.Length; length++)
                {
                    AssertReadsTheSame(definition, data[..length], lexicalLevel);
                }
            }
        }
    }

    private static void AssertCellReadsTheSame(byte[] cell, bool truncate)
    {
        var document = Iso8211DocumentReader.Read(cell);
        var ddr = Iso8211DataDescriptiveRecordReader.Read(document.DataDescriptiveRecord!);
        var truncated = 0;

        foreach (var record in document.DataRecords)
        {
            foreach (var field in record.Fields)
            {
                var definition = ddr.GetFieldDefinition(field.Tag);
                if (definition is null)
                {
                    continue;
                }

                AssertReadsTheSame(definition, field.Data, lexicalLevel: 0);

                // Truncating every field is slow; a few hundred cover each field type many times.
                if (truncate && truncated++ < 150)
                {
                    for (var length = 0; length < field.Data.Length; length++)
                    {
                        AssertReadsTheSame(definition, field.Data[..length], lexicalLevel: 0);
                        AssertReadsTheSame(definition, [.. field.Data[..length], FieldTerminator], lexicalLevel: 0);
                    }
                }
            }
        }
    }

    private static void AssertReadsTheSame(Iso8211FieldDefinition definition, byte[] data, int lexicalLevel)
    {
        var context = $"{definition.Tag} [{Convert.ToHexString(data)}] level {lexicalLevel}";
        var legacy = new LegacyIso8211FieldReader(definition, data, lexicalLevel);
        var current = new Iso8211FieldReader(definition, data, lexicalLevel);

        Assert.True(legacy.SubfieldCount == current.SubfieldCount, $"SubfieldCount: {context}");
        Assert.True(legacy.GroupCount == current.GroupCount, $"GroupCount: {context}");
        Assert.True(legacy.HasRepeatingGroups == current.HasRepeatingGroups, $"HasRepeatingGroups: {context}");

        for (var i = 0; i < legacy.SubfieldCount; i++)
        {
            Assert.True(
                legacy.GetSubfieldBytesAt(i).SequenceEqual(current.GetSubfieldBytesAt(i)),
                $"GetSubfieldBytesAt({i}): {context}");
            AssertSame(() => legacy.GetSubfieldAt<string>(i), () => current.GetSubfieldAt<string>(i), $"GetSubfieldAt<string>({i}): {context}");
            AssertSame(() => legacy.GetSubfieldAt<long>(i), () => current.GetSubfieldAt<long>(i), $"GetSubfieldAt<long>({i}): {context}");
            AssertSame(() => legacy.GetSubfieldAt<int>(i), () => current.GetSubfieldAt<int>(i), $"GetSubfieldAt<int>({i}): {context}");
            AssertSame(() => legacy.GetSubfieldAt<double>(i), () => current.GetSubfieldAt<double>(i), $"GetSubfieldAt<double>({i}): {context}");
            AssertSame(() => legacy.GetSubfieldAt<byte>(i), () => current.GetSubfieldAt<byte>(i), $"GetSubfieldAt<byte>({i}): {context}");
            AssertSame(() => Hex(legacy.GetSubfieldAt<byte[]>(i)), () => Hex(current.GetSubfieldAt<byte[]>(i)), $"GetSubfieldAt<byte[]>({i}): {context}");
        }

        foreach (var subfield in definition.SubfieldDefinitions)
        {
            var name = subfield.Name;
            AssertSame(
                () => legacy.TryGetSubfield<string>(name, out var v) ? v : "<none>",
                () => current.TryGetSubfield<string>(name, out var v) ? v : "<none>",
                $"TryGetSubfield<string>({name}): {context}");
            AssertSame(
                () => legacy.TryGetSubfield<uint>(name, out var v) ? v.ToString(CultureInfo.InvariantCulture) : "<none>",
                () => current.TryGetSubfield<uint>(name, out var v) ? v.ToString(CultureInfo.InvariantCulture) : "<none>",
                $"TryGetSubfield<uint>({name}): {context}");
            AssertSame(
                () => string.Join("|", legacy.GetSubfieldValues<string>(name)),
                () => string.Join("|", current.GetSubfieldValues<string>(name)),
                $"GetSubfieldValues<string>({name}): {context}");
            AssertSame(
                () => Hex(legacy.GetSubfieldBytes(name).ToArray()),
                () => Hex(current.GetSubfieldBytes(name).ToArray()),
                $"GetSubfieldBytes({name}): {context}");
        }

        AssertGroupAccessorsAgree(current, context);
    }

    /// <summary>
    /// Checks the new ordinal accessors against the group objects and indexed accessors.
    /// </summary>
    private static void AssertGroupAccessorsAgree(Iso8211FieldReader reader, string context)
    {
        var definition = reader.FieldDefinition;
        var groupStart = definition.HasRepeatingGroup ? definition.RepeatingSubfieldStartIndex : 0;
        var groups = reader.GetSubfieldGroups().ToArray();

        for (var g = 0; g < groups.Length; g++)
        {
            for (var k = 0; k < groups[g].Count; k++)
            {
                var ordinal = groupStart + k;
                Assert.Equal(ordinal, reader.GetOrdinal(definition.SubfieldDefinitions[ordinal].Name));
                AssertSame(
                    () => groups[g].GetSubfieldAt<string>(k),
                    () => reader.GetSubfield<string>(g, ordinal),
                    $"GetSubfield<string>({g}, {ordinal}): {context}");
                // Like TryGetSubfield(string), the Try form reports absence, not conversion failures.
                AssertSame(
                    () => reader.TryGetSubfield<string>(g, ordinal, out var value) ? value : "<absent>",
                    () => reader.GetSubfield<string>(g, ordinal),
                    $"TryGetSubfield<string>({g}, {ordinal}): {context}");
            }

            // Ordinals past a truncated final group are absent, not an error.
            for (var k = groups[g].Count; k < definition.SubfieldDefinitions.Count - groupStart; k++)
            {
                Assert.False(reader.TryGetSubfield<string>(g, groupStart + k, out _), $"TryGetSubfield past end: {context}");
            }
        }

        Assert.False(reader.TryGetSubfield<string>(groups.Length, groupStart, out _), $"TryGetSubfield past last group: {context}");
    }

    private static void AssertSame<T>(Func<T> legacy, Func<T> current, string message)
    {
        Assert.True(Outcome(legacy) == Outcome(current), $"{message}: {Outcome(legacy)} vs {Outcome(current)}");
    }

    private static string Outcome<T>(Func<T> read)
    {
        try
        {
            return "value " + Convert.ToString(read(), CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            return "throws " + ex.GetType().Name;
        }
    }

    private static string Hex(byte[] bytes) => Convert.ToHexString(bytes);

    private static Iso8211FieldDefinition Definition(string[] labels, int repeatingStart, string[] formats)
    {
        var subfields = new List<Iso8211SubfieldDefinition>();
        for (var i = 0; i < labels.Length; i++)
        {
            subfields.Add(new Iso8211SubfieldDefinition { Name = labels[i], Index = i, Format = ParseFormat(formats[i]) });
        }

        return new Iso8211FieldDefinition
        {
            Tag = "TEST",
            DataStructureCode = repeatingStart >= 0 ? Iso8211DataStructureCode.Array : Iso8211DataStructureCode.Vector,
            DataTypeCode = Iso8211DataTypeCode.MixedDataTypes,
            SubfieldDefinitions = subfields,
            RepeatingSubfieldStartIndex = repeatingStart,
        };
    }

    private static Iso8211SubfieldFormat ParseFormat(string format) => format switch
    {
        "A" => new() { FormatType = Iso8211SubfieldFormatType.CharacterData },
        "I" => new() { FormatType = Iso8211SubfieldFormatType.Integer },
        "R" => new() { FormatType = Iso8211SubfieldFormatType.Real },
        _ when format.StartsWith("A(", StringComparison.Ordinal) => new() { FormatType = Iso8211SubfieldFormatType.CharacterData, Width = int.Parse(format[2..^1], CultureInfo.InvariantCulture) },
        _ when format.StartsWith("I(", StringComparison.Ordinal) => new() { FormatType = Iso8211SubfieldFormatType.Integer, Width = int.Parse(format[2..^1], CultureInfo.InvariantCulture) },
        _ when format.StartsWith("B(", StringComparison.Ordinal) => new() { FormatType = Iso8211SubfieldFormatType.BitString, Width = int.Parse(format[2..^1], CultureInfo.InvariantCulture) / 8 },
        _ when format.StartsWith("b1", StringComparison.Ordinal) => new() { FormatType = Iso8211SubfieldFormatType.UnsignedInteger, Width = format[2] - '0' },
        _ when format.StartsWith("b2", StringComparison.Ordinal) => new() { FormatType = Iso8211SubfieldFormatType.SignedInteger, Width = format[2] - '0' },
        _ when format.StartsWith("b4", StringComparison.Ordinal) => new() { FormatType = Iso8211SubfieldFormatType.FloatingPoint, Width = format[2] - '0' },
        _ => throw new ArgumentOutOfRangeException(nameof(format), format),
    };

    /// <summary>
    /// Builds plausible field data: the leading subfields, then up to five repetitions, with
    /// variable-length values that sometimes contain spaces or are empty.
    /// </summary>
    private static byte[] RandomFieldData(Iso8211FieldDefinition definition, Random random, int lexicalLevel)
    {
        var subfields = definition.SubfieldDefinitions;
        var start = definition.HasRepeatingGroup ? definition.RepeatingSubfieldStartIndex : 0;
        var bytes = new List<byte>();

        void Append(Iso8211SubfieldFormat format)
        {
            if (format.IsFixedWidth)
            {
                if (format.FormatType is Iso8211SubfieldFormatType.CharacterData or Iso8211SubfieldFormatType.Integer)
                {
                    bytes.AddRange(Encoding.ASCII.GetBytes(random.Next(0, 10_000).ToString(CultureInfo.InvariantCulture).PadLeft(format.Width, '0')[..format.Width]));
                }
                else
                {
                    for (var b = 0; b < format.Width; b++)
                    {
                        // Include terminator values, which fixed-width binary data may hold.
                        bytes.Add(random.Next(4) == 0 ? FieldTerminator : (byte)random.Next(256));
                    }
                }

                return;
            }

            var text = format.FormatType switch
            {
                Iso8211SubfieldFormatType.Integer => random.Next(-500, 500).ToString(CultureInfo.InvariantCulture),
                Iso8211SubfieldFormatType.Real => (random.NextDouble() * 100).ToString("F2", CultureInfo.InvariantCulture),
                _ => random.Next(5) == 0 ? string.Empty : new string('X', random.Next(1, 8)) + (random.Next(2) == 0 ? " " : string.Empty),
            };
            if (lexicalLevel >= 2)
            {
                bytes.AddRange(Encoding.Unicode.GetBytes(text));
                bytes.Add(UnitTerminator);
                bytes.Add(0);
            }
            else
            {
                bytes.AddRange(Encoding.ASCII.GetBytes(text));
                bytes.Add(UnitTerminator);
            }
        }

        for (var i = 0; i < start; i++)
        {
            Append(subfields[i].Format);
        }

        var repetitions = definition.HasRepeatingGroup ? random.Next(0, 6) : 1;
        for (var r = 0; r < repetitions; r++)
        {
            for (var i = start; i < subfields.Count; i++)
            {
                Append(subfields[i].Format);
            }
        }

        return [.. bytes];
    }
}
