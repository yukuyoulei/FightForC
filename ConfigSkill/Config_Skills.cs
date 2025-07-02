// Auto-generated skill configuration file
using System.Collections.Generic;

public static class Config_Skills
{
    public static List<Skill> Skills = new List<Skill>
    {
        new Skill
        {
            skillName = "1",
            skillBlocks = new List<SkillBlock>
            {
                new SkillBlock
                {
                    triggerMilliseconds = 1,
                    effectType = EffectType.Damage,
                    effectValue = 0f,
                    effectString = ";"
                },
                new SkillBlock
                {
                    triggerMilliseconds = 0,
                    effectType = EffectType.Heal,
                    effectValue = 0f,
                    effectString = ";"
                },
            }
        },
    };
}
