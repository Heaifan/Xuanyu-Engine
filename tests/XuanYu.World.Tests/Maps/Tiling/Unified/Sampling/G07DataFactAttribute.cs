using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class G07DataFactAttribute : FactAttribute
{
    public G07DataFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("XYE_G07_WEST")) ||
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("XYE_G07_EAST")))
            Skip = "需要指定 XYE_G07_WEST 和 XYE_G07_EAST 真实 HGT。";
    }
}
