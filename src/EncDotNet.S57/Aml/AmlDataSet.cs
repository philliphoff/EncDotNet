using System.Diagnostics.CodeAnalysis;
using EncDotNet.S57.Charts;

namespace EncDotNet.S57.Aml;

/// <summary>
/// Represents a NATO Additional Military Layers (AML) cell: an <see cref="S57Chart"/> together with
/// its AML product, its structured metadata and the object catalogue that gives its codes their
/// meaning.
/// </summary>
/// <remarks>
/// <para>
/// An AML cell is an S-57 data set whose product specification (<c>DSID</c> <c>PRSP</c>) is one of
/// the AML codes 51-57, so <see cref="S57DocumentReader"/> and <see cref="S57Chart"/> read it as
/// they read any other cell. This type is a view over that chart: it doesn't copy its features or
/// spatial records.
/// </para>
/// <para>
/// Create it from a chart or a document, or load it with <see cref="FromFile(string)"/> or
/// <see cref="FromStream(Stream)"/>. Creating it from a data set that isn't an AML cell throws
/// <see cref="ArgumentException"/>; use <see cref="TryCreate(S57Chart, out AmlDataSet?)"/> to test
/// first.
/// </para>
/// </remarks>
public sealed class AmlDataSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmlDataSet"/> class over a chart.
    /// </summary>
    /// <param name="chart">The chart of an AML cell.</param>
    /// <exception cref="ArgumentNullException"><paramref name="chart"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="chart"/> has no data set identification, or its product isn't
    /// <see cref="S57ProductKind.Aml"/>.
    /// </exception>
    public AmlDataSet(S57Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        Identification = GetAmlIdentification(chart.Identification)
            ?? throw new ArgumentException(NotAmlMessage(chart.Identification), nameof(chart));
        Chart = chart;
        Product = Identification.Product;
        AmlProduct = Product.AmlProduct.GetValueOrDefault();
        Metadata = AmlDataSetMetadata.TryParse(Identification.Comment, out var metadata) ? metadata : null;
        Catalogue = S57ObjectCatalogue.For(Product);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmlDataSet"/> class over the chart built from a
    /// document.
    /// </summary>
    /// <param name="document">The document of an AML cell.</param>
    /// <exception cref="ArgumentNullException"><paramref name="document"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="document"/> has no data set identification, or its product isn't
    /// <see cref="S57ProductKind.Aml"/>.
    /// </exception>
    /// <remarks>The document is checked before the chart is built.</remarks>
    public AmlDataSet(S57Document document)
        : this(S57Chart.FromDocument(EnsureAml(document)))
    {
    }

    /// <summary>Gets the chart this view is over.</summary>
    public S57Chart Chart { get; }

    /// <summary>Gets the data set identification field (<c>DSID</c>).</summary>
    public S57DataSetIdentification Identification { get; }

    /// <summary>
    /// Gets the product identity of the cell. Its <see cref="S57ProductIdentity.Kind"/> is always
    /// <see cref="S57ProductKind.Aml"/>.
    /// </summary>
    public S57ProductIdentity Product { get; }

    /// <summary>Gets the AML product the cell belongs to, from <c>DSID</c> <c>PRSP</c>.</summary>
    public AmlProduct AmlProduct { get; }

    /// <summary>
    /// Gets the structured metadata from the <c>DSID</c> comment (<c>COMT</c>), or
    /// <see langword="null"/> when the comment is empty or isn't a list of AML <c>key=value</c> pairs.
    /// </summary>
    /// <remarks>
    /// AML data is classified. Systems that show it should display the cell's protective marking
    /// (<see cref="AmlDataSetMetadata.ProtectiveMarking"/>) prominently wherever its data is shown.
    /// </remarks>
    public AmlDataSetMetadata? Metadata { get; }

    /// <summary>
    /// Gets the object catalogue for the cell's AML edition, chosen by
    /// <see cref="S57ObjectCatalogue.For(S57ProductIdentity)"/>.
    /// </summary>
    public S57ObjectCatalogue Catalogue { get; }

    /// <summary>
    /// Tries to create a view over a chart.
    /// </summary>
    /// <param name="chart">The chart.</param>
    /// <param name="dataSet">
    /// When this method returns <see langword="true"/>, the view over <paramref name="chart"/>;
    /// otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="chart"/> is an AML cell; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="chart"/> is <see langword="null"/>.</exception>
    public static bool TryCreate(S57Chart chart, [NotNullWhen(true)] out AmlDataSet? dataSet)
    {
        ArgumentNullException.ThrowIfNull(chart);

        dataSet = GetAmlIdentification(chart.Identification) is null ? null : new AmlDataSet(chart);
        return dataSet is not null;
    }

    /// <summary>
    /// Tries to create a view over the chart built from a document.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="dataSet">
    /// When this method returns <see langword="true"/>, the view over the chart built from
    /// <paramref name="document"/>; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="document"/> is an AML cell; otherwise,
    /// <see langword="false"/>. The chart is built only when it is.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="document"/> is <see langword="null"/>.</exception>
    public static bool TryCreate(S57Document document, [NotNullWhen(true)] out AmlDataSet? dataSet)
    {
        ArgumentNullException.ThrowIfNull(document);

        dataSet = GetAmlIdentification(document.DataSetIdentification) is null ? null : new AmlDataSet(document);
        return dataSet is not null;
    }

    /// <summary>
    /// Loads an AML cell from a file.
    /// </summary>
    /// <param name="path">The path to the cell.</param>
    /// <returns>The AML cell.</returns>
    /// <exception cref="ArgumentException">The file isn't an AML cell.</exception>
    public static AmlDataSet FromFile(string path) => new(S57DocumentReader.ReadFromFile(path));

    /// <summary>
    /// Asynchronously loads an AML cell from a file.
    /// </summary>
    /// <param name="path">The path to the cell.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task whose result is the AML cell.</returns>
    /// <exception cref="ArgumentException">The file isn't an AML cell.</exception>
    public static async Task<AmlDataSet> FromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var document = await S57DocumentReader.ReadFromFileAsync(path, cancellationToken: cancellationToken).ConfigureAwait(false);
        return new AmlDataSet(document);
    }

    /// <summary>
    /// Loads an AML cell from a stream.
    /// </summary>
    /// <param name="stream">The stream holding the cell.</param>
    /// <returns>The AML cell.</returns>
    /// <exception cref="ArgumentException">The stream doesn't hold an AML cell.</exception>
    public static AmlDataSet FromStream(Stream stream) => new(S57DocumentReader.Read(stream));

    /// <summary>
    /// Asynchronously loads an AML cell from a stream.
    /// </summary>
    /// <param name="stream">The stream holding the cell.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task whose result is the AML cell.</returns>
    /// <exception cref="ArgumentException">The stream doesn't hold an AML cell.</exception>
    public static async Task<AmlDataSet> FromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var document = await S57DocumentReader.ReadAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return new AmlDataSet(document);
    }

    /// <summary>
    /// Gets the catalogue definition of a feature's object class.
    /// </summary>
    /// <param name="feature">The feature.</param>
    /// <returns>
    /// The object class with the feature's <see cref="S57TypedFeature.ObjectCode"/> in
    /// <see cref="Catalogue"/>, or <see langword="null"/> when the catalogue has none.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="feature"/> is <see langword="null"/>.</exception>
    public S57ObjectClassDefinition? GetObjectClass(S57TypedFeature feature)
    {
        ArgumentNullException.ThrowIfNull(feature);
        return Catalogue.GetObjectClass(feature.ObjectCode);
    }

    /// <summary>
    /// Gets the catalogue definition of an attribute value's attribute.
    /// </summary>
    /// <param name="value">The attribute value.</param>
    /// <returns>
    /// The attribute with the value's <see cref="S57AttributeValue.AttributeCode"/> in
    /// <see cref="Catalogue"/>, or <see langword="null"/> when the catalogue has none.
    /// </returns>
    public S57AttributeDefinition? GetAttribute(S57AttributeValue value) => Catalogue.GetAttribute(value.AttributeCode);

    /// <summary>
    /// Gets a feature's attribute values (<c>ATTF</c>) with their catalogue definitions.
    /// </summary>
    /// <param name="feature">The feature.</param>
    /// <returns>One entry for each of the feature's <see cref="S57TypedFeature.Attributes"/>, in order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="feature"/> is <see langword="null"/>.</exception>
    public IReadOnlyList<AmlAttributeValue> GetAttributes(S57TypedFeature feature)
    {
        ArgumentNullException.ThrowIfNull(feature);
        return Define(feature.Attributes);
    }

    /// <summary>
    /// Gets a feature's national attribute values (<c>NATF</c>) with their catalogue definitions.
    /// </summary>
    /// <param name="feature">The feature.</param>
    /// <returns>One entry for each of the feature's <see cref="S57TypedFeature.NationalAttributes"/>, in order.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="feature"/> is <see langword="null"/>.</exception>
    public IReadOnlyList<AmlAttributeValue> GetNationalAttributes(S57TypedFeature feature)
    {
        ArgumentNullException.ThrowIfNull(feature);
        return Define(feature.NationalAttributes);
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Identification.DataSetName} ({AmlProduct}, {Catalogue.Name})";

    private AmlAttributeValue[] Define(IReadOnlyList<S57AttributeValue> values)
    {
        var defined = new AmlAttributeValue[values.Count];
        for (var i = 0; i < defined.Length; i++)
        {
            defined[i] = new AmlAttributeValue(values[i], GetAttribute(values[i]));
        }

        return defined;
    }

    private static S57Document EnsureAml(S57Document document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return GetAmlIdentification(document.DataSetIdentification) is null
            ? throw new ArgumentException(NotAmlMessage(document.DataSetIdentification), nameof(document))
            : document;
    }

    private static S57DataSetIdentification? GetAmlIdentification(S57DataSetIdentification? identification) =>
        identification?.Product.Kind == S57ProductKind.Aml ? identification : null;

    private static string NotAmlMessage(S57DataSetIdentification? identification) => identification is null
        ? "The data set has no data set identification (DSID), so it isn't an AML cell."
        : $"The data set's product specification (PRSP {identification.ProductSpecification}) isn't an AML product.";
}
