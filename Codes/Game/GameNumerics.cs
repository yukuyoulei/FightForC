using System.Collections.Generic;

internal class GameNumerics : Entity
{
    public int curhp
    {
        get => GetNumeric(Consts.curhp);
        set => OnSet(Consts.curhp, value);
    }
    public bool alive => curhp > 0;
    public int speed
    {
        get => GetNumeric(Consts.speed);
        set => OnSet(Consts.speed, value);
    }
    public int hp
    {
        get => GetNumeric(Consts.hp);
        set => OnSet(Consts.hp, value);
    }

    Dictionary<string, int> dNumerics = new();
    public void OnSet(Dictionary<string, int> numerics)
    {
        dNumerics = numerics;
        if (numerics.TryGetValue("hp", out var hp))
            dNumerics[Consts.curhp] = hp;//回满血
    }
    public void OnSet(string key, int numeric)
    {
        dNumerics[key] = numeric;
    }
    public int GetNumeric(string key)
    {
        if (dNumerics.TryGetValue(key, out var value))
        {
            return value;
        }
        return 0;
    }
}
