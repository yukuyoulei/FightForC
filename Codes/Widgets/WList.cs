using System;
using System.Threading.Tasks;
using UnityEngine;

internal class WList<T> : Entity where T : Entity, new()
{
    Transform resource;
    public override void OnStart()
    {
        base.OnStart();
        resource = GetChild<WResource>().resource.transform;
    }
    Action<int, T> init;
    Action<int, T> refresh;
    public static WList<T> CreateStatic<T>(Entity parent, Transform transform, Action<int, T> init, Action<int, T> refresh) where T : Entity, new()
    {
        var l = parent.AddChild<WList<T>>(transform);
        l.init = init;
        l.refresh = refresh;
        return l;
    }
    public void Static(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var c = this.AddChild<T>(resource.GetChild(i));
            c.Parent = this;
            init?.Invoke(i, c);
        }
    }
}
