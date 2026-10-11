namespace EncDotNet.PerformanceTests;

/// <summary>
/// Checks a measured memory figure against its recorded budget.
/// </summary>
/// <remarks>
/// <para>
/// A budget fails in both directions. Going over it is a regression. Coming in well under
/// it means the code improved and the budget must be lowered to the new figure, so that a
/// later regression can't hide in the headroom the improvement left behind.
/// </para>
/// <para>
/// The tolerances are small because the figures are exact: allocation counts don't vary
/// between runs or machines, and retained sizes vary only by heap bookkeeping.
/// </para>
/// </remarks>
internal static class PerformanceBudget
{
    /// <summary>
    /// The fraction above an allocation budget that still passes, which absorbs
    /// small differences between .NET patch releases.
    /// </summary>
    private const double AllocatedOverTolerance = 0.01;

    /// <summary>
    /// The fraction below a budget at which the budget must be lowered.
    /// </summary>
    private const double UnderTolerance = 0.05;

    /// <summary>
    /// The fraction either side of a retained-memory budget that still passes.
    /// </summary>
    private const double RetainedTolerance = 0.03;

    /// <summary>
    /// Asserts that an allocation figure is within its budget.
    /// </summary>
    public static void AssertAllocated(string scenario, long actual, long budget) =>
        AssertWithin(scenario, "allocated", actual, budget, AllocatedOverTolerance, UnderTolerance);

    /// <summary>
    /// Asserts that a retained-memory figure is within its budget.
    /// </summary>
    public static void AssertRetained(string scenario, long actual, long budget) =>
        AssertWithin(scenario, "retained", actual, budget, RetainedTolerance, Math.Max(RetainedTolerance, UnderTolerance));

    private static void AssertWithin(string scenario, string kind, long actual, long budget, double over, double under)
    {
        TestContext.Current.TestOutputHelper?.WriteLine($"{scenario}: {actual:N0} bytes {kind} (budget {budget:N0})");

        // Budgets under 1 KB are checked exactly; a percentage of zero is no tolerance at all.
        var upper = budget < 1024 ? budget : (long)(budget * (1 + over));
        var lower = budget < 1024 ? budget : (long)(budget * (1 - under));

        if (actual > upper)
        {
            Assert.Fail(
                $"{scenario} {kind} {actual:N0} bytes, over its budget of {budget:N0} by {Percent(actual, budget)}. " +
                "Find the regression, or if the increase is intended, raise the budget in PerformanceBudgets.cs " +
                "and explain why in the pull request.");
        }

        if (actual < lower)
        {
            Assert.Fail(
                $"{scenario} {kind} {actual:N0} bytes, under its budget of {budget:N0} by {Percent(budget, actual)}. " +
                $"Lower the budget in PerformanceBudgets.cs to {actual:N0} to lock in the improvement.");
        }
    }

    private static string Percent(long larger, long smaller) =>
        smaller == 0 ? "∞" : $"{(larger - smaller) * 100.0 / smaller:F1}%";
}
