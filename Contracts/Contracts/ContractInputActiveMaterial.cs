using Atomcraft;
using Godot;

namespace Contracts;

public class ContractInputActiveMaterial : StaticMaterial
{
    public ContractInputActiveMaterial(short materialIndex, MaterialType materialType)
        : base(materialIndex, materialType)
    {
    }

    public override bool OnImpact(int posX, int posY, int impactPosX, int impactPosY, SimField field, int tick)
    {
        short materialTypeId = field.Get(posX, posY);
        if (materialTypeId == -1)
        {
            return false;
        }
        BaseMaterial baseMaterial = Materials.TryGetBaseMaterial(materialTypeId);
        if (baseMaterial == null)
        {
            return false;
        }
        if (Game.DeviceSettings.ExtinguishBurningMaterialsOnPickup && baseMaterial.MaterialType.Fire?.ExtinguishTargetMaterialName != null)
        {
            Materials.TryGetBaseMaterialId(baseMaterial.MaterialType.Fire.ExtinguishTargetMaterialName,
                out materialTypeId);
        }
        //Game.LocalSpaceship.AddMaterial(num, 1);

        if (Contracts.Inventory.GetAmountOfMaterialIn(materialTypeId) >= Contracts.GetActiveContractsCapacityIn(materialTypeId) * 2)
        {
            return false;
        }
        
        Contracts.Inventory.AddMaterialIn(materialTypeId, 1);
        field.Set(posX, posY, -1);
        
        // foreach (var contractTypeId in Contracts.ActiveContracts)
        // {
        //     if (Contracts.ContractTypes.TryGetValue(contractTypeId, out var contract))
        //     {
        //         if (contract.MaterialsIn.Any(material => material.MaterialTypeId == materialTypeId))
        //         {
        //             Contracts.TryApplyContract(contractTypeId);
        //         }
        //     }
        // }
        
        return true;
    }
}