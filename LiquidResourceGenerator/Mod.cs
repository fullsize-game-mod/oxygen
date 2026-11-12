using HarmonyLib;

namespace LiquidResourceGenerator
{
    public sealed class Mod : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS.LIQUIDRESOURCEGENERATOR.NAME", "液体资源生成器");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.LIQUIDRESOURCEGENERATOR.DESC", "电力驱动的液体资源生成器，无需原料或复制人操作，资源无需事先发现或拥有。");
            Strings.Add("STRINGS.BUILDINGS.PREFABS.LIQUIDRESOURCEGENERATOR.EFFECT", "通电后生成侧栏选定的液体元素，经底部液体管道出口排出。速度可在 0～100 kg/s 之间调节，默认 1 kg/s，实际输出受管道容量限制，工作时消耗 120 W 电力。自动化绿灯允许生产、红灯停机，未接线时允许生产。未接管道、出口阻塞、缓冲已满、未选择资源或速度为 0 时待机。通常以 20°C 生成，低温液体及熔融材料会调整至各自液态温度范围。");
            Strings.Add("STRINGS.UI.LIQUIDRESOURCEGENERATOR.RATE", "生成速度");
            Strings.Add("STRINGS.UI.LIQUIDRESOURCEGENERATOR.RATE_TOOLTIP", "调节每秒生成的资源质量，实际输出受液体管道容量限制。设为 0 时暂停生产；已生成的液体仍会排出。");
            base.OnLoad(harmony);
        }
    }
}
