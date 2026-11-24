using HarmonyLib;

namespace FoodPowerGenerator
{
    public sealed class Mod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODPOWERGENERATOR.NAME", "食物发电器");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODPOWERGENERATOR.DESC", "把食物或腐烂物转化为电力，优先消耗腐烂物。");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODPOWERGENERATOR.EFFECT",
                $"复制人会搬入腐烂物，以及在侧栏勾选的食物。腐烂物每千克产生 {FoodPowerGenerator.JoulesPerRotKg} J，食物每千卡产生 {FoodPowerGenerator.JoulesPerKilocalorie} J，额定功率 {FoodPowerGeneratorConfig.Watts} W。接入电路后有燃料即运转；可用自动化红灯停机。");
            base.OnLoad(harmony);
        }
    }

    [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
    internal static class RegisterGeneratorPatch
    {
        private static void Prefix()
        {
            ModUtil.AddBuildingToPlanScreen("Power", FoodPowerGeneratorConfig.ID);
        }
    }
}
