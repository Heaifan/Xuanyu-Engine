using System.Numerics;

namespace XuanYu.Editor.TerrainEditing;

public sealed partial class TerrainSculptStroke
{
    private const int RainAttemptsPerSecond = 3000;
    private double _rainRemainder;
    private int _rainSeed = 17;

    private int RainForTime(Vector2 point, float seconds)
    {
        if (seconds <= 0f || _brush.Strength == 0f) return 0;
        double budget = _rainRemainder + RainAttemptsPerSecond * (double)seconds;
        int count = (int)budget;
        _rainRemainder = budget - count;
        if (count == 0) return 0;
        return TerrainRainStamp.Apply(_field, point, _brush, count, _rainSeed++);
    }

    // 快速拖动后立即松键时，最后一批未消费路径仍需产生一次短时降雨。
    internal int FinishRain() => _rain && _path.Count > 0 ? Advance(1f / 30f) : 0;
}