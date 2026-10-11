using EncDotNet.S57;
using EncDotNet.Testing;

namespace EncDotNet.UnitTests;

/// <summary>
/// Pins <see cref="S57DocumentReader"/>'s exact output for the synthetic cells, so internal
/// rewrites of the reader and the ISO 8211 layer beneath it can't change what it returns.
/// </summary>
/// <remarks>
/// The expected hashes were recorded from the reader before its performance rewrite. If a
/// change to the reader's output is intended, or the synthetic generator changes, re-record
/// them from the failure message and explain why in the pull request.
/// </remarks>
public class S57DocumentReaderFingerprintTests
{
    public static TheoryData<string, string> Cells => new()
    {
        { nameof(SyntheticCellProfile.Harbour), "84052F8318DC1187D28970F80DA5DFD945FE87B7D380EB8816EF6DC3D8CE53F0" },
        { nameof(SyntheticCellProfile.Approach), "6605571B43635891A1A561731C0D71C9B8ABA83E403EF04D1BECC0C1C27A8799" },
        { nameof(SyntheticCellProfile.Large), "BDF0F099C0081528FF1A2AF49AEC2C5FC2A2FD4150F09D9E33802FAAE3DF9C3F" },
    };

    public static TheoryData<string, string> Updated => new()
    {
        { nameof(SyntheticCellProfile.Harbour), "E8C34F98BE73E8C537A2777AFACAB1CE32EA75EB3CBC0A2051F2A487B5078345" },
        { nameof(SyntheticCellProfile.Approach), "E8B1157B3BC0D167B6B32CDB220BE23EFCB7F184DA84F3DB4AF117C555C931B2" },
    };

    [Theory]
    [MemberData(nameof(Cells))]
    public void Read_SyntheticCell_MatchesRecordedOutput(string profileName, string expected)
    {
        var document = S57DocumentReader.Read(SyntheticS57Cell.Create(Profile(profileName)));

        Assert.Equal(expected, S57DocumentFingerprint.Compute(document));
    }

    [Theory]
    [MemberData(nameof(Updated))]
    public void ReadAndApplyUpdates_SyntheticCell_MatchesRecordedOutput(string profileName, string expected)
    {
        var profile = Profile(profileName);
        var document = S57DocumentReader.Read(SyntheticS57Cell.Create(profile));
        for (var n = 1; n <= 3; n++)
        {
            document = document.ApplyChanges(S57DocumentReader.Read(SyntheticS57Cell.CreateUpdate(profile, n)));
        }

        Assert.Equal(expected, S57DocumentFingerprint.Compute(document));
    }

    private static SyntheticCellProfile Profile(string name) => name switch
    {
        nameof(SyntheticCellProfile.Harbour) => SyntheticCellProfile.Harbour,
        nameof(SyntheticCellProfile.Approach) => SyntheticCellProfile.Approach,
        nameof(SyntheticCellProfile.Large) => SyntheticCellProfile.Large,
        _ => throw new ArgumentOutOfRangeException(nameof(name)),
    };
}
