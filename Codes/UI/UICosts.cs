using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public partial class UICosts : _UIBase
{
    public override void OnStart()
    {
        base.OnStart();

        costCell.gameObject.SetActive(false);
        RegisterCall<(Transform transform, int cost)>(Events.OnTowerSlotShow, OnTargetShow);
        RegisterCall(Events.Update, OnUpdate);
        RegisterCall<int>(Events.OnTowerSlotGetCoin, OnTargetGetCoin);
    }

    private void OnTargetGetCoin(int targetId)
    {
        if (!dCosts.ContainsKey(targetId))
            return;
        var cost = dCosts[targetId];
        cost.OnSetCost();
        if (cost.isActive)
            return;
        FastCall(Events.OnBuildTower, cost.anchor);
        dCosts.Remove(targetId);
        RemoveChild(cost);
    }

    private void OnUpdate()
    {
        foreach (var kv in dCosts)
        {
            kv.Value.GetTransform().position = Camera.main.WorldToScreenPoint(kv.Value.anchor.position) + Vector3.up * 100;
        }
    }

    Dictionary<int, ECostCell> dCosts = new();
    private void OnTargetShow((Transform transform, int cost) arg)
    {
        var cell = GameObject.Instantiate(this.costCell.gameObject).transform;
        cell.gameObject.SetActive(true);
        var ecell = this.AddChild<ECostCell>(cell);
        dCosts[arg.transform.GetInstanceID()] = ecell;
        ecell.OnSetCost(arg.cost, arg.transform);
        //cell��λ����arg.transform���3D����ӳ�䵽UI�ϵ�λ��
        var pos = Camera.main.WorldToScreenPoint(arg.transform.position);
        pos.y = Screen.height - pos.y; // Unity UI������ϵ�����½�Ϊԭ�㣬��3D��������ϵ�����Ͻ�Ϊԭ��
        cell.SetParent(costCell.transform.parent, true);

    }

    public override void OnShow()
    {
        base.OnShow();

    }
    class ECostCell : Entity
    {
        Text textCost;
        public Transform anchor { get; private set; }
        public override void OnStart()
        {
            textCost = this.GetMonoComponent<Text>("textCost");
        }
        int cost;
        public void OnSetCost(int cost, Transform anchor)
        {
            this.cost = cost;
            textCost.text = cost.ToString();
            this.anchor = anchor;
        }
        public void OnSetCost()
        {
            cost--;
            textCost.text = cost.ToString();
            isActive = false;
        }
    }

}
