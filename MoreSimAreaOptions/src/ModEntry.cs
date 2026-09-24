
using Godot;
using GodotMonoModLoader;
using GodotMonoModLoader.Atomcraft;

namespace MoreSimAreaOptions;

public class ModEntry : AtomcraftModEntry, IModInitializationProvider<ModConfig>
{
    public void Initialize(InitializationContext<ModConfig> context)
    {
        
        context.ModConfig.SimAreaOptions.ForEach(v => Consts.SUPPORTED_SIM_RESOLUTIONS.Add(v));
        
        GD.Print($"[MoreSimAreaOptions]: MoreSimAreaOptions Initialized.");
    }
}