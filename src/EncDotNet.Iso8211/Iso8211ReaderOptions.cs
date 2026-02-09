namespace EncDotNet.Iso8211;

/// <summary>
/// Options for configuring the <see cref="Iso8211Reader"/>.
/// </summary>
public sealed class Iso8211ReaderOptions
{
    /// <summary>
    /// Gets or sets the maximum depth to read. Default is 64.
    /// </summary>
    public int MaxDepth { get; set; } = 64;

    /// <summary>
    /// Gets or sets whether to skip validation for better performance. Default is false.
    /// </summary>
    public bool SkipValidation { get; set; }
}
