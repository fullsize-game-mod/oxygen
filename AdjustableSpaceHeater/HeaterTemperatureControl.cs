using System;
using System.Collections.Generic;
using UnityEngine;
using KSerialization;

namespace AdjustableSpaceHeater
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class HeaterTemperatureControl : KMonoBehaviour, ISingleSliderControl, ISim200ms,
        IGameObjectEffectDescriptor
    {
        [Serialize]
        private float targetKelvin = TemperatureSettings.DefaultKelvin;

        private Operational operational;
        private readonly List<int> cells = new List<int>(25);
        private readonly List<CellEnergy> changes = new List<CellEnergy>(25);

        private struct CellEnergy
        {
            internal int Cell;
            internal float Energy;
        }

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.TEMPERATURE";
        public string SliderUnits => GameUtil.GetTemperatureUnitSuffix();

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            operational = GetComponent<Operational>();
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // 保持存档字段不变，让旧版 Mod 保存的温度继续生效。
            targetKelvin = TemperatureSettings.ClampKelvin(targetKelvin);
        }

        public void Sim200ms(float dt)
        {
            if (!operational.IsOperational || dt <= 0f)
            {
                operational.SetActive(false);
                return;
            }

            int origin = Grid.PosToCell(this);
            if (!Grid.IsValidCell(origin))
            {
                operational.SetActive(false);
                return;
            }

            Grid.CellToXY(origin, out int originX, out int originY);
            cells.Clear();
            changes.Clear();
            GameUtil.GetNonSolidCells(origin, TemperatureSettings.Radius, cells);
            float totalAbsoluteEnergy = 0f;

            foreach (int cell in cells)
            {
                if (!Grid.IsValidCell(cell) || !Grid.AreCellsInSameWorld(cell, origin))
                    continue;

                Grid.CellToXY(cell, out int x, out int y);
                if (Math.Abs(x - originX) > TemperatureSettings.Radius ||
                    Math.Abs(y - originY) > TemperatureSettings.Radius)
                    continue;

                Element element = Grid.Element[cell];
                float mass = Grid.Mass[cell];
                // 允许冷凝后的液体继续换热；不修改真空、固体或其他小行星。
                if (Grid.Solid[cell] || (!element.IsGas && !element.IsLiquid) ||
                    mass <= 0f || element.specificHeatCapacity <= 0f)
                    continue;

                float energy = ThermalMath.RequiredEnergy(Grid.Temperature[cell], targetKelvin,
                    mass, element.specificHeatCapacity);
                if (energy == 0f)
                    continue;

                changes.Add(new CellEnergy { Cell = cell, Energy = energy });
                totalAbsoluteEnergy += Math.Abs(energy);
            }

            float scale = ThermalMath.EnergyScale(totalAbsoluteEnergy, dt);
            operational.SetActive(scale > 0f);
            foreach (CellEnergy change in changes)
            {
                // 负能量才是真正制冷；按各格热容量限幅，避免一步越过目标。
                float energy = change.Energy * scale;
                SimMessages.ModifyEnergy(change.Cell, energy, energy > 0f ? targetKelvin : 10000f,
                    SimMessages.EnergySourceID.StructureTemperature);
            }
        }

        public int SliderDecimalPlaces(int index) => 1;

        public float GetSliderMin(int index)
        {
            return GameUtil.GetConvertedTemperature(TemperatureSettings.MinKelvin);
        }

        public float GetSliderMax(int index)
        {
            return GameUtil.GetConvertedTemperature(TemperatureSettings.MaxKelvin);
        }

        public float GetSliderValue(int index)
        {
            return GameUtil.GetConvertedTemperature(targetKelvin);
        }

        public void SetSliderValue(float value, int index)
        {
            targetKelvin = TemperatureSettings.ClampKelvin(GameUtil.GetTemperatureConvertedToKelvin(value));
        }

        public string GetSliderTooltipKey(int index)
        {
            return Mod.TooltipKey;
        }

        public string GetSliderTooltip(int index)
        {
            return Mod.Tooltip + "目标：" +
                GameUtil.GetFormattedTemperature(targetKelvin) + GameUtil.GetTemperatureUnitSuffix();
        }

        public List<Descriptor> GetDescriptors(GameObject go)
        {
            return new List<Descriptor>
            {
                new Descriptor("双向温控：18 kDTU/s", Mod.Tooltip,
                    Descriptor.DescriptorType.Effect)
            };
        }
    }
}
