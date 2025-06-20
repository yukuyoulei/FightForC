using ConfigAuto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

internal class GameEnemies : Entity
{
    Transform enemies;
    public override void OnStart()
    {
        base.OnStart();

        enemies = this.GetMonoComponent<Transform>("enemies");

        RegisterCall(Events.OnBattleReady, OnBattleReady);
        RegisterCall(Events.OnEnemyDeath, OnEnemyDeath);
        RegisterCall<float>(Events.Update, OnUpdate);
        RegisterCall<Vector3>(Events.OnPlayerMove, OnPlayerMove);
    }

    private void OnEnemyDeath()
    {
        BornEnemy(enemies.GetChild(RandomHelper.Next(enemies.childCount)));
    }

    Vector3 playerPos;
    private void OnPlayerMove(Vector3 vector)
    {
        playerPos = vector;
    }

    List<EEnemy> remove = new();
    private void OnUpdate(float deltaTime)
    {
        foreach (var enemy in dEnemies.Values)
        {
            if (enemy.isActive)
            {
                enemy.OnUpdate(deltaTime, playerPos);
            }
            else
            {
                remove.Add(enemy);
            }
        }
        foreach (var enemy in remove)
        {
            dEnemies.Remove(enemy.uid);
            RemoveChild(enemy);
        }
    }

    Dictionary<int, EEnemy> dEnemies = new();
    private async void OnBattleReady()
    {
        var istart = RandomHelper.Next(enemies.childCount);
        for (int i = 0; i < 5; i++)
        {
            if (istart >= enemies.childCount)
            {
                istart = 0;
            }
            var born = enemies.GetChild(istart);
            istart++;
            BornEnemy(born);
        }
    }

    private async void BornEnemy(Transform born)
    {
        var monsterConf = Config_Monster.Monster.data.monsters[RandomHelper.Next(4) + 1];
        var enemy = await this.AddChild<EEnemy>(monsterConf.Prefab);
        enemy.OnSet(monsterConf);
        var tr = enemy.GetTransform();
        tr.position = born.position;
        dEnemies[enemy.uid] = enemy;
        FastCall(Events.OnEnemyBorn, (enemy.transform, enemy.gameNumerics.hp));
    }

    class EEnemy : Entity
    {
        Animator animator;
        public Transform transform;
        public GameNumerics gameNumerics;
        public override void OnStart()
        {
            base.OnStart();

            transform = this.GetTransform();
            gameNumerics = this.AddChild<GameNumerics>();

            animator = this.GetMonoComponent<Animator>("animator");

            RegisterCall<(int uid, int damage, Vector3 pos)>(Events.OnBulletMove, OnBulletMove);
        }

        private async void OnBulletMove((int uid, int damage, Vector3 pos) a)
        {
            if (Vector3.Distance(a.pos, transform.position) > conf.size)
                return;
            gameNumerics.curhp -= a.damage;
            FastCall(Events.OnBulletDamage, a.uid);
            FastCall(Events.OnEnemyHit, (transform.GetInstanceID(), gameNumerics.curhp));
            if (gameNumerics.curhp <= 0)
            {
                FastCall(Events.OnEnemyDeath, uid);
                UnregisterCall(Events.OnBulletMove);
                this.isActive = false;
            }
        }

        EMoveState eMoveState = EMoveState.Stopped;
        float IdleSec;
        Vector3? patrollingPos;
        internal void OnUpdate(float deltaTime, Vector3 playerPos)
        {
            if (Vector3.Distance(playerPos, transform.position) < Config_Monster.Monster.data.alertRound)
            {
                if (eMoveState != EMoveState.Moving)
                {
                    eMoveState = EMoveState.Moving;
                    animator.SetTrigger("Run");
                }
                MoveTowardsPos(playerPos, deltaTime);
            }
            else if (eMoveState == EMoveState.Patrolling)
            {
                if (!patrollingPos.HasValue)
                    patrollingPos = transform.position + new Vector3(RandomHelper.NextFloat(-15f, 15f)
                        , 0, RandomHelper.NextFloat(-15f, 15f));
                MoveTowardsPos(patrollingPos.Value, deltaTime);
                if (Vector3.Distance(patrollingPos.Value, transform.position) < 2)
                    OnMoveStop();
            }
            else
            {
                if (eMoveState == EMoveState.Moving)
                {
                    OnMoveStop();
                }
                else if (eMoveState == EMoveState.Stopped)
                {
                    IdleSec -= deltaTime;
                    if (IdleSec <= 0f)
                    {
                        eMoveState = EMoveState.Patrolling;
                        animator.SetTrigger("Run");
                    }
                }
            }
        }

        private void OnMoveStop()
        {
            eMoveState = EMoveState.Stopped;
            IdleSec = RandomHelper.NextFloat(1f, 3f);
            animator.SetTrigger("Idle");
        }

        private void MoveTowardsPos(Vector3 targetPos, float deltaTime)
        {
            var direction = (targetPos - transform.position).normalized;
            transform.position += direction * 2f * deltaTime;
            transform.LookAt(targetPos);
            FastCall(Events.OnEnemyMove, (transform.position, uid));
        }

        Config_Monster.monsters conf;
        internal void OnSet(Config_Monster.monsters monsterConf)
        {
            this.conf = monsterConf;
            gameNumerics.OnSet(monsterConf.numerics);
        }
    }
}
