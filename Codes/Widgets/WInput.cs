using System;
using UnityEngine;

internal class WInput : Entity
{
    public override void OnStart()
    {
        base.OnStart();

        RegisterCall<float>(Events.Update, OnUpdate);
    }

    private bool SomeKeyDown;
    private void OnUpdate(float obj)
    {
        var SomeKeyDown = false;

        if (Input.GetKey(KeyCode.S))
        {
            SomeKeyDown = true;
            FastCall(Events.Move, new Vector3(0, 180, 0));
        }
        else if (Input.GetKey(KeyCode.W))
        {
            SomeKeyDown = true;
            FastCall(Events.Move, new Vector3(0, 0, 0));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            SomeKeyDown = true;
            FastCall(Events.Move, new Vector3(0, 90, 0));
        }
        else if (Input.GetKey(KeyCode.A))
        {
            SomeKeyDown = true;
            FastCall(Events.Move, new Vector3(0, -90, 0));
        }
        if (!SomeKeyDown && this.SomeKeyDown)
        {
            this.SomeKeyDown = false;
            FastCall(Events.Stop);
        }
        else if (SomeKeyDown && !this.SomeKeyDown)
        {
            this.SomeKeyDown = true;
        }
    }
}
