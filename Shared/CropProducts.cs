using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ResourceGenerators
{
    internal static class CropProducts
    {
        // U60 changed cropVal from a single harvest to a list and changed its CLR type.
        // Resolve both layouts at runtime so the same assembly can load on earlier builds.
        private static readonly FieldInfo CropValue = typeof(Crop).GetField("cropVal");
        private static readonly FieldInfo Products = CropValue?.FieldType.GetField("products");
        private static readonly FieldInfo LegacyId = CropValue?.FieldType.GetField("cropId");
        private static readonly FieldInfo LegacyQuantity = CropValue?.FieldType.GetField("numProduced");
        private static readonly System.Type ProductType = CropValue?.FieldType.GetNestedType("ProductInfo");
        private static readonly FieldInfo ProductId = ProductType?.GetField("cropId");
        private static readonly FieldInfo ProductQuantity = ProductType?.GetField("quantity");

        internal static IEnumerable<Tag> GetTags(Crop crop)
        {
            object value = crop == null ? null : CropValue?.GetValue(crop);
            if (value == null)
                yield break;

            if (Products == null)
            {
                if (LegacyQuantity?.GetValue(value) is int count && count > 0 &&
                    LegacyId?.GetValue(value) is string id && id.Length > 0)
                    yield return new Tag(id);
                yield break;
            }

            if (!(Products.GetValue(value) is IList products))
                yield break;

            foreach (object product in products)
            {
                if (ProductQuantity?.GetValue(product) is float quantity && quantity > 0f &&
                    ProductId?.GetValue(product) is string id && id.Length > 0)
                    yield return new Tag(id);
            }
        }
    }
}
