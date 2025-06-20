internal class WUIFacade : Entity
{
    public UIFacade facade { get; set; }
    public override void OnStart()
    {
        base.OnStart();

        var w = parent.GetChild<WResource>();
        facade = w.resource.GetComponent<UIFacade>();
    }
}
