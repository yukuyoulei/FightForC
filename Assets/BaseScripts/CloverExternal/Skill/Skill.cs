
using System.Collections.Generic;

[System.Serializable]
public class Skill
{
    /// <summary>
    /// 技能名称
    /// </summary>
    public string skillName;
    /// <summary>
    /// CD时间
    /// </summary>
    public float cd;
    /// <summary>
    /// 技能释放半径
    /// </summary>
    public float radius;
    public List<SkillBlock> skillBlocks = new List<SkillBlock>();
}

[System.Serializable]
public class SkillBlock
{
    public int triggerMilliseconds;
    public EffectType effectType;
    public float effectValue;
    public string effectString;
}

public enum EffectType
{
    Damage,
    Sound,
    Effect,
    Bullet,
}
