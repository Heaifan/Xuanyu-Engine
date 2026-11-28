namespace XuanYu.Runtime;

public sealed partial class WorldRenderBridge
{
    public RepresentationSnapshot CaptureRepresentation(Guid id) => new(GetRepresentation(id),
        GetResource(id, EntityRepresentationKind.Symbol),
        _bindings.TryGetValue(id, out var binding) ? binding.SymbolStyle : BillboardStyle.Default);

    public void RestoreRepresentation(Guid id, RepresentationSnapshot state)
    {
        RequireEntity(id);
        if (!_bindings.TryGetValue(id, out var binding)) binding = new(null, null, EntityRepresentationKind.None);
        _bindings[id] = binding with { Symbol = state.Symbol, SymbolStyle = state.Style, Active = state.Active };
    }

    public void ClearSymbol(Guid id)
    {
        RequireEntity(id);
        if (!_bindings.TryGetValue(id, out var binding)) return;
        _bindings[id] = binding with
        {
            Symbol = null,
            Active = binding.Active == EntityRepresentationKind.Symbol ? EntityRepresentationKind.None : binding.Active
        };
    }
}