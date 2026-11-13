using TUNING;
using UnityEngine;

namespace AdjustableSpaceHeater
{
    public sealed class AdjustableSpaceHeaterConfig : IBuildingConfig
    {
        public const string ID = "AdjustableSpaceHeater";

        public override BuildingDef CreateBuildingDef()
        {
            // 复用原版外观和建造参数，温控与解锁使用独立建筑 ID。
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 2, 2, "spaceheater_kanim",
                30, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 1600f,
                BuildLocationRule.OnFloor, BUILDINGS.DECOR.BONUS.TIER1, NOISE_POLLUTION.NOISY.TIER2, 0.2f);
            def.RequiresPowerInput = true;
            def.EnergyConsumptionWhenActive = 120f;
            def.PowerInputOffset = new CellOffset(0, 0);
            def.AddLogicPowerPort = false;
            def.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(1, 0));
            def.OverheatTemperature = TemperatureSettings.OverheatKelvin;
            // 统一由温控组件换热，避免固定产热抵消制冷。
            def.SelfHeatKilowattsWhenActive = 0f;
            def.ExhaustKilowattsWhenActive = 0f;
            def.ViewMode = OverlayModes.Power.ID;
            def.AudioCategory = "HollowMetal";
            return def;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicOperationalController>().unNetworkedValue = 1;
            go.AddOrGet<HeaterTemperatureControl>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}
