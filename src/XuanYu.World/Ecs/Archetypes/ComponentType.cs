namespace XuanYu.World.Ecs;

public sealed class ComponentType
{
    private readonly Func<IComponentColumn> _create;
    public Type ValueType { get; }

    private ComponentType(Type valueType, Func<IComponentColumn> create)
    {
        ValueType = valueType;
        _create = create;
    }

    public static ComponentType Of<T>() where T : unmanaged =>
        new(typeof(T), static () => new ComponentColumn<T>());

    internal IComponentColumn CreateColumn() => _create();
}