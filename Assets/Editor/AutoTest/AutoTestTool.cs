// 2025/7/10 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class AutoTestTool : EditorWindow
{
    private bool isRecording = false;
    private bool isReplaying = false;

    [MenuItem("Tools/Auto Test Tool")]
    public static void ShowWindow()
    {
        GetWindow<AutoTestTool>("Auto Test Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auto Test Tool", EditorStyles.boldLabel);

        if (!isRecording && !isReplaying)
        {
            if (GUILayout.Button("Start Recording"))
            {
                StartRecording();
            }
        }
        else if (isRecording)
        {
            if (GUILayout.Button("Stop Recording"))
            {
                StopRecording();
            }
        }

        if (!isRecording && !isReplaying)
        {
            if (GUILayout.Button("Start Replay"))
            {
                StartReplay();
            }
        }
        else if (isReplaying)
        {
            GUILayout.Label("Replaying...");
        }
    }

    private void StartRecording()
    {
        isRecording = true;
        EventRecorder.StartRecording();
    }

    private void StopRecording()
    {
        isRecording = false;
        EventRecorder.StopRecording();
    }

    private void StartReplay()
    {
        isReplaying = true;
        EventRecorder.StartReplay(() =>
        {
            isReplaying = false;
        });
    }
}
