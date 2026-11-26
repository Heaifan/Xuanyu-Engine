using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07SampleAllocation
{
    [G07DataFact]
    public void RepeatedIntegerSamplingDoesNotAllocatePerSample()
    {
        string path = Environment.GetEnvironmentVariable("XYE_G07_WEST")!;
        var source = new GeoUnifiedSamples(new NasademTileCatalog([path]), "v1");
        source.Add(NasademHgtFile.Load(path));
        int x = (180 + 120) * 3600 + 1000, y = (90 + 23) * 3600 + 1000;
        for (int i = 0; i < 100; i++) source.Sample(x, y);
        long before = GC.GetAllocatedBytesForCurrentThread();
        GeoUnifiedSample value = default;
        for (int i = 0; i < 1000; i++) value = source.Sample(x, y);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(GeoSampleStatus.Valid, value.Status);
        Assert.InRange(allocated, 0, 1024);
    }
}
