using CloverExternal;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

public class Entry : MonoBehaviour
{
    void Start()
    {
        LoadAssembly();
    }

    private void LoadAssembly()
    {
        var bs = File.ReadAllBytes("Library/ScriptAssemblies/Codes.dll");
        var pdb = File.ReadAllBytes("Library/ScriptAssemblies/Codes.pdb");
        var assembly = Assembly.Load(bs, pdb);
        var type = assembly.GetType("StartUp");
        var method = type.GetMethod("Start", BindingFlags.Public | BindingFlags.Static);
        method.Invoke(null, null);
    }

    private void Update()
    {
        CodeLoader.UpdateAction?.Invoke();
    }
    private void OnApplicationPause(bool pause)
    {
        CodeLoader.ApplicationPauseAction?.Invoke(pause);
    }
    private void OnApplicationFocus(bool focus)
    {
        CodeLoader.ApplicationFocusAction?.Invoke(focus);
    }
    private void OnApplicationQuit()
    {
        CodeLoader.ApplicationQuitAction?.Invoke();
    }
}
