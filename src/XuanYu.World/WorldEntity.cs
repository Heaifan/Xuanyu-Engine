namespace XuanYu.World;

public sealed record WorldEntity(
    Guid Id,
    string Name,
    bool IsVisible,
    Transform Transform);