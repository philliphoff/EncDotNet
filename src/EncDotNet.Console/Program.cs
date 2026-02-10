// Dump DEPCNT (object code 43) edge data for diagnosis

using EncDotNet.Enc;
using EncDotNet.Enc.Charts;

var chartPath = "../../.expanded/US5WA18M/ENC_ROOT/US5WA18M/US5WA18M.000";
var fullPath = Path.GetFullPath(chartPath);
Console.WriteLine($"Loading chart from: {fullPath}");

var chart = S57Chart.FromFile(fullPath);
Console.WriteLine($"Loaded. LineFeatures: {chart.LineFeatures.Length}");

int count = 0;
foreach (var lineFeature in chart.LineFeatures)
{
    if (lineFeature.ObjectCode != 43) continue; // DEPCNT only
    if (count >= 5) break;
    
    Console.WriteLine($"\n=== DEPCNT Feature {lineFeature.RecordName} ===");
    Console.WriteLine($"  EdgeCount: {lineFeature.EdgeCount}");
    
    int edgeIdx = 0;
    foreach (var edgeRef in lineFeature.EdgeReferences)
    {
        var edge = chart.GetEdge(edgeRef.Name);
        var reverse = edgeRef.Orientation == S57Orientation.Reverse;
        var orientedStart = edge == null ? null : (reverse ? edge.EndNode : edge.BeginningNode);
        var orientedEnd = edge == null ? null : (reverse ? edge.BeginningNode : edge.EndNode);
        
        Console.WriteLine($"  Edge[{edgeIdx}]: Name={edgeRef.Name}, Orientation={edgeRef.Orientation}, Mask={edgeRef.Mask} ({(int)edgeRef.Mask}), Usage={edgeRef.Usage} ({(int)edgeRef.Usage})");
        Console.WriteLine($"    BeginNode={edge?.BeginningNode}, EndNode={edge?.EndNode}");
        Console.WriteLine($"    OrientedStart={orientedStart}, OrientedEnd={orientedEnd}");
        Console.WriteLine($"    IntermediatePoints={edge?.IntermediatePoints.Length ?? 0}");
        
        if (edge != null)
        {
            // Show start/end node positions
            if (edge.HasBeginningNode)
            {
                var bn = chart.GetConnectedNode(edge.BeginningNode!.Value);
                if (bn != null)
                {
                    var (lon, lat) = chart.ToDecimalDegrees(bn.Position);
                    Console.WriteLine($"    BeginNodePos: ({lat:F7}, {lon:F7})");
                }
            }
            if (edge.HasEndNode)
            {
                var en = chart.GetConnectedNode(edge.EndNode!.Value);
                if (en != null)
                {
                    var (lon, lat) = chart.ToDecimalDegrees(en.Position);
                    Console.WriteLine($"    EndNodePos: ({lat:F7}, {lon:F7})");
                }
            }
        }
        
        edgeIdx++;
    }
    
    // Check if first oriented start == last oriented end
    var firstRef = lineFeature.EdgeReferences[0];
    var lastRef = lineFeature.EdgeReferences[^1];
    var firstEdge = chart.GetEdge(firstRef.Name);
    var lastEdge = chart.GetEdge(lastRef.Name);
    var firstStart = firstEdge == null ? null : (firstRef.Orientation == S57Orientation.Reverse ? firstEdge.EndNode : firstEdge.BeginningNode);
    var lastEnd = lastEdge == null ? null : (lastRef.Orientation == S57Orientation.Reverse ? lastEdge.BeginningNode : lastEdge.EndNode);
    bool closed = firstStart.HasValue && lastEnd.HasValue && firstStart.Value == lastEnd.Value;
    Console.WriteLine($"  FirstOrientedStart={firstStart}, LastOrientedEnd={lastEnd}, Closed={closed}");
    
    count++;
}
