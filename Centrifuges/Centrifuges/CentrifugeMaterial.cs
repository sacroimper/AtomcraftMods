using Atomcraft;
using Godot;

namespace Centrifuges.Centrifuges;

public class CentrifugeMaterial : StaticMaterial
{
    public bool Clockwise;
    private static int NONE = 8;
    
    // Indexes based on Utils.AdjacentOffsets
    // 7 0 1   ↖ ↑ ↗
    // 6 X 2   ← X →
    // 5 4 3   ↙ ↓ ↘
    
    public static EightWayDirection[][] CentrifugeDirectionsCloclwise = [
        [EightWayDirection.UpRight, EightWayDirection.Up],
        [EightWayDirection.DownRight, EightWayDirection.Right],
        [EightWayDirection.DownRight, EightWayDirection.Right],
        [EightWayDirection.DownLeft, EightWayDirection.Down],
        [EightWayDirection.DownLeft, EightWayDirection.Down],
        [EightWayDirection.UpLeft, EightWayDirection.Left],
        [EightWayDirection.UpLeft, EightWayDirection.Left],
        [EightWayDirection.UpRight, EightWayDirection.Up],
    ];
    
    
    public static EightWayDirection[][] CentrifugeDirectionsCounterCloclwise = [
        [EightWayDirection.UpLeft, EightWayDirection.Up],
        [EightWayDirection.UpLeft, EightWayDirection.Up],
        [EightWayDirection.UpRight, EightWayDirection.Right],
        [EightWayDirection.UpRight, EightWayDirection.Right],
        [EightWayDirection.DownRight, EightWayDirection.Down],
        [EightWayDirection.DownRight, EightWayDirection.Down],
        [EightWayDirection.DownLeft, EightWayDirection.Left],
        [EightWayDirection.DownLeft, EightWayDirection.Left],
    ];
    
    public CentrifugeMaterial(short materialIndex, MaterialType materialType, bool clockwise)
        : base(materialIndex, materialType)
    {
        Clockwise = clockwise;
    }

    private static bool CanTraverse(short mat, short matThrough)
    {
        if (IsStatic(mat) || mat == -1 || mat == -2 
            || matThrough == -1 || matThrough == -2)
        {
            return false;
        }
        
        if (BaseMaterial.IsMatchFilterOffset(matThrough) && mat == BaseMaterial.BaseId(matThrough))
        {
            return true;
        }

        if (BaseMaterial.IsNonMatchFilterOffset(matThrough) && mat != BaseMaterial.BaseId(matThrough))
        {
            return true;
        }

        if (matThrough == Materials.TRAPDOOR_OPEN
            || matThrough.ToMaterial().WireIndex != null)
        {
            return true;
        }

        bool isSolid = Materials.IsSolid(mat);
        bool isLiquid = Materials.IsLiquid(mat);
        bool isGas = Materials.IsGas(mat);
        
        if (isSolid && (matThrough == Materials.ALLOW_SOLIDS || matThrough == Materials.BLOCK_LIQUIDS || matThrough == Materials.BLOCK_GASES)
            || isLiquid && (matThrough == Materials.ALLOW_LIQUIDS || matThrough == Materials.BLOCK_SOLIDS || matThrough == Materials.BLOCK_GASES)
            || isGas && (matThrough == Materials.ALLOW_GASES || matThrough == Materials.BLOCK_SOLIDS || matThrough == Materials.BLOCK_LIQUIDS))
        {
            return true;
        }

        return false;
    }

    public static bool IsStatic(short mat)
    {
        return Materials.IsStatic(mat) || BaseMaterial.IsMatchFilterOffset(mat) ||
               BaseMaterial.IsNonMatchFilterOffset(mat) || BaseMaterial.IsSensorOffset(mat);
    }

    public override bool Step(int posX, int posY, SimField field, int tick)
    {
        int localTick = Clockwise ? tick + 1 : tick;
        
        if (localTick % 2 != 0)
        {
            return false;
        }
        
        Span<Vector2I> offsets = Utils.AdjacentOffsets;
        Span<int> idxs = stackalloc int[8];
        Span<short> mats = stackalloc short[8];
        bool somethingMoved = false;
        EightWayDirection[][] centrifugeDirections = Clockwise ? CentrifugeDirectionsCloclwise : CentrifugeDirectionsCounterCloclwise;
        for (int i = 0; i < offsets.Length; i++)
        {
            int i1 = (i + (localTick / 2)) & 7;
            int curX = posX + offsets[i1].X;
            int curY = posY + offsets[i1].Y;
            if (centrifugeDirections[i1].Select(direction => direction.GetAdjacentInDirection(curX, curY))
                .Any(target => ConveyorMaterial.TryConvey(curX, curY, target.X, target.Y, field, tick)))
            {
                // GD.Print(" ---- TryConvey succeed ----");
                // GD.Print(" i1: ", i1, " curX: ", curX, " curY: ", curY);
                somethingMoved = true;
            }
            
            idxs[i1] = field.Index(curX, curY);
            mats[i1] = field.Get(idxs[i1]);
        }
        

        int someStatic = NONE;
        bool allAir = true;
        for (int i = 0; i < offsets.Length; i++)
        {
            int i1 = Clockwise ? 7 - i : i;
            int i2 = (Clockwise ? i1 - 1 : i1 + 1) & 7;
            if (IsStatic(mats[i1]) && !CanTraverse(mats[i2],mats[i1]))
            {
                someStatic = i;
            } 
            else if (mats[i] != -1)
            {
                allAir = false;
            } 
        }

        if (allAir)
        {
            return somethingMoved;
        }
        if (someStatic != NONE || localTick % 4 == 0) {
            // Full swap all 8 pixels around if possible
            // Start next to a static pixel
            int start = someStatic + (Clockwise ? -1 : 1);
            // End just before a full cycle
            int end = start + 6;
            bool needAir = someStatic != 8;
            // if (posX == 2981 && posY == 2442)
            // {
            //     GD.Print("--------------------------------------------");
            // }

            for (int i = start; i <= end; i++)
            {

                int i1 = (Clockwise ? 7 - i : i) & 7;
                int i2 = (Clockwise ? i1 - 1 : i1 + 1) & 7;

                // if (posX == 2981 && posY == 2442)
                // {
                //     GD.Print("i: ", i, " needAir: ", needAir);
                //     GD.Print("i1: ", i1, " mat1: ", mats[i1], " static1: ", Materials.IsStatic(mats[i1]));
                //     GD.Print("i2: ", i2, " mat2: ", mats[i2], " static2: ", Materials.IsStatic(mats[i2]));
                // }
                // GD.Print("i1: ", i1, " mat1: ", mats[i1], " static1: ", Materials.IsStatic(mats[i1]), " normal1: ", BaseMaterial.IsNormal(mats[i1]));
                // GD.Print(" isMatch: ", BaseMaterial.IsMatchFilterOffset(mats[i1]), " isNonMatch: ", BaseMaterial.IsNonMatchFilterOffset(mats[i1]), " baseId: ", BaseMaterial.BaseId(mats[i1]));
                // GD.Print("i2: ", i2, " mat2: ", mats[i2], " static2: ", Materials.IsStatic(mats[i2]), " normal2: ", BaseMaterial.IsNormal(mats[i2]));
                // GD.Print(" isMatch: ", BaseMaterial.IsMatchFilterOffset(mats[i2]), " isNonMatch: ", BaseMaterial.IsNonMatchFilterOffset(mats[i2]), " baseId: ", BaseMaterial.BaseId(mats[i2]));
                
                if (IsStatic(mats[i1])) // || (mats[i1] != -1 && !BaseMaterial.IsNormal(mats[i1])))
                {
                    needAir = true;
                    continue;
                }
                if (field.GetUpdatedWithinCurrentTick(idxs[i1]))
                {
                    continue;
                }
                if (needAir)
                {
                    if (mats[i1] != -1)
                    {
                        continue;
                    }
                    needAir = false;
                }
                
                if (IsStatic(mats[i2])) // || (mats[i2] != -1 && !BaseMaterial.IsNormal(mats[i1])))
                {
                    i++;
                    int i3 = (Clockwise ? i2 - 1 : i2 + 1) & 7;
                    // if (posX == 2981 && posY == 2442)
                    // {
                    //     GD.Print("i3: ", i3, " mat3: ", mats[i3], " static3: ", Materials.IsStatic(mats[i3]), " canTraverse: ", CanTraverse(mats[i3],mats[i2]));
                    // }
                    if (!CanTraverse(mats[i3],mats[i2]))
                    {
                        needAir = true;
                        continue;
                    }

                    i2 = i3;
                }
                if (field.GetUpdatedWithinCurrentTick(idxs[i2]))
                {
                    i++;
                    continue;
                }
                if (mats[i2] == -1 && someStatic != NONE)
                {
                    continue;
                }

                // if (posX == 2981 && posY == 2442)
                // {
                //     GD.Print("         SWAPPED!!!!!");
                // }
                BaseMaterial.SwapWithTarget(idxs[i2], posX + offsets[i2].X, posY + offsets[i2].Y, idxs[i1], posX + offsets[i1].X, posY + offsets[i1].Y, field, tick);
                mats[i2] = mats[i1];
                somethingMoved = true;
            }

            if (someStatic == NONE)
            {
                // for (int i = 0; i < offsets.Length - 1; i++)
                // {
                //     int i1 = (Clockwise ? 7 - i : i) & 7;
                //     if (field.GetUpdatedWithinCurrentTick(idxs[i1]))
                //     {
                //         break;
                //     }
                //     int i2 = (Clockwise ? i1 - 1 : i1 + 1) & 7;
                //
                //     BaseMaterial.SwapWithTarget(idxs[i2], posX + offsets[i2].X, posY + offsets[i2].Y, idxs[i1], posX + offsets[i1].X, posY + offsets[i1].Y, field, tick);
                // }
                field.SetUpdatedWithinCurrentTick(idxs[someStatic & 7]);
            }
        }




        // int num = posY - 1;
        // int sourceX = posX - 1;
        // int sourceX2 = posX - 1;
        // int sourceY = posY - 1;
        // int targetX = posX + 1;
        // int targetY = posY - 1;
        // ConveyorMaterial.TryConvey(sourceX, posY, posX, num, field, tick);
        // ConveyorMaterial.TryConvey(sourceX2, sourceY, posX, num, field, tick);
        // ConveyorMaterial.TryConvey(posX, num, targetX, targetY, field, tick);
        return somethingMoved;
    }
}