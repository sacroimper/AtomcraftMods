using Atomcraft;
using Godot;

namespace Contracts;

public class ContractOutputActiveMaterial : StaticMaterial
{
    public ContractOutputActiveMaterial(short materialIndex, MaterialType materialType)
        : base(materialIndex, materialType)
    {
    }

    private bool TryOutputMaterial(short materialTypeId, MaterialState materialState, int posX, int posY, SimField field, int tick)
    {
        Span<Vector2I> output = stackalloc Vector2I[8];
        Utils.GetAdjacent(posX, posY, output);
        if (materialState == MaterialState.Gas)
        {
            for (int i = 0; i < output.Length; i++)
            {
                Vector2I pos = output[i];
                short num = field.Get(pos);
                if (num != -2 && num == -1)
                {
                    field.Set(pos, materialTypeId);
                    Contracts.Inventory.RemoveMaterialOut(materialTypeId, 1);
                    return true;
                }
            }
        }
        else
        {
            for (int num2 = output.Length - 1; num2 >= 0; num2--)
            {
                Vector2I pos2 = output[num2];
                short num3 = field.Get(pos2);
                if (num3 != -2 && num3 == -1)
                {
                    field.Set(pos2, materialTypeId);
                    Contracts.Inventory.RemoveMaterialOut(materialTypeId, 1);
                    return true;
                }
            }
        }
        return false;
    }

    public override bool Step(int posX, int posY, SimField field, int tick)
    {
        
        short materialTypeId1 = field.Get(posX - 1, posY);
        short materialTypeId2 = field.Get(posX + 1, posY);
        
        if (materialTypeId1 == -1 && materialTypeId2 == -1)
        {
            return false;
        }
        
        BaseMaterial material1 = materialTypeId1.ToMaterial();
        BaseMaterial material2 = materialTypeId2.ToMaterial();
        bool flag1 = material1 != null && material1.State != MaterialState.Static && Contracts.Inventory.ContainsOut(materialTypeId1);
        bool flag2 = material2 != null && material2.State != MaterialState.Static && Contracts.Inventory.ContainsOut(materialTypeId2);
        
        if (!flag1 && !flag2)
        {
            return false;
        }

        short materialTypeId = -1;
        MaterialState state;
        if (flag1 && flag2)
        {
            materialTypeId = tick % 2 == 0 ? materialTypeId1 : materialTypeId2;
            state = tick % 2 == 0 ? material1.State : material2.State;
        } else if (flag1) {
            materialTypeId = materialTypeId1;
            state = material1.State;
        } else {
            materialTypeId = materialTypeId2;
            state = material2.State;
        }

        if (TryOutputMaterial(materialTypeId, state, posX, posY, field, tick))
        {
            // foreach (string contractTypeId in Contracts.ActiveContracts)
            // {
            //     if (Contracts.ContractTypes.TryGetValue(contractTypeId, out var contract))
            //     {
            //         foreach (MaterialAmount material in contract.MaterialsOut)
            //         {
            //             if (material.MaterialTypeId == materialTypeId)
            //             {
            //                 Contracts.TryApplyContract(contractTypeId);
            //             }
            //         }
            //     }
            // }
            return true;
        }

        return false;
    }
}