using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    /// Gets a feature record by its long name (LNAM), such as the target of a feature pointer.
    /// </summary>
    /// <param name="longName">The long name to find.</param>
    /// <returns>The first feature record with that long name, or <c>null</c> if there is none.</returns>
    /// <remarks>
    /// This searches every feature record. To resolve many pointers, build an
    /// <see cref="Charts.S57Chart"/> and use <see cref="Charts.S57Chart.GetFeature(S57LongName)"/>,
    /// which uses an index.
    /// </remarks>
    public S57FeatureRecord? GetFeatureRecord(S57LongName longName)
    {
        foreach (var record in FeatureRecords)
        {
            if (record.LongName == longName)
            {
                return record;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a feature record by its record name.
    /// </summary>
    /// <remarks>
    /// This searches every feature record. To look up many records, build an
    /// <see cref="Charts.S57Chart"/>, whose lookups use an index.
    /// </remarks>
    public S57FeatureRecord? GetFeatureRecord(S57RecordName name)
    {
        foreach (var record in FeatureRecords)
        {
            if (SameRecord(record.RecordName, name))
            {
                return record;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a vector record by its record name.
    /// </summary>
    /// <remarks>
    /// This searches every vector record. To look up many records, build an
    /// <see cref="Charts.S57Chart"/>, whose lookups use an index.
    /// </remarks>
    public S57VectorRecord? GetVectorRecord(S57RecordName name)
    {
        foreach (var record in VectorRecords)
        {
            if (SameRecord(record.RecordName, name))
            {
                return record;
            }
        }

        return null;
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
    /// <para>
    /// A control field's 1-based index and count must address entries that exist in the
    /// base record's array (S-57 Part 3, 8.4.3). An insert may target any position from 1
    /// to one past the last entry; a delete or modify must lie entirely within the array.
    /// A control field that falls outside these bounds means the update does not match
    /// the base cell, and no partial result is returned.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidDataException">An update control field (FSPC, FFPC, VRPC or SGCC)
    /// has an index or count outside the bounds of the base record's array. The message names
    /// the record (RCNM/RCID) and the control field.</exception>
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
        // Records by (RCNM, RCID), and their keys in output order. A delete leaves its key in
        // the order rather than searching the list for it; the output skips keys that no
        // longer have a record. A key inserted again after a delete is appended at its new
        // position, and only that position outputs its record.
        var records = new Dictionary<(int, int), T>(baseRecords.Count);
        var order = new List<(int, int)>(baseRecords.Count);
        HashSet<(int, int)>? deleted = null;
        Dictionary<(int, int), int>? reinsertedAt = null;

        foreach (var record in baseRecords)
        {
            var key = getKey(record);
            if (!records.TryAdd(key, record))
            {
                // A base that repeats a record name is malformed. Merge it as the list-based
                // merge always has, so its output doesn't change.
                return ApplyRecordChangesToRepeatedNames(baseRecords, updateRecords, getKey, applyModify);
            }

            order.Add(key);
        }

        foreach (var updateRecord in updateRecords)
        {
            var key = getKey(updateRecord);
            var instruction = GetUpdateInstruction(updateRecord);

            switch (instruction)
            {
                case S57UpdateInstruction.Insert:
                    ref var inserted = ref CollectionsMarshal.GetValueRefOrAddDefault(records, key, out var exists);
                    inserted = updateRecord;
                    if (!exists)
                    {
                        if (deleted is not null && deleted.Contains(key))
                        {
                            (reinsertedAt ??= [])[key] = order.Count;
                        }

                        order.Add(key);
                    }
                    break;

                case S57UpdateInstruction.Delete:
                    if (records.Remove(key))
                    {
                        (deleted ??= []).Add(key);
                    }
                    break;

                case S57UpdateInstruction.Modify:
                    ref var modified = ref CollectionsMarshal.GetValueRefOrNullRef(records, key);
                    if (!Unsafe.IsNullRef(ref modified))
                    {
                        modified = applyModify(modified, updateRecord);
                    }
                    break;
            }
        }

        var builder = ImmutableArray.CreateBuilder<T>(records.Count);
        for (var position = 0; position < order.Count; position++)
        {
            var key = order[position];
            if (records.TryGetValue(key, out var record) &&
                (reinsertedAt is null || !reinsertedAt.TryGetValue(key, out var current) || current == position))
            {
                builder.Add(record);
            }
        }

        return builder.MoveToImmutable();
    }

    // The list-based merge, kept for bases that repeat a record name: each appearance of a
    // name outputs its record, and a delete removes only the name's first appearance.
    private static ImmutableArray<T> ApplyRecordChangesToRepeatedNames<T>(
        IReadOnlyList<T> baseRecords,
        IReadOnlyList<T> updateRecords,
        Func<T, (int Rcnm, int Rcid)> getKey,
        Func<T, T, T> applyModify)
        where T : class
    {
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
            switch (GetUpdateInstruction(updateRecord))
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

    private static bool SameRecord(S57RecordName a, S57RecordName b) =>
        a.RecordNameCode == b.RecordNameCode && a.RecordId == b.RecordId;

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
            SpatialPointers = ApplyArrayUpdate([.. baseRecord.SpatialPointers], [.. updateRecord.SpatialPointers], updateRecord.SpatialPointerControl, baseRecord.RecordName, S57FieldTags.FSPC),
            FeaturePointers = ApplyArrayUpdate([.. baseRecord.FeaturePointers], [.. updateRecord.FeaturePointers], updateRecord.FeaturePointerControl, baseRecord.RecordName, S57FieldTags.FFPC),
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
            VectorPointers = ApplyArrayUpdate([.. baseRecord.VectorPointers], [.. updateRecord.VectorPointers], updateRecord.VectorPointerControl, baseRecord.RecordName, S57FieldTags.VRPC),
            Coordinates2D = ApplyArrayUpdate([.. baseRecord.Coordinates2D], [.. updateRecord.Coordinates2D], updateRecord.CoordinateControl, baseRecord.RecordName, S57FieldTags.SGCC),
            Soundings = ApplyArrayUpdate([.. baseRecord.Soundings], [.. updateRecord.Soundings], updateRecord.CoordinateControl, baseRecord.RecordName, S57FieldTags.SGCC),
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
        S57FieldUpdateControl? control,
        S57RecordName recordName,
        string controlFieldTag)
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

        // SGCC controls both SG2D and SG3D, so the splice helpers leave an array untouched when
        // the update carries no data for it (or, for a delete, the base has none) before
        // checking the range against it.
        return ctrl.UpdateInstruction switch
        {
            S57UpdateInstruction.Insert => SpliceInsert(baseArray, updateData, index, ctrl, recordName, controlFieldTag),
            S57UpdateInstruction.Delete => SpliceDelete(baseArray, index, ctrl, recordName, controlFieldTag),
            S57UpdateInstruction.Modify => SpliceModify(baseArray, updateData, index, ctrl, recordName, controlFieldTag),
            _ => baseArray
        };
    }

    private static ImmutableArray<T> SpliceInsert<T>(
        ImmutableArray<T> baseArray,
        ImmutableArray<T> items,
        int index,
        S57FieldUpdateControl ctrl,
        S57RecordName recordName,
        string controlFieldTag)
        where T : struct
    {
        if (items.IsDefaultOrEmpty)
        {
            return baseArray;
        }

        // An insert may target any existing position or one past the end (an append).
        if (index < 0 || index > baseArray.Length)
        {
            throw OutOfRange(ctrl, baseArray.Length, recordName, controlFieldTag);
        }

        return baseArray.InsertRange(index, items);
    }

    private static ImmutableArray<T> SpliceDelete<T>(
        ImmutableArray<T> baseArray,
        int index,
        S57FieldUpdateControl ctrl,
        S57RecordName recordName,
        string controlFieldTag)
        where T : struct
    {
        if (ctrl.Count <= 0 || baseArray.IsDefaultOrEmpty)
        {
            return baseArray;
        }

        EnsureRangeInBounds(baseArray.Length, index, ctrl, recordName, controlFieldTag);

        return baseArray.RemoveRange(index, ctrl.Count);
    }

    private static ImmutableArray<T> SpliceModify<T>(
        ImmutableArray<T> baseArray,
        ImmutableArray<T> items,
        int index,
        S57FieldUpdateControl ctrl,
        S57RecordName recordName,
        string controlFieldTag)
        where T : struct
    {
        if (items.IsDefaultOrEmpty)
        {
            return baseArray;
        }

        EnsureRangeInBounds(baseArray.Length, index, ctrl, recordName, controlFieldTag);

        // Modify = delete the existing range, then insert the new items at the same position.
        var withRemoved = baseArray.RemoveRange(index, ctrl.Count);
        return withRemoved.InsertRange(index, items);
    }

    private static void EnsureRangeInBounds(
        int length,
        int index,
        S57FieldUpdateControl ctrl,
        S57RecordName recordName,
        string controlFieldTag)
    {
        // Written as a subtraction so a large count cannot overflow index + count.
        if (index < 0 || index > length || ctrl.Count < 0 || ctrl.Count > length - index)
        {
            throw OutOfRange(ctrl, length, recordName, controlFieldTag);
        }
    }

    private static InvalidDataException OutOfRange(
        S57FieldUpdateControl ctrl,
        int length,
        S57RecordName recordName,
        string controlFieldTag)
    {
        return new InvalidDataException(
            $"Update control field {controlFieldTag} on record {recordName} " +
            $"({ctrl.UpdateInstruction} at index {ctrl.Index}, count {ctrl.Count}) is outside " +
            $"the base record's {length} entries (S-57 Part 3, 8.4.3). " +
            "The update does not match the base cell.");
    }
}
