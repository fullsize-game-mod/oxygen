using System;
using KSerialization;
using UnityEngine;

namespace SolidResourceGenerator
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class SolidResourceGenerator : KMonoBehaviour, ISim200ms, ISliderControl, IMultiSliderControl
    {
        internal const float DefaultKilogramsPerSecond = 1f;
        internal const float MaxKilogramsPerSecond = 100f;
        internal const float MaxItemsPerSecond = 100f;
        internal const int MaxItemsPerTick = 100;

        [Serialize]
        private float kilogramsPerSecond = DefaultKilogramsPerSecond;

        [Serialize]
        private float itemsPerSecond = 1f;

        [Serialize]
        private double pendingItems;

        [Serialize]
        private Tag pendingItemTag;

        private Operational operational;
        private Filterable filterable;
        private ISliderControl[] controls;

        public string SidescreenTitleKey => "STRINGS.UI.SOLIDRESOURCEGENERATOR.RATES";
        public bool SidescreenEnabled() => true;
        public ISliderControl[] sliderControls => controls ?? (controls = new ISliderControl[] { this, new ItemRateControl(this) });

        public string SliderTitleKey => "STRINGS.UI.SOLIDRESOURCEGENERATOR.RATE";
        public string SliderUnits => "kg/s";

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            operational = GetComponent<Operational>();
            filterable = GetComponent<Filterable>();
            filterable.onFilterChanged += OnSelectionChanged;
            Subscribe((int)GameHashes.CopySettings, OnCopySettings);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // 旧存档缺少此字段时沿用 1 kg/s，同时校验已保存的速度。
            SetSliderValue(kilogramsPerSecond, 0);
            SetItemRate(itemsPerSecond);
            if (double.IsNaN(pendingItems) || double.IsInfinity(pendingItems) || pendingItems < 0d)
                pendingItems = 0d;
            OnSelectionChanged(filterable.SelectedTag);
        }

        protected override void OnCleanUp()
        {
            filterable.onFilterChanged -= OnSelectionChanged;
            base.OnCleanUp();
        }

        private void OnSelectionChanged(Tag tag)
        {
            if (pendingItemTag == tag)
                return;
            pendingItemTag = tag;
            pendingItems = 0d;
        }

        private void OnCopySettings(object data)
        {
            var source = (data as GameObject)?.GetComponent<SolidResourceGenerator>();
            if (source != null)
            {
                SetSliderValue(source.kilogramsPerSecond, 0);
                SetItemRate(source.itemsPerSecond);
                pendingItems = 0d;
            }
        }

        public void Sim200ms(float dt)
        {
            OnSelectionChanged(filterable.SelectedTag);
            if (!operational.IsOperational ||
                dt <= 0f || float.IsNaN(dt) || float.IsInfinity(dt))
            {
                operational.SetActive(false);
                return;
            }

            bool isSolid = SolidResourceSupport.IsSupported(filterable.SelectedTag);
            GameObject itemPrefab = isSolid ? null : ItemResourceSupport.GetPrefab(filterable.SelectedTag);
            if (isSolid ? kilogramsPerSecond <= 0f : itemPrefab == null || itemsPerSecond <= 0f)
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

            if (isSolid)
            {
                Element element = ElementLoader.GetElement(filterable.SelectedTag);
                float temperature = Math.Max(1f, Math.Min(293.15f, element.highTemp - 1f));
                element.substance.SpawnResource(position, kilogramsPerSecond * dt, temperature, byte.MaxValue, 0);
            }
            else
            {
                pendingItems += (double)itemsPerSecond * dt;
                // 每件单独实例化，装备、蛋等物品不能通过修改一件的质量冒充多件。
                // 限制异常长时间步的单次实例化数量，余量保留到后续更新。
                int count = (int)Math.Min(MaxItemsPerTick, Math.Floor(pendingItems + 0.000001d));
                for (int i = 0; i < count; i++)
                {
                    GameObject item = GameUtil.KInstantiate(itemPrefab, position, Grid.SceneLayer.Ore);
                    PrimaryElement primary = item.GetComponent<PrimaryElement>();
                    primary.Units = 1f;
                    item.SetActive(true);
                }
                pendingItems = Math.Max(0d, pendingItems - count);
            }
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

        public string GetSliderTooltipKey(int index) => "STRINGS.UI.SOLIDRESOURCEGENERATOR.RATE_TOOLTIP";

        public string GetSliderTooltip(int index)
        {
            return $"当前生成速度：{kilogramsPerSecond:0.0} kg/s。设为 0 时暂停生产。";
        }

        private void SetItemRate(float value)
        {
            itemsPerSecond = float.IsNaN(value) || float.IsInfinity(value)
                ? 1f : Math.Max(0f, Math.Min(MaxItemsPerSecond, value));
        }

        private sealed class ItemRateControl : ISliderControl
        {
            private readonly SolidResourceGenerator owner;
            internal ItemRateControl(SolidResourceGenerator owner) { this.owner = owner; }
            public string SliderTitleKey => "STRINGS.UI.SOLIDRESOURCEGENERATOR.ITEM_RATE";
            public string SliderUnits => "件/s";
            public int SliderDecimalPlaces(int index) => 1;
            public float GetSliderMin(int index) => 0f;
            public float GetSliderMax(int index) => MaxItemsPerSecond;
            public float GetSliderValue(int index) => owner.itemsPerSecond;
            public void SetSliderValue(float value, int index) => owner.SetItemRate(value);
            public string GetSliderTooltipKey(int index) => "STRINGS.UI.SOLIDRESOURCEGENERATOR.ITEM_RATE_TOOLTIP";
            public string GetSliderTooltip(int index) => $"当前物品生成速度：{owner.itemsPerSecond:0.0} 件/s。不足一件时累计进度，设为 0 时暂停。";
        }
    }
}
