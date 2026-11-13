using System;
using System.Collections.Generic;
using System.Linq;
using AdjustableSpaceHeater;
using UnityEngine;

internal static class BuildingRegistrationChecks
{
    internal static void Run()
    {
        new Mod().OnLoad(new HarmonyLib.Harmony());
        var config = new AdjustableSpaceHeaterConfig();
        BuildingDef def = config.CreateBuildingDef();
        RegisterHeaterPatch.Prefix();
        Check(def.PrefabID != "SpaceHeater" && ModUtil.Buildings.Single() == ("Utilities", def.PrefabID),
            "The independent building must be registered in Utilities");
        Check(Strings.Values["STRINGS.BUILDINGS.PREFABS." + def.PrefabID.ToUpperInvariant() + ".NAME"] == "空间温控器",
            "The building must have its own display name");
        Check(def.Width == 2 && def.Height == 2 && def.Anim == "spaceheater_kanim",
            "The building must reuse the Space Heater appearance");
        Check(def.MaterialCategory.Contains("Metal") && def.Mass.SequenceEqual(new[] { 400f }),
            "The building must be constructible with starting metal ores");
        Check(def.RequiresPowerInput && def.EnergyConsumptionWhenActive == 120f,
            "Temperature control must still require 120 W power");
        Check(def.OverheatTemperature == TemperatureSettings.OverheatKelvin &&
            def.SelfHeatKilowattsWhenActive == 0f && def.ExhaustKilowattsWhenActive == 0f,
            "The building must preserve its overheat threshold without fixed heat generation");
        Check(!def.AddLogicPowerPort && def.LogicInputPorts.Single().Equals(new CellOffset(1, 0)),
            "The automation port must be separate from the power port");

        var go = new GameObject();
        config.DoPostConfigureComplete(go);
        Check(go.Components.OfType<HeaterTemperatureControl>().Count() == 1 &&
            go.Components.OfType<PoweredActiveController.Def>().Count() == 1 &&
            go.Components.OfType<LogicOperationalController>().Single().unNetworkedValue == 1,
            "The completed building needs temperature control, animation and unwired automation support");
        var patches = typeof(Mod).Assembly.GetTypes()
            .SelectMany(type => type.GetCustomAttributes(typeof(HarmonyLib.HarmonyPatch), false))
            .Cast<HarmonyLib.HarmonyPatch>().ToArray();
        Check(patches.Length == 1 && patches[0].Target == typeof(GeneratedBuildings),
            "The mod must only register its building, without patching vanilla buildings or research");
        Console.WriteLine("PASS: independent building registration, strings, starting materials, power, automation and no vanilla patches.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}

// 仅替代游戏边界，执行正式建筑配置和注册补丁；不模拟科技树、UI 或存档。
public abstract class IBuildingConfig
{
    public abstract BuildingDef CreateBuildingDef();
    public virtual void DoPostConfigureComplete(GameObject go) { }
}
public class BuildingDef
{
    public string PrefabID, Anim, AudioCategory, ViewMode;
    public int Width, Height;
    public float[] Mass;
    public string[] MaterialCategory;
    public bool RequiresPowerInput, AddLogicPowerPort = true;
    public float EnergyConsumptionWhenActive, OverheatTemperature;
    public float SelfHeatKilowattsWhenActive, ExhaustKilowattsWhenActive;
    public CellOffset PowerInputOffset;
    public List<CellOffset> LogicInputPorts;
}
public record struct CellOffset(int x, int y);
public enum BuildLocationRule { OnFloor }
public struct EffectorValues { }
public static class BuildingTemplates
{
    public static BuildingDef CreateBuildingDef(string id, int width, int height, string anim,
        int hitpoints, float constructionTime, float[] mass, string[] materials, float meltingPoint,
        BuildLocationRule location, EffectorValues decor, EffectorValues noise, float temperature)
    {
        return new BuildingDef { PrefabID = id, Width = width, Height = height, Anim = anim,
            Mass = mass, MaterialCategory = materials };
    }
}
public class LogicOperationalController
{
    public int unNetworkedValue;
    public static List<CellOffset> CreateSingleInputPortList(CellOffset offset) => new List<CellOffset> { offset };
}
public static class PoweredActiveController { public class Def { } }
public static class OverlayModes { public static class Power { public const string ID = "Power"; } }
public static class GeneratedBuildings { public static void LoadGeneratedBuildings() { } }
public static class ModUtil
{
    public static readonly List<(string Category, string ID)> Buildings = new List<(string, string)>();
    public static void AddBuildingToPlanScreen(string category, string id) => Buildings.Add((category, id));
}
public static class Strings
{
    public static readonly Dictionary<string, string> Values = new Dictionary<string, string>();
    public static void Add(string key, string value) => Values.Add(key, value);
}
namespace UnityEngine
{
    public class GameObject { public readonly List<object> Components = new List<object>(); }
    public static class ComponentExtensions
    {
        public static T AddOrGet<T>(this GameObject go) where T : new()
        {
            T component = go.Components.OfType<T>().FirstOrDefault();
            if (component == null) { component = new T(); go.Components.Add(component); }
            return component;
        }
        public static void AddOrGetDef<T>(this GameObject go) where T : new() => go.AddOrGet<T>();
    }
}
namespace TUNING
{
    public static class BUILDINGS
    {
        public static class CONSTRUCTION_MASS_KG { public static readonly float[] TIER4 = { 400f }; }
        public static class DECOR { public static class BONUS { public static EffectorValues TIER1; } }
    }
    public static class MATERIALS { public static readonly string[] ALL_METALS = { "Metal", "RefinedMetal" }; }
    public static class NOISE_POLLUTION { public static class NOISY { public static EffectorValues TIER2; } }
}
namespace AdjustableSpaceHeater { public class HeaterTemperatureControl { } }
namespace HarmonyLib
{
    public class Harmony { }
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HarmonyPatch : Attribute
    {
        public readonly Type Target;
        public HarmonyPatch(Type target, string method) { Target = target; }
    }
}
namespace KMod { public class UserMod2 { public virtual void OnLoad(HarmonyLib.Harmony harmony) { } } }
