
using Godot;

namespace MoreSimAreaOptions;

public static class ModEntry
{
    public static void Initialize()
    {
        
        Consts.SUPPORTED_SIM_RESOLUTIONS.Add(new Vector2I(8, 8));
        Consts.SUPPORTED_SIM_RESOLUTIONS.Add(new Vector2I(10, 10));
        Consts.SUPPORTED_SIM_RESOLUTIONS.Add(new Vector2I(12, 12));
        Consts.SUPPORTED_SIM_RESOLUTIONS.Add(new Vector2I(14, 14));
        Consts.SUPPORTED_SIM_RESOLUTIONS.Add(new Vector2I(16, 16));
        Consts.SUPPORTED_SIM_RESOLUTIONS.Add(new Vector2I(18, 18));
        Consts.SUPPORTED_SIM_RESOLUTIONS.Add(new Vector2I(20, 20));
        
        GD.Print($"[MoreSimAreaOptions] MoreSimAreaOptions Initialized.");
    }
}