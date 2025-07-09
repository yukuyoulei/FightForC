using System;
using UnityEngine;
using UnityEngine.UI;

public partial class UIBattle : _UIBase
{
    EJoystick wjoystick;
    public override void OnStart()
    {
        base.OnStart();

        wjoystick = this.AddChild<EJoystick>(this.GetMonoComponent<Transform>("joystick"));

        btnQuit.onClick.AddListener(OnQuitClick);

        RegisterCall(Events.Update, OnUpdate);
        RegisterCall<int>(Events.OnPlayerCoinUpdate, OnPlayerLostCoin);
    }
    public override void OnShow()
    {
        base.OnShow();
        textCoin.text = "0";
    }

    private void OnPlayerLostCoin(int coin)
    {
        textCoin.text = coin.ToString();
    }

    private void OnUpdate()
    {
        wjoystick.OnUpdate();

    }

    private void OnQuitClick()
    {
        Game.world.RemoveChild<BattleScene>();
        Game.world.RemoveChild<UIBattle>();
        Game.world.RemoveChild<UICosts>();
    }
    class EJoystick : Entity
    {
        Transform circle;
        Transform shower;
        public override void OnStart()
        {
            base.OnStart();
            circle = this.GetMonoComponent<Transform>("circle");
            shower = this.GetMonoComponent<Transform>("shower");
            shower.gameObject.SetActive(false);
        }

        Vector3 originalPoint;
        internal void OnUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                originalPoint = Input.mousePosition;
                shower.transform.position = originalPoint;
                shower.gameObject.SetActive(true);
            }
            if (!shower.gameObject.activeInHierarchy)
                return;
            if (!Input.GetMouseButton(0))
            {
                shower.gameObject.SetActive(false);
                circle.transform.localPosition = Vector3.zero;
                FastCall(Events.Stop);
                return;
            }
            var dir = Input.mousePosition - originalPoint;
            var d = dir.normalized;
            if (d.magnitude < 0.1f)
                return;
            circle.transform.localPosition = d * 30;
            float angle = Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;
            FastCall(Events.Move, new Vector3(0, angle));
        }
        private Vector3 tvector = new Vector3();
        /// <summary>
        /// 屏幕左边和世界坐标，需要交换yz
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        Vector3 exchangeYZ(Vector3 v)
        {
            tvector.x = v.x;
            tvector.z = v.y;
            tvector.y = v.z;
            return tvector;
        }
    }
}
