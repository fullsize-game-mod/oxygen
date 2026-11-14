using TUNING;
using UnityEngine;

namespace PlantResourceGenerator
{
    public sealed class PlantResourceGeneratorConfig : IBuildingConfig
    {
        public const string ID = "PlantResourceGenerator";

        public override BuildingDef CreateBuildingDef()
        {
            // 复用氧气扩散器的外观，不引入其耗藻、气压或供料逻辑。
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "mineraldeoxidizer_kanim",
                100, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.ALL_METALS, 1600f,
                BuildLocationRule.OnFloor, BUILDINGS.DECOR.PENALTY.TIER1, NOISE_POLLUTION.NONE, 0.2f);
            def.RequiresPowerInput = true;
            def.EnergyConsumptionWhenActive = 120f;
            def.PowerInputOffset = new CellOffset(0, 0);
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
            // 沿用原版的资源选择存档与复制设置，列表从作物收获配置读取。
            go.AddOrGet<Filterable>().filterElementState = Filterable.ElementState.None;
            go.AddOrGet<CopyBuildingSettings>().copyGroupTag = new Tag(ID);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicOperationalController>().unNetworkedValue = 1;
            go.AddOrGet<PlantResourceGenerator>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}
