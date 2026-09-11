using Atomcraft;
using Atomcraft.TestHarness;

namespace Centrifuges.Test;

/// <summary>
/// The blood separation recipe the centrifuges catalyze.
/// </summary>
public static class BloodSeparation
{

    /// <summary>
    /// Blood next to a running centrifuge separates into its components. The recipe is
    /// gated between 278 K and 313 K and rolls a 1-in-240 chance per tick, so the test
    /// waits for it rather than assuming a fixed number of ticks.
    /// </summary>
    [GameTest(Wall = "Granite")]
    public static void BloodSeparatesNextToACentrifuge(Region r)
    {
        var (cx, cy) = Fixtures.BuildPocket(r, 20, 5, 4, "Blood");
        r.Set(cx, cy, MaterialNames.Clockwise);
        r.Field<short>("core.heat").Fill(10, 50, 24, 13, 295);   // inside the recipe's window

        r.TicksUntil(() => r.Count("Red Blood Cell") > 0 || r.Count("Blood Plasma") > 0,
            4000, "blood separates into components");
    }

    /// <summary>
    /// Above 313 K the recipe must not fire. Without this the temperature gate could be
    /// deleted and every other test would still pass.
    ///
    /// Currently blocked by a game bug rather than a mod bug: Reaction.MaxTemperature is
    /// declared, parsed from JSON, and copied into the runtime struct, but never read, so
    /// no reaction has an upper temperature bound. The minimum is enforced in BaseMaterial's
    /// reaction loop; there is no corresponding maximum check anywhere. 16 of the 721 vanilla
    /// reactions are affected too, including the three banded Boudouard equilibria that exist
    /// precisely to fire in different temperature ranges.
    ///
    /// Kept and skipped rather than deleted: the assertion is correct, and it should start
    /// passing the day the game enforces the bound.
    /// </summary>
    [GameTest(Wall = "Granite",
        Skip = "blocked by game bug: Reaction.MaxTemperature is parsed but never enforced")]
    public static void BloodDoesNotSeparateWhenTooHot(Region r)
    {
        // Spawned hot, not heated afterwards: a pocket of blood warmed from ambient passes
        // straight through the recipe's 278-313 K window on the way up and separates during
        // the ramp, which would prove nothing. Pinned as well, since heat decays back toward
        // ambient over a run this long.
        var (cx, cy) = Fixtures.BuildPocket(r, 20, 5, 4, "Blood", 400);
        r.Set(cx, cy, MaterialNames.Clockwise);
        r.PinnedHeat = 400;
        r.Ticks(2000);

        if (r.HeatAt(cx - 1, cy) <= 313)
            throw new AssertionException(
                $"the region did not stay hot: {r.HeatAt(cx - 1, cy)} K beside the machine");

        if (r.Count("Red Blood Cell") > 0 || r.Count("White Blood Cell") > 0)
            throw new AssertionException(
                $"blood separated at 400 K, outside the recipe's 278-313 K window\n{r.Dump()}");
    }
}
