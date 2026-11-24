using Klei;
using System;
using UnityEngine;

namespace FoodPowerGenerator
{
    public sealed class FoodPowerGenerator : Generator
    {
        internal const int JoulesPerKilocalorie = 10;
        private const double JoulesPerCalorie = JoulesPerKilocalorie / 1000d;

        [MyCmpReq]
        private Storage storage = null;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            UpdatePowerStatus(false);
        }

        public override void EnergySim200ms(float dt)
        {
            base.EnergySim200ms(dt);
            // 只有可用电路才消耗食物；额定功率不受其他发电机的调校加成影响。
            float joules = operational.IsOperational && CircuitID != ushort.MaxValue && dt > 0f
                ? ConsumeFood(Math.Min(Capacity, BaseWattageRating * dt))
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

        private float ConsumeFood(float jouleBudget)
        {
            double remainingJoules = jouleBudget;
            // 整份消耗会从储存列表移除物品，倒序处理不会跳过下一份。
            for (int i = storage.items.Count - 1; i >= 0 && remainingJoules > 0d; i--)
            {
                GameObject item = storage.items[i];
                if (item == null || !item.TryGetComponent(out Edible edible) ||
                    !item.TryGetComponent(out PrimaryElement element))
                    continue;

                float units = element.Units;
                float caloriesPerUnit = edible.FoodInfo?.CaloriesPerUnit ?? 0f;
                if (!(units > 0f) || float.IsInfinity(units) ||
                    !(caloriesPerUnit > 0f) || float.IsInfinity(caloriesPerUnit))
                    continue;

                // 游戏记录的是每份的 cal，而不是每 kg 的 kcal；用实际扣除的份数结算。
                double joulesPerUnit = caloriesPerUnit * JoulesPerCalorie;
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
            return (float)(jouleBudget - remainingJoules);
        }

        private void UpdatePowerStatus(bool active)
        {
            var status = Db.Get().BuildingStatusItems;
            selectable.SetStatusItem(Db.Get().StatusItemCategories.Power,
                active ? status.Wattage : status.GeneratorOffline, this);
        }
    }
}
