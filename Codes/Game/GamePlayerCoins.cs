using System;
using UnityEngine;

internal class GamePlayerCoins : Entity
{
    Transform carry;
    public override void OnStart()
    {
        base.OnStart();

        carry = this.GetMonoComponent<Transform>("carry");
        CoinHelper.Count = 0;

        RegisterCall<Transform>(Events.OnPlayerCarry, OnPlayerCarry);
    }

    private void OnPlayerCarry(Transform transform)
    {
        transform.SetParent(carry);
        transform.localPosition = new Vector3(0, 0, carry.childCount * -0.5f);
        transform.localEulerAngles = Vector3.zero;
    }
}
