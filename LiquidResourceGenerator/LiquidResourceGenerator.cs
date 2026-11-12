using System;
using KSerialization;
using UnityEngine;

namespace LiquidResourceGenerator
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class LiquidResourceGenerator : KMonoBehaviour, ISim200ms, ISingleSliderControl
    {
        internal const float DefaultKilogramsPerSecond = 1f;
        internal const float MaxKilogramsPerSecond = 100f;
        internal const float StorageCapacityKg = 20f;

        [Serialize]
        private float kilogramsPerSecond = DefaultKilogramsPerSecond;

        private Operational operational;
        private Filterable filterable;
        private Storage storage;
        private ConduitDispenser dispenser;

        public string SliderTitleKey => "STRINGS.UI.LIQUIDRESOURCEGENERATOR.RATE";
        public string SliderUnits => "kg/s";

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            operational = GetComponent<Operational>();
            filterable = GetComponent<Filterable>();
            storage = GetComponent<Storage>();
            dispenser = GetComponent<ConduitDispenser>();
            Subscribe((int)GameHashes.CopySettings, OnCopySettings);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // 缺省为 1 kg/s，同时校验已保存的速度。
            SetSliderValue(kilogramsPerSecond, 0);
        }

        private void OnCopySettings(object data)
        {
            var source = (data as GameObject)?.GetComponent<LiquidResourceGenerator>();
            if (source != null)
                SetSliderValue(source.kilogramsPerSecond, 0);
        }

        public void Sim200ms(float dt)
        {
            if (!operational.IsOperational || kilogramsPerSecond <= 0f ||
                dt <= 0f || float.IsNaN(dt) || float.IsInfinity(dt))
            {
                operational.SetActive(false);
                return;
            }

            Element element = ElementLoader.GetElement(filterable.SelectedTag);
            // 读档后原资源可能被其他 Mod 移除，或不再是可生成的液体。
            if (!LiquidResourceSupport.IsSupported(filterable.SelectedTag))
            {
                operational.SetActive(false);
                return;
            }

            if (!dispenser.IsConnected || dispenser.blocked)
            {
                operational.SetActive(false);
                return;
            }

            float mass = Math.Min(kilogramsPerSecond * dt, storage.capacityKg - storage.MassStored());
            if (mass <= 0f)
            {
                operational.SetActive(false);
                return;
            }

            float temperature = LiquidResourceSupport.GetOutputTemperature(element);
            // 由原版排放器接入管网；缓冲满时不再生成，不丢弃尚未排出的液体。
            storage.AddLiquid(element.id, mass, temperature, byte.MaxValue, 0);
            operational.SetActive(true);
        }

        public int SliderDecimalPlaces(int index) => 1;
        public float GetSliderMin(int index) => 0f;
        public float GetSliderMax(int index) => MaxKilogramsPerSecond;
        public float GetSliderValue(int index) => kilogramsPerSecond;

        public void SetSliderValue(float value, int index)
        {
            kilogramsPerSecond = float.IsNaN(value) || float.IsInfinity(value)
                ? DefaultKilogramsPerSecond
                : Math.Max(0f, Math.Min(MaxKilogramsPerSecond, value));
        }

        public string GetSliderTooltipKey(int index) => "STRINGS.UI.LIQUIDRESOURCEGENERATOR.RATE_TOOLTIP";

        public string GetSliderTooltip(int index)
        {
            return $"当前生成速度：{kilogramsPerSecond:0.0} kg/s，实际输出受管道容量限制。设为 0 时暂停生产，已生成的液体仍会排出。";
        }
    }
}
