using ConfigAuto;
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
        var conf = Config_Tower.Tower.data.towers[0];
        var tower = await this.AddChild<ETower>(conf.Prefab);
        tower.OnSet(conf);
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
            RegisterCall<float>(Events.Update, OnUpdate);
            RegisterCall<(int uid, Vector3 pos)>(Events.OnEnemyDeath, OnEnemyDeath);
            RegisterCall<(Vector3 enemyPos, int uid)>(Events.OnEnemyMove, OnEnemyMove);
        }

        float cd;
        private void OnUpdate(float deltaSec)
        {
            if (!nearestEnemyID.HasValue)
                return;
            if (Vector3.Distance(nearestEnemyPos, transform.position) > conf.shootRadius)
            {
                // 如果敌人距离过远，则不射击
                return;
            }
            //如果transform的朝向和nearestEnemyPos相差超过5度，则先旋转，否则 FastCall(Events.OnPlayerShoot, transform);
            var dir = nearestEnemyPos - transform.position;
            dir.y = 0; // 保持水平面
            var angle = Vector3.Angle(transform.forward, dir);
            if (angle > 1f)
            {
                var targetRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, conf.rotateSpeed * deltaSec);
            }
            else
            {
                cd -= deltaSec;
                if (cd <= 0)
                {
                    cd = conf.shootCD;
                    FastCall(Events.OnPlayerShoot, (transform.eulerAngles, transform.position));
                }
            }
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
        public Config_Tower.towers conf { get; private set; }
        internal void OnSet(Config_Tower.towers conf)
        {
            this.conf = conf;
            cd = conf.shootCD;
        }
    }
}
