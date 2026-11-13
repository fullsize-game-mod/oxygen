using HarmonyLib;

namespace FoodResourceGenerator
{
    public sealed class Mod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODRESOURCEGENERATOR.NAME", "食物原料生成器");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODRESOURCEGENERATOR.DESC", "电力驱动的食物原料生成器，无需原料或复制人操作，自动支持当前游戏已加载的食物配方。");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.FOODRESOURCEGENERATOR.EFFECT", "通电后生成侧栏选定的食物配方输入物品，直接掉落在建筑脚下。列表从已加载配方自动生成，支持 DLC 与其他 Mod 的食物物品；水、泥土等元素和可孵化蛋不在范围内。速度可在 0～100 kg/s 之间调节，默认 1 kg/s，工作时消耗 120 W 电力。自动化绿灯允许生产、红灯停机，未接线时允许生产。未选择原料或速度为 0 时待机。产物以 20°C 生成，保留原版食用、烹饪和腐败规则。");
            Strings.Add("STRINGS.UI.FOODRESOURCEGENERATOR.RATE", "生成速度");
            Strings.Add("STRINGS.UI.FOODRESOURCEGENERATOR.RATE_TOOLTIP", "按质量调节每秒生成的食物原料。设为 0 时暂停生产。");
            base.OnLoad(harmony);
        }
    }
}
