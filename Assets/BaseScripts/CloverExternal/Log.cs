using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloverExternal
{
    public static class Log
    {
        public static void Debug(string message)
        {
            UnityEngine.Debug.Log($"[CloverExternal] {message}");
        }
        public static void Warning(string message)
        {
            UnityEngine.Debug.LogWarning($"[CloverExternal Warning] {message}");
        }
        public static void Error(string message)
        {
            UnityEngine.Debug.LogError($"[CloverExternal Error] {message}");
        }
        public static void Assert(bool condition, string message)
        {
            if (condition)
                return;
            UnityEngine.Debug.LogError($"[CloverExternal Assert] {message}");
        }
    }
}
