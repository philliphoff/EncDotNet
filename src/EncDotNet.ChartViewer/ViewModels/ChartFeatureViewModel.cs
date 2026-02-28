using System;
using System.Collections.Immutable;
using System.Linq;
using EncDotNet.ChartViewer.Models;
using EncDotNet.S57;
using ReactiveUI;

namespace EncDotNet.ChartViewer.ViewModels;

/// <summary>
/// ViewModel for a toggleable S-57 chart feature category containing individual features.
/// </summary>
public sealed class ChartFeatureViewModel : ViewModelBase
{
    private bool? _isChecked;
    private bool _updatingChildren;
    private bool _isExpanded;

    /// <summary>Gets the feature category definition.</summary>
    public S57FeatureCategory Category { get; }

    /// <summary>Gets the display name.</summary>
    public string Name => Category.Name;

    /// <summary>Gets the individual feature items in this category.</summary>
    public ImmutableArray<ChartFeatureItemViewModel> Features { get; }

    /// <summary>Gets whether this category contains only a single feature.</summary>
    public bool IsSingleFeature => Features.Length == 1;

    /// <summary>Gets or sets whether the category's feature list is expanded.</summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    /// <summary>
    /// Gets or sets the tri-state checked state for the category.
    /// true = all features visible, false = none visible, null = mixed.
    /// </summary>
    public bool? IsChecked
    {
        get => _isChecked;
        set
        {
            // When user clicks through to indeterminate, snap to checked (enable all)
            var effective = value ?? true;
            if (_isChecked is { } current && current == effective)
                return;

            _updatingChildren = true;
            foreach (var f in Features)
                f.IsVisible = effective;
            _updatingChildren = false;

            _isChecked = effective;
            this.RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Raised when any child feature's visibility changes.
    /// The argument is the child feature item that changed.
    /// </summary>
    public event EventHandler<ChartFeatureItemViewModel>? FeatureVisibilityChanged;

    public ChartFeatureViewModel(S57FeatureCategory category)
    {
        Category = category;
        Features = category.ObjectCodes
            .Select(code => new ChartFeatureItemViewModel(
                code, GetDisplayName(code, category), category.DefaultEnabled))
            .ToImmutableArray();

        foreach (var f in Features)
            f.IsVisibleChanged += OnChildVisibilityChanged;

        RefreshCheckedState();
    }

    private void OnChildVisibilityChanged(object? sender, bool isVisible)
    {
        if (!_updatingChildren)
            RefreshCheckedState();

        if (sender is ChartFeatureItemViewModel item)
            FeatureVisibilityChanged?.Invoke(this, item);
    }

    private void RefreshCheckedState()
    {
        var allVisible = Features.All(f => f.IsVisible);
        var noneVisible = Features.All(f => !f.IsVisible);
        _isChecked = allVisible ? true : noneVisible ? false : null;
        this.RaisePropertyChanged(nameof(IsChecked));
    }

    private static string GetDisplayName(S57ObjectCode code, S57FeatureCategory category)
    {
        // For single-code categories, use the category name
        if (category.ObjectCodes.Length == 1)
            return category.Name;

        return code switch
        {
            // Buoys
            S57ObjectCode.BOYLAT => "Lateral Buoy",
            S57ObjectCode.BOYCAR => "Cardinal Buoy",
            S57ObjectCode.BOYINB => "Installation Buoy",
            S57ObjectCode.BOYISD => "Isolated Danger Buoy",
            S57ObjectCode.BOYSAW => "Safe Water Buoy",
            S57ObjectCode.BOYSPP => "Special Purpose Buoy",
            // Beacons
            S57ObjectCode.BCNLAT => "Lateral Beacon",
            S57ObjectCode.BCNCAR => "Cardinal Beacon",
            S57ObjectCode.BCNISD => "Isolated Danger Beacon",
            S57ObjectCode.BCNSAW => "Safe Water Beacon",
            S57ObjectCode.BCNSPP => "Special Purpose Beacon",
            // Traffic Separation Scheme
            S57ObjectCode.TSSLPT => "Lane Part",
            S57ObjectCode.TSSRON => "Roundabout",
            S57ObjectCode.TSSCRS => "Crossing",
            S57ObjectCode.TSSBND => "Boundary",
            S57ObjectCode.TSEZNE => "Zone",
            S57ObjectCode.TSELNE => "Separation Line",
            S57ObjectCode.TWRTPT => "Two-way Route Part",
            S57ObjectCode.PRCARE => "Precautionary Area",
            S57ObjectCode.ISTZNE => "Inshore Traffic Zone",
            // Deep Water Routes
            S57ObjectCode.DWRTCL => "Centerline",
            S57ObjectCode.DWRTPT => "Route Part",
            // Recommended Track
            S57ObjectCode.RECTRC => "Recommended Track",
            S57ObjectCode.RCRTCL => "Route Centerline",
            S57ObjectCode.RCTLPT => "Traffic Lane Part",
            // Seabed
            S57ObjectCode.SBDARE => "Seabed Area",
            S57ObjectCode.WEDKLP => "Weed/Kelp",
            S57ObjectCode.SWPARE => "Swept Area",
            // Tides & Currents
            S57ObjectCode.TS_PRH => "Tidal Stream (Harmonic)",
            S57ObjectCode.TS_PNH => "Tidal Stream (Non-harmonic)",
            S57ObjectCode.TS_PAD => "Tidal Stream Panel",
            S57ObjectCode.TS_TIS => "Tidal Stream (Time Series)",
            S57ObjectCode.T_HMON => "Tide (Harmonic)",
            S57ObjectCode.T_NHMN => "Tide (Non-harmonic)",
            S57ObjectCode.T_TIMS => "Tide (Time Series)",
            S57ObjectCode.CURENT => "Current",
            S57ObjectCode.TIDEWY => "Tideway",
            S57ObjectCode.TS_FEB => "Tidal Stream (Flood/Ebb)",
            // Harbour Facilities
            S57ObjectCode.HRBARE => "Harbour Area",
            S57ObjectCode.HRBFAC => "Harbour Facility",
            S57ObjectCode.BERTHS => "Berth",
            S57ObjectCode.SMCFAC => "Small Craft Facility",
            S57ObjectCode.CRANES => "Crane",
            // Dams & Dykes
            S57ObjectCode.DAMCON => "Dam",
            S57ObjectCode.DYKCON => "Dyke",
            S57ObjectCode.CAUSWY => "Causeway",
            S57ObjectCode.TUNNEL => "Tunnel",
            S57ObjectCode.GATCON => "Gate",
            // Other Nav Aids
            S57ObjectCode.LITFLT => "Light Float",
            S57ObjectCode.LITVES => "Light Vessel",
            S57ObjectCode.RTPBCN => "Radar Transponder",
            S57ObjectCode.DAYMAR => "Daymark",
            S57ObjectCode.TOPMAR => "Topmark",
            // Safety Stations
            S57ObjectCode.RSCSTA => "Rescue Station",
            S57ObjectCode.CGUSTA => "Coastguard Station",
            S57ObjectCode.SISTAT => "Signal Station (Traffic)",
            S57ObjectCode.SISTAW => "Signal Station (Warning)",
            // Sand Waves & Turbulence
            S57ObjectCode.SNDWAV => "Sand Waves",
            S57ObjectCode.WATTUR => "Water Turbulence",
            // Offshore/Industrial Areas
            S57ObjectCode.OSPARE => "Offshore Production",
            S57ObjectCode.ICNARE => "Incineration Area",
            S57ObjectCode.FRPARE => "Free Port Area",
            S57ObjectCode.CTSARE => "Cargo Transhipment",
            // Boundaries & Zones
            S57ObjectCode.CONZNE => "Contiguous Zone",
            S57ObjectCode.EXEZNE => "Exclusive Economic Zone",
            S57ObjectCode.TESARE => "Territorial Sea",
            S57ObjectCode.FSHZNE => "Fishery Zone",
            S57ObjectCode.CUSZNE => "Custom Zone",
            S57ObjectCode.ADMARE => "Administration Area",
            // Fishing
            S57ObjectCode.FSHFAC => "Fishing Facility",
            S57ObjectCode.FSHGRD => "Fishing Ground",
            S57ObjectCode.MARCUL => "Marine Farm/Culture",
            // Land Features
            S57ObjectCode.VEGATN => "Vegetation",
            S57ObjectCode.LNDRGN => "Land Region",
            S57ObjectCode.LNDELV => "Land Elevation",
            S57ObjectCode.LAKSHR => "Lake Shore",
            S57ObjectCode.RIVBNK => "River Bank",
            // Transport
            S57ObjectCode.RAILWY => "Railway",
            S57ObjectCode.ROADWY => "Road",
            S57ObjectCode.RUNWAY => "Runway",
            S57ObjectCode.AIRARE => "Airport/Airfield",
            _ => code.ToString(),
        };
    }
}
