using System.Collections.Immutable;

namespace EncDotNet.S57.ExchangeSets;

/// <summary>
/// Provides methods to read an S-57 exchange set from a directory on disk.
/// </summary>
/// <remarks>
/// <para>
/// This reader scans a directory for the <c>CATALOG.031</c> file, a base cell file
/// (<c>.000</c> extension), and any update files (<c>.001</c>, <c>.002</c>, etc.),
/// then constructs an <see cref="S57ExchangeSet"/> describing the file layout.
/// </para>
/// <para>
/// No file contents are parsed; only the directory structure is examined.
/// </para>
/// </remarks>
public static class S57ExchangeSetReader
{
    private const string CatalogFileName = "CATALOG.031";
    private const string BaseCellExtension = ".000";

    /// <summary>
    /// Reads the exchange set layout from the specified directory.
    /// </summary>
    /// <param name="path">The path to the root directory of the exchange set.</param>
    /// <returns>The exchange set describing the files found in the directory.</returns>
    /// <exception cref="DirectoryNotFoundException">The directory does not exist.</exception>
    /// <exception cref="FileNotFoundException">No base cell file was found.</exception>
    public static S57ExchangeSet Read(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Exchange set directory not found: {path}");
        }

        // Find the CATALOG.031 file (optional).
        string? catalogPath = Directory.EnumerateFiles(path, CatalogFileName, SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        // Find the base cell file (.000) anywhere under the root.
        string? baseCellPath = Directory.EnumerateFiles(path, $"*{BaseCellExtension}", SearchOption.AllDirectories)
            .FirstOrDefault();

        if (baseCellPath is null)
        {
            throw new FileNotFoundException($"No base cell file ({BaseCellExtension}) found in exchange set.", BaseCellExtension);
        }

        // Derive the cell name stem from the base cell file to locate updates.
        string cellDirectory = Path.GetDirectoryName(baseCellPath)!;
        string cellStem = Path.GetFileNameWithoutExtension(baseCellPath);

        // Find update files (.001, .002, …) matching the cell stem, ordered by extension.
        var updatePaths = Directory.EnumerateFiles(cellDirectory, $"{cellStem}.*")
            .Where(f => IsUpdateExtension(Path.GetExtension(f)))
            .OrderBy(f => Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new S57ExchangeSet
        {
            CatalogFileName = catalogPath is not null ? Path.GetRelativePath(path, catalogPath) : null,
            BaseCellFileName = Path.GetRelativePath(path, baseCellPath),
            UpdateFileNames = updatePaths.Select(f => Path.GetRelativePath(path, f)).ToImmutableArray()
        };
    }

    /// <summary>
    /// Returns <see langword="true"/> if the extension looks like an S-57 update extension
    /// (<c>.001</c> through <c>.999</c>), excluding the base cell extension (<c>.000</c>).
    /// </summary>
    private static bool IsUpdateExtension(string extension)
    {
        return extension.Length == 4
            && extension[0] == '.'
            && int.TryParse(extension.AsSpan(1), out int number)
            && number > 0;
    }
}
