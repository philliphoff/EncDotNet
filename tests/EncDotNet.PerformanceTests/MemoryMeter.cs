namespace EncDotNet.PerformanceTests;

/// <summary>
/// Measures managed memory in ways that are exact and repeatable, unlike elapsed time.
/// </summary>
internal static class MemoryMeter
{
    /// <summary>
    /// Returns the bytes allocated on the current thread by one call to <paramref name="action"/>.
    /// </summary>
    /// <remarks>
    /// The action runs twice first, so JIT compilation, type loading and lazily built static
    /// caches are not counted. <see cref="GC.GetAllocatedBytesForCurrentThread"/> is exact and
    /// per-thread, so tests on other threads don't disturb the count. The action returns
    /// nothing so that measuring a value-returning call doesn't box its result.
    /// </remarks>
    public static long AllocatedBytes(Action action)
    {
        action();
        action();

        var before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    /// <summary>
    /// Returns the bytes of the managed heap still held by the object <paramref name="factory"/> returns.
    /// </summary>
    /// <remarks>
    /// This counts the whole process heap, so callers must not run alongside other tests; see
    /// <see cref="RetainedMemoryCollection"/>.
    /// </remarks>
    public static long RetainedBytes(Func<object> factory)
    {
        GC.KeepAlive(factory());

        var before = GC.GetTotalMemory(forceFullCollection: true);
        var result = factory();
        var after = GC.GetTotalMemory(forceFullCollection: true);
        GC.KeepAlive(result);
        return after - before;
    }
}
