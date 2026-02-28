using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace EncDotNet.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
