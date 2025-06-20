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
        public override void OnStart()
        {
            base.OnStart();
        }
    }
}
