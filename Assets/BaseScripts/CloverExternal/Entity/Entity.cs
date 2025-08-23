
using System;
using System.Collections.Generic;

public abstract partial class Entity
{
    private static int s_uid;
    public int uid { get; private set; } = ++s_uid;
    public Entity Parent { get; set; }
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
        if (child.Parent != null)
        {
            child.Parent.MoveChild(child, this);
            return;
        }
        this.children.Add(child.uid, child);
        child.Parent = this;
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
        if (child.Parent == entity) return; // Already moved to the correct parent
        child.Parent.children.Remove(child.uid);
        child.Parent = entity;
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
        ClearAllClusters();
        Parent = null;
    }
    private Dictionary<string, List<int>> s_clusters = new Dictionary<string, List<int>>();
    private void AddToCluster(string clusterName, int entityId)
    {
        if (Parent == null) return;

        if (!Parent.s_clusters.ContainsKey(clusterName))
        {
            Parent.s_clusters[clusterName] = new List<int>();
        }
        Parent.s_clusters[clusterName].Add(entityId);
    }

    public void RemoveFromCluster(string clusterName, int entityId)
    {
        if (Parent == null) return;

        if (Parent.s_clusters.ContainsKey(clusterName))
        {
            Parent.s_clusters[clusterName].Remove(entityId);
        }
    }

    public List<int> GetCluster(string clusterName)
    {
        if (!s_clusters.TryGetValue(clusterName, out var cluster))
        {
            cluster = new List<int>();
            s_clusters[clusterName] = cluster;
        }
        return cluster;
    }

    public void ClearCluster(string clusterName)
    {
        if (s_clusters.ContainsKey(clusterName))
        {
            s_clusters.Remove(clusterName);
        }
    }

    public void ClearAllClusters()
    {
        s_clusters.Clear();
    }

    /// <summary>
    /// 聚合到指定的集群中
    /// </summary>
    /// <param name="clusterName"></param>
    public void JoinCluster(string clusterName)
    {
        AddToCluster(clusterName, this.uid);
    }

    public void LeaveCluster(string clusterName)
    {
        RemoveFromCluster(clusterName, this.uid);
    }
    public bool IsInCluster(string clusterName)
    {
        if (Parent == null) return false;

        return Parent.s_clusters.ContainsKey(clusterName) && Parent.s_clusters[clusterName].Contains(this.uid);
    }
    public virtual void OnStart() { }
    public virtual void OnShow() { }
    public virtual void OnDestroy() { }
}
