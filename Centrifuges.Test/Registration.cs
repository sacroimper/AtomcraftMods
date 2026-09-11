using Atomcraft;
using Atomcraft.TestHarness;

namespace Centrifuges.Test;

/// <summary>
/// The mod's materials exist and are wired up. These fail first and loudest when a data
/// file or a Harmony postfix has gone missing, which makes every other failure easier to
/// read.
/// </summary>
public static class Registration
{

    [GameTest]
    public static void MaterialsAreRegistered(Region r)
    {
        foreach (var name in new[] { MaterialNames.Bits, MaterialNames.Clockwise, MaterialNames.CounterClockwise,
                                     "Red Blood Cell", "Blood Plasma", "White Blood Cell" })
        {
            if (Materials.GetBaseMaterialId(name) == -1)
                throw new AssertionException($"material '{name}' is not registered");
        }
    }

    /// <summary>
    /// The built centrifuges must be static. A mechanical pixel that falls leaves whatever
    /// it was built into, and the mod's own Step assumes it stays put.
    /// </summary>
    [GameTest]
    public static void BuiltCentrifugesAreStatic(Region r)
    {
        foreach (var name in new[] { MaterialNames.Clockwise, MaterialNames.CounterClockwise })
        {
            r.Clear();
            r.Set(20, 20, name);
            r.Ticks(20);
            r.AssertAt(20, 20, name);
        }
    }

    /// <summary>
    /// Each centrifuge is registered with a CentrifugeMaterial rather than the plain
    /// BaseMaterial the game creates by default, which is what gives it behavior at all.
    /// </summary>
    [GameTest]
    public static void CentrifugesUseTheirCustomClass(Region r)
    {
        foreach (var name in new[] { MaterialNames.Clockwise, MaterialNames.CounterClockwise })
        {
            var material = Materials.TryGetBaseMaterial(name);
            if (material == null)
                throw new AssertionException($"no BaseMaterial for '{name}'");
            if (material.GetType().Name != "CentrifugeMaterial")
                throw new AssertionException(
                    $"'{name}' is a {material.GetType().Name}, expected CentrifugeMaterial. " +
                    "The Materials.InitializeCustomClasses postfix may not have run.");
        }
    }

}
