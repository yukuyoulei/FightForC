using CloverExternal;
using System.Threading.Tasks;
using UnityEngine;

internal class World : Entity
{
    public World()
    {
        OnStart();
    }
    public override void OnStart()
    {
        base.OnStart();
        Log.Debug($"World Started");
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        ClearChildren();
        Log.Debug($"World Destroyed");
    }
}
