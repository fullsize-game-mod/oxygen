using System;
using KSerialization;

namespace OxygenOnly20C
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class ElectrolyzerGasMode : KMonoBehaviour, ISidescreenButtonControl
    {
        [Serialize]
        private bool produceHydrogen;

        private ElementConverter converter;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            converter = GetComponent<ElementConverter>();
            ApplyOutput();
        }

        public string SidescreenTitle => IsChinese ? "电解器产气模式" : "Electrolyzer output";

        public string SidescreenButtonText => IsChinese
            ? (produceHydrogen ? "切换为纯氧" : "切换为纯氢")
            : (produceHydrogen ? "Switch to oxygen" : "Switch to hydrogen");

        public string SidescreenButtonTooltip => IsChinese
            ? (produceHydrogen ? "当前产物为纯氢。" : "当前产物为纯氧。")
            : (produceHydrogen ? "Currently producing only hydrogen." : "Currently producing only oxygen.");

        public void SetButtonTextOverride(ButtonMenuTextOverride textOverride) { }
        public bool SidescreenEnabled() => true;
        public bool SidescreenButtonInteractable() => converter != null && converter.outputElements?.Length == 1;
        public int HorizontalGroupID() => -1;
        public int ButtonSideScreenSortOrder() => 0;

        public void OnSidescreenButtonPressed()
        {
            if (!SidescreenButtonInteractable())
                return;

            produceHydrogen = !produceHydrogen;
            ApplyOutput();
        }

        private void ApplyOutput()
        {
            if (!SidescreenButtonInteractable())
                return;

            SimHashes gas = produceHydrogen ? SimHashes.Hydrogen : SimHashes.Oxygen;
            if (converter.outputElements[0].elementHash == gas)
                return;

            // 状态栏按元素记录产量，换气体前先移除旧气体的状态。
            if (converter.isSpawned)
                converter.smi.RemoveStatusItems();

            // 保留原产气槽的累计器和其他配置。
            converter.outputElements[0].elementHash = gas;
        }

        private static bool IsChinese =>
            Localization.GetCurrentLanguageCode()?.StartsWith("zh", StringComparison.OrdinalIgnoreCase) == true;
    }
}
