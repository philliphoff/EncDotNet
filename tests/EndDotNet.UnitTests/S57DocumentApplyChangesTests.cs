using System.Collections.Immutable;
using EncDotNet.S57;

namespace EndDotNet.UnitTests;

/// <summary>
/// Unit tests for <see cref="S57Document.ApplyChanges"/>.
/// </summary>
public class S57DocumentApplyChangesTests
{
    #region Helpers

    private static S57Document CreateEmptyDocument()
    {
        return new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
    }

    private static S57FeatureRecord CreateFeature(
        int rcid,
        S57ObjectCode objl = S57ObjectCode.DEPARE,
        S57UpdateInstruction ruin = S57UpdateInstruction.Insert,
        S57GeometricPrimitive prim = S57GeometricPrimitive.Area,
        ImmutableArray<S57AttributeValue>? attributes = null,
        ImmutableArray<S57SpatialPointer>? spatialPointers = null,
        S57FieldUpdateControl? spatialPointerControl = null,
        ImmutableArray<S57FeaturePointer>? featurePointers = null,
        S57FieldUpdateControl? featurePointerControl = null)
    {
        return new S57FeatureRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, rcid),
            Primitive = prim,
            Group = 1,
            ObjectCode = objl,
            RecordVersion = 1,
            UpdateInstruction = ruin,
            Attributes = attributes ?? ImmutableArray<S57AttributeValue>.Empty,
            NationalAttributes = ImmutableArray<S57AttributeValue>.Empty,
            SpatialPointers = spatialPointers ?? ImmutableArray<S57SpatialPointer>.Empty,
            SpatialPointerControl = spatialPointerControl,
            FeaturePointers = featurePointers ?? ImmutableArray<S57FeaturePointer>.Empty,
            FeaturePointerControl = featurePointerControl,
        };
    }

    private static S57VectorRecord CreateVector(
        int rcid,
        int rcnm = S57RecordNameCodes.Edge,
        S57UpdateInstruction ruin = S57UpdateInstruction.Insert,
        ImmutableArray<S57AttributeValue>? attributes = null,
        ImmutableArray<S57VectorPointer>? vectorPointers = null,
        S57FieldUpdateControl? vectorPointerControl = null,
        ImmutableArray<S57Coordinate2D>? coordinates = null,
        ImmutableArray<S57Sounding>? soundings = null,
        S57FieldUpdateControl? coordinateControl = null)
    {
        return new S57VectorRecord
        {
            RecordName = S57RecordName.FromRcnmRcid(rcnm, rcid),
            RecordVersion = 1,
            UpdateInstruction = ruin,
            Attributes = attributes ?? ImmutableArray<S57AttributeValue>.Empty,
            VectorPointers = vectorPointers ?? ImmutableArray<S57VectorPointer>.Empty,
            VectorPointerControl = vectorPointerControl,
            Coordinates2D = coordinates ?? ImmutableArray<S57Coordinate2D>.Empty,
            Soundings = soundings ?? ImmutableArray<S57Sounding>.Empty,
            CoordinateControl = coordinateControl,
        };
    }

    private static S57AttributeValue Attr(int code, string value) => new(code, value);

    private static S57SpatialPointer SpatialPtr(int rcnm, int rcid) => new()
    {
        Name = S57RecordName.FromRcnmRcid(rcnm, rcid),
        Orientation = S57Orientation.Forward,
        Usage = S57UsageIndicator.Exterior,
        Mask = S57MaskingIndicator.Show
    };

    private static S57Coordinate2D Coord(int x, int y) => new() { X = x, Y = y };

    #endregion

    #region Feature Record-Level Operations

    [Fact]
    public void ApplyChanges_InsertFeature_AddsRecord()
    {
        var baseDoc = CreateEmptyDocument();
        var update = new S57Document
        {
            FeatureRecords = [CreateFeature(1, ruin: S57UpdateInstruction.Insert)],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Single(result.FeatureRecords);
        Assert.Equal(1, result.FeatureRecords[0].RecordName.RecordId);
    }

    [Fact]
    public void ApplyChanges_DeleteFeature_RemovesRecord()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = [CreateFeature(1), CreateFeature(2), CreateFeature(3)],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords = [CreateFeature(2, ruin: S57UpdateInstruction.Delete)],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Equal(2, result.FeatureRecords.Count);
        Assert.Equal(1, result.FeatureRecords[0].RecordName.RecordId);
        Assert.Equal(3, result.FeatureRecords[1].RecordName.RecordId);
    }

    [Fact]
    public void ApplyChanges_DeleteNonexistentFeature_NoEffect()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = [CreateFeature(1)],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords = [CreateFeature(99, ruin: S57UpdateInstruction.Delete)],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Single(result.FeatureRecords);
        Assert.Equal(1, result.FeatureRecords[0].RecordName.RecordId);
    }

    [Fact]
    public void ApplyChanges_ModifyFeature_PreservesIdentityAndPrimitive()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, objl: S57ObjectCode.DEPARE, prim: S57GeometricPrimitive.Area,
                    attributes: [Attr(100, "10.0"), Attr(200, "20.0")])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, ruin: S57UpdateInstruction.Modify,
                    attributes: [Attr(200, "25.0")])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Single(result.FeatureRecords);
        var f = result.FeatureRecords[0];
        Assert.Equal(S57ObjectCode.DEPARE, f.ObjectCode);
        Assert.Equal(S57GeometricPrimitive.Area, f.Primitive);
        Assert.Equal(2, f.Attributes.Count);
        Assert.Equal("10.0", f.Attributes[0].Value);
        Assert.Equal("25.0", f.Attributes[1].Value);
    }

    [Fact]
    public void ApplyChanges_ModifyNonexistentFeature_NoEffect()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = [CreateFeature(1)],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords = [CreateFeature(99, ruin: S57UpdateInstruction.Modify)],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Single(result.FeatureRecords);
        Assert.Equal(1, result.FeatureRecords[0].RecordName.RecordId);
    }

    [Fact]
    public void ApplyChanges_MultipleFeatureOperations_AppliedInOrder()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = [CreateFeature(1), CreateFeature(2)],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(2, ruin: S57UpdateInstruction.Delete),
                CreateFeature(3, ruin: S57UpdateInstruction.Insert),
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Equal(2, result.FeatureRecords.Count);
        Assert.Equal(1, result.FeatureRecords[0].RecordName.RecordId);
        Assert.Equal(3, result.FeatureRecords[1].RecordName.RecordId);
    }

    #endregion

    #region Vector Record-Level Operations

    [Fact]
    public void ApplyChanges_InsertVector_AddsRecord()
    {
        var baseDoc = CreateEmptyDocument();
        var update = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords = [CreateVector(10, ruin: S57UpdateInstruction.Insert)]
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Single(result.VectorRecords);
        Assert.Equal(10, result.VectorRecords[0].RecordName.RecordId);
    }

    [Fact]
    public void ApplyChanges_DeleteVector_RemovesRecord()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords = [CreateVector(10), CreateVector(20)]
        };
        var update = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords = [CreateVector(10, ruin: S57UpdateInstruction.Delete)]
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Single(result.VectorRecords);
        Assert.Equal(20, result.VectorRecords[0].RecordName.RecordId);
    }

    [Fact]
    public void ApplyChanges_ModifyVector_PreservesRecordName()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords =
            [
                CreateVector(10, coordinates: [Coord(100, 200), Coord(300, 400)])
            ]
        };
        var update = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords =
            [
                CreateVector(10, ruin: S57UpdateInstruction.Modify,
                    attributes: [Attr(1, "newval")])
            ]
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Single(result.VectorRecords);
        var v = result.VectorRecords[0];
        Assert.Equal(10, v.RecordName.RecordId);
        Assert.Equal(S57RecordNameCodes.Edge, v.RecordName.RecordNameCode);
        Assert.Single(v.Attributes);
    }

    #endregion

    #region Attribute Merge Tests

    [Fact]
    public void ApplyChanges_ModifyFeature_MergesAttributes()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, attributes: [Attr(10, "A"), Attr(20, "B"), Attr(30, "C")])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, ruin: S57UpdateInstruction.Modify,
                    attributes: [Attr(20, "B2"), Attr(40, "D")])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        var attrs = result.FeatureRecords[0].Attributes;
        Assert.Equal(4, attrs.Count);
        Assert.Equal("A", attrs[0].Value);   // ATTL=10 unchanged
        Assert.Equal("B2", attrs[1].Value);  // ATTL=20 updated
        Assert.Equal("C", attrs[2].Value);   // ATTL=30 unchanged
        Assert.Equal("D", attrs[3].Value);   // ATTL=40 new
    }

    [Fact]
    public void ApplyChanges_ModifyFeature_EmptyValueDeletesAttribute()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, attributes: [Attr(10, "A"), Attr(20, "B"), Attr(30, "C")])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, ruin: S57UpdateInstruction.Modify,
                    attributes: [Attr(20, "")])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        var attrs = result.FeatureRecords[0].Attributes;
        Assert.Equal(2, attrs.Count);
        Assert.Equal(10, attrs[0].AttributeCode);
        Assert.Equal(30, attrs[1].AttributeCode);
    }

    [Fact]
    public void ApplyChanges_ModifyFeature_NoUpdateAttributes_PreservesBase()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, attributes: [Attr(10, "A"), Attr(20, "B")])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, ruin: S57UpdateInstruction.Modify)
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Equal(2, result.FeatureRecords[0].Attributes.Count);
        Assert.Equal("A", result.FeatureRecords[0].Attributes[0].Value);
        Assert.Equal("B", result.FeatureRecords[0].Attributes[1].Value);
    }

    #endregion

    #region Spatial Pointer Array Splice Tests (FSPC)

    [Fact]
    public void ApplyChanges_FspcInsert_InsertsPointersAtIndex()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1,
                    spatialPointers:
                    [
                        SpatialPtr(S57RecordNameCodes.Edge, 1),
                        SpatialPtr(S57RecordNameCodes.Edge, 2),
                        SpatialPtr(S57RecordNameCodes.Edge, 3)
                    ])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, ruin: S57UpdateInstruction.Modify,
                    spatialPointerControl: new S57FieldUpdateControl
                    {
                        UpdateInstruction = S57UpdateInstruction.Insert,
                        Index = 2, // 1-based → insert before position 2
                        Count = 2
                    },
                    spatialPointers:
                    [
                        SpatialPtr(S57RecordNameCodes.Edge, 10),
                        SpatialPtr(S57RecordNameCodes.Edge, 11)
                    ])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        var ptrs = result.FeatureRecords[0].SpatialPointers;
        Assert.Equal(5, ptrs.Count);
        Assert.Equal(1, ptrs[0].Name.RecordId);
        Assert.Equal(10, ptrs[1].Name.RecordId);
        Assert.Equal(11, ptrs[2].Name.RecordId);
        Assert.Equal(2, ptrs[3].Name.RecordId);
        Assert.Equal(3, ptrs[4].Name.RecordId);
    }

    [Fact]
    public void ApplyChanges_FspcDelete_RemovesPointersAtIndex()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1,
                    spatialPointers:
                    [
                        SpatialPtr(S57RecordNameCodes.Edge, 1),
                        SpatialPtr(S57RecordNameCodes.Edge, 2),
                        SpatialPtr(S57RecordNameCodes.Edge, 3),
                        SpatialPtr(S57RecordNameCodes.Edge, 4)
                    ])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, ruin: S57UpdateInstruction.Modify,
                    spatialPointerControl: new S57FieldUpdateControl
                    {
                        UpdateInstruction = S57UpdateInstruction.Delete,
                        Index = 2, // 1-based → delete starting at position 2
                        Count = 2
                    })
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        var ptrs = result.FeatureRecords[0].SpatialPointers;
        Assert.Equal(2, ptrs.Count);
        Assert.Equal(1, ptrs[0].Name.RecordId);
        Assert.Equal(4, ptrs[1].Name.RecordId);
    }

    [Fact]
    public void ApplyChanges_FspcModify_ReplacesPointersAtIndex()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1,
                    spatialPointers:
                    [
                        SpatialPtr(S57RecordNameCodes.Edge, 1),
                        SpatialPtr(S57RecordNameCodes.Edge, 2),
                        SpatialPtr(S57RecordNameCodes.Edge, 3)
                    ])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, ruin: S57UpdateInstruction.Modify,
                    spatialPointerControl: new S57FieldUpdateControl
                    {
                        UpdateInstruction = S57UpdateInstruction.Modify,
                        Index = 2, // 1-based → replace at position 2
                        Count = 1
                    },
                    spatialPointers: [SpatialPtr(S57RecordNameCodes.Edge, 99)])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        var ptrs = result.FeatureRecords[0].SpatialPointers;
        Assert.Equal(3, ptrs.Count);
        Assert.Equal(1, ptrs[0].Name.RecordId);
        Assert.Equal(99, ptrs[1].Name.RecordId);
        Assert.Equal(3, ptrs[2].Name.RecordId);
    }

    #endregion

    #region Coordinate Array Splice Tests (SGCC)

    [Fact]
    public void ApplyChanges_SgccInsert_InsertsCoordinatesAtIndex()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords =
            [
                CreateVector(1,
                    coordinates: [Coord(100, 200), Coord(300, 400), Coord(500, 600)])
            ]
        };
        var update = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords =
            [
                CreateVector(1, ruin: S57UpdateInstruction.Modify,
                    coordinateControl: new S57FieldUpdateControl
                    {
                        UpdateInstruction = S57UpdateInstruction.Insert,
                        Index = 3, // 1-based → insert before position 3
                        Count = 1
                    },
                    coordinates: [Coord(999, 888)])
            ]
        };

        var result = baseDoc.ApplyChanges(update);

        var coords = result.VectorRecords[0].Coordinates2D;
        Assert.Equal(4, coords.Count);
        Assert.Equal(100, coords[0].X);
        Assert.Equal(300, coords[1].X);
        Assert.Equal(999, coords[2].X);
        Assert.Equal(500, coords[3].X);
    }

    [Fact]
    public void ApplyChanges_SgccDelete_RemovesCoordinatesAtIndex()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords =
            [
                CreateVector(1,
                    coordinates: [Coord(10, 20), Coord(30, 40), Coord(50, 60), Coord(70, 80)])
            ]
        };
        var update = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords =
            [
                CreateVector(1, ruin: S57UpdateInstruction.Modify,
                    coordinateControl: new S57FieldUpdateControl
                    {
                        UpdateInstruction = S57UpdateInstruction.Delete,
                        Index = 1,
                        Count = 2
                    })
            ]
        };

        var result = baseDoc.ApplyChanges(update);

        var coords = result.VectorRecords[0].Coordinates2D;
        Assert.Equal(2, coords.Count);
        Assert.Equal(50, coords[0].X);
        Assert.Equal(70, coords[1].X);
    }

    [Fact]
    public void ApplyChanges_SgccModify_ReplacesCoordinatesAtIndex()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords =
            [
                CreateVector(1,
                    coordinates: [Coord(10, 20), Coord(30, 40), Coord(50, 60)])
            ]
        };
        var update = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords =
            [
                CreateVector(1, ruin: S57UpdateInstruction.Modify,
                    coordinateControl: new S57FieldUpdateControl
                    {
                        UpdateInstruction = S57UpdateInstruction.Modify,
                        Index = 2,
                        Count = 1
                    },
                    coordinates: [Coord(999, 888)])
            ]
        };

        var result = baseDoc.ApplyChanges(update);

        var coords = result.VectorRecords[0].Coordinates2D;
        Assert.Equal(3, coords.Count);
        Assert.Equal(10, coords[0].X);
        Assert.Equal(999, coords[1].X);
        Assert.Equal(50, coords[2].X);
    }

    #endregion

    #region No Control Field — Wholesale Replacement

    [Fact]
    public void ApplyChanges_ModifyWithoutControl_ReplacesArrayWholesale()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1,
                    spatialPointers:
                    [
                        SpatialPtr(S57RecordNameCodes.Edge, 1),
                        SpatialPtr(S57RecordNameCodes.Edge, 2)
                    ])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, ruin: S57UpdateInstruction.Modify,
                    spatialPointers:
                    [
                        SpatialPtr(S57RecordNameCodes.Edge, 50),
                        SpatialPtr(S57RecordNameCodes.Edge, 60),
                        SpatialPtr(S57RecordNameCodes.Edge, 70)
                    ])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        var ptrs = result.FeatureRecords[0].SpatialPointers;
        Assert.Equal(3, ptrs.Count);
        Assert.Equal(50, ptrs[0].Name.RecordId);
        Assert.Equal(60, ptrs[1].Name.RecordId);
        Assert.Equal(70, ptrs[2].Name.RecordId);
    }

    [Fact]
    public void ApplyChanges_ModifyWithoutControlAndNoUpdateData_PreservesBaseArray()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords =
            [
                CreateVector(1, coordinates: [Coord(10, 20), Coord(30, 40)])
            ]
        };
        var update = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords =
            [
                CreateVector(1, ruin: S57UpdateInstruction.Modify,
                    attributes: [Attr(1, "val")])
            ]
        };

        var result = baseDoc.ApplyChanges(update);

        var coords = result.VectorRecords[0].Coordinates2D;
        Assert.Equal(2, coords.Count);
        Assert.Equal(10, coords[0].X);
        Assert.Equal(30, coords[1].X);
    }

    #endregion

    #region DataSet Metadata Tests

    [Fact]
    public void ApplyChanges_UpdateDsid_TakesUpdateDsid()
    {
        var baseDsid = new S57DataSetIdentification { UpdateNumber = "0", EditionNumber = "2" };
        var updateDsid = new S57DataSetIdentification { UpdateNumber = "1", EditionNumber = "2" };
        var baseDoc = new S57Document
        {
            DataSetIdentification = baseDsid,
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            DataSetIdentification = updateDsid,
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Equal("1", result.DataSetIdentification?.UpdateNumber);
    }

    [Fact]
    public void ApplyChanges_NullUpdateDsid_PreservesBaseDsid()
    {
        var baseDsid = new S57DataSetIdentification { UpdateNumber = "0" };
        var baseDoc = new S57Document
        {
            DataSetIdentification = baseDsid,
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            DataSetIdentification = null,
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Equal("0", result.DataSetIdentification?.UpdateNumber);
    }

    #endregion

    #region Record Order Preservation

    [Fact]
    public void ApplyChanges_PreservesOriginalRecordOrder()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(3), CreateFeature(1), CreateFeature(2)
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, ruin: S57UpdateInstruction.Modify,
                    attributes: [Attr(1, "updated")])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Equal(3, result.FeatureRecords.Count);
        Assert.Equal(3, result.FeatureRecords[0].RecordName.RecordId);
        Assert.Equal(1, result.FeatureRecords[1].RecordName.RecordId);
        Assert.Equal(2, result.FeatureRecords[2].RecordName.RecordId);
    }

    [Fact]
    public void ApplyChanges_InsertedRecordsAppearAtEnd()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = [CreateFeature(1), CreateFeature(2)],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords = [CreateFeature(3, ruin: S57UpdateInstruction.Insert)],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        Assert.Equal(3, result.FeatureRecords.Count);
        Assert.Equal(3, result.FeatureRecords[2].RecordName.RecordId);
    }

    #endregion

    #region Feature Pointer Array Splice Tests (FFPC)

    [Fact]
    public void ApplyChanges_FfpcInsert_InsertsFeaturePointersAtIndex()
    {
        var basePtr = new S57FeaturePointer
        {
            Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, 100),
            Relationship = S57RelationshipIndicator.Master,
            Comment = ""
        };
        var newPtr = new S57FeaturePointer
        {
            Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, 200),
            Relationship = S57RelationshipIndicator.Slave,
            Comment = ""
        };
        var baseDoc = new S57Document
        {
            FeatureRecords = [CreateFeature(1, featurePointers: [basePtr])],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };
        var update = new S57Document
        {
            FeatureRecords =
            [
                CreateFeature(1, ruin: S57UpdateInstruction.Modify,
                    featurePointerControl: new S57FieldUpdateControl
                    {
                        UpdateInstruction = S57UpdateInstruction.Insert,
                        Index = 2,
                        Count = 1
                    },
                    featurePointers: [newPtr])
            ],
            VectorRecords = ImmutableArray<S57VectorRecord>.Empty
        };

        var result = baseDoc.ApplyChanges(update);

        var ptrs = result.FeatureRecords[0].FeaturePointers;
        Assert.Equal(2, ptrs.Count);
        Assert.Equal(100, ptrs[0].Name.RecordId);
        Assert.Equal(200, ptrs[1].Name.RecordId);
    }

    #endregion

    #region Vector Pointer Array Splice Tests (VRPC)

    [Fact]
    public void ApplyChanges_VrpcDelete_RemovesVectorPointersAtIndex()
    {
        var ptr1 = new S57VectorPointer
        {
            Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, 1),
            Orientation = S57Orientation.Forward,
            Usage = S57UsageIndicator.NotApplicable,
            Topology = S57TopologyIndicator.Beginning,
            Mask = S57MaskingIndicator.NotApplicable
        };
        var ptr2 = new S57VectorPointer
        {
            Name = S57RecordName.FromRcnmRcid(S57RecordNameCodes.ConnectedNode, 2),
            Orientation = S57Orientation.Forward,
            Usage = S57UsageIndicator.NotApplicable,
            Topology = S57TopologyIndicator.End,
            Mask = S57MaskingIndicator.NotApplicable
        };
        var baseDoc = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords = [CreateVector(10, vectorPointers: [ptr1, ptr2])]
        };
        var update = new S57Document
        {
            FeatureRecords = ImmutableArray<S57FeatureRecord>.Empty,
            VectorRecords =
            [
                CreateVector(10, ruin: S57UpdateInstruction.Modify,
                    vectorPointerControl: new S57FieldUpdateControl
                    {
                        UpdateInstruction = S57UpdateInstruction.Delete,
                        Index = 1,
                        Count = 1
                    })
            ]
        };

        var result = baseDoc.ApplyChanges(update);

        var ptrs = result.VectorRecords[0].VectorPointers;
        Assert.Single(ptrs);
        Assert.Equal(2, ptrs[0].Name.RecordId);
    }

    #endregion

    #region Empty Update Document

    [Fact]
    public void ApplyChanges_EmptyUpdate_ReturnsEquivalentDocument()
    {
        var baseDoc = new S57Document
        {
            FeatureRecords = [CreateFeature(1), CreateFeature(2)],
            VectorRecords = [CreateVector(10)]
        };
        var update = CreateEmptyDocument();

        var result = baseDoc.ApplyChanges(update);

        Assert.Equal(2, result.FeatureRecords.Count);
        Assert.Single(result.VectorRecords);
    }

    #endregion
}
