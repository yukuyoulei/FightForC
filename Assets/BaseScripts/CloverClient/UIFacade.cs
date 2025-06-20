using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFacade : MonoBehaviour
{
    public string uiname;
    public List<UIElement> uielements;
    public Dictionary<string, UIElement> dElements;

    public bool Contains(Transform tr)
    {
        foreach (var ele in uielements)
        {
            if (ele.component == null)
                continue;
            if (ele.component.gameObject.transform == tr)
                return true;
        }
        return false;
    }

    private void Awake()
    {
        dElements = new();
        foreach (var ele in uielements)
        {
            dElements[ele.name] = ele;
        }
    }
}

[Serializable]
public class UIElement
{
    public Component component;
    public string comtype;
    public string name;
    public string originalName;
}