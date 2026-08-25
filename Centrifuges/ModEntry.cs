using Atomcraft;
using Centrifuges.Centrifuges;
using HarmonyLib;
using Godot;

namespace Centrifuges;

public static class ModEntry
{
    public static void Initialize()
    {
        var harmony = new Harmony("sacroimper.Centrifuges");

        GD.Print("[Centrifuges] Harmony PatchAll.");

        harmony.PatchAll();

        GD.Print($"[Centrifuges] Centrifuges Initialized.");
    }
    
    [HarmonyPatch(typeof(Craftables), nameof(Craftables.Init))]
    public class CraftablesPatch
    {
        public static void Postfix()
        {
            Craftables.Add("Bits of Centrifuge", new Dictionary<string, int>
            {
                { "Carbon", 1 },
                { "Bronze", 1 }
            }, null, "sacroimper.CRAFTABLE_CENTRIFUGE");
            Craftables.GetCategory(CraftableCategoryIndex.Movement).MaterialTypeIds
                .Add("Bits of Centrifuge".ToMaterialTypeId());
        }
    }

    [HarmonyPatch(typeof(Materials), "InitializeCustomClasses")]
    public class MaterialsPatch
    {
        public static void Postfix()
        {
            Materials.AddBaseMaterial(new CentrifugeMaterial(
                Materials.GetBaseMaterialId("Centrifuge (Clockwise)"),
                Materials.TryGetMaterialType("Centrifuge (Clockwise)").Value, true));
            Materials.AddBaseMaterial(new CentrifugeMaterial(
                Materials.GetBaseMaterialId("Centrifuge (Counter-clockwise)"),
                Materials.TryGetMaterialType("Centrifuge (Counter-clockwise)").Value, false));
        }
    }

    [HarmonyPatch(typeof(MaterialColorDelegates), nameof(MaterialColorDelegates.Init))]
    public class MaterialColorDelegatesPatch
    {
        public static void Postfix()
        {
            MaterialColorDelegates.Lookup["CentrifugeClockwise"] = (BaseMaterial m) =>  BuildCentrifugeSampler(m, true);
            MaterialColorDelegates.Lookup["CentrifugeCounterClockwise"] = (BaseMaterial m) =>  BuildCentrifugeSampler(m, false);
        }
    }
    

    private static ColorSampler BuildCentrifugeSampler(BaseMaterial baseMaterial, bool cloclwise)
    {
        Color[] states =
        [
            baseMaterial.Color.Lerp(Colors.White, 0.5f),
            baseMaterial.Color.Lerp(Colors.LightGray, 0.5f),
            baseMaterial.Color.Lerp(Colors.Gray, 0.5f),
            baseMaterial.Color.Lerp(Colors.DarkGray, 0.5f)
        ];
        if (cloclwise)
        {
            return (int x, int y, int tick) => states[3 - ((x + (2 * y) - tick) & 3)];
        }
        return (int x, int y, int tick) => states[(x + (2 * y) + tick) & 3]; 
    }
}