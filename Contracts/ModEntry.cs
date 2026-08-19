using Atomcraft;
using HarmonyLib;
using Godot;

namespace Contracts;

public static class ModEntry
{
    public static void Initialize()
    {
        var harmony = new Harmony("sacroimper.Contracts");

        GD.Print("[Contracts] Harmony PatchAll.");

        harmony.PatchAll();

        GD.Print($"[Contracts] Contracts Initialized.");
    }

    public static void OnWorldLoad(Contracts.SaveData_Contracts? modData)
    {
        if (modData != null)
        {
            Contracts.ActiveContracts = modData.ActiveContracts;
            Contracts.Inventory = new Contracts.ContractsInventory(modData.Inventory);
            GD.Print("modData: ", modData.Inventory.MaterialsIn[0].MaterialTypeName);
            GD.Print("modData: ", modData.Inventory.ToString());
        }
        else
        {
            Contracts.ActiveContracts = [];
            Contracts.Inventory = new Contracts.ContractsInventory();

            Contracts.ActiveContracts.AddRange(Contracts.ContractTypes.Keys);
        }

        GD.Print(Contracts.Inventory.ToString());
    }

    public static Contracts.SaveData_Contracts OnWorldSave()
    {
        return new Contracts.SaveData_Contracts(Contracts.ActiveContracts, Contracts.Inventory);
    }

    [HarmonyPatch(typeof(Craftables), nameof(Craftables.Init))]
    public class CraftablesPatch
    {
        public static void Postfix()
        {
            Craftables.Add("Bits of Contract Input", new Dictionary<string, int>
            {
                { "Carbon", 1 },
                { "Bronze", 1 }
            }, "sacroimper.CRAFTABLE_CONTRACT_INPUT");
            Craftables.GetCategory(CraftableCategoryIndex.Movement).MaterialTypeIds
                .Add("Bits of Contract Input".ToMaterialTypeId());
            Craftables.Add("Bits of Contract Output", new Dictionary<string, int>
            {
                { "Carbon", 1 },
                { "Bronze", 1 }
            }, "sacroimper.CRAFTABLE_CONTRACT_OUTPUT");
            Craftables.GetCategory(CraftableCategoryIndex.Movement).MaterialTypeIds
                .Add("Bits of Contract Output".ToMaterialTypeId());
        }
    }

    [HarmonyPatch(typeof(Materials), "InitializeCustomClasses")]
    public class MaterialsPatch
    {
        public static void Postfix()
        {
            Materials.AddBaseMaterial(new ContractInputActiveMaterial(
                Materials.GetBaseMaterialId("Contract Input (Active)"),
                Materials.TryGetMaterialType("Contract Input (Active)").Value));
            Materials.AddBaseMaterial(new ContractOutputActiveMaterial(
                Materials.GetBaseMaterialId("Contract Output (Active)"),
                Materials.TryGetMaterialType("Contract Output (Active)").Value));
        }
    }

    [HarmonyPatch(typeof(Simulation))]
    public class SimulationPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Simulation.Init))]
        public static void InitPostfix()
        {
            Contracts.Init();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Simulation.Step))]
        public static void StepPostfix()
        {
            Contracts.ActiveContracts?.ForEach(contractTypeId => Contracts.TryApplyContract(contractTypeId));
        }
    }
}