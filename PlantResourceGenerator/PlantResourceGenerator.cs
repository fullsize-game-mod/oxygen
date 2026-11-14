using System;
using KSerialization;
using UnityEngine;

namespace PlantResourceGenerator
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class PlantResourceGenerator : KMonoBehaviour, ISim200ms, ISingleSliderControl
    {
        internal const float DefaultKilogramsPerSecond = 1f;
        internal const float MaxKilogramsPerSecond = 100f;

        [Serialize]
        private float kilogramsPerSecond = DefaultKilogramsPerSecond;

        private Operational operational;
        private Filterable filterable;

        public string SliderTitleKey => "STRINGS.UI.PLANTRESOURCEGENERATOR.RATE";
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
            // 保留有效的存档速度，修复异常值。
            SetSliderValue(kilogramsPerSecond, 0);
        }

        private void OnCopySettings(object data)
        {
            var source = (data as GameObject)?.GetComponent<PlantResourceGenerator>();
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

            GameObject prefab = PlantResourceSupport.GetPrefab(filterable.SelectedTag);
            // 未加载的 DLC 或被其他 Mod 移除的收获物不能继续生产。
            if (prefab == null)
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

            // 实例化收获物本身，保留食用、腐败、种植和工业用途，不能用基础元素代替。
            GameObject harvest = GameUtil.KInstantiate(prefab, position, Grid.SceneLayer.Ore);
            PrimaryElement primaryElement = harvest.GetComponent<PrimaryElement>();
            // 按质量计产，Mass 会依照预制体的 MassPerUnit 换算谷粒等原料的单位数。
            primaryElement.Mass = kilogramsPerSecond * dt;
            Element element = ElementLoader.GetElement(filterable.SelectedTag);
            primaryElement.Temperature = element != null && element.tag == filterable.SelectedTag
                ? Math.Max(1f, Math.Min(293.15f, element.highTemp - 1f)) : 293.15f;
            harvest.SetActive(true);
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

        public string GetSliderTooltipKey(int index) => "STRINGS.UI.PLANTRESOURCEGENERATOR.RATE_TOOLTIP";

        public string GetSliderTooltip(int index)
        {
            return $"当前生成速度：{kilogramsPerSecond:0.0} kg/s。设为 0 时暂停生产。";
        }
    }
}
