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

        RegisterCall<(int uid,Vector3 pos)>(Events.OnEnemyDeath, OnEnemyDeath);
        RegisterCall<(Vector3 enemyPos, int uid)>(Events.OnEnemyMove, OnEnemyMove);

        FastCall(Events.OnPlayerMove, player.position);
    }

    Vector3 nearestEnemyPos;
    int? nearestEnemyID;
    float nearestSub;
    private void OnEnemyDeath((int uid, Vector3 pos) a)
    {
        if (nearestEnemyID.HasValue && nearestEnemyID.Value == a.uid)
        {
            nearestEnemyID = null;
        }
    }
    private void OnEnemyMove((Vector3 enemyPos, int uid) tuple)
    {
        var sub = Vector3.Distance(tuple.enemyPos, player.position);
        if (!nearestEnemyID.HasValue || sub < nearestSub)
        {
            nearestEnemyID = tuple.uid;
            nearestSub = sub;
            nearestEnemyPos = tuple.enemyPos;
        }
        else if (nearestEnemyID.HasValue && nearestEnemyID.Value == tuple.uid)
        {
            nearestEnemyPos = tuple.enemyPos;
            nearestSub = sub;
        }
    }
    const float CD = 0.2f; // 射击冷却时间
    float cd = CD;
    private void OnUpdate(float deltaSec)
    {
        cd -= deltaSec;
        if (cd <= 0)
        {
            cd = CD;
            if (!nearestEnemyID.HasValue)
                return;
            animator.SetTrigger("Shot");

            // 计算朝向最近敌人
            Vector3 dir = nearestEnemyPos - player.position;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                player.rotation = lookRotation; // 让角色朝向最近敌人
                FastCall(Events.OnPlayerShoot, (lookRotation.eulerAngles, player.position));
            }
            else
            {
                FastCall(Events.OnPlayerShoot, (player.eulerAngles, player.position));
            }
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

        // 计算旋转对应的前进方向
        var moveDir = Quaternion.Euler(direction) * Vector3.forward;
        player.localPosition += moveDir.normalized * Time.deltaTime * SPEED;
        FastCall(Events.OnPlayerMove, player.position);
    }
}
