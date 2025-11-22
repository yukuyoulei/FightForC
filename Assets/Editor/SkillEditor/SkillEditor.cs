// 2025/7/2 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static SkillEditor;
using static Unity.Collections.AllocatorManager;

public class SkillEditor : EditorWindow
{
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

                if (GUILayout.Button(skills[i].skillName, GUILayout.Width(150)))
                {
                    selectedSkillIndex = i;
                }

                // 上移按钮
                if (i > 0)
                {
                    if (GUILayout.Button("↑", GUILayout.Width(25)))
                    {
                        SwapSkills(i, i - 1);
                        break;
                    }
                }
                else
                {
                    GUILayout.Space(29);
                }

                // 下移按钮
                if (i < skills.Count - 1)
                {
                    if (GUILayout.Button("↓", GUILayout.Width(25)))
                    {
                        SwapSkills(i, i + 1);
                        break;
                    }
                }
                else
                {
                    GUILayout.Space(29);
                }

                if (GUILayout.Button("Delete", GUILayout.Width(60)))
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

        // 使用反射渲染技能的所有字段
        GUILayout.BeginVertical("box");
        GUILayout.Label("Skill Properties", EditorStyles.boldLabel);
        RenderObjectFields(skills[selectedSkillIndex]);
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 交换两个技能的位置
    /// </summary>
    /// <param name="index1">第一个技能索引</param>
    /// <param name="index2">第二个技能索引</param>
    private void SwapSkills(int index1, int index2)
    {
        if (index1 >= 0 && index1 < skills.Count && index2 >= 0 && index2 < skills.Count)
        {
            Skill temp = skills[index1];
            skills[index1] = skills[index2];
            skills[index2] = temp;
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

    /// <summary>
    /// 通过反射渲染对象的所有字段
    /// </summary>
    /// <param name="obj">要渲染的对象</param>
    private void RenderObjectFields(object obj)
    {
        if (obj == null) return;

        Type objType = obj.GetType();
        FieldInfo[] fields = objType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            string fieldName = field.Name;
            object fieldValue = field.GetValue(obj);
            Type fieldType = field.FieldType;

            try
            {
                if (fieldType == typeof(int))
                {
                    int intValue = EditorGUILayout.IntField(fieldName, (int)fieldValue);
                    field.SetValue(obj, intValue);
                }
                else if (fieldType == typeof(float))
                {
                    float floatValue = EditorGUILayout.FloatField(fieldName, (float)fieldValue);
                    field.SetValue(obj, floatValue);
                }
                else if (fieldType == typeof(string))
                {
                    string stringValue = EditorGUILayout.TextField(fieldName, (string)fieldValue);
                    field.SetValue(obj, stringValue);
                }
                else if (fieldType == typeof(bool))
                {
                    bool boolValue = EditorGUILayout.Toggle(fieldName, (bool)fieldValue);
                    field.SetValue(obj, boolValue);
                }
                else if (fieldType.IsEnum)
                {
                    Enum enumValue = EditorGUILayout.EnumPopup(fieldName, (Enum)fieldValue);
                    field.SetValue(obj, enumValue);
                }
                else if (fieldType == typeof(Vector2))
                {
                    Vector2 vector2Value = EditorGUILayout.Vector2Field(fieldName, (Vector2)fieldValue);
                    field.SetValue(obj, vector2Value);
                }
                else if (fieldType == typeof(Vector3))
                {
                    Vector3 vector3Value = EditorGUILayout.Vector3Field(fieldName, (Vector3)fieldValue);
                    field.SetValue(obj, vector3Value);
                }
                else if (fieldType == typeof(Vector4))
                {
                    Vector4 vector4Value = EditorGUILayout.Vector4Field(fieldName, (Vector4)fieldValue);
                    field.SetValue(obj, vector4Value);
                }
                else if (fieldType == typeof(Color))
                {
                    Color colorValue = EditorGUILayout.ColorField(fieldName, (Color)fieldValue);
                    field.SetValue(obj, colorValue);
                }
                else if (typeof(IList).IsAssignableFrom(fieldType) && fieldType.IsGenericType)
                {
                    // 处理List<T>类型
                    Type elementType = fieldType.GetGenericArguments()[0];
                    IList list = (IList)fieldValue;
                    
                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"{fieldName} (List<{elementType.Name}>)", EditorStyles.boldLabel);
                    
                    int newCount = EditorGUILayout.IntField("Size", list.Count);
                    if (newCount != list.Count)
                    {
                        // 调整列表大小
                        while (list.Count < newCount)
                        {
                            list.Add(Activator.CreateInstance(elementType));
                        }
                        while (list.Count > newCount)
                        {
                            list.RemoveAt(list.Count - 1);
                        }
                    }
                    
                    // 渲染列表中的每个元素
                    for (int i = 0; i < list.Count; i++)
                    {
                        GUILayout.BeginVertical("box");
                        
                        // 添加移动按钮行
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Element {i + 1}", EditorStyles.boldLabel);
                        
                        // 上移按钮
                        if (i > 0)
                        {
                            if (GUILayout.Button("↑", GUILayout.Width(20)))
                            {
                                object temp = list[i];
                                list[i] = list[i - 1];
                                list[i - 1] = temp;
                                // 注意：这里不break，因为我们需要继续渲染
                            }
                        }
                        
                        // 下移按钮
                        if (i < list.Count - 1)
                        {
                            if (GUILayout.Button("↓", GUILayout.Width(20)))
                            {
                                object temp = list[i];
                                list[i] = list[i + 1];
                                list[i + 1] = temp;
                                // 注意：这里不break，因为我们需要继续渲染
                            }
                        }
                        
                        GUILayout.EndHorizontal();
                        
                        object element = list[i];
                        if (element != null && !elementType.IsPrimitive && elementType != typeof(string) && !elementType.IsEnum)
                        {
                            // 对于复杂类型，递归渲染其字段
                            RenderObjectFields(element);
                        }
                        else
                        {
                            // 对于简单类型，直接渲染
                            if (elementType == typeof(int))
                            {
                                list[i] = EditorGUILayout.IntField("Value", (int)element);
                            }
                            else if (elementType == typeof(float))
                            {
                                list[i] = EditorGUILayout.FloatField("Value", (float)element);
                            }
                            else if (elementType == typeof(string))
                            {
                                list[i] = EditorGUILayout.TextField("Value", (string)element);
                            }
                            else if (elementType == typeof(bool))
                            {
                                list[i] = EditorGUILayout.Toggle("Value", (bool)element);
                            }
                            else if (elementType.IsEnum)
                            {
                                list[i] = EditorGUILayout.EnumPopup("Value", (Enum)element);
                            }
                        }
                        
                        GUILayout.EndVertical();
                    }
                    
                    GUILayout.EndVertical();
                }
                else if (!fieldType.IsPrimitive && fieldType != typeof(string) && !fieldType.IsEnum)
                {
                    // 对于其他复杂类型，递归渲染
                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"{fieldName} ({fieldType.Name})", EditorStyles.boldLabel);
                    RenderObjectFields(fieldValue);
                    GUILayout.EndVertical();
                }
                else
                {
                    // 对于不支持的类型，显示只读字段
                    EditorGUILayout.LabelField(fieldName, fieldValue?.ToString() ?? "null");
                }
            }
            catch (Exception e)
            {
                EditorGUILayout.LabelField(fieldName, $"Error: {e.Message}");
            }
        }
    }

    [System.Serializable]
    private class SkillWrapper
    {
        public List<Skill> skills;
    }
}