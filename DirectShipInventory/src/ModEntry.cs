
using HarmonyLib;
using Godot;

namespace DirectShipInventory;

public static class ModEntry
{
    public static void Initialize()
    {
        var harmony = new Harmony("com.sacroimper.Harmony");

        GD.Print($"[DirectShipInventory] Harmony PatchAll.");
        
        harmony.PatchAll();
        
        GD.Print($"[DirectShipInventory] DirectShipInventory Initialized.");
    }
}