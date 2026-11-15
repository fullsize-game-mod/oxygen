using HarmonyLib;

namespace SolidResourceGenerator
{
    public sealed class Mod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDRESOURCEGENERATOR.NAME", "固体资源生成器");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDRESOURCEGENERATOR.DESC", "电力驱动的资源与物品生成器，无需原料或复制人操作，资源无需事先发现或拥有。");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.SOLIDRESOURCEGENERATOR.EFFECT", "通电后生成侧栏选定的固体元素或可拾取物品，包括数据磁盘、装备、食物、种子和蛋。固体速度为 0～100 kg/s，物品速度为 0～100 件/s，默认均为 1；物品不足一件时累计进度。工作时消耗 120 W 电力。自动化绿灯允许生产、红灯停机，未接线时允许生产。产物直接掉落在建筑脚下；未选择有效资源或对应速度为 0 时待机。");
            Strings.Add("STRINGS.UI.SOLIDRESOURCEGENERATOR.RATES", "生成速度");
            Strings.Add("STRINGS.UI.SOLIDRESOURCEGENERATOR.RATE", "固体生成速度");
            Strings.Add("STRINGS.UI.SOLIDRESOURCEGENERATOR.RATE_TOOLTIP", "选择固体元素时，每秒生成的资源质量。设为 0 时暂停固体生产。");
            Strings.Add("STRINGS.UI.SOLIDRESOURCEGENERATOR.ITEM_RATE", "物品生成速度");
            Strings.Add("STRINGS.UI.SOLIDRESOURCEGENERATOR.ITEM_RATE_TOOLTIP", "选择物品时，每秒生成的完整件数。不足一件时累计进度，设为 0 时暂停物品生产。");
            base.OnLoad(harmony);
        }
    }
}
