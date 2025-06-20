using CloverExternal;
using System.Threading.Tasks;
using UnityEngine;

internal class WResource : Entity
{
    public override void OnStart()
    {
        base.OnStart();
    }
    public GameObject resource { get; set; }
    public async Task<T> LoadResource<T>(string resourceName) where T : Entity, new()
    {
        try
        {
            resource = GameObject.Instantiate(await ResourceHelper.Load<GameObject>(resourceName));
            resource.transform.localScale = Vector3.one;
            return parent.AddChild<T>(this);
        }
        catch (System.Exception e)
        {
            Log.Error($"Failed to load resource '{resourceName}': {e.Message}");
            return null;
        }
    }
    public T AttachResource<T>(GameObject obj) where T : Entity, new()
    {
        resource = obj;
        return parent.AddChild<T>(this);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        if (resource == null)
            return;
        GameObject.Destroy(resource);
        resource = null;
    }
}
