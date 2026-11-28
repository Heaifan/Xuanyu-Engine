namespace XuanYu.World.Ecs;

internal interface IComponentColumn
{
    void AppendDefault();
    void RemoveAtSwapBack(int row);
    void CopyTo(int row, IComponentColumn target, int targetRow);
}