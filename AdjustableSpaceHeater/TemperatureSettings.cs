using System;

namespace AdjustableSpaceHeater
{
    internal static class TemperatureSettings
    {
        // 游戏内部使用开尔文，摄氏范围和过热阈值只在这里定义。
        internal const float MinKelvin = 73.15f;
        internal const float MaxKelvin = 473.15f;
        internal const float DefaultKelvin = 343.15f;
        internal const float OverheatKelvin = 1273.15f;
        internal const float ThermalKilowatts = 18f;
        internal const int Radius = 2;

        internal static float ClampKelvin(float kelvin)
        {
            return Math.Max(MinKelvin, Math.Min(MaxKelvin, kelvin));
        }
    }
}
