using System;

namespace AdjustableSpaceHeater
{
    internal static class ThermalMath
    {
        // 比热单位为 kDTU/(kg·K)，因此能量结果可直接交给游戏的 kJ 接口。
        internal static float RequiredEnergy(float currentKelvin, float targetKelvin,
            float mass, float specificHeatCapacity)
        {
            return (targetKelvin - currentKelvin) * mass * specificHeatCapacity;
        }

        internal static float EnergyScale(float totalAbsoluteEnergy, float deltaTime)
        {
            if (totalAbsoluteEnergy <= 0f || deltaTime <= 0f)
                return 0f;

            // 冷热格子的需求取绝对值求和，防止相互抵消后突破整机功率上限。
            return Math.Min(1f, TemperatureSettings.ThermalKilowatts * deltaTime / totalAbsoluteEnergy);
        }
    }
}
