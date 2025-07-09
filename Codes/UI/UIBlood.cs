using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class UIBlood : _UIBase
{
    public override void OnStart()
    {
        base.OnStart();

        bloodCell.gameObject.SetActive(false);
        RegisterCall(Events.Update, OnUpdate);
        RegisterCall<(Transform transform, int hp)>(Events.OnEnemyBorn, OnEnemyBorn);
        RegisterCall<(int instanceId, int curHp)>(Events.OnEnemyHit, OnEnemyHit);

        this.GetTransform().GetComponentInParent<Canvas>().worldCamera = Camera.main;
    }

    private void OnEnemyHit((int instanceId, int curHp) a)
    {
        var blood = dBlood[a.instanceId];
        blood.OnSetBlood(a.curHp);
        if (blood.isActive)
            return;
        dBlood.Remove(a.instanceId);
        RemoveChild(blood);
    }

    private void OnUpdate()
    {
        foreach (var kv in dBlood)
        {
            if (kv.Value.anchor == null)
            {
                kv.Value.isActive = false;
                continue;
            }
            //overlay
            //kv.Value.GetTransform().position = Camera.main.WorldToScreenPoint(kv.Value.anchor.position) + Vector3.up * 100;
            var transform = kv.Value.GetTransform();
            transform.position = kv.Value.anchor.position + Vector3.up * 8;
            transform.LookAt(Camera.main.transform);
            transform.localEulerAngles = new Vector3(-transform.localEulerAngles.x, 0, 0);
        }
    }

    Dictionary<int, EBloodCell> dBlood = new();
    private void OnEnemyBorn((Transform transform, int hp) arg)
    {
        var cell = GameObject.Instantiate(this.bloodCell.gameObject).transform;
        cell.gameObject.SetActive(true);
        cell.localScale = Vector3.one * 0.1f;
        var ecell = this.AddChild<EBloodCell>(cell);
        dBlood[arg.transform.GetInstanceID()] = ecell;
        ecell.OnSetBlood(arg.hp, arg.transform);
        //cell的位置是arg.transform这个3D对象映射到UI上的位置
        var pos = Camera.main.WorldToScreenPoint(arg.transform.position);
        pos.y = Screen.height - pos.y; // Unity UI的坐标系是左下角为原点，而3D世界坐标系是左上角为原点
        cell.SetParent(bloodCell.transform.parent, true);

    }

    public override void OnShow()
    {
        base.OnShow();

    }
    class EBloodCell : Entity
    {
        Text textBlood;
        Slider slider;
        public Transform anchor { get; private set; }
        public override void OnStart()
        {
            slider = this.GetMonoComponent<Slider>("slider");
            textBlood = this.GetMonoComponent<Text>("textBlood");
        }
        public int curBlood;
        public float maxBlood;
        public void OnSetBlood(int maxBlood, Transform anchor)
        {
            this.maxBlood = maxBlood;
            curBlood = maxBlood;
            textBlood.text = maxBlood.ToString();
            this.anchor = anchor;
            slider.value = 1f;
        }
        public void OnSetBlood(int curBlood)
        {
            this.curBlood -= curBlood;
            textBlood.text = $"{curBlood}/{(int)maxBlood}";
            slider.value = curBlood / maxBlood;
            if (curBlood <= 0)
            {
                this.isActive = false;
            }
        }
    }

}
