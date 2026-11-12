using System;

namespace LiquidResourceGenerator
{
    internal static class LiquidResourceSupport
    {
        // 列表与生产共用检查，物品标签不能冒充元素；留出 1 K 的相变余量。
        internal static bool IsSupported(Tag tag)
        {
            Element element = ElementLoader.GetElement(tag);
            return element != null && element.tag == tag && element.IsLiquid && !element.disabled &&
                element.substance != null && Assets.TryGetPrefab(tag) != null &&
                !float.IsNaN(element.lowTemp) && !float.IsInfinity(element.lowTemp) &&
                !float.IsNaN(element.highTemp) && !float.IsInfinity(element.highTemp) &&
                Math.Max(1f, element.lowTemp + 1f) <= element.highTemp - 1f;
        }

        internal static float GetOutputTemperature(Element element)
        {
            return Math.Max(Math.Max(1f, element.lowTemp + 1f), Math.Min(293.15f, element.highTemp - 1f));
        }
    }
}
