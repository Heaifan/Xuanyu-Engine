namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public int TerrainNaturalPeakSeed { get; private set; } = 7429;

    public void SetTerrainNaturalPeakSeed(int seed) => TerrainNaturalPeakSeed = seed;
}