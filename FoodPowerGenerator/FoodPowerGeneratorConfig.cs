using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace FoodPowerGenerator
{
    public sealed class FoodPowerGeneratorConfig : IBuildingConfig
    {
        public const string ID = "FoodPowerGenerator";
        internal const int Watts = 600;
        internal const int StorageCapacityKg = 20;
        private const int RotStorageCapacityKg = 5;

        public override BuildingDef CreateBuildingDef()
        {
            // 复用煤炭发电机外观，不添加煤炭消耗或废气组件。
            var def = BuildingTemplates.CreateBuildingDef(ID, 3, 3, "generatorphos_kanim",
                100, 30f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.ALL_METALS, 1600f,
                BuildLocationRule.OnFloor, BUILDINGS.DECOR.PENALTY.TIER2, NOISE_POLLUTION.NOISY.TIER5, 0.2f);
            def.RequiresPowerOutput = true;
            def.GeneratorWattageRating = Watts;
            def.GeneratorBaseCapacity = Watts;
            def.PowerOutputOffset = new CellOffset(0, 0);
            def.ViewMode = OverlayModes.Power.ID;
            def.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 1));
            def.SelfHeatKilowattsWhenActive = 1f;
            def.AudioCategory = "HollowMetal";
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            var storage = go.AddOrGet<Storage>();
            storage.capacityKg = StorageCapacityKg;
            storage.showInUI = true;
            storage.showDescriptor = true;
            storage.allowItemRemoval = true;
            storage.storageFilters = new List<Tag> { GameTags.Edible };
            storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
            storage.fetchCategory = Storage.FetchCategory.GeneralStorage;

            // 使用原版储存筛选和搬运，未勾选食物时不派发供料任务。
            go.AddOrGet<TreeFilterable>();
            go.AddOrGet<StorageLocker>();
            go.AddOrGet<CopyBuildingSettings>().copyGroupTag = new Tag(ID);
            Prioritizable.AddRef(go);
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery, false);
            go.AddOrGet<LoopingSounds>();

            // 单独配送腐烂物，避免把整个有机物分类加进食物筛选，或挤占食物库存。
            var rotStorage = go.AddComponent<Storage>();
            rotStorage.capacityKg = RotStorageCapacityKg;
            rotStorage.showInUI = true;
            rotStorage.allowItemRemoval = true;
            go.AddOrGet<FoodPowerGenerator>().rotStorage = rotStorage;

            var rotDelivery = go.AddOrGet<ManualDeliveryKG>();
            rotDelivery.SetStorage(rotStorage);
            rotDelivery.RequestedItemTag = FoodPowerGenerator.RotPileTag;
            rotDelivery.capacity = RotStorageCapacityKg;
            rotDelivery.refillMass = 1f;
            rotDelivery.MinimumMass = 0f;
            rotDelivery.operationalRequirement = Operational.State.Operational;
            rotDelivery.allowPause = true;
            rotDelivery.ShowStatusItem = false;
            rotDelivery.choreTypeIDHash = Db.Get().ChoreTypes.PowerFetch.IdHash;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicOperationalController>().unNetworkedValue = 1;
            go.AddOrGet<FoodPowerGenerator>().powerDistributionOrder = 9;
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}
