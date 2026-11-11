using System;
using AdjustableSpaceHeater;

static void Near(float actual, float expected, string scenario)
{
    if (Math.Abs(actual - expected) > 0.0002f)
        throw new Exception($"{scenario}: expected {expected}, got {actual}");
}

static float Step(float current, float target, float capacity, float dt)
{
    float demand = ThermalMath.RequiredEnergy(current, target, capacity, 1f);
    float energy = demand * ThermalMath.EnergyScale(Math.Abs(demand), dt);
    if (Math.Abs(energy) > 18f * Math.Max(0f, dt) + 0.0001f)
        throw new Exception("Thermal power limit exceeded");
    return current + energy / capacity;
}

float coldTarget = 73.15f;
float hotTarget = 473.15f;
float room = 293.15f;
if (ThermalMath.RequiredEnergy(room, coldTarget, 1f, 1f) >= 0f)
    throw new Exception("A -200°C setpoint must remove heat from a 20°C cell");
if (ThermalMath.RequiredEnergy(room, hotTarget, 1f, 1f) <= 0f)
    throw new Exception("A 200°C setpoint must add heat to a 20°C cell");
Near(Step(room, coldTarget, 1f, 0.2f), room - 3.6f, "Cooling power");
Near(Step(room, hotTarget, 1f, 0.2f), room + 3.6f, "Heating power");
Near(Step(73.16f, coldTarget, 0.01f, 0.2f), coldTarget, "Cooling cannot cross target");
Near(Step(473.14f, hotTarget, 0.01f, 0.2f), hotTarget, "Heating cannot cross target");
Near(Step(room, room, 1f, 0.2f), room, "Stable setpoint");
Near(Step(room, coldTarget, 1f, 0f), room, "Zero elapsed time");
Near(Step(room, coldTarget, 1f, -1f), room, "Negative elapsed time");

// 同一范围内冷热需求不能抵消，否则会绕过整机功率上限。
float a = ThermalMath.RequiredEnergy(300f, 250f, 2f, 1f);
float b = ThermalMath.RequiredEnergy(200f, 250f, 2f, 1f);
float scale = ThermalMath.EnergyScale(Math.Abs(a) + Math.Abs(b), 0.2f);
Near(Math.Abs(a * scale) + Math.Abs(b * scale), 3.6f, "Mixed cells share absolute energy budget");
Near(a * scale, -1.8f, "Mixed hot cell cools");
Near(b * scale, 1.8f, "Mixed cold cell heats");
Near(ThermalMath.RequiredEnergy(300f, 250f, 10f, 4f), -2000f, "Liquid heat capacity");

float current = room;
for (int tick = 0; tick < 100; tick++)
    current = Step(current, coldTarget, 1f, 0.2f);
Near(current, coldTarget, "Cooling converges from 20°C to -200°C");
for (int tick = 0; tick < 150; tick++)
    current = Step(current, hotTarget, 1f, 0.2f);
Near(current, hotTarget, "Reversed target heats from -200°C to 200°C");
Near(TemperatureSettings.ClampKelvin(0f), coldTarget, "Lower range clamp");
Near(TemperatureSettings.ClampKelvin(1273.15f), hotTarget, "Upper range clamp");
Console.WriteLine("PASS: heating/cooling direction, power budget, mixed cells, heat capacity, no overshoot, target reversal and range boundaries.");
