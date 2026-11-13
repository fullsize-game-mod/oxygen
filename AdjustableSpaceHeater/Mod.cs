using HarmonyLib;

namespace AdjustableSpaceHeater
{
    public sealed class Mod : KMod.UserMod2
    {
        internal const string TooltipKey = "STRINGS.UI.ADJUSTABLESPACEHEATER.TOOLTIP";
        internal const string Tooltip = "低于目标温度时加热，高于目标温度时制冷。";

        public override void OnLoad(Harmony harmony)
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS.ADJUSTABLESPACEHEATER.NAME", "空间温控器");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.ADJUSTABLESPACEHEATER.DESC", "开局即可建造的独立温控建筑，低于目标温度时加热，高于目标温度时制冷。");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.ADJUSTABLESPACEHEATER.EFFECT", "调节周围气体与液体的温度，目标可设为 -200～200°C，换热功率为 18 kDTU/s，工作时消耗 120 W 电力。无需研究；自动化绿灯允许工作、红灯停机，未接线时允许工作。");
            Strings.Add(TooltipKey, Tooltip);
            base.OnLoad(harmony);
        }
    }
}
