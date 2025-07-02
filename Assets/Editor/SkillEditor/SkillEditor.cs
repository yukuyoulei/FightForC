// 2025/7/2 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static SkillEditor;
using static Unity.Collections.AllocatorManager;

public class SkillEditor : EditorWindow
{
    [System.Serializable]
    public class Skill
    {
        public string skillName;
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
        Heal,
        Buff,
        Debuff
    }

    private List<Skill> skills = new List<Skill>();
    private int selectedSkillIndex = -1;
    private string newSkillName = "";
    private string saveFilePath;

    [MenuItem("Tools/Skill Editor")]
    public static void ShowWindow()
    {
        GetWindow<SkillEditor>("Skill Editor");
    }

    private void OnEnable()
    {
        saveFilePath = Path.Combine(Application.dataPath, "skills_data.json");
        LoadSkills();
    }

    private void OnGUI()
    {
        GUILayout.Label("Skill Editor", EditorStyles.boldLabel);

        if (selectedSkillIndex == -1)
        {
            GUILayout.Label("Skill List", EditorStyles.boldLabel);
            for (int i = 0; i < skills.Count; i++)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(skills[i].skillName, GUILayout.Width(200)))
                {
                    selectedSkillIndex = i;
                }

                if (GUILayout.Button("Delete", GUILayout.Width(70)))
                {
                    if (EditorUtility.DisplayDialog("Delete Skill", $"Are you sure you want to delete the skill '{skills[i].skillName}'?", "Yes", "No"))
                    {
                        skills.RemoveAt(i);
                        break;
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            newSkillName = GUILayout.TextField(newSkillName, GUILayout.Width(200));
            if (GUILayout.Button("Add Skill", GUILayout.Width(100)) && !string.IsNullOrEmpty(newSkillName))
            {
                skills.Add(new Skill { skillName = newSkillName });
                newSkillName = "";
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            RenderSkillEditor();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Save Skills"))
        {
            SaveSkills();
        }

        if (GUILayout.Button("Export Skills to Code"))
        {
            ExportSkillsToCode();
        }
    }

    private void RenderSkillEditor()
    {
        GUILayout.Label($"Editing Skill: {skills[selectedSkillIndex].skillName}", EditorStyles.boldLabel);

        if (GUILayout.Button("Back to Skill List"))
        {
            selectedSkillIndex = -1;
            return;
        }

        if (GUILayout.Button("Add Skill Block"))
        {
            skills[selectedSkillIndex].skillBlocks.Add(new SkillBlock());
        }

        for (int i = 0; i < skills[selectedSkillIndex].skillBlocks.Count; i++)
        {
            GUILayout.BeginVertical("box");

            GUILayout.Label($"Skill Block {i + 1}");

            skills[selectedSkillIndex].skillBlocks[i].triggerMilliseconds = EditorGUILayout.IntField("Trigger Milliseconds", skills[selectedSkillIndex].skillBlocks[i].triggerMilliseconds);
            skills[selectedSkillIndex].skillBlocks[i].effectType = (EffectType)EditorGUILayout.EnumPopup("Effect Type", skills[selectedSkillIndex].skillBlocks[i].effectType);
            skills[selectedSkillIndex].skillBlocks[i].effectValue = EditorGUILayout.FloatField("Effect Value", skills[selectedSkillIndex].skillBlocks[i].effectValue);
            skills[selectedSkillIndex].skillBlocks[i].effectString = EditorGUILayout.TextField("Effect String", skills[selectedSkillIndex].skillBlocks[i].effectString);

            if (GUILayout.Button("Remove Skill Block"))
            {
                if (EditorUtility.DisplayDialog("Remove Skill Block", $"Are you sure you want to remove Skill Block {i + 1}?", "Yes", "No"))
                {
                    skills[selectedSkillIndex].skillBlocks.RemoveAt(i);
                    break;
                }
            }

            GUILayout.EndVertical();
        }
    }

    private void SaveSkills()
    {
        try
        {
            string json = JsonUtility.ToJson(new SkillWrapper { skills = this.skills }, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log("Skills saved successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save skills: {e.Message}");
        }
    }

    private void ExportSkillsToCode()
    {
        string exportPath = Path.Combine(Application.dataPath, "../ConfigSkill/Config_Skills.cs");
        try
        {
            using (StreamWriter writer = new StreamWriter(exportPath))
            {
                writer.WriteLine("// Auto-generated skill configuration file");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine();
                writer.WriteLine("public static class Config_Skills");
                writer.WriteLine("{");
                writer.WriteLine("    public static List<Skill> Skills = new List<Skill>");
                writer.WriteLine("    {");

                for (int i = 0; i < skills.Count; i++)
                {
                    Skill skill = skills[i];
                    writer.WriteLine("        new Skill");
                    writer.WriteLine("        {");
                    writer.WriteLine($"            skillName = \"{skill.skillName}\",");
                    writer.WriteLine("            skillBlocks = new List<SkillBlock>");
                    writer.WriteLine("            {");

                    for (int j = 0; j < skill.skillBlocks.Count; j++)
                    {
                        SkillBlock block = skill.skillBlocks[j];
                        writer.WriteLine("                new SkillBlock");
                        writer.WriteLine("                {");
                        writer.WriteLine($"                    triggerMilliseconds = {block.triggerMilliseconds},");
                        writer.WriteLine($"                    effectType = EffectType.{block.effectType},");
                        writer.WriteLine($"                    effectValue = {block.effectValue}f,");
                        writer.WriteLine($"                    effectString = \"{block.effectString};\"");
                        writer.WriteLine("                },");
                    }

                    writer.WriteLine("            }");
                    writer.WriteLine("        },");
                }

                writer.WriteLine("    };");
                writer.WriteLine("}");
            }

            AssetDatabase.Refresh();
            Debug.Log("Skills exported to Config_Skills.cs successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to export skills to code: {e.Message}");
        }
    }

    private void LoadSkills()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                SkillWrapper wrapper = JsonUtility.FromJson<SkillWrapper>(json);
                skills = wrapper.skills;
                Debug.Log("Skills loaded successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load skills: {e.Message}");
            }
        }
        else
        {
            Debug.Log("No save file found, starting with an empty skill list.");
        }
    }

    [System.Serializable]
    private class SkillWrapper
    {
        public List<Skill> skills;
    }
}