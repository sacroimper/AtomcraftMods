
using HarmonyLib;
using Godot;

namespace DirectShipInventory;

public static class ModEntry
{
    public static void Initialize()
    {
        var harmony = new Harmony("sacroimper.DirectShipInventory");

        GD.Print($"[DirectShipInventory] Harmony PatchAll.");
        
        harmony.PatchAll();
        
        GD.Print($"[DirectShipInventory] DirectShipInventory Initialized.");
    }
}