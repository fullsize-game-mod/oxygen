using System;
using KSerialization;
using UnityEngine;

namespace SolidResourceGenerator
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class SolidResourceGenerator : KMonoBehaviour, ISim200ms, ISingleSliderControl
    {
        internal const float DefaultKilogramsPerSecond = 1f;
        internal const float MaxKilogramsPerSecond = 100f;

        [Serialize]
        private float kilogramsPerSecond = DefaultKilogramsPerSecond;

        private Operational operational;
        private Filterable filterable;

        public string SliderTitleKey => "STRINGS.UI.SOLIDRESOURCEGENERATOR.RATE";
        public string SliderUnits => "kg/s";

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            operational = GetComponent<Operational>();
            filterable = GetComponent<Filterable>();
            Subscribe((int)GameHashes.CopySettings, OnCopySettings);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // 旧存档缺少此字段时沿用 1 kg/s，同时校验已保存的速度。
            SetSliderValue(kilogramsPerSecond, 0);
        }

        private void OnCopySettings(object data)
        {
            var source = (data as GameObject)?.GetComponent<SolidResourceGenerator>();
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
            // 读档后原资源可能被其他 Mod 移除；拒绝无效元素或没有掉落物预制体的资源。
            if (!CanGenerate(element))
            {
                operational.SetActive(false);
                return;
            }

            Vector3 position = transform.position + new Vector3(0f, 0.5f, 0f);
            if (!Grid.IsValidCell(Grid.PosToCell(position)))
            {
                operational.SetActive(false);
                return;
            }

            // 冰等低熔点资源必须低于相变温度生成；后续自然换热仍由游戏处理。
            float temperature = Math.Max(1f, Math.Min(293.15f, element.highTemp - 1f));
            // 使用原版生成接口合并同格同类掉落物，避免每次更新都堆出新对象。
            element.substance.SpawnResource(position, kilogramsPerSecond * dt, temperature, byte.MaxValue, 0);
            operational.SetActive(true);
        }

        // 选择列表与生产共用条件，避免未发现资源可选却无法生成。
        internal static bool CanGenerate(Element element)
        {
            return element != null && element.IsSolid && !element.disabled && element.substance != null &&
                element.highTemp > 1f && Assets.TryGetPrefab(element.tag) != null;
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

        public string GetSliderTooltipKey(int index) => "STRINGS.UI.SOLIDRESOURCEGENERATOR.RATE_TOOLTIP";

        public string GetSliderTooltip(int index)
        {
            return $"当前生成速度：{kilogramsPerSecond:0.0} kg/s。设为 0 时暂停生产。";
        }
    }
}
