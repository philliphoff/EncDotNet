using System.Runtime.CompilerServices;
using EncDotNet.S57;
using EncDotNet.S57.ExchangeSets;

namespace EncDotNet.UnitTests;

/// <summary>
/// The small synthetic cell and catalogue that the S-57 writer goldens are written from, and
/// helpers for the golden files.
/// </summary>
/// <remarks>
/// The cell isn't a real chart: it is built by hand to use every field the writer writes, with
/// ATTF at lexical level 1 (ISO 8859-1) and NATF at lexical level 2 (UCS-2). Set
/// <c>ENCDOTNET_UPDATE_GOLDENS=1</c> to rewrite the golden files from it.
/// </remarks>
internal static class S57WriterTestCells
{
    public const string CellFileName = "XX5WRITE.000";

    public static S57Document SmallCell(int attfLexicalLevel = 1, int natfLexicalLevel = 2) => new()
    {
        DataSetIdentification = new S57DataSetIdentification
        {
            RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.DataSetGeneralInfo, 1),
            ExchangePurpose = 1,
            IntendedUsage = 5,
            DataSetName = CellFileName,
            EditionNumber = "1",
            UpdateNumber = "0",
            UpdateApplicationDate = "20260101",
            IssueDate = "20260102",
            S57EditionNumber = "03.1",
            ProductSpecification = 1,
            ProductSpecificationDescription = "",
            ProductSpecificationEdition = "2.0",
            ApplicationProfile = 1,
            ProducingAgency = 550,
            DataStructure = 2,
            AttfLexicalLevel = attfLexicalLevel,
            NatfLexicalLevel = natfLexicalLevel,
            Comment = "EncDotNet S-57 writer golden cell"
        },
        DataSetParameters = new S57DataSetParameters
        {
            RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.DataSetGeoReference, 1),
            HorizontalDatum = 2,
            VerticalDatum = 3,
            SoundingDatum = 23,
            CompilationScale = 22000,
            DepthUnits = 1,
            HeightUnits = 1,
            PositionalUnits = 1,
            CoordinateUnits = 1,
            CoordinateMultiplicationFactor = 10_000_000,
            SoundingMultiplicationFactor = 10,
            Comment = ""
        },
        VectorRecords =
        [
            new S57VectorRecord
            {
                RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.IsolatedNode, 1),
                RecordVersion = 1,
                UpdateInstruction = S57UpdateInstruction.Insert,
                Attributes = [new S57AttributeValue(402, "1")], // QUAPOS
                Coordinates2D = [new S57Coordinate2D { Y = 501_234_567, X = -12_345_678 }]
            },
            new S57VectorRecord
            {
                RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.IsolatedNode, 2),
                RecordVersion = 1,
                UpdateInstruction = S57UpdateInstruction.Insert,
                Soundings =
                [
                    new S57Sounding { Y = 501_200_000, X = -12_300_000, Depth = 123 },
                    new S57Sounding { Y = 501_210_000, X = -12_310_000, Depth = -15 },
                    new S57Sounding { Y = 501_220_000, X = -12_320_000, Depth = 0 }
                ]
            },
            new S57VectorRecord
            {
                RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, 3),
                RecordVersion = 1,
                UpdateInstruction = S57UpdateInstruction.Insert,
                Coordinates2D = [new S57Coordinate2D { Y = 501_000_000, X = -12_500_000 }]
            },
            new S57VectorRecord
            {
                RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, 4),
                RecordVersion = 1,
                UpdateInstruction = S57UpdateInstruction.Insert,
                Coordinates2D = [new S57Coordinate2D { Y = 501_500_000, X = -12_000_000 }]
            },
            new S57VectorRecord
            {
                RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, 5),
                RecordVersion = 1,
                UpdateInstruction = S57UpdateInstruction.Insert,
                VectorPointers =
                [
                    new S57VectorPointer
                    {
                        Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, 3),
                        Orientation = S57Orientation.NotApplicable,
                        Usage = S57UsageIndicator.NotApplicable,
                        Topology = S57TopologyIndicator.Beginning,
                        Mask = S57MaskingIndicator.NotApplicable
                    },
                    new S57VectorPointer
                    {
                        Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, 4),
                        Orientation = S57Orientation.NotApplicable,
                        Usage = S57UsageIndicator.NotApplicable,
                        Topology = S57TopologyIndicator.End,
                        Mask = S57MaskingIndicator.NotApplicable
                    }
                ],
                Coordinates2D =
                [
                    new S57Coordinate2D { Y = 501_100_000, X = -12_400_000 },
                    new S57Coordinate2D { Y = 501_300_000, X = -12_200_000 }
                ]
            }
        ],
        FeatureRecords =
        [
            new S57FeatureRecord
            {
                RecordName = Feature(1, 1),
                Primitive = S57GeometricPrimitive.Line,
                Group = 2,
                ObjectCode = S57ObjectCode.M_COVR,
                RecordVersion = 1,
                UpdateInstruction = S57UpdateInstruction.Insert,
                Attributes = [new S57AttributeValue(18, "1")], // CATCOV
                SpatialPointers = [Edge(5)]
            },
            new S57FeatureRecord
            {
                RecordName = Feature(2, 2),
                Primitive = S57GeometricPrimitive.Point,
                Group = 2,
                ObjectCode = S57ObjectCode.BOYLAT,
                RecordVersion = 1,
                UpdateInstruction = S57UpdateInstruction.Insert,
                Attributes =
                [
                    new S57AttributeValue(4, "2"),                       // BOYSHP
                    new S57AttributeValue(75, "3,1"),                    // COLOUR
                    new S57AttributeValue(116, "Bouée du Chêne 45°") // OBJNAM, Latin-1
                ],
                NationalAttributes =
                [
                    new S57AttributeValue(301, "Noordzee 1 Ωμέγα") // NOBJNM, Greek
                ],
                FeaturePointers =
                [
                    new S57FeaturePointer
                    {
                        Name = new S57LongName(550, 3, 1),
                        Relationship = S57RelationshipIndicator.Peer,
                        Comment = "equipment"
                    }
                ],
                SpatialPointers = [Node(1)]
            },
            new S57FeatureRecord
            {
                RecordName = Feature(3, 3),
                Primitive = S57GeometricPrimitive.Point,
                Group = 2,
                ObjectCode = S57ObjectCode.LIGHTS,
                RecordVersion = 1,
                UpdateInstruction = S57UpdateInstruction.Insert,
                Attributes = [new S57AttributeValue(75, "1"), new S57AttributeValue(141, "4")], // COLOUR, SIGPER
                SpatialPointers = [Node(1)]
            },
            new S57FeatureRecord
            {
                RecordName = Feature(4, 4),
                Primitive = S57GeometricPrimitive.Point,
                Group = 2,
                ObjectCode = S57ObjectCode.SOUNDG,
                RecordVersion = 1,
                UpdateInstruction = S57UpdateInstruction.Insert,
                SpatialPointers = [Node(2)]
            },
            new S57FeatureRecord
            {
                RecordName = Feature(5, 5),
                Primitive = S57GeometricPrimitive.None,
                Group = 2,
                ObjectCode = S57ObjectCode.C_AGGR,
                RecordVersion = 1,
                UpdateInstruction = S57UpdateInstruction.Insert,
                FeaturePointers =
                [
                    new S57FeaturePointer { Name = new S57LongName(550, 2, 1), Relationship = S57RelationshipIndicator.Master, Comment = "" },
                    new S57FeaturePointer { Name = new S57LongName(550, 3, 1), Relationship = S57RelationshipIndicator.Slave, Comment = "" }
                ]
            }
        ]
    };

    /// <summary>
    /// The catalogue of an exchange set holding <see cref="SmallCell"/>.
    /// </summary>
    public static S57Catalog SmallCatalog(string cellCrc) => new()
    {
        Entries =
        [
            new S57CatalogEntry
            {
                RecordName = "CD",
                RecordId = 1,
                FileName = "CATALOG.031",
                Volume = "V01X01",
                Implementation = "ASC"
            },
            new S57CatalogEntry
            {
                RecordName = "CD",
                RecordId = 2,
                FileName = $"XX5WRITE\\{CellFileName}",
                LongFileName = "EncDotNet writer golden cell",
                Volume = "V01X01",
                Implementation = "BIN",
                SouthernmostLatitude = 50.1,
                WesternmostLongitude = -1.25,
                NorthernmostLatitude = 50.15,
                EasternmostLongitude = -1.2,
                CrcChecksum = cellCrc
            }
        ]
    };

    /// <summary>
    /// Asserts that bytes match a golden file, or rewrites the file when
    /// <c>ENCDOTNET_UPDATE_GOLDENS</c> is set.
    /// </summary>
    public static void AssertMatchesGolden(byte[] actual, string fileName, [CallerFilePath] string callerPath = "")
    {
        if (Environment.GetEnvironmentVariable("ENCDOTNET_UPDATE_GOLDENS") == "1")
        {
            var sourcePath = Path.Combine(Path.GetDirectoryName(callerPath) ?? ".", "Golden", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath) ?? ".");
            File.WriteAllBytes(sourcePath, actual);
            return;
        }

        var expected = File.ReadAllBytes(GoldenPath(fileName));
        Assert.Equal(expected, actual);
    }

    public static string GoldenPath(string fileName) => Path.Combine(AppContext.BaseDirectory, "Golden", fileName);

    private static S57RecordName Feature(int rcid, int fidn) => new()
    {
        RecordNameCode = S57RecordNameCodes.Feature,
        RecordId = rcid,
        AgencyCode = 550,
        FeatureId = fidn,
        FeatureSubdivision = 1
    };

    private static S57SpatialPointer Node(int rcid) => new()
    {
        Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.IsolatedNode, rcid),
        Orientation = S57Orientation.NotApplicable,
        Usage = S57UsageIndicator.NotApplicable,
        Mask = S57MaskingIndicator.NotApplicable
    };

    private static S57SpatialPointer Edge(int rcid) => new()
    {
        Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, rcid),
        Orientation = S57Orientation.Forward,
        Usage = S57UsageIndicator.Exterior,
        Mask = S57MaskingIndicator.Show
    };
}
