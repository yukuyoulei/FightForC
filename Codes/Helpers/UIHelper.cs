using System.Threading.Tasks;

internal static class UIHelper
{
    public static async Task<T> Create<T>() where T : _UIBase, new()
    {
        return await Create<T>(Game.world);
    }
    public static async Task<T> Create<T>(Entity parent) where T : _UIBase, new()
    {
        var t = typeof(T);
        var ui = await parent.AddChild<T>($"UI/{t.Name}");
        return ui;
    }
}
