using System;
using System.Collections.Generic;
using UnityEngine;

internal class GameTowerSlot : Entity
{
    Transform towerSlots;
    Transform carry;
    List<Transform> lTowerSlot = new();
    Dictionary<int, int> dCosts = new();
    const int COST = 10;
    public override void OnStart()
    {
        base.OnStart();

        towerSlots = this.GetMonoComponent<Transform>("targets");
        carry = this.GetMonoComponent<Transform>("carry");
        for (var i = 0; i < towerSlots.childCount; i++)
        {
            var tr = towerSlots.GetChild(i);
            lTowerSlot.Add(tr);
            dCosts[tr.GetInstanceID()] = COST;
        }
        RegisterCall<Vector3>(Events.OnPlayerMove, OnPlayerMove);
        RegisterCall<float>(Events.Update, OnUpdate);
        RegisterCall(Events.OnBattleReady, OnBattleReady);
    }

    private void OnBattleReady()
    {
        foreach (var t in lTowerSlot)
        {
            FastCall(Events.OnTowerSlotShow, (t, COST));
        }
    }

    Transform reachedTowerSlot;
    DateTime? reachTime;
    record FlyCell(Transform coin, Transform towerSlot, int lastCoin);
    List<FlyCell> flying = new();
    private void OnUpdate(float deltaTime)
    {
        UpdatingFlying(deltaTime);
        if (!playerPos.HasValue) return;
        if (reachedTowerSlot == null) return;
        if ((DateTime.Now - reachTime.Value) < TimeSpan.FromMilliseconds(100)) return;
        if (CoinHelper.Count == 0) return;
        if (carry.childCount == 0) return;
        if (!dCosts.ContainsKey(reachedTowerSlot.GetInstanceID())) return;
        if (dCosts[reachedTowerSlot.GetInstanceID()] == 0) return;
        CoinHelper.Count--;
        dCosts[reachedTowerSlot.GetInstanceID()]--;
        var coin = carry.GetChild(carry.childCount - 1);
        coin.SetParent(reachedTowerSlot, true);
        flying.Add(new FlyCell(coin, reachedTowerSlot, dCosts[reachedTowerSlot.GetInstanceID()]));
        if (dCosts[reachedTowerSlot.GetInstanceID()] <= 0)
        {
            dCosts.Remove(reachedTowerSlot.GetInstanceID());
        }
    }

    List<FlyCell> removing = new List<FlyCell>();
    private void UpdatingFlying(float deltaTime)
    {
        foreach (var f in flying)
        {
            f.coin.position = Vector3.MoveTowards(f.coin.position, f.towerSlot.position, 20f * deltaTime);
            if (Vector3.Distance(f.coin.position, f.towerSlot.position) < 1f)
            {
                FastCall(Events.OnTowerSlotGetCoin, f.towerSlot.GetInstanceID());
                FastCall(Events.FlyCoinReuse, f.coin);
                removing.Add(f);
            }
        }
        foreach (var r in removing)
        {
            flying.Remove(r);
        }
        removing.Clear();
    }

    private Vector3? playerPos;
    private void OnPlayerMove(Vector3 vector)
    {
        playerPos = vector;
        reachedTowerSlot = null;
        foreach (var t in lTowerSlot)
        {
            if (t == null)
                continue;
            if (!t.gameObject.activeInHierarchy)
                continue;
            // 检查目标是否在玩家附近
            if (Vector3.Distance(t.position, playerPos.Value) < 5f)
            {
                reachedTowerSlot = t;
                if (!reachTime.HasValue)
                    reachTime = DateTime.Now;
            }
        }
        if (reachedTowerSlot == null)
            reachTime = null;
    }
}