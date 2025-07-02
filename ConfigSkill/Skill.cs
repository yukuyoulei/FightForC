// 2025/7/2 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using System.Collections.Generic;

// Enum to define the types of effects a skill can have
public enum EffectType
{
    Damage,      // Represents a damage-dealing skill
    Heal,        // Represents a healing skill
    Buff,        // Represents a buff skill that enhances attributes
    Debuff       // Represents a debuff skill that weakens attributes
}

// Class representing a skill
[Serializable]
public class Skill
{
    // Name of the skill
    public string skillName;

    // List of skill blocks defining the behavior of the skill
    public List<SkillBlock> skillBlocks = new List<SkillBlock>();
}

// Class representing a block within a skill
[Serializable]
public class SkillBlock
{
    // Time in milliseconds when the block is triggered
    public int triggerMilliseconds;

    // The type of effect this block applies
    public EffectType effectType;

    // The value of the effect (e.g., damage amount, healing amount)
    public float effectValue;

    // Additional string data for the effect (e.g., effect description)
    public string effectString;
}