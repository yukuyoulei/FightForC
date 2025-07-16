// 2025/7/10 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class RecordedEvent
{
    public float Time; // 事件发生的时间
    public string ClickedObjectName; // 被点击对象的名称
    public bool IsUIButton; // 是否是 UI 按钮

    public RecordedEvent(float time, string clickedObjectName, bool isUIButton)
    {
        Time = time;
        ClickedObjectName = clickedObjectName;
        IsUIButton = isUIButton;
    }
}
