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
        RegisterCall<float>(Events.Update, OnUpdate);
        RegisterCall<Vector3>(Events.OnPlayerMove, OnPlayerMove);
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
            var monsterConf = Config_Monster.Monster.data.monsters[RandomHelper.Next(4) + 1];
            var enemy = await this.AddChild<EEnemy>(monsterConf.Prefab);
            enemy.OnSet(monsterConf);
            var tr = enemy.GetTransform();
            tr.position = born.position;
            dEnemies[enemy.uid] = enemy;
            FastCall(Events.OnEnemyBorn, (enemy.shower, enemy.gameNumerics.hp));
        }
    }

    class EEnemy : Entity
    {
        Animator animator;
        public Transform shower => this.GetTransform();
        public GameNumerics gameNumerics;
        public override void OnStart()
        {
            base.OnStart();

            gameNumerics = this.AddChild<GameNumerics>();

            animator = this.GetMonoComponent<Animator>("animator");

            RegisterCall<(int uid, int damage, Vector3 pos)>(Events.OnBulletMove, OnBulletMove);
        }

        private async void OnBulletMove((int uid, int damage, Vector3 pos) a)
        {
            if (Vector3.Distance(a.pos, this.GetTransform().position) > conf.size)
                return;
            gameNumerics.curhp -= a.damage;
            FastCall(Events.OnBulletDamage, a.uid);
            FastCall(Events.OnEnemyHit, (shower.GetInstanceID(), gameNumerics.curhp));
            if (gameNumerics.curhp <= 0)
            {
                UnregisterCall(Events.OnBulletMove);
                this.isActive = false;
            }
        }

        EMoveState eMoveState = EMoveState.Stopped;
        internal void OnUpdate(float deltaTime, Vector3 playerPos)
        {
            if (Vector3.Distance(playerPos, this.GetTransform().position) < Config_Monster.Monster.data.alertRound)
            {
                if (eMoveState == EMoveState.Stopped)
                {
                    eMoveState = EMoveState.Moving;
                    animator.SetTrigger("Run");
                }
                MoveTowardsPlayer(playerPos, deltaTime);
            }
            else
            {
                if (eMoveState == EMoveState.Moving)
                {
                    eMoveState = EMoveState.Stopped;
                    animator.SetTrigger("Idle");
                }
            }
        }

        private void MoveTowardsPlayer(Vector3 playerPos, float deltaTime)
        {
            var tr = this.GetTransform();
            var direction = (playerPos - tr.position).normalized;
            tr.position += direction * 2f * deltaTime;
            tr.LookAt(playerPos);
        }

        Config_Monster.monsters conf;
        internal void OnSet(Config_Monster.monsters monsterConf)
        {
            this.conf = monsterConf;
            gameNumerics.OnSet(monsterConf.numerics);
        }
    }
}
