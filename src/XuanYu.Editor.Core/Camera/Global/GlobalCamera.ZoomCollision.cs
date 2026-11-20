using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.Camera;

public sealed partial class GlobalCamera
{
    private void ConstrainZoomPath(double previous)
    {
        double requested = DistanceMeters;
        if (_groundHeight is null || requested == previous) return;
        EcefPosition end = PositionEcef;
        DistanceMeters = previous;
        if (!IsAboveTerrain()) return;

        if (_sweepTerrain is not null)
        {
            double fraction = _sweepTerrain(PositionEcef, end, TerrainClearanceMeters);
            DistanceMeters = previous + (requested - previous) * fraction;
            return;
        }
        // 仅为没有格元或网格数据的外部点采样器保留兼容路径。
        int steps = (int)Math.Clamp(
            Math.Ceiling(Math.Abs(requested - previous) / 8d), 1d, 128d);
        for (int i = 1; i <= steps; i++)
        {
            double candidate = previous + (requested - previous) * i / steps;
            DistanceMeters = candidate;
            if (IsAboveTerrain()) continue;

            double allowed = previous + (requested - previous) * (i - 1d) / steps;
            double blocked = candidate;
            for (int j = 0; j < 18; j++)
            {
                DistanceMeters = (allowed + blocked) * 0.5d;
                if (IsAboveTerrain()) allowed = DistanceMeters;
                else blocked = DistanceMeters;
            }
            DistanceMeters = allowed;
            return;
        }
        DistanceMeters = requested;
    }
}
