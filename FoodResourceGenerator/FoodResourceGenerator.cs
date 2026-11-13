using System;
using KSerialization;
using UnityEngine;

namespace FoodResourceGenerator
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class FoodResourceGenerator : KMonoBehaviour, ISim200ms, ISingleSliderControl
    {
        internal const float DefaultKilogramsPerSecond = 1f;
        internal const float MaxKilogramsPerSecond = 100f;

        [Serialize]
        private float kilogramsPerSecond = DefaultKilogramsPerSecond;

        private Operational operational;
        private Filterable filterable;

        public string SliderTitleKey => "STRINGS.UI.FOODRESOURCEGENERATOR.RATE";
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
            var source = (data as GameObject)?.GetComponent<FoodResourceGenerator>();
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

            GameObject prefab = FoodResourceSupport.GetPrefab(filterable.SelectedTag);
            // 未加载的 DLC 或被其他 Mod 移除的原料不能继续生产。
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

            // 食物必须实例化自身预制体，保留热量、腐败和烹饪属性，不能生成基础元素。
            GameObject food = GameUtil.KInstantiate(prefab, position, Grid.SceneLayer.Ore);
            PrimaryElement primaryElement = food.GetComponent<PrimaryElement>();
            // 按质量计产，Mass 会依照预制体的 MassPerUnit 换算谷粒等原料的单位数。
            primaryElement.Mass = kilogramsPerSecond * dt;
            primaryElement.Temperature = 293.15f;
            food.SetActive(true);
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

        public string GetSliderTooltipKey(int index) => "STRINGS.UI.FOODRESOURCEGENERATOR.RATE_TOOLTIP";

        public string GetSliderTooltip(int index)
        {
            return $"当前生成速度：{kilogramsPerSecond:0.0} kg/s。设为 0 时暂停生产。";
        }
    }
}
