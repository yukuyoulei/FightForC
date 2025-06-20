using CloverExternal;
using System;
using UnityEngine;

internal class GamePlayer : Entity
{
    Transform player;
    Animator animator;
    GameNumerics gameNumerics;
    public override void OnStart()
    {
        base.OnStart();

        player = this.GetMonoComponent<Transform>("player");
        animator = player.GetComponentInChildren<Animator>();

        gameNumerics = this.AddChild<GameNumerics>();

        RegisterCall<Vector3>(Events.Move, OnMove);
        RegisterCall<float>(Events.Update, OnUpdate);
        RegisterCall(Events.Stop, OnStop);

        FastCall(Events.OnPlayerMove, player.position);
    }

    const float CD = 0.2f; // 射击冷却时间
    float cd = CD;
    private void OnUpdate(float deltaSec)
    {
        cd -= deltaSec;
        if (cd <= 0)
        {
            cd = CD;
            animator.SetTrigger("Shot");
            FastCall(Events.OnPlayerShoot, player);
        }
    }

    private void OnStop()
    {
        if (!this.direction.HasValue)
            return;
        this.direction = null;
        animator.SetTrigger("Idle");
    }

    Vector3? direction;
    const float SPEED = 20;
    private void OnMove(Vector3 direction)
    {
        if (!this.direction.HasValue)
        {
            this.direction = direction;
            animator.SetTrigger("Run");
        }

        var dir = Quaternion.Euler(direction);
        player.localRotation = Quaternion.Lerp(player.localRotation, dir, Time.deltaTime * 10f);

        float speed = SPEED;
        // 只有当旋转接近目标角度时才允许移动
        if (Quaternion.Angle(player.localRotation, dir) > 10f)
            speed = speed / 3 * 2;

        player.localPosition += player.forward * Time.deltaTime * speed;
        FastCall(Events.OnPlayerMove, player.position);
    }
}
