using System.Text.Json.Serialization;
using Atomcraft;
using Godot;
using Newtonsoft.Json;
using FileAccess = Godot.FileAccess;

namespace Contracts;

public static class Contracts
{

    [Serializable]
    public class SaveData_MaterialAmount
    {
        public string MaterialTypeName;
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
        public string ContractTypeId;
        public List<SaveData_MaterialAmount> Cost = [];
        public List<SaveData_MaterialAmount> Reward = [];

        public Serializable_ContractType()
        {
        }

        public Serializable_ContractType(ContractType contractType)
        {
            ContractTypeId = contractType.ContractTypeId;
            Cost = contractType.Cost.ConvertAll(m => new SaveData_MaterialAmount(m));
            Reward = contractType.Reward.ConvertAll(m => new SaveData_MaterialAmount(m));
        }
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
        public List<MaterialAmount> Cost;
        public List<MaterialAmount> Reward;
        
        public ContractType(string contractTypeId, List<MaterialAmount> cost, List<MaterialAmount> reward)
        {
            ContractTypeId = contractTypeId;
            Cost = cost;
            Reward = reward;
        }
        
        public ContractType(Serializable_ContractType contractType)
        {
            ContractTypeId = contractType.ContractTypeId;
            Cost = contractType.Cost.ConvertAll(m => new MaterialAmount(m));
            Reward = contractType.Reward.ConvertAll(m => new MaterialAmount(m));
        }

    }
    
    [Serializable]
    public class SaveData_Contracts
    {
        public List<string> ActiveContracts = [];
        public SaveData_ContractsInventory Inventory = new();

        public SaveData_Contracts()
        {
        }

        public SaveData_Contracts(List<string> activeContracts, ContractsInventory inventory)
        {
            ActiveContracts = activeContracts;
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

    public static Dictionary<string, ContractType> ContractTypes;
    public static List<string> ActiveContracts;
    public static ContractsInventory Inventory;
    
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
        if (LoadFile("res://Contracts/Data/Contracts.json", out var content))
        {
            List<Serializable_ContractType> contractTypes = JsonConvert.DeserializeObject<List<Serializable_ContractType>>(content);
            if (contractTypes != null)
            {
                ContractTypes = contractTypes.ToDictionary(kvp => kvp.ContractTypeId, kvp => new ContractType(kvp));
            }
            else
            {
                ContractTypes = new Dictionary<string, ContractType>();
            }
            
        }
    }
    
    
        
    public static int GetActiveContractsCapacityIn(short materialTypeId)
    {
        int amount = 0;
        foreach (string contractTypeId in ActiveContracts)
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
        return amount;
    }
    
    public static int GetActiveContractsCapacityOut(short materialTypeId)
    {
        int amount = 0;
        foreach (string contractTypeId in ActiveContracts)
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
        return amount;
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
        if (contract.Reward.Any(materialOut => 2 * GetActiveContractsCapacityOut(materialOut.MaterialTypeId) < (Inventory.GetAmountOfMaterialOut(materialOut.MaterialTypeId) + materialOut.Amount)))
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

        return true;
    }
}