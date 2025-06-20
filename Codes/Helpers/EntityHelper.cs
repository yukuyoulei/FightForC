using System.Threading.Tasks;
using UnityEngine;

internal static class EntityHelper
{
    public static Transform GetTransform(this Entity entity)
    {
        var wfacade = entity.GetChild<WUIFacade>();
        if (wfacade == null)
        {
            wfacade = entity.AddChild<WUIFacade>();
            wfacade.OnStart();
        }
        return wfacade.facade.transform;
    }
    public static T GetMonoComponent<T>(this Entity entity, string name) where T : Component
    {
        var wfacade = entity.GetChild<WUIFacade>();
        if (wfacade == null)
        {
            wfacade = entity.AddChild<WUIFacade>();
            wfacade.OnStart();
        }
        return wfacade.facade.dElements[name].component.GetComponent<T>();
    }
    public static T AddChildWithResource<T>(this Entity parent, WResource r) where T : Entity, new()
    {
        var nr = parent.AddChild<WResource>();
        nr.resource = r.resource;
        var child = parent.AddChild<T>(nr);
        return child;
    }
    public static async Task<T> AddChild<T>(this Entity parent, string resourcePath) where T : Entity, new()
    {
        var r = parent.AddChild<WResource>();
        var ent = await r.LoadResource<T>(resourcePath);
        ent.AddChild(r);
        return ent;
    }
    public static T AddChild<T>(this Entity parent, Transform resource) where T : Entity, new()
    {
        var r = parent.AddChild<WResource>();
        var ent = r.AttachResource<T>(resource.gameObject);
        ent.AddChild(r);
        return ent;
    }
}
