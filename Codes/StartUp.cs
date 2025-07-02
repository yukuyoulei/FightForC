using CloverExternal;
using System;
using System.Threading.Tasks;
using UnityEngine;

public static class StartUp
{
    public static void Start()
    {
        Log.Debug($"Application Start");
        InitActions();

        InitGame();
    }

    private static async void InitGame()
    {
        await UIHelper.Create<UIStart>();
    }

    private static void InitActions()
    {
        CodeLoader.UpdateAction = Update;
        CodeLoader.ApplicationQuitAction = ApplicationQuit;
        CodeLoader.ApplicationPauseAction = ApplicationPause;
        CodeLoader.ApplicationFocusAction = ApplicationFocus;

        Log.Debug($"Loaded skill count: {Config_Skills.Skills.Count}");
    }

    private static void ApplicationFocus(bool focus)
    {
        Log.Debug($"Application focus changed: {focus}");
    }

    private static void ApplicationPause(bool pause)
    {
        Log.Debug($"Application pause changed: {pause}");
    }

    private static void ApplicationQuit()
    {
        Log.Debug("Application is quitting.");
        Game.world.OnDestroy();
    }

    private static void Update()
    {
        Entity.FastCall(Events.Update, Time.deltaTime);
    }
}
