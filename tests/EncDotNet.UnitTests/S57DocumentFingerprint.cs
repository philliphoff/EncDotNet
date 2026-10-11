using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using EncDotNet.S57;

namespace EncDotNet.UnitTests;

/// <summary>
/// Reduces an <see cref="S57Document"/> to a SHA-256 hash of a canonical text dump of every
/// value it holds, so tests can pin a reader's exact output across internal rewrites.
/// </summary>
internal static class S57DocumentFingerprint
{
    public static string Compute(S57Document document)
    {
        var text = new StringBuilder();
        text.AppendLine(document.DataSetIdentification?.ToString() ?? "no DSID");
        text.AppendLine(document.DataSetParameters?.ToString() ?? "no DSPM");

        foreach (var feature in document.FeatureRecords)
        {
            text.Append("F ").Append(Name(feature.RecordName))
                .Append(' ').Append((int)feature.Primitive)
                .Append(' ').Append(feature.Group)
                .Append(' ').Append((int)feature.ObjectCode)
                .Append(' ').Append(feature.RecordVersion)
                .Append(' ').Append((int)feature.UpdateInstruction)
                .AppendLine();
            Attributes(text, "A", feature.Attributes);
            Attributes(text, "N", feature.NationalAttributes);
            foreach (var p in feature.SpatialPointers)
            {
                text.Append(" S ").Append(Name(p.Name)).Append(' ')
                    .Append((int)p.Orientation).Append(' ').Append((int)p.Usage).Append(' ').Append((int)p.Mask).AppendLine();
            }

            foreach (var p in feature.FeaturePointers)
            {
                text.Append(" P ").Append(Name(p.Name)).Append(' ')
                    .Append((int)p.Relationship).Append(' ').Append(p.Comment).AppendLine();
            }

            Control(text, "SC", feature.SpatialPointerControl);
            Control(text, "PC", feature.FeaturePointerControl);
        }

        foreach (var vector in document.VectorRecords)
        {
            text.Append("V ").Append(Name(vector.RecordName))
                .Append(' ').Append(vector.RecordVersion)
                .Append(' ').Append((int)vector.UpdateInstruction)
                .AppendLine();
            Attributes(text, "A", vector.Attributes);
            foreach (var p in vector.VectorPointers)
            {
                text.Append(" V ").Append(Name(p.Name)).Append(' ')
                    .Append((int)p.Orientation).Append(' ').Append((int)p.Usage).Append(' ')
                    .Append((int)p.Topology).Append(' ').Append((int)p.Mask).AppendLine();
            }

            foreach (var c in vector.Coordinates2D)
            {
                text.Append(" C ").Append(c.Y).Append(' ').Append(c.X).AppendLine();
            }

            foreach (var s in vector.Soundings)
            {
                text.Append(" D ").Append(s.Y).Append(' ').Append(s.X).Append(' ').Append(s.Depth).AppendLine();
            }

            Control(text, "VC", vector.VectorPointerControl);
            Control(text, "CC", vector.CoordinateControl);
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString())));
    }

    private static string Name(S57RecordName name) => string.Create(
        CultureInfo.InvariantCulture,
        $"{name.RecordNameCode}/{name.RecordId}/{name.AgencyCode}/{name.FeatureId}/{name.FeatureSubdivision}");

    private static void Attributes(StringBuilder text, string kind, IReadOnlyList<S57AttributeValue> attributes)
    {
        foreach (var a in attributes)
        {
            text.Append(' ').Append(kind).Append(' ').Append(a.AttributeCode).Append('=').Append(a.Value).AppendLine();
        }
    }

    private static void Control(StringBuilder text, string kind, S57FieldUpdateControl? control)
    {
        if (control is { } c)
        {
            text.Append(' ').Append(kind).Append(' ')
                .Append((int)c.UpdateInstruction).Append(' ').Append(c.Index).Append(' ').Append(c.Count).AppendLine();
        }
    }
}
