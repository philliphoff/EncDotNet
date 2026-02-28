using System.Collections.Immutable;
using EncDotNet.S57;
using EncDotNet.S57.Charts;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for the S-57 Chart types in the Charts namespace.
/// </summary>
public class S57ChartTests
{
    #region Test Data Helpers

    /// <summary>
    /// Creates a minimal S57Document for testing.
    /// </summary>
    private static S57Document CreateDocument(
        S57DataSetIdentification? dsid = null,
        S57DataSetParameters? dspm = null,
        S57FeatureRecord[]? features = null,
        S57VectorRecord[]? vectors = null)
    {
        return new S57Document
        {
            DataSetIdentification = dsid,
            DataSetParameters = dspm,
            FeatureRecords = features?.ToImmutableArray() ?? ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords = vectors?.ToImmutableArray() ?? ImmutableArray<S57VectorRecord>.Empty
        };
    }

    /// <summary>
    /// Creates an isolated node vector record.
    /// </summary>
    private static S57VectorRecord CreateIsolatedNodeRecord(
        int rcid,
        S57Coordinate2D? position = null,
        S57Sounding[]? soundings = null,
        S57AttributeValue[]? attributes = null)
    {
        return new S57VectorRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.IsolatedNode, rcid),
            RecordVersion = 1,
            UpdateInstruction = S57UpdateInstruction.Insert,
            Coordinates2D = position.HasValue 
                ? ImmutableArray.Create(position.Value) 
                : ImmutableArray<S57Coordinate2D>.Empty,
            Soundings = soundings?.ToImmutableArray() ?? ImmutableArray<S57Sounding>.Empty,
            Attributes = attributes?.ToImmutableArray() ?? ImmutableArray<S57AttributeValue>.Empty,
            VectorPointers = ImmutableArray<S57VectorPointer>.Empty
        };
    }

    /// <summary>
    /// Creates a connected node vector record.
    /// </summary>
    private static S57VectorRecord CreateConnectedNodeRecord(
        int rcid,
        S57Coordinate2D position,
        S57AttributeValue[]? attributes = null)
    {
        return new S57VectorRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, rcid),
            RecordVersion = 1,
            UpdateInstruction = S57UpdateInstruction.Insert,
            Coordinates2D = ImmutableArray.Create(position),
            Soundings = ImmutableArray<S57Sounding>.Empty,
            Attributes = attributes?.ToImmutableArray() ?? ImmutableArray<S57AttributeValue>.Empty,
            VectorPointers = ImmutableArray<S57VectorPointer>.Empty
        };
    }

    /// <summary>
    /// Creates an edge vector record.
    /// </summary>
    private static S57VectorRecord CreateEdgeRecord(
        int rcid,
        S57Coordinate2D[]? intermediatePoints = null,
        int? beginningNodeId = null,
        int? endNodeId = null,
        S57AttributeValue[]? attributes = null)
    {
        var pointers = new List<S57VectorPointer>();

        if (beginningNodeId.HasValue)
        {
            pointers.Add(new S57VectorPointer
            {
                Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, beginningNodeId.Value),
                Topology = S57TopologyIndicator.Beginning,
                Orientation = S57Orientation.Forward,
                Usage = S57UsageIndicator.NotApplicable,
                Mask = S57MaskingIndicator.NotApplicable
            });
        }

        if (endNodeId.HasValue)
        {
            pointers.Add(new S57VectorPointer
            {
                Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, endNodeId.Value),
                Topology = S57TopologyIndicator.End,
                Orientation = S57Orientation.Forward,
                Usage = S57UsageIndicator.NotApplicable,
                Mask = S57MaskingIndicator.NotApplicable
            });
        }

        return new S57VectorRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, rcid),
            RecordVersion = 1,
            UpdateInstruction = S57UpdateInstruction.Insert,
            Coordinates2D = intermediatePoints?.ToImmutableArray() ?? ImmutableArray<S57Coordinate2D>.Empty,
            Soundings = ImmutableArray<S57Sounding>.Empty,
            Attributes = attributes?.ToImmutableArray() ?? ImmutableArray<S57AttributeValue>.Empty,
            VectorPointers = pointers.ToImmutableArray()
        };
    }

    /// <summary>
    /// Creates a face vector record.
    /// </summary>
    private static S57VectorRecord CreateFaceRecord(
        int rcid,
        (int edgeId, S57Orientation orientation, S57UsageIndicator usage)[]? exteriorEdges = null,
        (int edgeId, S57Orientation orientation, S57UsageIndicator usage)[]? interiorEdges = null,
        S57AttributeValue[]? attributes = null)
    {
        var pointers = new List<S57VectorPointer>();

        if (exteriorEdges != null)
        {
            foreach (var (edgeId, orientation, usage) in exteriorEdges)
            {
                pointers.Add(new S57VectorPointer
                {
                    Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, edgeId),
                    Topology = S57TopologyIndicator.Beginning, // Not used for face-edge refs
                    Orientation = orientation,
                    Usage = usage,
                    Mask = S57MaskingIndicator.Show
                });
            }
        }

        if (interiorEdges != null)
        {
            foreach (var (edgeId, orientation, usage) in interiorEdges)
            {
                pointers.Add(new S57VectorPointer
                {
                    Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, edgeId),
                    Topology = S57TopologyIndicator.Beginning,
                    Orientation = orientation,
                    Usage = usage,
                    Mask = S57MaskingIndicator.Show
                });
            }
        }

        return new S57VectorRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Face, rcid),
            RecordVersion = 1,
            UpdateInstruction = S57UpdateInstruction.Insert,
            Coordinates2D = ImmutableArray<S57Coordinate2D>.Empty,
            Soundings = ImmutableArray<S57Sounding>.Empty,
            Attributes = attributes?.ToImmutableArray() ?? ImmutableArray<S57AttributeValue>.Empty,
            VectorPointers = pointers.ToImmutableArray()
        };
    }

    /// <summary>
    /// Creates a feature record.
    /// </summary>
    private static S57FeatureRecord CreateFeatureRecord(
        int rcid,
        S57GeometricPrimitive primitive,
        S57ObjectCode objectCode,
        int group = 2,
        S57AttributeValue[]? attributes = null,
        S57SpatialPointer[]? spatialPointers = null,
        S57FeaturePointer[]? featurePointers = null,
        S57AttributeValue[]? nationalAttributes = null)
    {
        return new S57FeatureRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, rcid),
            Primitive = primitive,
            ObjectCode = objectCode,
            Group = group,
            RecordVersion = 1,
            UpdateInstruction = S57UpdateInstruction.Insert,
            Attributes = attributes?.ToImmutableArray() ?? ImmutableArray<S57AttributeValue>.Empty,
            NationalAttributes = nationalAttributes?.ToImmutableArray() ?? ImmutableArray<S57AttributeValue>.Empty,
            SpatialPointers = spatialPointers?.ToImmutableArray() ?? ImmutableArray<S57SpatialPointer>.Empty,
            FeaturePointers = featurePointers?.ToImmutableArray() ?? ImmutableArray<S57FeaturePointer>.Empty
        };
    }

    /// <summary>
    /// Creates a spatial pointer to a vector record.
    /// </summary>
    private static S57SpatialPointer CreateSpatialPointer(
        int recordNameCode,
        int rcid,
        S57Orientation orientation = S57Orientation.Forward,
        S57UsageIndicator usage = S57UsageIndicator.Exterior,
        S57MaskingIndicator mask = S57MaskingIndicator.Show)
    {
        return new S57SpatialPointer
        {
            Name = S57RecordName.FromRcnmRcid(recordNameCode, rcid),
            Orientation = orientation,
            Usage = usage,
            Mask = mask
        };
    }

    #endregion

    #region S57Chart Creation Tests

    [Fact]
    public void FromDocument_NullDocument_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => S57Chart.FromDocument(null!));
    }

    [Fact]
    public void FromDocument_EmptyDocument_CreatesEmptyChart()
    {
        // Arrange
        var document = CreateDocument();

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.NotNull(chart);
        Assert.Null(chart.Identification);
        Assert.Null(chart.Parameters);
        Assert.Empty(chart.IsolatedNodes);
        Assert.Empty(chart.ConnectedNodes);
        Assert.Empty(chart.Edges);
        Assert.Empty(chart.Faces);
        Assert.Empty(chart.PointFeatures);
        Assert.Empty(chart.LineFeatures);
        Assert.Empty(chart.AreaFeatures);
        Assert.Empty(chart.MetaFeatures);
        Assert.Empty(chart.AllFeatures);
    }

    [Fact]
    public void FromDocument_WithIdentification_PreservesIdentification()
    {
        // Arrange
        var dsid = new S57DataSetIdentification
        {
            RecordName = S57RecordName.FromRcnmRcid(10, 1),
            DataSetName = "US5WA51M",
            EditionNumber = "2",
            IntendedUsage = 5,
            ProducingAgency = 540
        };
        var document = CreateDocument(dsid: dsid);

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.NotNull(chart.Identification);
        Assert.Equal("US5WA51M", chart.Identification.DataSetName);
        Assert.Equal("2", chart.Identification.EditionNumber);
        Assert.Equal(5, chart.Identification.IntendedUsage);
        Assert.Equal(540, chart.Identification.ProducingAgency);
    }

    [Fact]
    public void FromDocument_WithParameters_PreservesParametersAndDerivedProperties()
    {
        // Arrange
        var dspm = new S57DataSetParameters
        {
            RecordName = S57RecordName.FromRcnmRcid(20, 1),
            CompilationScale = 22000,
            CoordinateMultiplicationFactor = 10000000,
            SoundingMultiplicationFactor = 10,
            HorizontalDatum = 2,
            VerticalDatum = 17
        };
        var document = CreateDocument(dspm: dspm);

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.NotNull(chart.Parameters);
        Assert.Equal(22000, chart.CompilationScale);
        Assert.Equal(10000000, chart.CoordinateMultiplicationFactor);
        Assert.Equal(10, chart.SoundingMultiplicationFactor);
    }

    [Fact]
    public void FromDocument_WithoutParameters_UsesDefaultMultiplicationFactors()
    {
        // Arrange
        var document = CreateDocument();

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Equal(10000000, chart.CoordinateMultiplicationFactor);
        Assert.Equal(10, chart.SoundingMultiplicationFactor);
        Assert.Equal(0, chart.CompilationScale);
    }

    #endregion

    #region S57IsolatedNode Tests

    [Fact]
    public void IsolatedNode_WithPosition_HasPositionAndNoSoundings()
    {
        // Arrange
        var position = new S57Coordinate2D { X = -1225000000, Y = 475000000 };
        var vectorRecord = CreateIsolatedNodeRecord(1, position: position);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Single(chart.IsolatedNodes);
        var node = chart.IsolatedNodes.Values.First();
        Assert.True(node.HasPosition);
        Assert.False(node.HasSoundings);
        Assert.NotNull(node.Position);
        Assert.Equal(-1225000000, node.Position!.Value.X);
        Assert.Equal(475000000, node.Position!.Value.Y);
        Assert.True(node.Soundings.IsDefaultOrEmpty);
    }

    [Fact]
    public void IsolatedNode_WithSoundings_HasSoundingsAndNoPosition()
    {
        // Arrange
        var soundings = new[]
        {
            new S57Sounding { X = -1225000000, Y = 475000000, Depth = 150 },
            new S57Sounding { X = -1225100000, Y = 475100000, Depth = 200 },
            new S57Sounding { X = -1225200000, Y = 475200000, Depth = 250 }
        };
        var vectorRecord = CreateIsolatedNodeRecord(1, soundings: soundings);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Single(chart.IsolatedNodes);
        var node = chart.IsolatedNodes.Values.First();
        Assert.False(node.HasPosition);
        Assert.True(node.HasSoundings);
        Assert.Null(node.Position);
        Assert.Equal(3, node.Soundings.Length);
        Assert.Equal(150, node.Soundings[0].Depth);
        Assert.Equal(200, node.Soundings[1].Depth);
        Assert.Equal(250, node.Soundings[2].Depth);
    }

    [Fact]
    public void IsolatedNode_WithAttributes_PreservesAttributes()
    {
        // Arrange
        var attributes = new[]
        {
            new S57AttributeValue(1, "Value1"),
            new S57AttributeValue(2, "Value2")
        };
        var position = new S57Coordinate2D { X = 0, Y = 0 };
        var vectorRecord = CreateIsolatedNodeRecord(1, position: position, attributes: attributes);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var node = chart.IsolatedNodes.Values.First();
        Assert.Equal(2, node.Attributes.Length);
        Assert.Equal(1, node.Attributes[0].AttributeCode);
        Assert.Equal("Value1", node.Attributes[0].Value);
    }

    [Fact]
    public void IsolatedNode_PreservesRecordMetadata()
    {
        // Arrange
        var position = new S57Coordinate2D { X = 0, Y = 0 };
        var vectorRecord = new S57VectorRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.IsolatedNode, 42),
            RecordVersion = 3,
            UpdateInstruction = S57UpdateInstruction.Modify,
            Coordinates2D = ImmutableArray.Create(position),
            Soundings = ImmutableArray<S57Sounding>.Empty,
            Attributes = ImmutableArray<S57AttributeValue>.Empty,
            VectorPointers = ImmutableArray<S57VectorPointer>.Empty
        };
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var node = chart.IsolatedNodes.Values.First();
        Assert.Equal(S57RecordNameCodes.IsolatedNode, node.RecordName.RecordNameCode);
        Assert.Equal(42, node.RecordName.RecordId);
        Assert.Equal(3, node.RecordVersion);
        Assert.Equal(S57UpdateInstruction.Modify, node.UpdateInstruction);
    }

    #endregion

    #region S57ConnectedNode Tests

    [Fact]
    public void ConnectedNode_WithPosition_HasCorrectPosition()
    {
        // Arrange
        var position = new S57Coordinate2D { X = -1225000000, Y = 475000000 };
        var vectorRecord = CreateConnectedNodeRecord(1, position);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Single(chart.ConnectedNodes);
        var node = chart.ConnectedNodes.Values.First();
        Assert.Equal(-1225000000, node.Position.X);
        Assert.Equal(475000000, node.Position.Y);
    }

    [Fact]
    public void ConnectedNode_WithoutCoordinates_ThrowsInvalidOperationException()
    {
        // Arrange
        var vectorRecord = new S57VectorRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, 1),
            RecordVersion = 1,
            UpdateInstruction = S57UpdateInstruction.Insert,
            Coordinates2D = ImmutableArray<S57Coordinate2D>.Empty,
            Soundings = ImmutableArray<S57Sounding>.Empty,
            Attributes = ImmutableArray<S57AttributeValue>.Empty,
            VectorPointers = ImmutableArray<S57VectorPointer>.Empty
        };
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => S57Chart.FromDocument(document));
    }

    [Fact]
    public void ConnectedNode_MultiplNodes_IndexedByRecordName()
    {
        // Arrange
        var node1 = CreateConnectedNodeRecord(1, new S57Coordinate2D { X = 100, Y = 200 });
        var node2 = CreateConnectedNodeRecord(2, new S57Coordinate2D { X = 300, Y = 400 });
        var node3 = CreateConnectedNodeRecord(3, new S57Coordinate2D { X = 500, Y = 600 });
        var document = CreateDocument(vectors: new[] { node1, node2, node3 });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Equal(3, chart.ConnectedNodes.Count);
        
        var retrieved1 = chart.GetConnectedNode(S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, 1));
        Assert.NotNull(retrieved1);
        Assert.Equal(100, retrieved1.Position.X);

        var retrieved2 = chart.GetConnectedNode(S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, 2));
        Assert.NotNull(retrieved2);
        Assert.Equal(300, retrieved2.Position.X);
    }

    #endregion

    #region S57Edge Tests

    [Fact]
    public void Edge_WithIntermediatePoints_HasCorrectPoints()
    {
        // Arrange
        var points = new[]
        {
            new S57Coordinate2D { X = 100, Y = 200 },
            new S57Coordinate2D { X = 150, Y = 250 },
            new S57Coordinate2D { X = 200, Y = 300 }
        };
        var vectorRecord = CreateEdgeRecord(1, intermediatePoints: points);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Single(chart.Edges);
        var edge = chart.Edges.Values.First();
        Assert.True(edge.HasIntermediatePoints);
        Assert.Equal(3, edge.IntermediatePoints.Length);
        Assert.Equal(100, edge.IntermediatePoints[0].X);
        Assert.Equal(200, edge.IntermediatePoints[0].Y);
    }

    [Fact]
    public void Edge_WithBeginningAndEndNodes_HasCorrectNodeReferences()
    {
        // Arrange
        var vectorRecord = CreateEdgeRecord(1, beginningNodeId: 10, endNodeId: 20);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var edge = chart.Edges.Values.First();
        Assert.True(edge.HasBeginningNode);
        Assert.True(edge.HasEndNode);
        Assert.Equal(10, edge.BeginningNode!.Value.RecordId);
        Assert.Equal(20, edge.EndNode!.Value.RecordId);
        Assert.Equal(S57RecordNameCodes.ConnectedNode, edge.BeginningNode!.Value.RecordNameCode);
        Assert.Equal(S57RecordNameCodes.ConnectedNode, edge.EndNode!.Value.RecordNameCode);
    }

    [Fact]
    public void Edge_WithNoNodeReferences_HasNullNodeProperties()
    {
        // Arrange
        var points = new[] { new S57Coordinate2D { X = 100, Y = 200 } };
        var vectorRecord = CreateEdgeRecord(1, intermediatePoints: points);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var edge = chart.Edges.Values.First();
        Assert.False(edge.HasBeginningNode);
        Assert.False(edge.HasEndNode);
        Assert.Null(edge.BeginningNode);
        Assert.Null(edge.EndNode);
    }

    [Fact]
    public void Edge_WithNoIntermediatePoints_HasEmptyPointsArray()
    {
        // Arrange
        var vectorRecord = CreateEdgeRecord(1, beginningNodeId: 1, endNodeId: 2);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var edge = chart.Edges.Values.First();
        Assert.False(edge.HasIntermediatePoints);
        Assert.True(edge.IntermediatePoints.IsDefaultOrEmpty);
    }

    #endregion

    #region S57Face Tests

    [Fact]
    public void Face_WithExteriorBoundary_HasCorrectEdgeReferences()
    {
        // Arrange
        var exteriorEdges = new[]
        {
            (1, S57Orientation.Forward, S57UsageIndicator.Exterior),
            (2, S57Orientation.Reverse, S57UsageIndicator.Exterior),
            (3, S57Orientation.Forward, S57UsageIndicator.Exterior)
        };
        var vectorRecord = CreateFaceRecord(1, exteriorEdges: exteriorEdges);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Single(chart.Faces);
        var face = chart.Faces.Values.First();
        Assert.True(face.HasExteriorBoundary);
        Assert.Equal(3, face.ExteriorBoundary.Length);
        
        Assert.Equal(1, face.ExteriorBoundary[0].EdgeName.RecordId);
        Assert.Equal(S57Orientation.Forward, face.ExteriorBoundary[0].Orientation);
        
        Assert.Equal(2, face.ExteriorBoundary[1].EdgeName.RecordId);
        Assert.Equal(S57Orientation.Reverse, face.ExteriorBoundary[1].Orientation);
    }

    [Fact]
    public void Face_WithInteriorBoundaries_HasCorrectHoles()
    {
        // Arrange
        var exteriorEdges = new[]
        {
            (1, S57Orientation.Forward, S57UsageIndicator.Exterior)
        };
        var interiorEdges = new[]
        {
            (10, S57Orientation.Forward, S57UsageIndicator.Interior),
            (11, S57Orientation.Forward, S57UsageIndicator.Interior)
        };
        var vectorRecord = CreateFaceRecord(1, exteriorEdges: exteriorEdges, interiorEdges: interiorEdges);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var face = chart.Faces.Values.First();
        Assert.True(face.HasExteriorBoundary);
        Assert.True(face.HasInteriorBoundaries);
        Assert.Single(face.ExteriorBoundary);
        Assert.Equal(2, face.InteriorBoundaries.Length);
        Assert.Equal(10, face.InteriorBoundaries[0].EdgeName.RecordId);
        Assert.Equal(11, face.InteriorBoundaries[1].EdgeName.RecordId);
    }

    [Fact]
    public void Face_WithTruncatedExterior_IncludesInExteriorBoundary()
    {
        // Arrange
        var exteriorEdges = new[]
        {
            (1, S57Orientation.Forward, S57UsageIndicator.Exterior),
            (2, S57Orientation.Forward, S57UsageIndicator.ExteriorTruncated)
        };
        var vectorRecord = CreateFaceRecord(1, exteriorEdges: exteriorEdges);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var face = chart.Faces.Values.First();
        Assert.Equal(2, face.ExteriorBoundary.Length);
        Assert.Equal(S57UsageIndicator.ExteriorTruncated, face.ExteriorBoundary[1].Usage);
    }

    [Fact]
    public void Face_WithNoEdges_HasEmptyBoundaries()
    {
        // Arrange
        var vectorRecord = CreateFaceRecord(1);
        var document = CreateDocument(vectors: new[] { vectorRecord });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var face = chart.Faces.Values.First();
        Assert.False(face.HasExteriorBoundary);
        Assert.False(face.HasInteriorBoundaries);
    }

    #endregion

    #region S57PointFeature Tests

    [Fact]
    public void PointFeature_WithSpatialReference_HasCorrectReference()
    {
        // Arrange
        var spatialPtr = CreateSpatialPointer(S57RecordNameCodes.IsolatedNode, 42);
        var feature = CreateFeatureRecord(1, S57GeometricPrimitive.Point, S57ObjectCode.LIGHTS, spatialPointers: new[] { spatialPtr });
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Single(chart.PointFeatures);
        var pointFeature = chart.PointFeatures[0];
        Assert.True(pointFeature.HasSpatialReferences);
        Assert.Single(pointFeature.SpatialReferences);
        Assert.Equal(42, pointFeature.SpatialReferences[0].Name.RecordId);
        Assert.NotNull(pointFeature.PrimarySpatialReference);
        Assert.Equal(42, pointFeature.PrimarySpatialReference!.Value.Name.RecordId);
    }

    [Fact]
    public void PointFeature_WithoutSpatialReference_HasEmptyReferences()
    {
        // Arrange
        var feature = CreateFeatureRecord(1, S57GeometricPrimitive.Point, S57ObjectCode.LIGHTS);
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var pointFeature = chart.PointFeatures[0];
        Assert.False(pointFeature.HasSpatialReferences);
        Assert.Null(pointFeature.PrimarySpatialReference);
    }

    [Fact]
    public void PointFeature_WithAttributes_PreservesAttributesAndHasHelperMethods()
    {
        // Arrange
        var attributes = new[]
        {
            new S57AttributeValue(116, "RED"),
            new S57AttributeValue(117, "1"),
            new S57AttributeValue(116, "GREEN") // Duplicate attribute code
        };
        var feature = CreateFeatureRecord(1, S57GeometricPrimitive.Point, S57ObjectCode.LIGHTS, attributes: attributes);
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var pointFeature = chart.PointFeatures[0];
        Assert.True(pointFeature.HasAttributes);
        Assert.Equal(3, pointFeature.Attributes.Length);
        
        // Single value lookup
        Assert.Equal("RED", pointFeature.GetAttributeValue(116));
        Assert.Equal("1", pointFeature.GetAttributeValue(117));
        Assert.Null(pointFeature.GetAttributeValue(999));
        
        // Multiple value lookup
        var values = pointFeature.GetAttributeValues(116).ToList();
        Assert.Equal(2, values.Count);
        Assert.Contains("RED", values);
        Assert.Contains("GREEN", values);
    }

    [Fact]
    public void PointFeature_PreservesAllMetadata()
    {
        // Arrange
        var feature = CreateFeatureRecord(
            rcid: 99,
            primitive: S57GeometricPrimitive.Point,
            objectCode: S57ObjectCode.LIGHTS,
            group: 2
        );
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var pointFeature = chart.PointFeatures[0];
        Assert.Equal(99, pointFeature.RecordName.RecordId);
        Assert.Equal(S57RecordNameCodes.Feature, pointFeature.RecordName.RecordNameCode);
        Assert.Equal(S57ObjectCode.LIGHTS, pointFeature.ObjectCode);
        Assert.Equal(2, pointFeature.Group);
        Assert.Equal(1, pointFeature.RecordVersion);
        Assert.Equal(S57UpdateInstruction.Insert, pointFeature.UpdateInstruction);
    }

    #endregion

    #region S57LineFeature Tests

    [Fact]
    public void LineFeature_WithEdgeReferences_HasCorrectReferences()
    {
        // Arrange
        var spatialPtrs = new[]
        {
            CreateSpatialPointer(S57RecordNameCodes.Edge, 1),
            CreateSpatialPointer(S57RecordNameCodes.Edge, 2, S57Orientation.Reverse),
            CreateSpatialPointer(S57RecordNameCodes.Edge, 3)
        };
        var feature = CreateFeatureRecord(1, S57GeometricPrimitive.Line, S57ObjectCode.COALNE, spatialPointers: spatialPtrs);
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Single(chart.LineFeatures);
        var lineFeature = chart.LineFeatures[0];
        Assert.True(lineFeature.HasEdgeReferences);
        Assert.Equal(3, lineFeature.EdgeCount);
        Assert.Equal(3, lineFeature.EdgeReferences.Length);
        Assert.Equal(S57Orientation.Reverse, lineFeature.EdgeReferences[1].Orientation);
    }

    [Fact]
    public void LineFeature_WithoutEdges_HasEmptyReferences()
    {
        // Arrange
        var feature = CreateFeatureRecord(1, S57GeometricPrimitive.Line, S57ObjectCode.COALNE);
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var lineFeature = chart.LineFeatures[0];
        Assert.False(lineFeature.HasEdgeReferences);
        Assert.Equal(0, lineFeature.EdgeCount);
    }

    #endregion

    #region S57AreaFeature Tests

    [Fact]
    public void AreaFeature_WithFaceReference_HasCorrectReference()
    {
        // Arrange
        var spatialPtr = CreateSpatialPointer(S57RecordNameCodes.Face, 5);
        var feature = CreateFeatureRecord(1, S57GeometricPrimitive.Area, S57ObjectCode.DEPARE, spatialPointers: new[] { spatialPtr });
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Single(chart.AreaFeatures);
        var areaFeature = chart.AreaFeatures[0];
        Assert.True(areaFeature.HasFaceReference);
        Assert.NotNull(areaFeature.FaceReference);
        Assert.Equal(5, areaFeature.FaceReference!.Value.Name.RecordId);
        Assert.Equal(S57RecordNameCodes.Face, areaFeature.FaceReference!.Value.Name.RecordNameCode);
    }

    [Fact]
    public void AreaFeature_WithoutFace_HasNullReference()
    {
        // Arrange
        var feature = CreateFeatureRecord(1, S57GeometricPrimitive.Area, S57ObjectCode.DEPARE);
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var areaFeature = chart.AreaFeatures[0];
        Assert.False(areaFeature.HasFaceReference);
        Assert.Null(areaFeature.FaceReference);
    }

    #endregion

    #region S57MetaFeature Tests

    [Fact]
    public void MetaFeature_HasNoGeometry()
    {
        // Arrange
        var attributes = new[] { new S57AttributeValue(1, "MetaValue") };
        var feature = CreateFeatureRecord(1, S57GeometricPrimitive.None, S57ObjectCode.M_COVR, attributes: attributes);
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Single(chart.MetaFeatures);
        var metaFeature = chart.MetaFeatures[0];
        Assert.Equal((S57ObjectCode)302, metaFeature.ObjectCode);
        Assert.Equal("MetaValue", metaFeature.GetAttributeValue(1));
    }

    [Fact]
    public void MetaFeature_WithRelatedFeatures_PreservesRelationships()
    {
        // Arrange
        var relatedFeatures = new[]
        {
            new S57FeaturePointer
            {
                Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, 10),
                Relationship = S57RelationshipIndicator.Master,
                Comment = "Test"
            }
        };
        var feature = CreateFeatureRecord(1, S57GeometricPrimitive.None, S57ObjectCode.M_COVR, featurePointers: relatedFeatures);
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var metaFeature = chart.MetaFeatures[0];
        Assert.True(metaFeature.HasRelatedFeatures);
        Assert.Single(metaFeature.RelatedFeatures);
        Assert.Equal(10, metaFeature.RelatedFeatures[0].Name.RecordId);
        Assert.Equal(S57RelationshipIndicator.Master, metaFeature.RelatedFeatures[0].Relationship);
    }

    [Fact]
    public void MetaFeature_WithNationalAttributes_PreservesNationalAttributes()
    {
        // Arrange
        var nationalAttrs = new[] { new S57AttributeValue(1, "国家属性") };
        var feature = CreateFeatureRecord(1, S57GeometricPrimitive.None, S57ObjectCode.M_COVR, nationalAttributes: nationalAttrs);
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        var metaFeature = chart.MetaFeatures[0];
        Assert.True(metaFeature.HasNationalAttributes);
        Assert.Single(metaFeature.NationalAttributes);
        Assert.Equal("国家属性", metaFeature.NationalAttributes[0].Value);
    }

    #endregion

    #region Feature Type Categorization Tests

    [Fact]
    public void FromDocument_MixedFeatures_CategorizesCorrectly()
    {
        // Arrange
        var pointFeature1 = CreateFeatureRecord(1, S57GeometricPrimitive.Point, S57ObjectCode.LIGHTS);
        var pointFeature2 = CreateFeatureRecord(2, S57GeometricPrimitive.Point, (S57ObjectCode)76);
        var lineFeature = CreateFeatureRecord(3, S57GeometricPrimitive.Line, S57ObjectCode.COALNE);
        var areaFeature1 = CreateFeatureRecord(4, S57GeometricPrimitive.Area, S57ObjectCode.DEPARE);
        var areaFeature2 = CreateFeatureRecord(5, S57GeometricPrimitive.Area, S57ObjectCode.DEPCNT);
        var areaFeature3 = CreateFeatureRecord(6, S57GeometricPrimitive.Area, S57ObjectCode.DEPARE);
        var metaFeature = CreateFeatureRecord(7, S57GeometricPrimitive.None, S57ObjectCode.M_COVR);

        var document = CreateDocument(features: new[]
        {
            pointFeature1, pointFeature2, lineFeature,
            areaFeature1, areaFeature2, areaFeature3, metaFeature
        });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Equal(2, chart.PointFeatures.Length);
        Assert.True(chart.LineFeatures.Length == 1);
        Assert.Equal(3, chart.AreaFeatures.Length);
        Assert.True(chart.MetaFeatures.Length == 1);
        Assert.Equal(7, chart.AllFeatures.Count);
    }

    #endregion

    #region Spatial Record Categorization Tests

    [Fact]
    public void FromDocument_MixedSpatialRecords_CategorizesCorrectly()
    {
        // Arrange
        var isolatedNode1 = CreateIsolatedNodeRecord(1, new S57Coordinate2D { X = 100, Y = 200 });
        var isolatedNode2 = CreateIsolatedNodeRecord(2, soundings: new[] { new S57Sounding { X = 0, Y = 0, Depth = 100 } });
        var connectedNode1 = CreateConnectedNodeRecord(3, new S57Coordinate2D { X = 300, Y = 400 });
        var connectedNode2 = CreateConnectedNodeRecord(4, new S57Coordinate2D { X = 500, Y = 600 });
        var edge = CreateEdgeRecord(5, beginningNodeId: 3, endNodeId: 4);
        var face = CreateFaceRecord(6, exteriorEdges: new[] { (5, S57Orientation.Forward, S57UsageIndicator.Exterior) });

        var document = CreateDocument(vectors: new[]
        {
            isolatedNode1, isolatedNode2, connectedNode1, connectedNode2, edge, face
        });

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert
        Assert.Equal(2, chart.IsolatedNodes.Count);
        Assert.Equal(2, chart.ConnectedNodes.Count);
        Assert.Single(chart.Edges);
        Assert.Single(chart.Faces);
    }

    #endregion

    #region Query Methods Tests

    [Fact]
    public void GetFeaturesByObjectCode_ReturnsMatchingFeatures()
    {
        // Arrange
        var feature1 = CreateFeatureRecord(1, S57GeometricPrimitive.Point, S57ObjectCode.LIGHTS);
        var feature2 = CreateFeatureRecord(2, S57GeometricPrimitive.Point, (S57ObjectCode)76);
        var feature3 = CreateFeatureRecord(3, S57GeometricPrimitive.Point, S57ObjectCode.LIGHTS);
        var document = CreateDocument(features: new[] { feature1, feature2, feature3 });

        // Act
        var chart = S57Chart.FromDocument(document);
        var matching = chart.GetFeaturesByObjectCode(S57ObjectCode.LIGHTS).ToList();

        // Assert
        Assert.Equal(2, matching.Count);
        Assert.All(matching, f => Assert.Equal(S57ObjectCode.LIGHTS, f.ObjectCode));
    }

    [Fact]
    public void GetPointFeaturesByObjectCode_ReturnsOnlyPointFeatures()
    {
        // Arrange
        var pointFeature = CreateFeatureRecord(1, S57GeometricPrimitive.Point, S57ObjectCode.LIGHTS);
        var areaFeature = CreateFeatureRecord(2, S57GeometricPrimitive.Area, S57ObjectCode.LIGHTS);
        var document = CreateDocument(features: new[] { pointFeature, areaFeature });

        // Act
        var chart = S57Chart.FromDocument(document);
        var matching = chart.GetPointFeaturesByObjectCode(S57ObjectCode.LIGHTS).ToList();

        // Assert
        Assert.Single(matching);
        Assert.Equal(1, matching[0].RecordName.RecordId);
    }

    [Fact]
    public void GetLineFeaturesByObjectCode_ReturnsOnlyLineFeatures()
    {
        // Arrange
        var lineFeature = CreateFeatureRecord(1, S57GeometricPrimitive.Line, S57ObjectCode.COALNE);
        var areaFeature = CreateFeatureRecord(2, S57GeometricPrimitive.Area, S57ObjectCode.COALNE);
        var document = CreateDocument(features: new[] { lineFeature, areaFeature });

        // Act
        var chart = S57Chart.FromDocument(document);
        var matching = chart.GetLineFeaturesByObjectCode(S57ObjectCode.COALNE).ToList();

        // Assert
        Assert.Single(matching);
    }

    [Fact]
    public void GetAreaFeaturesByObjectCode_ReturnsOnlyAreaFeatures()
    {
        // Arrange
        var lineFeature = CreateFeatureRecord(1, S57GeometricPrimitive.Line, S57ObjectCode.DEPARE);
        var areaFeature = CreateFeatureRecord(2, S57GeometricPrimitive.Area, S57ObjectCode.DEPARE);
        var document = CreateDocument(features: new[] { lineFeature, areaFeature });

        // Act
        var chart = S57Chart.FromDocument(document);
        var matching = chart.GetAreaFeaturesByObjectCode(S57ObjectCode.DEPARE).ToList();

        // Assert
        Assert.Single(matching);
    }

    [Fact]
    public void GetFeature_ExistingRecord_ReturnsFeature()
    {
        // Arrange
        var feature = CreateFeatureRecord(42, S57GeometricPrimitive.Point, S57ObjectCode.LIGHTS);
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);
        var retrieved = chart.GetFeature(S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, 42));

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(42, retrieved.RecordName.RecordId);
    }

    [Fact]
    public void GetFeature_NonExistingRecord_ReturnsNull()
    {
        // Arrange
        var feature = CreateFeatureRecord(1, S57GeometricPrimitive.Point, S57ObjectCode.LIGHTS);
        var document = CreateDocument(features: new[] { feature });

        // Act
        var chart = S57Chart.FromDocument(document);
        var retrieved = chart.GetFeature(S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, 999));

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public void GetIsolatedNode_ExistingRecord_ReturnsNode()
    {
        // Arrange
        var node = CreateIsolatedNodeRecord(42, new S57Coordinate2D { X = 100, Y = 200 });
        var document = CreateDocument(vectors: new[] { node });

        // Act
        var chart = S57Chart.FromDocument(document);
        var retrieved = chart.GetIsolatedNode(S57RecordName.FromRcnmRcid(S57RecordNameCodes.IsolatedNode, 42));

        // Assert
        Assert.NotNull(retrieved);
    }

    [Fact]
    public void GetIsolatedNode_NonExistingRecord_ReturnsNull()
    {
        // Arrange
        var document = CreateDocument();

        // Act
        var chart = S57Chart.FromDocument(document);
        var retrieved = chart.GetIsolatedNode(S57RecordName.FromRcnmRcid(S57RecordNameCodes.IsolatedNode, 999));

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public void GetEdge_ExistingRecord_ReturnsEdge()
    {
        // Arrange
        var edge = CreateEdgeRecord(10, beginningNodeId: 1, endNodeId: 2);
        var document = CreateDocument(vectors: new[] { edge });

        // Act
        var chart = S57Chart.FromDocument(document);
        var retrieved = chart.GetEdge(S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, 10));

        // Assert
        Assert.NotNull(retrieved);
    }

    [Fact]
    public void GetFace_ExistingRecord_ReturnsFace()
    {
        // Arrange
        var face = CreateFaceRecord(20);
        var document = CreateDocument(vectors: new[] { face });

        // Act
        var chart = S57Chart.FromDocument(document);
        var retrieved = chart.GetFace(S57RecordName.FromRcnmRcid(S57RecordNameCodes.Face, 20));

        // Assert
        Assert.NotNull(retrieved);
    }

    #endregion

    #region Coordinate Conversion Tests

    [Fact]
    public void ToDecimalDegrees_IntCoordinate_ConvertsCorrectly()
    {
        // Arrange
        var dspm = new S57DataSetParameters { CoordinateMultiplicationFactor = 10000000 };
        var document = CreateDocument(dspm: dspm);
        var chart = S57Chart.FromDocument(document);

        // Act
        var result = chart.ToDecimalDegrees(-1225000000);

        // Assert
        Assert.Equal(-122.5, result, 6);
    }

    [Fact]
    public void ToDecimalDegrees_Coordinate2D_ConvertsCorrectly()
    {
        // Arrange
        var dspm = new S57DataSetParameters { CoordinateMultiplicationFactor = 10000000 };
        var document = CreateDocument(dspm: dspm);
        var chart = S57Chart.FromDocument(document);
        var coord = new S57Coordinate2D { X = -1225000000, Y = 475000000 };

        // Act
        var (longitude, latitude) = chart.ToDecimalDegrees(coord);

        // Assert
        Assert.Equal(-122.5, longitude, 6);
        Assert.Equal(47.5, latitude, 6);
    }

    [Fact]
    public void ToDepth_ConvertsCorrectly()
    {
        // Arrange
        var dspm = new S57DataSetParameters { SoundingMultiplicationFactor = 10 };
        var document = CreateDocument(dspm: dspm);
        var chart = S57Chart.FromDocument(document);

        // Act
        var result = chart.ToDepth(155);

        // Assert
        Assert.Equal(15.5, result, 6);
    }

    [Fact]
    public void ToDecimalValues_Sounding_ConvertsCorrectly()
    {
        // Arrange
        var dspm = new S57DataSetParameters
        {
            CoordinateMultiplicationFactor = 10000000,
            SoundingMultiplicationFactor = 10
        };
        var document = CreateDocument(dspm: dspm);
        var chart = S57Chart.FromDocument(document);
        var sounding = new S57Sounding { X = -1225000000, Y = 475000000, Depth = 155 };

        // Act
        var (longitude, latitude, depth) = chart.ToDecimalValues(sounding);

        // Assert
        Assert.Equal(-122.5, longitude, 6);
        Assert.Equal(47.5, latitude, 6);
        Assert.Equal(15.5, depth, 6);
    }

    #endregion

    #region S57SpatialRecord Factory Tests

    [Fact]
    public void SpatialRecordCreate_IsolatedNode_ReturnsCorrectType()
    {
        // Arrange
        var vectorRecord = CreateIsolatedNodeRecord(1, new S57Coordinate2D { X = 0, Y = 0 });

        // Act
        var result = S57SpatialRecord.Create(vectorRecord);

        // Assert
        Assert.IsType<S57IsolatedNode>(result);
    }

    [Fact]
    public void SpatialRecordCreate_ConnectedNode_ReturnsCorrectType()
    {
        // Arrange
        var vectorRecord = CreateConnectedNodeRecord(1, new S57Coordinate2D { X = 0, Y = 0 });

        // Act
        var result = S57SpatialRecord.Create(vectorRecord);

        // Assert
        Assert.IsType<S57ConnectedNode>(result);
    }

    [Fact]
    public void SpatialRecordCreate_Edge_ReturnsCorrectType()
    {
        // Arrange
        var vectorRecord = CreateEdgeRecord(1);

        // Act
        var result = S57SpatialRecord.Create(vectorRecord);

        // Assert
        Assert.IsType<S57Edge>(result);
    }

    [Fact]
    public void SpatialRecordCreate_Face_ReturnsCorrectType()
    {
        // Arrange
        var vectorRecord = CreateFaceRecord(1);

        // Act
        var result = S57SpatialRecord.Create(vectorRecord);

        // Assert
        Assert.IsType<S57Face>(result);
    }

    [Fact]
    public void SpatialRecordCreate_UnknownType_ThrowsArgumentException()
    {
        // Arrange
        var vectorRecord = new S57VectorRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(999, 1), // Unknown record type
            RecordVersion = 1,
            UpdateInstruction = S57UpdateInstruction.Insert,
            Coordinates2D = ImmutableArray<S57Coordinate2D>.Empty,
            Soundings = ImmutableArray<S57Sounding>.Empty,
            Attributes = ImmutableArray<S57AttributeValue>.Empty,
            VectorPointers = ImmutableArray<S57VectorPointer>.Empty
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => S57SpatialRecord.Create(vectorRecord));
    }

    [Fact]
    public void SpatialRecordCreate_NullSource_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => S57SpatialRecord.Create(null!));
    }

    #endregion

    #region S57TypedFeature Factory Tests

    [Fact]
    public void TypedFeatureCreate_Point_ReturnsCorrectType()
    {
        // Arrange
        var featureRecord = CreateFeatureRecord(1, S57GeometricPrimitive.Point, S57ObjectCode.LIGHTS);

        // Act
        var result = S57TypedFeature.Create(featureRecord);

        // Assert
        Assert.IsType<S57PointFeature>(result);
    }

    [Fact]
    public void TypedFeatureCreate_Line_ReturnsCorrectType()
    {
        // Arrange
        var featureRecord = CreateFeatureRecord(1, S57GeometricPrimitive.Line, S57ObjectCode.COALNE);

        // Act
        var result = S57TypedFeature.Create(featureRecord);

        // Assert
        Assert.IsType<S57LineFeature>(result);
    }

    [Fact]
    public void TypedFeatureCreate_Area_ReturnsCorrectType()
    {
        // Arrange
        var featureRecord = CreateFeatureRecord(1, S57GeometricPrimitive.Area, S57ObjectCode.DEPARE);

        // Act
        var result = S57TypedFeature.Create(featureRecord);

        // Assert
        Assert.IsType<S57AreaFeature>(result);
    }

    [Fact]
    public void TypedFeatureCreate_None_ReturnsMetaFeature()
    {
        // Arrange
        var featureRecord = CreateFeatureRecord(1, S57GeometricPrimitive.None, S57ObjectCode.M_COVR);

        // Act
        var result = S57TypedFeature.Create(featureRecord);

        // Assert
        Assert.IsType<S57MetaFeature>(result);
    }

    [Fact]
    public void TypedFeatureCreate_NullSource_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => S57TypedFeature.Create(null!));
    }

    #endregion

    #region S57EdgeReference Tests

    [Fact]
    public void EdgeReference_Constructor_SetsAllProperties()
    {
        // Arrange
        var edgeName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, 42);

        // Act
        var edgeRef = new S57EdgeReference(
            edgeName,
            S57Orientation.Reverse,
            S57UsageIndicator.Interior,
            S57MaskingIndicator.Mask);

        // Assert
        Assert.Equal(42, edgeRef.EdgeName.RecordId);
        Assert.Equal(S57Orientation.Reverse, edgeRef.Orientation);
        Assert.Equal(S57UsageIndicator.Interior, edgeRef.Usage);
        Assert.Equal(S57MaskingIndicator.Mask, edgeRef.Mask);
    }

    [Fact]
    public void EdgeReference_ToString_ReturnsFormattedString()
    {
        // Arrange
        var edgeName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, 42);
        var edgeRef = new S57EdgeReference(
            edgeName,
            S57Orientation.Forward,
            S57UsageIndicator.Exterior,
            S57MaskingIndicator.Show);

        // Act
        var result = edgeRef.ToString();

        // Assert
        Assert.Contains("Orientation=Forward", result);
        Assert.Contains("Usage=Exterior", result);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void CompleteChart_AllComponentsIntegrate()
    {
        // Arrange - Create a complete chart with topology
        var dsid = new S57DataSetIdentification
        {
            RecordName = S57RecordName.FromRcnmRcid(10, 1),
            DataSetName = "TESTCHART"
        };
        var dspm = new S57DataSetParameters
        {
            RecordName = S57RecordName.FromRcnmRcid(20, 1),
            CompilationScale = 22000,
            CoordinateMultiplicationFactor = 10000000,
            SoundingMultiplicationFactor = 10
        };

        // Create spatial records for a simple triangle
        var node1 = CreateConnectedNodeRecord(1, new S57Coordinate2D { X = 0, Y = 0 });
        var node2 = CreateConnectedNodeRecord(2, new S57Coordinate2D { X = 10000000, Y = 0 });
        var node3 = CreateConnectedNodeRecord(3, new S57Coordinate2D { X = 5000000, Y = 10000000 });

        var edge1 = CreateEdgeRecord(1, beginningNodeId: 1, endNodeId: 2);
        var edge2 = CreateEdgeRecord(2, beginningNodeId: 2, endNodeId: 3);
        var edge3 = CreateEdgeRecord(3, beginningNodeId: 3, endNodeId: 1);

        var face = CreateFaceRecord(1, exteriorEdges: new[]
        {
            (1, S57Orientation.Forward, S57UsageIndicator.Exterior),
            (2, S57Orientation.Forward, S57UsageIndicator.Exterior),
            (3, S57Orientation.Forward, S57UsageIndicator.Exterior)
        });

        // Create features
        var pointOnNode = CreateFeatureRecord(
            1, S57GeometricPrimitive.Point, S57ObjectCode.LIGHTS,
            spatialPointers: new[] { CreateSpatialPointer(S57RecordNameCodes.ConnectedNode, 1) });

        var lineFeature = CreateFeatureRecord(
            2, S57GeometricPrimitive.Line, S57ObjectCode.COALNE,
            spatialPointers: new[]
            {
                CreateSpatialPointer(S57RecordNameCodes.Edge, 1),
                CreateSpatialPointer(S57RecordNameCodes.Edge, 2)
            });

        var areaFeature = CreateFeatureRecord(
            3, S57GeometricPrimitive.Area, S57ObjectCode.DEPARE,
            spatialPointers: new[] { CreateSpatialPointer(S57RecordNameCodes.Face, 1) });

        var document = CreateDocument(
            dsid: dsid,
            dspm: dspm,
            vectors: new[] { node1, node2, node3, edge1, edge2, edge3, face },
            features: new[] { pointOnNode, lineFeature, areaFeature }
        );

        // Act
        var chart = S57Chart.FromDocument(document);

        // Assert - Verify entire structure
        Assert.NotNull(chart.Identification);
        Assert.Equal("TESTCHART", chart.Identification.DataSetName);
        Assert.Equal(22000, chart.CompilationScale);

        Assert.Equal(3, chart.ConnectedNodes.Count);
        Assert.Equal(3, chart.Edges.Count);
        Assert.True(chart.Faces.Count == 1);

        Assert.Single(chart.PointFeatures);
        Assert.Single(chart.LineFeatures);
        Assert.Single(chart.AreaFeatures);

        // Verify topology navigation
        var retrievedEdge = chart.GetEdge(S57RecordName.FromRcnmRcid(S57RecordNameCodes.Edge, 1));
        Assert.NotNull(retrievedEdge);
        Assert.True(retrievedEdge.HasBeginningNode);
        Assert.True(retrievedEdge.HasEndNode);

        var beginNode = chart.GetConnectedNode(retrievedEdge.BeginningNode!.Value);
        Assert.NotNull(beginNode);
        Assert.Equal(0, beginNode.Position.X);

        var retrievedFace = chart.GetFace(S57RecordName.FromRcnmRcid(S57RecordNameCodes.Face, 1));
        Assert.NotNull(retrievedFace);
        Assert.Equal(3, retrievedFace.ExteriorBoundary.Length);

        // Verify feature-spatial references
        var retrievedArea = chart.AreaFeatures[0];
        Assert.True(retrievedArea.HasFaceReference);
        var faceFromArea = chart.GetFace(retrievedArea.FaceReference!.Value.Name);
        Assert.NotNull(faceFromArea);
    }

    #endregion
}
