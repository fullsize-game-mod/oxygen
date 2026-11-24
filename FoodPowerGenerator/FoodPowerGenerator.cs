using Klei;
using System;
using UnityEngine;

namespace FoodPowerGenerator
{
    public sealed class FoodPowerGenerator : Generator
    {
        internal const int JoulesPerKilocalorie = 10;
        internal const int JoulesPerRotKg = 600;
        internal static readonly Tag RotPileTag = new Tag(RotPileConfig.ID);
        private const double JoulesPerCalorie = JoulesPerKilocalorie / 1000d;

        [MyCmpReq]
        private Storage storage = null;

        [SerializeField]
        internal Storage rotStorage;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            UpdatePowerStatus(false);
        }

        public override void EnergySim200ms(float dt)
        {
            base.EnergySim200ms(dt);
            // 只有可用电路才消耗燃料；额定功率不受其他发电机的调校加成影响。
            float joules = operational.IsOperational && CircuitID != ushort.MaxValue && dt > 0f
                ? GenerateFromFuel(Math.Min(Capacity, BaseWattageRating * dt))
                : 0f;
            bool active = joules > 0f;
            if (active)
                GenerateJoules(joules, false);
            else
                ResetJoules();

            if (operational.IsActive != active)
            {
                operational.SetActive(active, false);
                UpdatePowerStatus(active);
            }
        }

        private float GenerateFromFuel(float jouleBudget)
        {
            // 先用腐烂物，剩余功率再消耗仍可食用的食物，两种燃料共用同一功率上限。
            double remainingJoules = ConsumeFuel(rotStorage, jouleBudget);
            remainingJoules = ConsumeFuel(storage, remainingJoules);
            return (float)(jouleBudget - remainingJoules);
        }

        private static double ConsumeFuel(Storage storage, double remainingJoules)
        {
            // 整份消耗会从储存列表移除物品，倒序处理不会跳过下一份。
            for (int i = storage.items.Count - 1; i >= 0 && remainingJoules > 0d; i--)
            {
                GameObject item = storage.items[i];
                if (item == null ||
                    !item.TryGetComponent(out PrimaryElement element))
                    continue;

                float units = element.Units;
                double joulesPerUnit = GetJoulesPerUnit(item, element);
                if (!(units > 0f) || float.IsInfinity(units) ||
                    !(joulesPerUnit > 0d) || double.IsInfinity(joulesPerUnit))
                    continue;

                double targetUnits = Math.Max(0d, units - remainingJoules / joulesPerUnit);
                float remainingUnits = (float)targetUnits;
                // 份数只能存为 float；向上取到可表示的余量，避免扣掉超过本周期预算的食物。
                if (remainingUnits < targetUnits)
                    remainingUnits = BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(remainingUnits) + 1);
                if (remainingUnits == units)
                    continue;

                double consumedUnits = (double)units - remainingUnits;
                remainingJoules -= consumedUnits * joulesPerUnit;
                if (remainingUnits == 0f)
                {
                    storage.ConsumeIgnoringDisease(item);
                }
                else
                {
                    var disease = SimUtil.GetPercentOfDisease(element, (float)(consumedUnits / units));
                    element.Units = remainingUnits;
                    element.ModifyDiseaseCount(-disease.count, "FoodPowerGenerator");
                    storage.Trigger((int)GameHashes.OnStorageChange, item);
                }
            }
            return remainingJoules;
        }

        private static double GetJoulesPerUnit(GameObject item, PrimaryElement element)
        {
            // 腐烂物没有食物热量，只按原版 RotPile 的实际质量结算，不把其它堆肥物当燃料。
            if (item.TryGetComponent(out KPrefabID prefab) && prefab.PrefabTag == RotPileTag)
                return (double)element.MassPerUnit * JoulesPerRotKg;

            // 食物记录的是每份的 cal，而不是每 kg 的 kcal。
            return item.TryGetComponent(out Edible edible)
                ? (edible.FoodInfo?.CaloriesPerUnit ?? 0f) * JoulesPerCalorie
                : 0d;
        }

        private void UpdatePowerStatus(bool active)
        {
            var status = Db.Get().BuildingStatusItems;
            selectable.SetStatusItem(Db.Get().StatusItemCategories.Power,
                active ? status.Wattage : status.GeneratorOffline, this);
        }
    }
}
