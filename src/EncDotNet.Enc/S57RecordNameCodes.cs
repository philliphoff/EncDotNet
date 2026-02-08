namespace EncDotNet.Enc;

/// <summary>
/// S-57 Record Name Codes (RCNM) that identify the type of record.
/// </summary>
public static class S57RecordNameCodes
{
    /// <summary>Data Set General Information Record</summary>
    public const int DataSetGeneralInfo = 10;

    /// <summary>Data Set Geographic Reference Record</summary>
    public const int DataSetGeoReference = 20;

    /// <summary>Data Set History Record</summary>
    public const int DataSetHistory = 30;

    /// <summary>Data Set Accuracy Record</summary>
    public const int DataSetAccuracy = 40;

    /// <summary>Catalogue Cross Reference Record</summary>
    public const int CatalogueCrossReference = 50;

    /// <summary>Data Dictionary Definition Record</summary>
    public const int DataDictionaryDefinition = 60;

    /// <summary>Data Dictionary Domain Record</summary>
    public const int DataDictionaryDomain = 70;

    /// <summary>Data Dictionary Schema Record</summary>
    public const int DataDictionarySchema = 80;

    /// <summary>Feature Record</summary>
    public const int Feature = 100;

    /// <summary>Isolated Node (VI)</summary>
    public const int IsolatedNode = 110;

    /// <summary>Connected Node (VC)</summary>
    public const int ConnectedNode = 120;

    /// <summary>Edge (VE)</summary>
    public const int Edge = 130;

    /// <summary>Face (VF)</summary>
    public const int Face = 140;
}
