using HarmonyLib;

namespace SolidResourceGenerator
{
    public sealed class Mod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDRESOURCEGENERATOR.NAME", "固体资源生成器");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDRESOURCEGENERATOR.DESC", "电力驱动的固体资源生成器，无需原料或复制人操作，资源无需事先发现或拥有。");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDRESOURCEGENERATOR.EFFECT", "通电后生成侧栏选定的固体元素，速度可在 0～100 kg/s 之间调节，默认 1 kg/s，工作时消耗 120 W 电力。自动化绿灯允许生产、红灯停机，未接线时允许生产。产物直接掉落在建筑脚下；未选择资源或速度为 0 时待机。");
            Strings.Add("STRINGS.UI.SOLIDRESOURCEGENERATOR.RATE", "生成速度");
            Strings.Add("STRINGS.UI.SOLIDRESOURCEGENERATOR.RATE_TOOLTIP", "调节每秒生成的资源质量。设为 0 时暂停生产。");
            base.OnLoad(harmony);
        }
    }
}
