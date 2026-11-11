using HarmonyLib;
using UnityEngine;

namespace AdjustableSpaceHeater
{
    [HarmonyPatch(typeof(SpaceHeaterConfig), nameof(SpaceHeaterConfig.CreateBuildingDef))]
    internal static class SpaceHeaterDefinitionPatch
    {
        private static void Postfix(BuildingDef __result)
        {
            // 修改基础过热温度，保留建材的原版耐热加成。
            __result.OverheatTemperature = TemperatureSettings.OverheatKelvin;
            // 统一由温控组件换热，避免原版固定产热抵消制冷。
            __result.SelfHeatKilowattsWhenActive = 0f;
            __result.ExhaustKilowattsWhenActive = 0f;
        }
    }

    [HarmonyPatch(typeof(SpaceHeaterConfig), nameof(SpaceHeaterConfig.DoPostConfigureComplete))]
    internal static class SpaceHeaterTemperatureControlPatch
    {
        private static void Postfix(GameObject go)
        {
            // 在未生成实例的预制体配置阶段移除原版单向状态机。
            // 必须立即移除，避免同帧实例化时出现两个组件争用 Operational。
            Object.DestroyImmediate(go.GetComponent<SpaceHeater>());
            go.AddOrGet<HeaterTemperatureControl>();
        }
    }
}
