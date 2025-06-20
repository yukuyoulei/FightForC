using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class UIFacadeHierarchy : Editor
{
    static UIFacadeHierarchy()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItem;
    }

    static void HierarchyWindowItem(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (obj == null)
            return;
        if (obj.GetComponent<UIFacade>() != null)
        {
            var text = "¡ï";
            Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));
            float xx = selectionRect.x + selectionRect.width - textSize.x;
            float yy = selectionRect.y;
            GUI.Label(new Rect(xx, yy, textSize.x, textSize.y), text, styleUIFacade.Value);
        }
        if (obj.transform.parent != null)
        {
            var uif = obj.transform.parent.GetComponentInParent<UIFacade>();
            if (uif == null)
                return;
            if (uif.uielements == null)
                return;
            UIElement found = null;
            foreach (var uiele in uif.uielements)
            {
                if (uiele == null)
                    continue;
                if (uiele.component == null)
                    continue;
                if (uiele.component?.gameObject == obj)
                {
                    found = uiele;
                    break;
                }
            }
            if (found == null)
            {
                float x = selectionRect.x + selectionRect.width - 54;
                float y = selectionRect.y;
                if (GUI.Button(new Rect(x, y, 35, 18), "+"))
                {
                    uif.uielements.Add(new UIElement()
                    {
                        component = obj.transform,
                        comtype = "Transform",
                        name = obj.name,
                    });
                    var o = uif.gameObject;
                    while (o.transform.parent != null)
                    {
                        var p = o.transform.parent;
                        var pf = p.GetComponentInParent<UIFacade>();
                        if (pf == null)
                            break;
                        o = pf.gameObject;
                    }
                    UIFacadeInspector.SaveByGameObject(o);
                }
            }
            else
            {
                string text = found.name; // ¸½¼ÓÎÄ×Ö
                Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(text));

                float x = selectionRect.x + selectionRect.width - textSize.x - 18;
                float y = selectionRect.y;

                GUI.TextField(new Rect(x, y, textSize.x + 5, textSize.y), text, styleUIElement.Value);
            }
        }
    }
    static Lazy<GUIStyle> styleUIElement = new Lazy<GUIStyle>(() =>
     {
         return new GUIStyle()
         {
             alignment = TextAnchor.MiddleCenter,
             fontSize = 12,
             normal = new GUIStyleState()
             {
                 textColor = Color.green,
             },
         };
     });
    static Lazy<GUIStyle> styleUIFacade = new Lazy<GUIStyle>(() =>
     {
         return new GUIStyle()
         {
             alignment = TextAnchor.MiddleCenter,
             fontSize = 15,
             normal = new GUIStyleState()
             {
                 textColor = Color.yellow,
             },
         };
     });
}