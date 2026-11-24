using HarmonyLib;

namespace FoodPowerGenerator
{
    public sealed class Mod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODPOWERGENERATOR.NAME", "食物发电器");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODPOWERGENERATOR.DESC", "把复制人能吃的食物转化为电力。热量越高，同样多的食物能发的电越多。");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODPOWERGENERATOR.EFFECT",
                $"在侧栏勾选供料的食物，由复制人搬入。每千卡产生 {FoodPowerGenerator.JoulesPerKilocalorie} J，额定功率 {FoodPowerGeneratorConfig.Watts} W。接入电路后有食物即运转；可用自动化红灯停机。");
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
