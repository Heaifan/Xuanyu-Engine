namespace XuanYu.Editor.Camera;

[Flags]
public enum GameCameraKey
{
    None = 0,
    Forward = 1,
    Backward = 2,
    Left = 4,
    Right = 8
}

public static class GameCameraKeyMap
{
    public static GameCameraKey FromKeySymbol(uint code) => code switch
    {
        0x57 or 0x77 => GameCameraKey.Forward,
        0x53 or 0x73 => GameCameraKey.Backward,
        0x41 or 0x61 => GameCameraKey.Left,
        0x44 or 0x64 => GameCameraKey.Right,
        _ => GameCameraKey.None
    };
}