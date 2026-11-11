using HarmonyLib;

namespace AdjustableSpaceHeater
{
    public sealed class Mod : KMod.UserMod2
    {
        internal const string TooltipKey = "STRINGS.UI.ADJUSTABLESPACEHEATER.TOOLTIP";
        internal const string Tooltip = "低于目标温度时加热，高于目标温度时制冷。";

        public override void OnLoad(Harmony harmony)
        {
            Strings.Add(TooltipKey, Tooltip);
            base.OnLoad(harmony);
        }
    }
}
