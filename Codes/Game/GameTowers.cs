using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

internal class GameTowers : Entity
{
    public override void OnStart()
    {
        base.OnStart();

        RegisterCall<Transform>(Events.OnBuildTower, OnBuildTower);
    }

    Dictionary<int, ETower> dTowers = new();
    private async void OnBuildTower(Transform target)
    {
        var tower = await this.AddChild<ETower>("Prefab/Weapon Hammer");
        Transform towerTr = tower.GetTransform();
        towerTr.position = target.position;
        dTowers[towerTr.GetInstanceID()] = tower;
    }
    class ETower : Entity
    {
        Transform transform;
        public override void OnStart()
        {
            base.OnStart();
            transform = this.GetTransform();
            RegisterCall<(Vector3 enemyPos, int uid)>(Events.OnEnemyMove, OnEnemyMove);
            RegisterCall<float>(Events.Update, OnUpdate);
            RegisterCall<int>(Events.OnEnemyDeath, OnEnemyDeath);
        }

        private void OnEnemyDeath(int uid)
        {
            if (nearestEnemyID.HasValue && nearestEnemyID.Value == uid)
            {
                nearestEnemyID = null;
            }
        }

        const float CD = 0.2f; // 射击冷却时间
        float cd = CD;
        const float RotateSpeed = 60f; // 每秒旋转速度
        private void OnUpdate(float deltaSec)
        {
            if (!nearestEnemyID.HasValue)
                return;
            //如果transform的朝向和nearestEnemyPos相差超过5度，则先旋转，否则 FastCall(Events.OnPlayerShoot, transform);
            var dir = nearestEnemyPos - transform.position;
            dir.y = 0; // 保持水平面
            var angle = Vector3.Angle(transform.forward, dir);
            if (angle > 1f)
            {
                var targetRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotateSpeed * deltaSec);
            }
            else
            {
                cd -= deltaSec;
                if (cd <= 0)
                {
                    cd = CD;
                    FastCall(Events.OnPlayerShoot, transform);
                }
            }
        }

        Vector3 nearestEnemyPos;
        int? nearestEnemyID;
        float nearestSub;
        private void OnEnemyMove((Vector3 enemyPos, int uid) tuple)
        {
            var sub = Vector3.Distance(tuple.enemyPos, transform.position);
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

    }
}
