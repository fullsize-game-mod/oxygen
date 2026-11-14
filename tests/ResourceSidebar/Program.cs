using System;
using System.Collections.Generic;
using UnityEngine;

static Dictionary<Tag, HashSet<Tag>> Options(object generator)
{
    var filter = new Filterable { Component = generator };
    var options = new Dictionary<Tag, HashSet<Tag>> { [GameTags.Void] = new HashSet<Tag> { GameTags.Void } };
    SolidResourceGenerator.GeneratorSolidOptionsPatch.Postfix(filter, ref options);
    LiquidResourceGenerator.GeneratorLiquidOptionsPatch.Postfix(filter, ref options);
    FoodResourceGenerator.GeneratorFoodOptionsPatch.Postfix(filter, ref options);
    PlantResourceGenerator.GeneratorPlantOptionsPatch.Postfix(filter, ref options);
    return options;
}

// Model the installed game's category lists and per-tag recycle pool. SetData
// retains inactive categories; refreshing a category recycles all its old rows.
// A tag in two cached categories can therefore produce the reported duplicate key.
var vanilla = Options(null);
var solid = Options(new SolidResourceGenerator.SolidResourceGenerator());
var liquid = Options(new LiquidResourceGenerator.LiquidResourceGenerator());
var food = Options(new FoodResourceGenerator.FoodResourceGenerator());
var plant = Options(new PlantResourceGenerator.PlantResourceGenerator());
var menus = new[] { vanilla, solid, liquid, food, plant };
foreach (var first in menus)
foreach (var second in menus)
{
    var screen = new SidebarPool();
    for (int refresh = 0; refresh < 30; refresh++)
    {
        screen.SetData(first);
        screen.SetData(second);
    }
}
var sharedScreen = new SidebarPool();
for (int refresh = 0; refresh < 100; refresh++)
    foreach (var menu in menus)
        sharedScreen.SetData(menu);

foreach (var menu in menus)
{
    if (!menu.TryGetValue(GameTags.Void, out var none) || !none.SetEquals(new[] { GameTags.Void }))
        throw new Exception("The none row must use the vanilla reserved Void category.");
    foreach (var entry in menu)
        if (entry.Key != GameTags.Void && entry.Value.Contains(GameTags.Void))
            throw new Exception("The none row cannot also appear in a resource category.");
}
Console.WriteLine("PASS: repeated refresh and every switch between vanilla, solid, liquid, food and plant menus; overlapping solid/food harvests; no duplicate recycle keys; canonical none category.");

sealed class SidebarPool
{
    readonly Dictionary<Tag, List<Tag>> categories = new Dictionary<Tag, List<Tag>>();
    readonly Dictionary<Tag, object> pooledRows = new Dictionary<Tag, object>();
    public void SetData(Dictionary<Tag, HashSet<Tag>> data)
    {
        foreach (var entry in data)
        {
            if (categories.TryGetValue(entry.Key, out var oldRows))
                foreach (var tag in oldRows)
                    pooledRows.Add(tag, new object()); // Same strict Add as RecycleItemRow.
            var rows = new List<Tag>();
            categories[entry.Key] = rows;
            foreach (var tag in entry.Value)
            {
                pooledRows.Remove(tag); // GetOrCreateItemRow consumes a pooled row.
                rows.Add(tag);
            }
        }
    }
}

public readonly record struct Tag(string Name) { public bool IsValid => !string.IsNullOrEmpty(Name); }
public static class GameTags
{
    public static readonly Tag Void = new Tag("Void"), Other = new Tag("Other"), CookingIngredient = new Tag("CookingIngredient");
}
public class Filterable
{
    public object Component;
    public T GetComponent<T>() => Component is T value ? value : default;
    public Dictionary<Tag, HashSet<Tag>> GetTagOptions() => throw new NotImplementedException();
}
public class Element { public Tag tag, materialCategory; }
public static class ElementLoader
{
    public static readonly List<Element> elements = new List<Element>
    {
        new Element { tag = new Tag("Iron"), materialCategory = new Tag("Metal") },
        new Element { tag = new Tag("Water"), materialCategory = new Tag("Liquid") },
        new Element { tag = new Tag("ModSolid") }
    };
}
public static class GeneratedBuildings { public static void LoadGeneratedBuildings() { } }
public static class ModUtil { public static void AddBuildingToPlanScreen(string category, string id) { } }
public class FilterSideScreen { public bool isLogicFilter; public bool IsValidForTarget(GameObject target) => false; }
namespace UnityEngine { public class GameObject { public T GetComponent<T>() => default; } }
namespace SolidResourceGenerator
{
    public class SolidResourceGenerator { }
    public static class SolidResourceGeneratorConfig { public const string ID = "SolidResourceGenerator"; }
    public static class SolidResourceSupport { public static bool IsSupported(Tag tag) => tag.Name == "Iron" || tag.Name == "ModSolid"; }
}
namespace LiquidResourceGenerator
{
    public class LiquidResourceGenerator { }
    public static class LiquidResourceGeneratorConfig { public const string ID = "LiquidResourceGenerator"; }
    public static class LiquidResourceSupport { public static bool IsSupported(Tag tag) => tag.Name == "Water"; }
}
namespace FoodResourceGenerator
{
    public class FoodResourceGenerator { }
    public static class FoodResourceGeneratorConfig { public const string ID = "FoodResourceGenerator"; }
    public static class FoodResourceSupport
    {
        public static HashSet<Tag> GetIngredients() => new HashSet<Tag> { new Tag("Meat") };
        public static GameObject GetPrefab(Tag tag) => new GameObject();
    }
}
namespace PlantResourceGenerator
{
    public class PlantResourceGenerator { }
    public static class PlantResourceGeneratorConfig { public const string ID = "PlantResourceGenerator"; }
    public static class PlantResourceSupport
    {
        public static HashSet<Tag> GetHarvests() => new HashSet<Tag> { new Tag("Iron"), new Tag("Meat"), new Tag("Flower") };
        public static Tag GetCategory(Tag tag) => tag.Name == "Iron" ? new Tag("Metal") :
            tag.Name == "Meat" ? GameTags.CookingIngredient : GameTags.Other;
    }
}
namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HarmonyPatch : Attribute { public HarmonyPatch(Type type, string method) { } }
}
