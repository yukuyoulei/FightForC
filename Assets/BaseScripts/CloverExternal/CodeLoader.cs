using System;
using UnityEngine;
namespace CloverExternal
{
    public static class CodeLoader
    {
        public static readonly string[] ExtraFiles = { "ConfigSkill" };

        public static Action UpdateAction;
        public static Action ApplicationQuitAction;
        public static Action<bool> ApplicationPauseAction;
        public static Action<bool> ApplicationFocusAction;
    }
}