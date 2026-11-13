using HarmonyLib;

namespace AdjustableSpaceHeater
{
    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    internal static class RegisterHeaterPatch
    {
        internal static void Prefix()
        {
            // 不加入科技树，让新旧存档都能直接在“实用”菜单建造。
            ModUtil.AddBuildingToPlanScreen("Utilities", AdjustableSpaceHeaterConfig.ID);
        }
    }
}
