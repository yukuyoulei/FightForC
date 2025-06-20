using System.Threading.Tasks;
using UnityEngine;

internal static class ResourceHelper
{
    public static async Task<T> Load<T>(string path) where T : Object
    {
        try
        {
            return Resources.Load<T>(path);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load resource '{path}': {e.Message}");
            return null;
        }
    }
}
