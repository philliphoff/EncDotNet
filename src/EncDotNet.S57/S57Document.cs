using System.Collections.Immutable;

namespace EncDotNet.S57;

/// <summary>
/// Represents a complete S-57 Electronic Navigational Chart (ENC) document.
/// </summary>
public sealed record S57Document
{
    /// <summary>Gets the data set identification (DSID field).</summary>
    public S57DataSetIdentification? DataSetIdentification { get; init; }

    /// <summary>Gets the data set parameters (DSPM field).</summary>
    public S57DataSetParameters? DataSetParameters { get; init; }

    /// <summary>Gets all feature records.</summary>
    public IReadOnlyList<S57FeatureRecord> FeatureRecords { get; init; } = [];

    /// <summary>Gets all vector (spatial) records.</summary>
    public IReadOnlyList<S57VectorRecord> VectorRecords { get; init; } = [];

    /// <summary>
    /// Gets a feature record by its record name.
    /// </summary>
    public S57FeatureRecord? GetFeatureRecord(S57RecordName name)
    {
        return FeatureRecords.FirstOrDefault(r =>
            r.RecordName.RecordNameCode == name.RecordNameCode &&
            r.RecordName.RecordId == name.RecordId);
    }

    /// <summary>
    /// Gets a vector record by its record name.
    /// </summary>
    public S57VectorRecord? GetVectorRecord(S57RecordName name)
    {
        return VectorRecords.FirstOrDefault(r =>
            r.RecordName.RecordNameCode == name.RecordNameCode &&
            r.RecordName.RecordId == name.RecordId);
    }

    /// <summary>
    /// Gets all feature records with the specified object code.
    /// </summary>
    public IEnumerable<S57FeatureRecord> GetFeaturesByObjectCode(S57ObjectCode objectCode)
    {
        return FeatureRecords.Where(r => r.ObjectCode == objectCode);
    }

    /// <summary>
    /// Gets the coordinate multiplication factor for converting integer coordinates to decimal degrees.
    /// </summary>
    public int CoordinateMultiplicationFactor =>
        DataSetParameters?.CoordinateMultiplicationFactor ?? 10000000;

    /// <summary>
    /// Gets the sounding multiplication factor for converting integer soundings to real values.
    /// </summary>
    public int SoundingMultiplicationFactor =>
        DataSetParameters?.SoundingMultiplicationFactor ?? 10;

    /// <summary>
    /// Applies an S-57 update document to this document and returns a new document reflecting the changes.
    /// </summary>
    /// <param name="update">An update document (typically parsed from a .001, .002, etc. file)
    /// whose feature and vector records carry <see cref="S57UpdateInstruction"/> values indicating
    /// insert, delete, or modify operations.</param>
    /// <returns>A new <see cref="S57Document"/> with the updates applied.</returns>
    /// <remarks>
    /// <para>
    /// Record-level operations are driven by the <c>RUIN</c> field on each update record:
    /// <list type="bullet">
    ///   <item><description><see cref="S57UpdateInstruction.Insert"/> — adds the record to the document.</description></item>
    ///   <item><description><see cref="S57UpdateInstruction.Delete"/> — removes the record identified by RCNM+RCID.</description></item>
    ///   <item><description><see cref="S57UpdateInstruction.Modify"/> — updates the matching record in place.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// For <see cref="S57UpdateInstruction.Modify"/>:
    /// <list type="bullet">
    ///   <item><description>Attributes (ATTF/NATF) are merged by ATTL code; an empty ATVL deletes the attribute.</description></item>
    ///   <item><description>Array fields (FSPT, FFPT, VRPT, SG2D, SG3D) are spliced using their control fields
    ///   (FSPC, FFPC, VRPC, SGCC) when present.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public S57Document ApplyChanges(S57Document update)
    {
        var features = ApplyRecordChanges(
            FeatureRecords,
            update.FeatureRecords,
            r => (r.RecordName.RecordNameCode, r.RecordName.RecordId),
            ApplyFeatureModify);

        var vectors = ApplyRecordChanges(
            VectorRecords,
            update.VectorRecords,
            r => (r.RecordName.RecordNameCode, r.RecordName.RecordId),
            ApplyVectorModify);

        return new S57Document
        {
            DataSetIdentification = update.DataSetIdentification ?? DataSetIdentification,
            DataSetParameters = update.DataSetParameters ?? DataSetParameters,
            FeatureRecords = features,
            VectorRecords = vectors
        };
    }

    private static ImmutableArray<T> ApplyRecordChanges<T>(
        IReadOnlyList<T> baseRecords,
        IReadOnlyList<T> updateRecords,
        Func<T, (int Rcnm, int Rcid)> getKey,
        Func<T, T, T> applyModify)
        where T : class
    {
        // Build a mutable dictionary keyed by (RCNM, RCID) for efficient lookup.
        // Use a list to preserve insertion order for records not touched by updates.
        var dict = new Dictionary<(int, int), T>(baseRecords.Count);
        var orderedKeys = new List<(int, int)>(baseRecords.Count);

        foreach (var record in baseRecords)
        {
            var key = getKey(record);
            dict[key] = record;
            orderedKeys.Add(key);
        }

        foreach (var updateRecord in updateRecords)
        {
            var key = getKey(updateRecord);
            var instruction = GetUpdateInstruction(updateRecord);

            switch (instruction)
            {
                case S57UpdateInstruction.Insert:
                    dict[key] = updateRecord;
                    if (!orderedKeys.Contains(key))
                    {
                        orderedKeys.Add(key);
                    }
                    break;

                case S57UpdateInstruction.Delete:
                    dict.Remove(key);
                    orderedKeys.Remove(key);
                    break;

                case S57UpdateInstruction.Modify:
                    if (dict.TryGetValue(key, out var existing))
                    {
                        dict[key] = applyModify(existing, updateRecord);
                    }
                    break;
            }
        }

        var builder = ImmutableArray.CreateBuilder<T>(orderedKeys.Count);
        foreach (var key in orderedKeys)
        {
            if (dict.TryGetValue(key, out var record))
            {
                builder.Add(record);
            }
        }

        return builder.ToImmutable();
    }

    private static S57UpdateInstruction GetUpdateInstruction<T>(T record) => record switch
    {
        S57FeatureRecord f => f.UpdateInstruction,
        S57VectorRecord v => v.UpdateInstruction,
        _ => throw new InvalidOperationException($"Unexpected record type: {typeof(T).Name}")
    };

    private static S57FeatureRecord ApplyFeatureModify(S57FeatureRecord baseRecord, S57FeatureRecord updateRecord)
    {
        return new S57FeatureRecord
        {
            RecordName = baseRecord.RecordName,
            Primitive = baseRecord.Primitive,
            Group = baseRecord.Group,
            ObjectCode = baseRecord.ObjectCode,
            RecordVersion = updateRecord.RecordVersion,
            UpdateInstruction = updateRecord.UpdateInstruction,
            Attributes = MergeAttributes(baseRecord.Attributes, updateRecord.Attributes),
            NationalAttributes = MergeAttributes(baseRecord.NationalAttributes, updateRecord.NationalAttributes),
            SpatialPointers = ApplyArrayUpdate([.. baseRecord.SpatialPointers], [.. updateRecord.SpatialPointers], updateRecord.SpatialPointerControl),
            FeaturePointers = ApplyArrayUpdate([.. baseRecord.FeaturePointers], [.. updateRecord.FeaturePointers], updateRecord.FeaturePointerControl),
        };
    }

    private static S57VectorRecord ApplyVectorModify(S57VectorRecord baseRecord, S57VectorRecord updateRecord)
    {
        return new S57VectorRecord
        {
            RecordName = baseRecord.RecordName,
            RecordVersion = updateRecord.RecordVersion,
            UpdateInstruction = updateRecord.UpdateInstruction,
            Attributes = MergeAttributes(baseRecord.Attributes, updateRecord.Attributes),
            VectorPointers = ApplyArrayUpdate([.. baseRecord.VectorPointers], [.. updateRecord.VectorPointers], updateRecord.VectorPointerControl),
            Coordinates2D = ApplyArrayUpdate([.. baseRecord.Coordinates2D], [.. updateRecord.Coordinates2D], updateRecord.CoordinateControl),
            Soundings = ApplyArrayUpdate([.. baseRecord.Soundings], [.. updateRecord.Soundings], updateRecord.CoordinateControl),
        };
    }

    private static IReadOnlyList<S57AttributeValue> MergeAttributes(
        IReadOnlyList<S57AttributeValue> baseAttrs,
        IReadOnlyList<S57AttributeValue> updateAttrs)
    {
        if (updateAttrs.Count == 0)
        {
            return baseAttrs;
        }

        // Build a dictionary from base attributes, keyed by ATTL.
        var merged = new Dictionary<int, S57AttributeValue>(baseAttrs.Count);
        foreach (var attr in baseAttrs)
        {
            merged[attr.AttributeCode] = attr;
        }

        // Apply updates: empty value means delete, otherwise insert/replace.
        foreach (var attr in updateAttrs)
        {
            if (string.IsNullOrEmpty(attr.Value))
            {
                merged.Remove(attr.AttributeCode);
            }
            else
            {
                merged[attr.AttributeCode] = attr;
            }
        }

        // Preserve the order: base attributes first (keeping survivors), then new ones.
        var builder = ImmutableArray.CreateBuilder<S57AttributeValue>(merged.Count);
        var added = new HashSet<int>();

        foreach (var attr in baseAttrs)
        {
            if (merged.ContainsKey(attr.AttributeCode))
            {
                builder.Add(merged[attr.AttributeCode]);
                added.Add(attr.AttributeCode);
            }
        }

        foreach (var attr in updateAttrs)
        {
            if (!added.Contains(attr.AttributeCode) && merged.ContainsKey(attr.AttributeCode))
            {
                builder.Add(merged[attr.AttributeCode]);
                added.Add(attr.AttributeCode);
            }
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<T> ApplyArrayUpdate<T>(
        ImmutableArray<T> baseArray,
        ImmutableArray<T> updateData,
        S57FieldUpdateControl? control)
        where T : struct
    {
        if (control is null)
        {
            // No control field — if update data is present, replace wholesale; otherwise keep base.
            return updateData.IsDefaultOrEmpty ? baseArray : updateData;
        }

        var ctrl = control.Value;
        // Convert from 1-based S-57 index to 0-based.
        int index = ctrl.Index - 1;

        return ctrl.UpdateInstruction switch
        {
            S57UpdateInstruction.Insert => SpliceInsert(baseArray, updateData, index),
            S57UpdateInstruction.Delete => SpliceDelete(baseArray, index, ctrl.Count),
            S57UpdateInstruction.Modify => SpliceModify(baseArray, updateData, index, ctrl.Count),
            _ => baseArray
        };
    }

    private static ImmutableArray<T> SpliceInsert<T>(ImmutableArray<T> baseArray, ImmutableArray<T> items, int index)
        where T : struct
    {
        if (items.IsDefaultOrEmpty)
        {
            return baseArray;
        }

        return baseArray.InsertRange(index, items);
    }

    private static ImmutableArray<T> SpliceDelete<T>(ImmutableArray<T> baseArray, int index, int count)
        where T : struct
    {
        if (count <= 0 || baseArray.IsDefaultOrEmpty)
        {
            return baseArray;
        }

        return baseArray.RemoveRange(index, count);
    }

    private static ImmutableArray<T> SpliceModify<T>(ImmutableArray<T> baseArray, ImmutableArray<T> items, int index, int count)
        where T : struct
    {
        if (items.IsDefaultOrEmpty)
        {
            return baseArray;
        }

        // Modify = delete the existing range, then insert the new items at the same position.
        var withRemoved = baseArray.RemoveRange(index, count);
        return withRemoved.InsertRange(index, items);
    }
}
