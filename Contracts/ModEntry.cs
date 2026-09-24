using Atomcraft;
using HarmonyLib;
using Godot;
using GodotMonoModLoader;
using GodotMonoModLoader.Atomcraft;
using Newtonsoft.Json.Linq;

namespace Contracts;

public class ModEntry : AtomcraftModEntry, IModInitializationProvider<JTokenDictionaryModConfig>, IUniverseLoadSaveProvider<Contracts.SaveData_Contracts>
{
    
    public void Initialize(InitializationContext<JTokenDictionaryModConfig> context)
    {
        
        HarmonyPatchAll();
        
        GD.Print("[Contracts]: Harmony PatchAll.");
        
        context.ModConfig.SetAndGetWithDefault(ref Contracts.INPUT_MAX_STORAGE);
        context.ModConfig.SetAndGetWithDefault(ref Contracts.OUTPUT_MAX_STORAGE);
        
        GD.Print("[Contracts]: Contracts Initialized.");
    }

    public void OnUniverseLoad(UniverseLoadContext<Contracts.SaveData_Contracts> context)
    {
        Contracts.SaveData_Contracts? modData = context.ModData;
        if (modData != null)
        {
            Contracts.ContractsData = modData.ContractList.ToDictionary(contract => contract.ContractTypeId, contract => contract);
            Contracts.Inventory = new Contracts.ContractsInventory(modData.Inventory);
        }
        else
        {
            Contracts.ContractsData = [];
            Contracts.Inventory = new Contracts.ContractsInventory();
        }

    }

    public Contracts.SaveData_Contracts? OnUniverseSave(UniverseSaveContext context)
    {
        if (Contracts.ContractsData.Count > 0)
        {
            return new Contracts.SaveData_Contracts(Contracts.ContractsData, Contracts.Inventory);
        }

        return null;
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
            }, null, "sacroimper.CRAFTABLE_CONTRACT_INPUT");
            Craftables.GetCategory(CraftableCategoryIndex.Movement).MaterialTypeIds
                .Add("Bits of Contract Input".ToMaterialTypeId());
            Craftables.Add("Bits of Contract Output", new Dictionary<string, int>
            {
                { "Carbon", 1 },
                { "Bronze", 1 }
            }, null, "sacroimper.CRAFTABLE_CONTRACT_OUTPUT");
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
                Materials.TryGetMaterialType("Contract Input (Active)")!.Value));
            Materials.AddBaseMaterial(new ContractOutputActiveMaterial(
                Materials.GetBaseMaterialId("Contract Output (Active)"),
                Materials.TryGetMaterialType("Contract Output (Active)")!.Value));
        }
    }

    [HarmonyPatch(typeof(Simulation))]
    public class SimulationPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Simulation.Init))]
        public static void InitPostfix()
        {
            try
            {
                Contracts.Init();
            }
            catch (Exception e)
            {
                GD.PrintErr("Error loading contract list: ", e);
                throw;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Simulation.Step))]
        public static void StepPostfix()
        {
            Contracts.ActiveContracts().Do(contractTypeId => Contracts.TryApplyContract(contractTypeId));
        }
    }
    
    
    [HarmonyPatch(typeof(MapWindow))]
    public class MapWindowPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MapWindow.Init))]
        public static void InitPostfix(MapWindow __instance)
        {
            Button contractsButton = new Button()
            {
                Text = "Contracts",
                OffsetLeft = 20,
                OffsetTop = 450,
                OffsetRight = 80,
                OffsetBottom = 510,
            };
            
            __instance.AddChild(contractsButton);
            contractsButton.Pressed += Contracts.OpenContractsWindow;
        }
    }

    void IUniverseLoadSaveProvider.OnUniverseSave(UniverseSaveContext context)
    {
        throw new NotImplementedException();
    }
}