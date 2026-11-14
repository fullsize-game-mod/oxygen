using HarmonyLib;

namespace PlantResourceGenerator
{
    public sealed class Mod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS.PLANTRESOURCEGENERATOR.NAME", "植物收获物生成器");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.PLANTRESOURCEGENERATOR.DESC", "电力驱动的植物收获物生成器，无需种植、原料或复制人操作，自动列出当前已加载作物的收获物。");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.PLANTRESOURCEGENERATOR.EFFECT", "通电后生成侧栏选定的植物收获物，直接掉落在建筑脚下。支持可拾取的食用和非食用产物，包含作为收获物的谷粒、豆类；无需事先发现或拥有。速度可在 0～100 kg/s 之间调节，默认 1 kg/s，工作时消耗 120 W 电力。自动化绿灯允许生产、红灯停机，未接线时允许生产。未选择资源或速度为 0 时待机。产物通常为 20°C，保留原版用途和腐败规则。");
            Strings.Add("STRINGS.UI.PLANTRESOURCEGENERATOR.RATE", "生成速度");
            Strings.Add("STRINGS.UI.PLANTRESOURCEGENERATOR.RATE_TOOLTIP", "按质量调节每秒生成的植物收获物。设为 0 时暂停生产。");
            base.OnLoad(harmony);
        }
    }
}
