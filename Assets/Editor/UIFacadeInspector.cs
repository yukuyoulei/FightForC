using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using UnityEngine.UI;
using System.IO;

[CustomEditor(typeof(UIFacade))]
public class UIFacadeInspector : Editor
{
    string filepath = "../Codes/UI/";
    int? removeIdx;
    const string asmdefPath = "Assets/BaseScripts/CodeReimporter/CodeReimporter1.asmdef";
    public override void OnInspectorGUI()
    {
        var facade = target as UIFacade;

        if (string.IsNullOrEmpty(facade.uiname))
            facade.uiname = facade.name;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("uiname:");
        facade.uiname = EditorGUILayout.TextField(facade.uiname);
        EditorGUILayout.EndHorizontal();

        if (facade.uielements == null)
            facade.uielements = new();
        for (var i = 0; i < facade.uielements.Count; i++)
        {
            var ele = facade.uielements[i];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i}.", GUILayout.Width(20));
            string name = string.IsNullOrEmpty(ele.name) ? (ele.component == null ? "" : ele.component.name) : ele.name;
            name = string.IsNullOrEmpty(name) ? "" : (name[..1].ToLower() + name[1..]);
            ele.name = EditorGUILayout.TextField(name);
            if (string.IsNullOrEmpty(ele.comtype) || ele.comtype == "Transform")
            {
                ele.comtype = GetPreferType(ele.component);
            }
            if (ele.component != null)
                ele.originalName = ele.component.name;
            EditorGUILayout.TextField(ele.originalName);
            ele.comtype = EditorGUILayout.TextField(ele.comtype);
            ele.component = EditorGUILayout.ObjectField(ele.component, GetElementComponentType(ele.component), true, GUILayout.Width(120)) as Component;
            if (GUILayout.Button("-", GUILayout.Width(20)))
                removeIdx = i;
            EditorGUILayout.EndHorizontal();
        }
        if (removeIdx.HasValue)
        {
            facade.uielements.RemoveAt(removeIdx.Value);
            removeIdx = null;
        }
        EditorGUILayout.Space(1);
        if (GUILayout.Button("Add a element"))
            facade.uielements.Add(new UIElement());

        EditorGUILayout.Space(5);
        if (GUILayout.Button("Auto Collect"))
        {
            AutoCollect(facade);
        }

        EditorGUILayout.Space(5);
        var dir = Path.Combine(Application.dataPath, filepath);
        var facadefile = $"{dir}{facade.uiname}_facade.cs";
        var rawfile = $"{dir}{facade.uiname}.cs";
        EditorGUILayout.LabelField($"Save to {facadefile}");
        if (GUILayout.Button("Gen C# File"))
        {
            GenFile(facade, facadefile, rawfile);
        }
        if (GUILayout.Button("Gen Widget Code"))
        {
            GenWidgetCode(facade);
        }
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Save"))
        {
            SaveByGameObject(facade.gameObject);
        }
        if (GUILayout.Button("ToggleAsmdef", GUILayout.Height(30)))
        {
            var fall = File.ReadAllText(asmdefPath);
            if (fall.Contains("CodeReimporter1"))
                fall = fall.Replace("CodeReimporter1", "CodeReimporter");
            else
                fall = fall.Replace("CodeReimporter", "CodeReimporter1");
            File.WriteAllText(asmdefPath, fall);
            AssetDatabase.Refresh();
        }
    }

    private void GenWidgetCode(UIFacade facade)
    {
        var codes = @$"class {facade.uiname}:Entity
{{
    [--vs]
    public override void OnStart()
    {{
        [--vsgetref]
    }}
}}
";
        var vs = "";
        var vsgetref = "";
        foreach (var ele in facade.uielements)
        {
            vs += @$"{ele.comtype} {ele.name};
";
            vsgetref += $@"{ele.name} = this.GetMonoComponent<{ele.comtype}>(""{ele.name}"");
";
        }
        codes = codes.Replace("[--vs]", vs).Replace("[--vsgetref]", vsgetref);

        UnityEngine.GUIUtility.systemCopyBuffer = codes;
    }

    private void AutoCollect(UIFacade facade)
    {
        var trs = facade.GetComponentsInChildren<Transform>();
        foreach (var tr in trs)
        {
            if (facade.Contains(tr))
                continue;
            facade.uielements.Add(new UIElement()
            {
                component = tr,
                comtype = GetPreferType(tr),
                name = tr.name,
            });
        }
    }

    private string GetPreferType(Component component)
    {
        if (component == null)
            return "Transform";
        if (component.GetComponent<Button>() != null)
            return "Button";
        if (component.GetComponent<Text>() != null)
            return "Text";
        if (component.GetComponent<InputField>() != null)
            return "InputField";
        if (component.GetComponent<Image>() != null)
            return "Image";
        if (component.GetComponent<RawImage>() != null)
            return "RawImage";
        if (component.GetComponent<Animation>() != null)
            return "Animation";
        if (component.GetComponent<RectTransform>() != null)
            return "RectTransform";
        return "Transform";
    }

    private void GenFile(UIFacade facade, string facadefile, string rawfile)
    {
        var btns = "";
        var btnlisteners = "";
        var texts = "";
        var widges = "";
        foreach (var ele in facade.uielements)
        {
            widges += @$"    public {ele.comtype} {ele.name} => this.GetMonoComponent<{ele.comtype}>(""{ele.name}"");
";
            if (ele.comtype == "Text")
            {
                texts += @$"        {ele.name}.text = ""{ele.component.GetComponent<Text>().text}"";
";
            }
            if (ele.comtype == "Button")
            {
                btns += $@"        {ele.name}.onClick.AddListener(onclick_{ele.name});
";
                btnlisteners += @$"    private void onclick_{ele.name}()
    {{
        
    }}
";
            }
        }
        var code = @$"//本文件为自动生成，不要手动修改。
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public partial class {facade.uiname}
{{
{widges}
}}
";
        var finfo = new FileInfo(facadefile);
        if (!finfo.Directory.Exists)
        {
            finfo.Directory.Create();
        }
        File.WriteAllText(facadefile, code);
        Debug.Log($"Save file {facadefile}");

        if (File.Exists(rawfile))
            return;
        code = @$"using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public partial class {facade.uiname} : Entity
{{
    public override void OnStart()
    {{
        base.OnStart();

{btns}
    }}
    public override void OnShow()
    {{
        base.OnShow();
{texts}
    }}
{btnlisteners}
}}
";
        File.WriteAllText(rawfile, code);
        Debug.Log($"Save file {rawfile}");
    }

    public static void SaveByGameObject(GameObject go)
    {
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        if (prefabStage != null)
        {
            PrefabUtility.SaveAsPrefabAsset(prefabStage.prefabContentsRoot, prefabStage.assetPath);
            Debug.Log($"Prefab Mode Saved {prefabStage.assetPath}");
        }
        else
        {
            GameObject instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            if (instanceRoot == null)
            {
                AssetDatabase.SaveAssets();
                return;
            }

            GameObject instancePrefab = PrefabUtility.GetCorrespondingObjectFromSource(instanceRoot);
            string assetPath = AssetDatabase.GetAssetPath(instancePrefab);
            PrefabUtility.SaveAsPrefabAsset(instanceRoot, assetPath);
            Debug.Log($"Scene Mode Saved {assetPath}");
        }
    }

    private System.Type GetElementComponentType(Component component)
    {
        if (component == null)
            return typeof(Transform);
        else
            return component.GetType();
    }
}
