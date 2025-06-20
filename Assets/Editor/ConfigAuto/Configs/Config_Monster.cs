using System;
using System.Collections.Generic;
public partial class Config_Monster
{
    public object data = new
    {
        moveSpeed = 15f,
        alertRound = 30f,

        monsters = new Dictionary<int, object> 
        {
            [1] = new { id = 1, Prefab = "Prefab/Enemy A", size = 3, numerics = new Dictionary<object, object>() { {"hp", 10}, {"att", 1}, {"def", 1} } },
            [2] = new { id = 2, Prefab = "Prefab/Enemy B", size = 3, numerics = new Dictionary<object, object>() { {"hp", 20}, {"att", 2}, {"def", 1} } },
            [3] = new { id = 3, Prefab = "Prefab/Enemy C", size = 3, numerics = new Dictionary<object, object>() { {"hp", 30}, {"att", 3}, {"def", 1} } },
            [4] = new { id = 4, Prefab = "Prefab/Enemy D", size = 3, numerics = new Dictionary<object, object>() { {"hp", 40}, {"att", 4}, {"def", 1} } },
        }
    };
}