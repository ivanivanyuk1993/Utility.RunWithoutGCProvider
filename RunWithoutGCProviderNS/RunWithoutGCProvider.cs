using System.Reactive.Linq;

namespace RunWithoutGCProviderNS;

public static class RunWithoutGCProvider
{
    private const long BenchmarkNoGCMemory = (long) 1 << 27;

    public static async Task<T> RunWithoutGC<T>(Func<Task<T>> taskFactory)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        if (GC.TryStartNoGCRegion(totalSize: BenchmarkNoGCMemory))
            try
            {
                return await taskFactory();
            }
            finally
            {
                GC.EndNoGCRegion();
            }

        throw new Exception(message: $"{nameof(GC.TryStartNoGCRegion)} returned false");
    }

    public static Task<T> RunWithoutGC<T>(Func<IObservable<T>> observableFactory)
    {
        return RunWithoutGC(
            taskFactory: async () => await observableFactory()
        );
    }
}