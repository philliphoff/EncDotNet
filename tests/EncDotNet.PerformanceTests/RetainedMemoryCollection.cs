namespace EncDotNet.PerformanceTests;

/// <summary>
/// Runs the retained-memory tests on their own, after the parallel tests finish, because
/// <see cref="GC.GetTotalMemory(bool)"/> sees every thread's allocations.
/// </summary>
[CollectionDefinition(nameof(RetainedMemoryCollection), DisableParallelization = true)]
public sealed class RetainedMemoryCollection;
