using HarmonyLib;

namespace FoodResourceGenerator
{
    public sealed class Mod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODRESOURCEGENERATOR.NAME", "食物原料生成器");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODRESOURCEGENERATOR.DESC", "电力驱动的食物原料生成器，无需原料或复制人操作，原料无需事先发现或拥有。");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODRESOURCEGENERATOR.EFFECT", "通电后生成侧栏选定的肉、生蛋液、鱼肉或作物等食物原料，直接掉落在建筑脚下。速度可调 0～100 kg/s，默认 1 kg/s，工作耗电 120 W。自动化绿灯允许生产、红灯停机，未接线时允许生产。未选择原料或速度为 0 时待机。产物以 20°C 生成，保留原版食用、烹饪和腐败规则；不生产可孵化的动物蛋。");
            Strings.Add("STRINGS.UI.FOODRESOURCEGENERATOR.RATE", "生成速度");
            Strings.Add("STRINGS.UI.FOODRESOURCEGENERATOR.RATE_TOOLTIP", "按质量调节每秒生成的食物原料。设为 0 时暂停生产。");
            base.OnLoad(harmony);
        }
    }
}
