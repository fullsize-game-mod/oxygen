using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace OxygenOnly20C
{
    [HarmonyPatch(typeof(ElectrolyzerConfig), nameof(ElectrolyzerConfig.ConfigureBuildingTemplate))]
    internal static class ConfigureElectrolyzerPatch
    {
        internal const float OutputTemperature = 293.15f;

        private static void Postfix(GameObject go, Tag prefab_tag)
        {
            if (prefab_tag != new Tag(ElectrolyzerConfig.ID))
                return;

            var converter = go.GetComponent<ElementConverter>();
            converter.outputElements = new[] { CombineOutputs(converter.outputElements) };
            go.GetComponent<Electrolyzer>().maxMass = float.PositiveInfinity;
            go.AddOrGet<ElectrolyzerGasMode>();
        }

        internal static ElementConverter.OutputElement CombineOutputs(ElementConverter.OutputElement[] outputs)
        {
            float hydrogenRate = 0f;
            ElementConverter.OutputElement oxygen = default;
            bool foundOxygen = false;
            foreach (var output in outputs)
            {
                if (output.elementHash == SimHashes.Hydrogen)
                    hydrogenRate += output.massGenerationRate;
                if (output.elementHash == SimHashes.Oxygen)
                {
                    if (foundOxygen)
                        throw new InvalidOperationException("[OxygenOnly20C] Multiple oxygen outputs on Electrolyzer.");
                    oxygen = output;
                    foundOxygen = true;
                }
            }

            if (!foundOxygen)
                throw new InvalidOperationException("[OxygenOnly20C] Oxygen output is missing on Electrolyzer.");

            oxygen.massGenerationRate += hydrogenRate;
            oxygen.minOutputTemperature = OutputTemperature;
            oxygen.useEntityTemperature = false;
            return oxygen;
        }
    }

    [HarmonyPatch(typeof(Electrolyzer), "OnSpawn")]
    internal static class ElectrolyzerPressurePatch
    {
        private static void Prefix(Electrolyzer __instance)
        {
            if (__instance.GetComponent<ElectrolyzerGasMode>() != null)
                __instance.maxMass = float.PositiveInfinity;
        }
    }

    [HarmonyPatch(typeof(ElementConverter), "ConvertMass")]
    internal static class OutputTemperaturePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            var max = AccessTools.Method(typeof(Mathf), nameof(Mathf.Max), new[] { typeof(float), typeof(float) });
            var minTemperature = AccessTools.Field(typeof(ElementConverter.OutputElement),
                nameof(ElementConverter.OutputElement.minOutputTemperature));

            int temperatureCall = -1;
            for (int i = 2; i < code.Count; i++)
            {
                if (!code[i].Calls(max) || !code[i - 2].LoadsField(minTemperature))
                    continue;
                if (temperatureCall >= 0)
                    throw new InvalidOperationException("[OxygenOnly20C] Ambiguous converter temperature calculation.");

                temperatureCall = i;
            }

            if (temperatureCall < 0)
                throw new InvalidOperationException("[OxygenOnly20C] Converter temperature calculation was not found.");

            // 原版会取进水温度与最低产气温度的较高值。只对本模组控制的电解器固定温度。
            var loadConverter = new CodeInstruction(OpCodes.Ldarg_0);
            var call = code[temperatureCall];
            loadConverter.labels.AddRange(call.labels);
            call.labels.Clear();
            loadConverter.blocks.AddRange(call.blocks);
            call.blocks.Clear();
            call.operand = AccessTools.Method(typeof(OutputTemperaturePatch), nameof(GetOutputTemperature));
            code.Insert(temperatureCall, loadConverter);
            return code;
        }

        private static float GetOutputTemperature(float minimumTemperature, float inputTemperature,
            ElementConverter converter)
        {
            return converter.GetComponent<ElectrolyzerGasMode>() != null
                ? ConfigureElectrolyzerPatch.OutputTemperature
                : Mathf.Max(minimumTemperature, inputTemperature);
        }
    }
}
