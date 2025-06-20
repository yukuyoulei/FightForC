internal static class Events
{
    public const string Update = "Update";

    public const string Move = "Move";
    public const string Stop = "Stop";

    public const string OnPlayerMove = "OnPlayerMove";//玩家移动
    public const string OnPlayerCarry = "OnPlayerCarry";//玩家增加携带的金币模型
    public const string FlyCoinReuse = "FlyCoinReuse";//飞金币复用
    public const string OnBuildTower = "OnBuildTower";//建造塔
    public const string OnTowerSlotShow = "OnTowerSlotShow";//塔槽显示
    public const string OnTowerSlotGetCoin = "OnTowerSlotGetCoin";//塔槽获取金币
    public const string OnPlayerShoot = "OnPlayerShoot";//玩家射击
    public const string OnBulletMove = "OnBulletMove";//子弹移动
    public const string OnBulletDamage = "OnBulletDamage";//子弹造成了伤害
    public const string OnEnemyBorn = "OnEnemyBorn";//敌人出生
    public const string OnEnemyHit = "OnEnemyHit";//敌人被击中
    public const string OnEnemyDeath = "OnEnemyDeath";//敌人死亡
    public const string OnEnemyMove = "OnEnemyMove";//敌人移动

    public const string OnBattleReady = "OnBattleReady";//战斗准备就绪
}
