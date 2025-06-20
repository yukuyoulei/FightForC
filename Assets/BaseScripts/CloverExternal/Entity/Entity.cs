
using System;
using System.Collections.Generic;

public abstract partial class Entity
{
    private static int s_uid;
    public int uid { get; private set; } = ++s_uid;
    public Entity parent { get; set; }
    public Dictionary<int, Entity> children { get; set; } = new ();

    public bool isActive { get; set; } = true;
    public T AddChild<T>(params Entity[] widgets) where T : Entity, new()
    {
        var child = new T();
        AddChild(child, widgets);
        return child;
    }
    public void AddChild(Entity child, params Entity[] widgets)
    {
        if (child == null) return;
        foreach (var widget in widgets)
        {
            if (widget == null) continue;
            MoveChild(widget, child);
        }
        if (child.parent != null)
        {
            child.parent.MoveChild(child, this);
            return;
        }
        child.parent = this;
        child.OnStart();
    }
    public T GetChild<T>() where T : Entity
    {
        foreach (var c in children.Values)
        {
            if (c is T)
                return c as T;
        }
        return null;
    }
    public Entity GetChild(int uid)
    {
        if (children.TryGetValue(uid, out var child))
        {
            return child;
        }
        return null;
    }
    private void MoveChild(Entity child, Entity entity)
    {
        if (child == null) return;
        if (child.parent == entity) return; // Already moved to the correct parent
        child.parent.children.Remove(child.uid);
        child.parent = entity;
        entity.children.Add(child.uid, child);
    }
    public void RemoveChild<T>() where T : Entity
    {
        foreach (var child in children.Values)
        {
            if (child is T)
            {
                RemoveChild(child);
                return;
            }
        }
    }
    public void RemoveChild(Entity child)
    {
        if (child == null) return;
        child.Internal_OnDestroy();

    }
    public void ClearChildren()
    {
        foreach (var child in children.Values)
        {
            child.Internal_OnDestroy();
        }
        children.Clear();
    }

    private void Internal_OnDestroy()
    {
        isActive = false;
        ClearChildren();
        OnDestroy();
        UnregisterAllCalls();
        parent = null;
    }

    public virtual void OnStart() { }
    public virtual void OnShow() { }
    public virtual void OnDestroy() { }
}
