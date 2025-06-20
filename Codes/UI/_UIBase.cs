public abstract class _UIBase : Entity
{
    public void CloseMe()
    {
        parent.RemoveChild(this);
    }
}
