using System.Text.Json.Serialization;
using Atomcraft;
using Godot;
using GodotMonoModLoader;
using HarmonyLib;
using Newtonsoft.Json;
using FileAccess = Godot.FileAccess;

namespace Contracts;

public static class Contracts
{

    public static int INPUT_MAX_STORAGE = 5;
    public static int OUTPUT_MAX_STORAGE = 5;
    
    [Serializable]
    public class SaveData_MaterialAmount
    {
        public string MaterialTypeName = null!;
        public int Amount;

        public SaveData_MaterialAmount()
        {
        }
        
        public SaveData_MaterialAmount(MaterialAmount  materialAmount)
        {
            MaterialTypeName = materialAmount.MaterialTypeId.ToMaterialName();
            Amount = materialAmount.Amount;
        }
    }
    
    [Serializable]
    public class Serializable_ContractType
    {
        public string ContractTypeId = null!;
        public string Name = null!;
        public string Chain = null!;
        public int StarsRequired = 0;
        //public string Icon;
        public List<SaveData_MaterialAmount> Cost = [];
        public List<SaveData_MaterialAmount> Reward = [];

    }
    
    public class MaterialAmount
    {
        public short MaterialTypeId;
        public int Amount;
    
        public MaterialAmount(short materialTypeId, int amount)
        {
            MaterialTypeId = materialTypeId;
            Amount = amount;
        }
        public MaterialAmount(SaveData_MaterialAmount  materialAmount)
        {
            MaterialTypeId = materialAmount.MaterialTypeName.ToMaterialTypeId();
            Amount = materialAmount.Amount;
        }
    }

    [Serializable]
    public class ContractType
    {
        public string ContractTypeId;
        public string Name;
        public string Chain;
        public int StarsRequired;
        public Texture2D? Icon;
        public List<MaterialAmount> Cost;
        public List<MaterialAmount> Reward;
        
        public string? PreviousContractTypeId;
        
        public ContractType(Serializable_ContractType contractType)
        {
            ContractTypeId = contractType.ContractTypeId;
            Name = contractType.Name;
            Chain = contractType.Chain;
            StarsRequired = contractType.StarsRequired;
            
            // if (!string.IsNullOrEmpty(contractType.Icon))
            // {
            //     Texture2D texture2D = (Texture2D)ResourceLoader.Load(contractType.Icon);
            //     if (texture2D == null)
            //     {
            //         GD.PrintErr("Failed to load contract texture from path: " + contractType.Icon);
            //     }
            //     else
            //     {
            //         Icon = texture2D;
            //     }
            // }

            Cost = contractType.Cost.ConvertAll(m => new MaterialAmount(m));
            Reward = contractType.Reward.ConvertAll(m => new MaterialAmount(m));
        }

    }

    public class ContractChain(string name)
    {
        public string Name = name;
        public List<ContractType> Contracts = new();
    }


    [Serializable]
    public class SaveData_Contract
    {
        public string ContractTypeId;
        public bool Active;
        public int TimesDone;

        public SaveData_Contract(string contractTypeId)
        {
            ContractTypeId = contractTypeId;
        }

        public int GetStars()
        {
            return TimesDone switch
            {
                >= 50 => 5,
                >= 25 => 4,
                >= 10 => 3,
                >= 5  => 2,
                >= 1  => 1,
                _     => 0
            };
        }
    }

    [Serializable]
    public class SaveData_Contracts : IModSaveData
    {
        public List<SaveData_Contract> ContractList = [];
        public SaveData_ContractsInventory Inventory = new();

        public SaveData_Contracts()
        {
        }

        public SaveData_Contracts(Dictionary<string, SaveData_Contract> contractList, ContractsInventory inventory)
        {
            ContractList = contractList.Values.ToList();
            Inventory = new(inventory);
        }
    }
    
    [Serializable]
    public class SaveData_ContractsInventory
    {
        public List<SaveData_MaterialAmount> MaterialsIn = [];
        public List<SaveData_MaterialAmount> MaterialsOut = [];

        
        public SaveData_ContractsInventory()
        {
        }
        
        public SaveData_ContractsInventory(ContractsInventory inventory)
        {
            MaterialsIn = inventory.MaterialsIn.ConvertAll(m => new SaveData_MaterialAmount(m));
            MaterialsOut = inventory.MaterialsOut.ConvertAll(m => new SaveData_MaterialAmount(m));
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    
    [Serializable]
    public class ContractsInventory
    {
        
        public List<MaterialAmount> MaterialsIn = [];
        public List<MaterialAmount> MaterialsOut = [];
        
        public ContractsInventory() { }
        
        public ContractsInventory(SaveData_ContractsInventory contractsInventory)
        {
            MaterialsIn = contractsInventory.MaterialsIn.ConvertAll(m => new MaterialAmount(m));
            MaterialsOut = contractsInventory.MaterialsOut.ConvertAll(m => new MaterialAmount(m));
        }

        public bool ContainsIn(short materialTypeId) => this.GetAmountOfMaterialIn(materialTypeId) > 0;
        public bool ContainsOut(short materialTypeId) => this.GetAmountOfMaterialOut(materialTypeId) > 0;
        
        public void AddMaterialIn(short materialTypeId, int amount)
        {
            foreach (MaterialAmount material in MaterialsIn)
            {
                if (material.MaterialTypeId == materialTypeId)
                {
                    material.Amount += amount;
                    return;
                }
            }
            
            MaterialsIn.Add(new MaterialAmount(materialTypeId, amount));
        }
        
        public void RemoveMaterialIn(short materialTypeId, int amount)
        {
            for (int i = MaterialsIn.Count - 1; i >= 0; i--)
            {
                MaterialAmount material = MaterialsIn[i];
                if (material.MaterialTypeId == materialTypeId)
                {
                    if (material.Amount <= amount)
                    {
                        MaterialsIn.RemoveAt(i);
                    }
                    else
                    {
                        material.Amount -= amount;
                    }
                    break;
                }
            }
        }
        
        public int GetAmountOfMaterialIn(short materialTypeId)
        {
            int amount = 0;
            foreach (MaterialAmount material in MaterialsIn)
            {
                if (material.MaterialTypeId == materialTypeId)
                {
                    amount += material.Amount;
                }
            }
            return amount;
        }
        
        public void AddMaterialOut(short materialTypeId, int amount)
        {
            foreach (MaterialAmount material in MaterialsOut)
            {
                if (material.MaterialTypeId == materialTypeId)
                {
                    material.Amount += amount;
                    return;
                }
            }
            MaterialsOut.Add(new MaterialAmount(materialTypeId, amount));
        }
        
        public void RemoveMaterialOut(short materialTypeId, int amount)
        {
            for (int i = MaterialsOut.Count - 1; i >= 0; i--)
            {
                MaterialAmount material = MaterialsOut[i];
                if (material.MaterialTypeId == materialTypeId)
                {
                    if (material.Amount <= amount)
                    {
                        MaterialsOut.RemoveAt(i);
                    }
                    else
                    {
                        material.Amount -= amount;
                    }
                    break;
                }
            }
        }
        
        public int GetAmountOfMaterialOut(short materialTypeId)
        {
            int amount = 0;
            foreach (MaterialAmount material in MaterialsOut)
            {
                if (material.MaterialTypeId == materialTypeId)
                {
                    amount += material.Amount;
                }
            }
            return amount;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public static Dictionary<string, ContractType> ContractTypes { get; set; } = [];
    public static Dictionary<string, ContractChain> Chains { get; set; } = [];
    public static Dictionary<string, SaveData_Contract> ContractsData { get; set; } = [];
    public static ContractsInventory Inventory { get; set; } = new();
    public static ContractsWindow Window { get; set; } = null!;

    public static SaveData_Contract? GetData(string contractTypeId)
    {
        return ContractsData.GetValueOrDefault(contractTypeId);
    }
    
    public static int GetCurrentStars()
    {
        return ContractsData.Values.Sum(c => c.GetStars());
    }
    
    public static void ToggleContractActive(string contractTypeId)
    {
        if (ContractsData.TryGetValue(contractTypeId, out SaveData_Contract? contract))
        {
            contract.Active = !contract.Active;
        }
        else
        {
            ContractsData.Add(contractTypeId, new SaveData_Contract(contractTypeId)
            {
                Active = true
            });
        }
    }

    public static IEnumerable<string> ActiveContracts()
    {
        return ContractsData.Values.Where((c) => c.Active).Select((c) => c.ContractTypeId);
    }

    public static bool IsActive(string contractTypeId)
    {
        return GetData(contractTypeId)?.Active ?? false;
    }

    public static bool IsUnlocked(string contractTypeId)
    {
        ContractType? contractType = ContractTypes.GetValueOrDefault(contractTypeId);
        string? previousContractTypeId = contractType?.PreviousContractTypeId;
        return (previousContractTypeId == null || GetData(previousContractTypeId)?.TimesDone > 0) && GetCurrentStars() >= contractType?.StarsRequired;
    }
    
    public static bool LoadFile(string filePath, out string content)
    {
        Godot.FileAccess fileAccess = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
        if (fileAccess == null)
        {
            GD.PrintErr("Failed to open file: " + filePath);
            content = string.Empty;
            return false;
        }

        Error error = fileAccess.GetError();
        switch (error)
        {
            case Error.Ok:
                content = fileAccess.GetAsText();
                fileAccess.Close();
                return true;
            case Error.AlreadyInUse:
                GD.PrintErr("Access denied to file: " + filePath);
                break;
            default:
                GD.PrintErr("Error loading file: " + error);
                break;
            case Error.FileNotFound:
                break;
        }

        content = string.Empty;
        return false;
    }
    
    public static void Init()
    {
        GD.Print("[Contracts] Loading contracts...");
        Chains = new Dictionary<string, ContractChain>();
        ContractTypes = new Dictionary<string, ContractType>();
        
        if (LoadContracts())
        {
            GD.Print("[Contracts] ", ContractTypes.Count, " contracts loaded.");
            try
            {
                Window = GD.Load<PackedScene>("res://Contracts/Resources/UI/ContractsWindow.tscn").Instantiate<ContractsWindow>();
                Window.Init();
                Gameplay.Windows.Add(Window);
                Gameplay.Instance.AddChild(Window);
            }
            catch (Exception e)
            {
                GD.PrintErr("Error initializing contract window: ", e);
                throw;
            }

        }
    }

    public static bool LoadContracts()
    {
        try
        {
            if (LoadContractsFile("res://Contracts/Resources/Contracts.json"))
            {
                return true;
            }
        }
        catch (Exception e)
        {
            GD.PrintErr("[Contracts] Error loading contracts: ", e);
        }

        return false;
    }

    public static bool LoadContractsFile(string file)
    {
        if (LoadFile(file, out var content))
        {
            List<Serializable_ContractType>? contractTypes = JsonConvert.DeserializeObject<List<Serializable_ContractType>>(content);
            if (contractTypes != null)
            {

                foreach (Serializable_ContractType sContractType in contractTypes)
                {
                    ContractType contractType = new ContractType(sContractType);

                    if (ContractTypes.TryAdd(contractType.ContractTypeId, contractType))
                    {
                        if (!Chains.TryGetValue(contractType.Chain, out ContractChain? chain))
                        {
                            chain = new ContractChain(contractType.Chain);
                            Chains.Add(contractType.Chain, chain);
                        }
                        else
                        {
                            contractType.PreviousContractTypeId = chain.Contracts.Last().ContractTypeId;
                        }

                        chain.Contracts.Add(contractType);
                    }
                }

                return true;
            }
        }

        return false;
    }
    
    public static int GetActiveContractsCapacityIn(short materialTypeId)
    {
        int amount = 0;
        foreach (string contractTypeId in ActiveContracts())
        {
            if (ContractTypes.TryGetValue(contractTypeId, out var contract))
            {
                foreach (MaterialAmount material in contract.Cost)
                {
                    if (material.MaterialTypeId == materialTypeId)
                    {
                        amount += material.Amount;
                    }
                }
            }
        }
        return amount * INPUT_MAX_STORAGE;
    }

    public static int GetActiveContractsCapacityOut(short materialTypeId)
    {
        int amount = 0;
        foreach (string contractTypeId in ActiveContracts())
        {
            if (ContractTypes.TryGetValue(contractTypeId, out var contract))
            {
                foreach (MaterialAmount material in contract.Reward)
                {
                    if (material.MaterialTypeId == materialTypeId)
                    {
                        amount += material.Amount;
                    }
                }
            }
        }
        return amount * OUTPUT_MAX_STORAGE;
    }
    
    public static bool TryApplyContract(string contractTypeId)
    {
        if (!ContractTypes.TryGetValue(contractTypeId, out var contract))
        {
            // GD.Print("Contract doesn't exist: " + contractTypeId);
            return false;
        }

        if (contract.Cost.Any(materialIn => materialIn.Amount > Inventory.GetAmountOfMaterialIn(materialIn.MaterialTypeId)))
        {
            // GD.Print("Not enough material In: " + contractTypeId);
            return false;
        }
        if (contract.Reward.Any(materialOut => GetActiveContractsCapacityOut(materialOut.MaterialTypeId) < (Inventory.GetAmountOfMaterialOut(materialOut.MaterialTypeId) + materialOut.Amount)))
        {
            // GD.Print("Not enough capacity Out: " + contractTypeId);
            return false;
        }
        
        foreach (MaterialAmount materialIn in contract.Cost)
        {
            // GD.Print("RemoveMaterialIn: " + materialIn.MaterialTypeId.ToMaterialName() + materialIn.Amount);
            Inventory.RemoveMaterialIn(materialIn.MaterialTypeId, materialIn.Amount);
        }
        foreach (MaterialAmount materialOut in contract.Reward)
        {
            // GD.Print("RemoveMaterialIn: " + materialOut.MaterialTypeId.ToMaterialName() + materialOut.Amount);
            Inventory.AddMaterialOut(materialOut.MaterialTypeId, materialOut.Amount);
        }

        ContractsData[contractTypeId].TimesDone += 1;

        if (Window.Visible)
        {
            Window.Refresh();
        }

        return true;
    }

    public static void OpenContractsWindow()
    {
        Gameplay.SetCurrentWindowId((WindowId) 13);
    }
}