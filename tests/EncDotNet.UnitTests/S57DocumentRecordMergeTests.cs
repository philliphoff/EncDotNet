using System.Collections.Immutable;
using EncDotNet.S57;

namespace EncDotNet.UnitTests;

/// <summary>
/// Checks that <see cref="S57Document.ApplyChanges"/> orders the merged records exactly as its
/// earlier list-based merge did, including for base cells that repeat a record name.
/// </summary>
public class S57DocumentRecordMergeTests
{
    [Fact]
    public void ApplyChanges_RandomUpdates_MatchListBasedMerge()
    {
        var random = new Random(8211);
        for (var trial = 0; trial < 2_000; trial++)
        {
            var source = 0;
            var keySpace = random.Next(1, 8);

            // A base cell, sometimes repeating a record name.
            var baseRecords = new List<S57FeatureRecord>();
            for (var i = random.Next(0, 8); i > 0; i--)
            {
                baseRecords.Add(Feature(random.Next(keySpace), ++source, version: 1, S57UpdateInstruction.Insert));
            }

            var updateRecords = new List<S57FeatureRecord>();
            for (var i = random.Next(0, 12); i > 0; i--)
            {
                var instruction = (S57UpdateInstruction)random.Next(1, 4);
                updateRecords.Add(Feature(random.Next(keySpace), ++source, version: 2 + i, instruction));
            }

            var document = new S57Document { FeatureRecords = baseRecords, VectorRecords = [] };
            var actual = document.ApplyChanges(new S57Document { FeatureRecords = updateRecords, VectorRecords = [] })
                .FeatureRecords
                .Select(r => (r.RecordName.RecordId, (int)r.ObjectCode, r.RecordVersion));

            Assert.Equal(ListBasedMerge(baseRecords, updateRecords), actual);
        }
    }

    // The merge as it was before it counted appearances: a list of keys searched with
    // Contains and Remove. A modify keeps the base record's object code and takes the
    // update's version, as ApplyChanges does.
    private static List<(int, int, int)> ListBasedMerge(List<S57FeatureRecord> baseRecords, List<S57FeatureRecord> updateRecords)
    {
        var records = new Dictionary<int, (int ObjectCode, int Version)>();
        var order = new List<int>();
        foreach (var r in baseRecords)
        {
            records[r.RecordName.RecordId] = ((int)r.ObjectCode, r.RecordVersion);
            order.Add(r.RecordName.RecordId);
        }

        foreach (var r in updateRecords)
        {
            var key = r.RecordName.RecordId;
            switch (r.UpdateInstruction)
            {
                case S57UpdateInstruction.Insert:
                    records[key] = ((int)r.ObjectCode, r.RecordVersion);
                    if (!order.Contains(key))
                    {
                        order.Add(key);
                    }

                    break;
                case S57UpdateInstruction.Delete:
                    records.Remove(key);
                    order.Remove(key);
                    break;
                case S57UpdateInstruction.Modify:
                    if (records.TryGetValue(key, out var existing))
                    {
                        records[key] = (existing.ObjectCode, r.RecordVersion);
                    }

                    break;
            }
        }

        return [.. order.Where(records.ContainsKey).Select(k => (k, records[k].ObjectCode, records[k].Version))];
    }

    private static S57FeatureRecord Feature(int rcid, int source, int version, S57UpdateInstruction instruction) => new()
    {
        RecordName = S57RecordName.FromRcnmRcid(S57RecordNameCodes.Feature, rcid),
        Primitive = S57GeometricPrimitive.None,
        Group = 2,
        ObjectCode = (S57ObjectCode)source,
        RecordVersion = version,
        UpdateInstruction = instruction,
        Attributes = ImmutableArray<S57AttributeValue>.Empty,
        NationalAttributes = ImmutableArray<S57AttributeValue>.Empty,
        SpatialPointers = ImmutableArray<S57SpatialPointer>.Empty,
        FeaturePointers = ImmutableArray<S57FeaturePointer>.Empty,
    };
}
