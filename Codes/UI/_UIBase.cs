public abstract class _UIBase : Entity
{
    public void CloseMe()
    {
        Parent.RemoveChild(this);
    }
}
