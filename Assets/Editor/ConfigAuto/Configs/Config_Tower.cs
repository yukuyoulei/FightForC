using System;
using System.Collections.Generic;
public partial class Config_Tower
{
    public object data = new
    {
        towers = new object[]
        {
            new { level = 1, Prefab = "Prefab/Weapon Hammer", shootRadius = 40f, rotateSpeed = 60, shootCD = 0.2f, },
            new { level = 2, Prefab = "Weapon HandGun", shootRadius = 45f, rotateSpeed = 80, shootCD = 0.2f,},
            new { level = 3, Prefab = "Weapon SubMachineGun", shootRadius = 50f, rotateSpeed = 100, shootCD = 0.2f,},
        },
    };
}
