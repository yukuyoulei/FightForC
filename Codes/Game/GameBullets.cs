using ConfigAuto;
using System.Collections.Generic;
using UnityEngine;

internal class GameBullets : Entity
{
    Dictionary<int, EBullet> dBullets = new();
    public override void OnStart()
    {
        base.OnStart();
        RegisterCall<float>(Events.Update, OnUpdate);
        RegisterCall<(Vector3 rot, Vector3 pos)>(Events.OnPlayerShoot, OnPlayerShoot);
        RegisterCall<int>(Events.OnBulletDamage, OnBulletDamage);
    }

    private void OnBulletDamage(int uid)
    {
        if (!dBullets.ContainsKey(uid))
            return;
        dBullets[uid].isActive = false;
    }

    private async void OnPlayerShoot((Vector3 rot, Vector3 pos) player)
    {
        var dir = player.rot;
        var pos = player.pos;
        var conf = Config_Bullet.Bullet.data.bullet[1];
        var bullet = await this.AddChild<EBullet>(conf.Prefab);
        bullet.OnSet(conf, pos, dir);
        dBullets[bullet.uid] = bullet;
    }

    List<EBullet> remove = new();
    private void OnUpdate(float deltaSec)
    {
        foreach (var bullet in dBullets.Values)
        {
            if (bullet.isActive)
                bullet.OnUpdate(deltaSec);
            else
                remove.Add(bullet);
        }
        foreach (var bullet in remove)
        {
            dBullets.Remove(bullet.uid);
            RemoveChild(bullet);
        }
    }

    class EBullet : Entity
    {
        Transform shower;
        GameNumerics gameNumerics;
        public override void OnStart()
        {
            base.OnStart();

            shower = this.GetMonoComponent<Transform>("shower");
            gameNumerics = this.AddChild<GameNumerics>();
        }

        Config_Bullet.bullet conf;
        internal void OnSet(Config_Bullet.bullet conf, Vector3 pos, Vector3 dir)
        {
            this.conf = conf;
            gameNumerics.OnSet(conf.numerics);
            shower.position = pos;
            shower.eulerAngles = dir;
        }

        float range;
        internal void OnUpdate(float deltaSec)
        {
            var delta = shower.forward * gameNumerics.speed * deltaSec;
            shower.position += delta;
            FastCall(Events.OnBulletMove, (uid, conf.numerics[Consts.damage], shower.position));
            range += delta.magnitude;
            if (range > conf.numerics["range"])
            {
                this.isActive = false;
            }
        }
    }
}
