using System;
using System.Collections.Generic;
public partial class Config_Bullet
{
    public object data = new
    {
        moveSpeed = 15f,

        bullet = new Dictionary<int, object>()
        {
            {1, new { id = 1, Prefab = "Prefab/Bullet Case", numerics = new Dictionary<object, object>() { {"speed", 60}, {"damage", 5}, {"range", 100} } } },
            {2, new { id = 2, Prefab = "Prefab/Bullet Case", numerics = new Dictionary<object, object>() { {"speed", 65}, {"damage", 10}, {"range", 150} } } },
            {3, new { id = 3, Prefab = "Prefab/Bullet Case", numerics = new Dictionary<object, object>() { {"speed", 70}, {"damage", 15}, {"range", 200} } } },
            {4, new { id = 4, Prefab = "Prefab/Bullet Case", numerics = new Dictionary<object, object>() { {"speed", 75}, {"damage", 20}, {"range", 250} } } },
        }
    };
}