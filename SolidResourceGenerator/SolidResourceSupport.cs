namespace SolidResourceGenerator
{
    internal static class SolidResourceSupport
    {
        // 列表与生产共用检查；物品即使映射到基础元素，也不能冒充该元素生成。
        internal static bool IsSupported(Tag tag)
        {
            Element element = ElementLoader.GetElement(tag);
            return element != null && element.tag == tag && element.IsSolid && !element.disabled &&
                element.substance != null && element.highTemp > 1f && Assets.TryGetPrefab(tag) != null;
        }
    }
}
