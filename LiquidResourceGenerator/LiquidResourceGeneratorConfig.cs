using TUNING;
using UnityEngine;

namespace LiquidResourceGenerator
{
    public sealed class LiquidResourceGeneratorConfig : IBuildingConfig
    {
        public const string ID = "LiquidResourceGenerator";

        public override BuildingDef CreateBuildingDef()
        {
            // 复用氧气扩散器的外观，不引入其耗藻、气压或供料逻辑。
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "mineraldeoxidizer_kanim",
                100, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.ALL_METALS, 1600f,
                BuildLocationRule.OnFloor, BUILDINGS.DECOR.PENALTY.TIER1, NOISE_POLLUTION.NONE, 0.2f);
            def.RequiresPowerInput = true;
            def.EnergyConsumptionWhenActive = 120f;
            def.PowerInputOffset = new CellOffset(0, 0);
            def.OutputConduitType = ConduitType.Liquid;
            def.UtilityOutputOffset = new CellOffset(0, 0);
            def.AddLogicPowerPort = false;
            // 自动化口放在上方格，与底部电源口分开显示。
            def.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 1));
            def.Overheatable = false;
            def.Floodable = false;
            def.AudioCategory = "HollowMetal";
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            // 原版 Filterable 保存、读取和复制选择；资源列表由本 Mod 补齐未发现元素。
            go.AddOrGet<Filterable>().filterElementState = Filterable.ElementState.Liquid;
            go.AddOrGet<CopyBuildingSettings>().copyGroupTag = new Tag(ID);
            Storage storage = BuildingTemplates.CreateDefaultStorage(go);
            storage.capacityKg = LiquidResourceGenerator.StorageCapacityKg;
            storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicOperationalController>().unNetworkedValue = 1;
            var dispenser = go.AddOrGet<ConduitDispenser>();
            dispenser.conduitType = ConduitType.Liquid;
            dispenser.alwaysDispense = true;
            go.AddOrGet<LiquidResourceGenerator>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}
