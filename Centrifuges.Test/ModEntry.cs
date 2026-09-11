namespace Centrifuges.Test;

/// <summary>
/// Loader entry point. The harness finds tests by attribute across every loaded assembly,
/// so there is nothing to register here; this exists because the loader requires an
/// Initialize() when a module declares an initClass.
/// </summary>
public static class ModEntry
{
    public static void Initialize()
    {
        // The loader's dependencies carry no version constraint, so a mismatched harness
        // would otherwise surface as a MissingMethodException once a test runs.
        Atomcraft.TestHarness.Harness.RequireVersion("0.2");
        Godot.GD.Print("[Centrifuges.Test] loaded");
    }
}
