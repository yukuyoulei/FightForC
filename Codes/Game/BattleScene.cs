using CloverExternal;
using System.Threading.Tasks;
using UnityEngine;

internal class BattleScene : Entity
{
    public void LoadBattle()
    {
        this.AddChild<WInput>();
        this.AddChildWithResource<GameCamera>(GetChild<WResource>());
        this.AddChildWithResource<GamePlayerCoins>(GetChild<WResource>());
        this.AddChildWithResource<GameCollections>(GetChild<WResource>());
        this.AddChildWithResource<GameTowerSlot>(GetChild<WResource>());
        this.AddChildWithResource<GameTowers>(GetChild<WResource>());
        this.AddChildWithResource<GameEnemies>(GetChild<WResource>());
        this.AddChildWithResource<GameBullets>(GetChild<WResource>());
        this.AddChildWithResource<GamePlayer>(GetChild<WResource>());
    }
}
