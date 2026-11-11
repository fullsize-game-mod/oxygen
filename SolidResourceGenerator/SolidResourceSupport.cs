namespace SolidResourceGenerator
{
    internal static class SolidResourceSupport
    {
        internal static bool IsSupported(Tag tag)
        {
            Element element = ElementLoader.GetElement(tag);
            return element != null && element.tag == tag && element.IsSolid && !element.disabled &&
                element.substance != null && element.highTemp > 1f && Assets.TryGetPrefab(tag) != null;
        }
    }
}
