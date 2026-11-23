using System.Text.Json;

namespace XuanYu.Editor.Pages;

internal static class GeoOverlayPreferencesStore
{
    private static string FilePath => Path.Combine(Directory.GetCurrentDirectory(),
        "artifacts", "editor", "geo-overlay.json");

    public static GeoOverlayPreferences Load()
    {
        if (!File.Exists(FilePath)) return new();
        try
        {
            var value = JsonSerializer.Deserialize<GeoOverlayPreferences>(
                File.ReadAllText(FilePath)) ?? new();
            return value with { MiniMapSize = Math.Clamp(value.MiniMapSize, 120, 280) };
        }
        catch (Exception error) when (error is JsonException or IOException or
            UnauthorizedAccessException)
        {
            return new();
        }
    }

    public static void Save(GeoOverlayPreferences value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath,
            JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
    }
}