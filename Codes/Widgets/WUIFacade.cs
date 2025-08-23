using CloverExternal;

internal class WUIFacade : Entity
{
    public UIFacade facade { get; set; }
    public override void OnStart()
    {
        base.OnStart();

        var w = Parent.GetChild<WResource>();
        Log.Assert(w.resource != null, $"resource is null!!");
        facade = w.resource.GetComponent<UIFacade>();
        Log.Assert(facade != null, $"{w.resource.name}上缺少 UIFacade 控件");
    }
}
