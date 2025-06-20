using CloverExternal;
using System;
using System.Collections.Generic;

public partial class Entity
{
    class CallCellArgs<T> : CallCellBase
    {
        public Action<T> actionWithT;
    }
    class CallCell : CallCellBase
    {
    }
    abstract class CallCellBase
    {
        public bool unregisterred;
        public string eventName;
        public Action action;
    }

    private Dictionary<string, CallCellBase> _localCalls;
    private Dictionary<string, CallCellBase> localCalls
    {
        get
        {
            if (_localCalls == null)
            {
                _localCalls = new();
            }
            return _localCalls;
        }
    }
    private static Dictionary<string, List<CallCellBase>> dCalls = new();
    protected void RegisterCall(string eventName, Action call)
    {
        Log.Assert(isActive, $"实例已经被回收，不应该再注册新的消息 {eventName}");
        if (!dCalls.ContainsKey(eventName))
        {
            dCalls[eventName] = new();
        }
        if (localCalls.ContainsKey(eventName))
            return;
        var one = new CallCell();
        one.unregisterred = false;
        one.action = call;
        one.eventName = eventName;
        dCalls[eventName].Add(one);
        localCalls.Add(eventName, one);
    }
    protected void RegisterCall<T>(string eventName, Action<T> call)
    {
        Log.Assert(isActive, $"实例已经被回收，不应该再注册新的消息 {eventName}");
        if (!dCalls.ContainsKey(eventName))
        {
            dCalls[eventName] = new();
        }
        if (localCalls.ContainsKey(eventName))
            return;
        var one = new CallCellArgs<T>();
        one.unregisterred = false;
        one.actionWithT = call;
        one.action = null;
        one.eventName = eventName;
        dCalls[eventName].Add(one);
        localCalls.Add(eventName, one);
    }
    protected void UnregisterCall(string eventName)
    {
        if (!localCalls.TryGetValue(eventName, out var call))
            return;
        call.unregisterred = true;
        dCalls[eventName].Remove(call);
        localCalls.Remove(eventName);
    }
    public static void FastCall<T>(string eventName, T t)
    {
        if (!dCalls.TryGetValue(eventName, out var list))
        {
            return;
        }
        var count = list.Count;
        for (var i = count - 1; i >= 0; i--)
        {
            var c = list[i];
            if (c.unregisterred)
            {
                list.Remove(c);
                continue;
            }
            if (c is CallCellArgs<T> cca)
                cca.actionWithT(t);
            else
                c.action();
        }
    }
    public static void FastCall(string eventName)
    {
        if (!dCalls.TryGetValue(eventName, out var list))
        {
            return;
        }
        var count = list.Count;
        for (var i = count - 1; i >= 0; i--)
        {
            var c = list[i];
            if (c.unregisterred)
            {
                list.Remove(c);
                continue;
            }
            c.action();
        }
    }
    protected void UnregisterAllCalls()
    {
        foreach (var s in localCalls)
        {
            s.Value.unregisterred = true;
            dCalls[s.Value.eventName].Remove(s.Value);
        }
        localCalls.Clear();
    }
}
