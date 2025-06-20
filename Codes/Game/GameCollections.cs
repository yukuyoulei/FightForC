using CloverExternal;
using System;
using System.Collections.Generic;
using UnityEngine;

internal class GameCollections : Entity
{
    List<Transform> lCollections = new();
    List<Transform> flying = new();
    List<Vector3> poses = new();
    Transform carry;
    Transform collections;
    public override void OnStart()
    {
        base.OnStart();
        carry = this.GetMonoComponent<Transform>("carry");
        collections = this.GetMonoComponent<Transform>("collections");

        for (var i = 0; i < collections.childCount; i++)
        {
            var tr = collections.GetChild(i);
            lCollections.Add(tr);
        }

        RegisterCall<Vector3>(Events.OnPlayerMove, OnPlayerMove);
        RegisterCall<float>(Events.Update, OnUpdate);
        RegisterCall<Transform>(Events.FlyCoinReuse, FlyCoinReuse);
    }

    private void FlyCoinReuse(Transform transform)
    {
        transform.SetParent(collections);
        var p = poses[0];
        transform.position = p;
        transform.localEulerAngles = Vector3.zero;
        poses.RemoveAt(0);
        poses.Add(p);
        lCollections.Add(transform);
    }

    List<Transform> removeFlying = new();
    const float SPEED = 30;
    private void OnUpdate(float deltaTime)
    {
        if (!playerPos.HasValue) return;
        if (flying.Count == 0) return;
        foreach (var f in flying)
        {
            if (f == null)
                continue;
            if (!f.gameObject.activeInHierarchy)
                continue;
            //f飞向玩家背上的金币堆
            var pos = carry.position + new Vector3(0, 0.5f * CoinHelper.Count, 0);
            var direction = (pos - f.position).normalized;
            var speed = SPEED * deltaTime; // 每帧移动速度
            f.position += direction * speed;
            f.localRotation = Quaternion.Lerp(f.localRotation, carry.localRotation, Time.deltaTime * 5f);
            // 如果距离玩家位置小于0.5f，则将其从飞行列表中移除,忽略y轴
            if (Vector3.Distance(new Vector3(f.position.x, 0, f.position.z)
                , new Vector3(pos.x, 0, pos.z)) < 0.5f)
            {
                removeFlying.Add(f);
                FastCall(Events.OnPlayerCarry, f);
            }
        }
        foreach (var r in removeFlying)
            flying.Remove(r);
        removeFlying.Clear();
    }
    private Vector3? playerPos;
    List<Transform> removeCollections = new();
    private void OnPlayerMove(Vector3 vector)
    {
        playerPos = vector;
        if (lCollections.Count == 0) return;
        foreach (var collection in lCollections)
        {
            if (collection == null)
                continue;
            if (!collection.gameObject.activeInHierarchy)
                continue;
            var pos = collection.position;
            if (Vector3.Distance(vector, pos) < 10)
            {
                poses.Add(collection.position);
                flying.Add(collection);
                CoinHelper.Count++;
                removeCollections.Add(collection);
            }
        }
        foreach (var r in removeCollections)
            lCollections.Remove(r);
        removeCollections.Clear();
    }
}
