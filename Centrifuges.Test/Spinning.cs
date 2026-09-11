using Atomcraft;
using Atomcraft.TestHarness;

namespace Centrifuges.Test;

/// <summary>
/// The machines move what sits around them, and the two turn opposite ways.
/// </summary>
public static class Spinning
{

    /// <summary>
    /// A loose pixel next to a running centrifuge is moved. Direction is asserted
    /// separately; this only establishes that the machine does something at all, which is
    /// the assertion most likely to catch a broken Step.
    /// </summary>
    [GameTest(Wall = "Granite", StartTick = 0)]
    public static void ACentrifugeMovesAnAdjacentPixel(Region r)
    {
        const int cx = 20, cy = 20;
        r.Set(cx, cy, MaterialNames.Clockwise);
        r.Set(cx, cy - 1, "Sand");        // directly above

        r.TicksUntil(() => r.At(cx, cy - 1) != "Sand", 64,
            "the centrifuge moves the pixel above it");

        r.AssertCount("Sand", 1);
        r.AssertAt(cx, cy, MaterialNames.Clockwise);
    }

    /// <summary>
    /// MaterialNames.Clockwise and counter-clockwise must not behave identically. Asserting they differ
    /// is more robust than asserting an exact destination, which depends on the tick the
    /// machine happens to be on.
    /// </summary>
    /// <summary>
    /// A sealed 3x3 chamber cut into solid rock, with the machine at its center. The eight
    /// cells around it are the only air, so a grain cannot fall out of reach and whatever
    /// movement is observed is the centrifuge's, not gravity's.
    ///
    /// Two grains are placed a quarter turn apart rather than opposite each other, so a
    /// symmetric bug cannot produce a passing result by accident. Two more sit two cells
    /// out, outside the machine's eight-cell neighborhood, and must not move at all.
    /// </summary>
    [GameTest(Wall = "Granite", ChunksWide = 2)]
    public static void CentrifugesTurnTheExpectedWay(Region r)
    {
        const int cy = 30;
        const int leftX = 20, rightX = 80;

        var cw = Spin(r, leftX, cy, MaterialNames.Clockwise);
        var ccw = Spin(r, rightX, cy, MaterialNames.CounterClockwise);

        var report =
            $"  clockwise         Sand {Path(cw.Sand)}  Iron {Path(cw.Iron)}\n" +
            $"  counter-clockwise Sand {Path(ccw.Sand)}  Iron {Path(ccw.Iron)}\n";

        // Measured as which side of bottom the grains are held on, not as net revolutions.
        //
        // A loose grain never laps the machine: the centrifuge lifts it up the rising side,
        // gravity returns it down the far side, and net rotation over any window averages to
        // zero however well the machine works. What does distinguish the two is the arc the
        // grains occupy. MaterialNames.Clockwise holds them toward SW and W; counter-clockwise toward SE
        // and E. Index 4 is straight down, and the indices run clockwise from north.
        var cwLean = Lean(cw.Sand) + Lean(cw.Iron);
        var ccwLean = Lean(ccw.Sand) + Lean(ccw.Iron);

        if (cwLean <= 0)
            throw new AssertionException(
                $"the clockwise centrifuge did not hold its grains on the clockwise side of bottom " +
                $"(lean {cwLean:F2}, expected positive)\n{report}");
        if (ccwLean >= 0)
            throw new AssertionException(
                $"the counter-clockwise centrifuge did not hold its grains on the counter-clockwise side " +
                $"(lean {ccwLean:F2}, expected negative)\n{report}");
        if (cwLean - ccwLean < 1.0)
            throw new AssertionException(
                $"the two directions are barely distinguishable (clockwise {cwLean:F2}, " +
                $"counter-clockwise {ccwLean:F2})\n{report}");

        if (!cw.ControlsHeld)
            throw new AssertionException($"the clockwise machine moved a grain two cells away\n{report}{r.Dump()}");
        if (!ccw.ControlsHeld)
            throw new AssertionException($"the counter-clockwise machine moved a grain two cells away\n{report}{r.Dump()}");
    }

    /// <summary>
    /// With all eight cells occupied there is nowhere for a grain to fall, so the ring
    /// turns as a body and a marked grain laps the machine. This is the configuration the
    /// mod is really for; a lone grain never completes a revolution, because the machine
    /// lifts it up the rising side and gravity returns it down the far side.
    ///
    /// A packed ring holds no static pixel, so CentrifugeMaterial only performs its full
    /// swap when its local tick is divisible by four. Thirty-two ticks is eight swaps, one
    /// complete lap.
    /// </summary>
    [GameTest(Wall = "Granite", ChunksWide = 2)]
    public static void APackedRingCarriesGrainsAllTheWayRound(Region r)
    {
        const int cy = 30;
        const int leftX = 20, rightX = 80;

        var cw = Lap(r, leftX, cy, MaterialNames.Clockwise);
        var ccw = Lap(r, rightX, cy, MaterialNames.CounterClockwise);

        var report = $"  clockwise         {Path(cw)}\n  counter-clockwise {Path(ccw)}\n";

        var cwTurn = Turn(cw);
        var ccwTurn = Turn(ccw);

        // A full lap is eight eighths. Requiring most of one rules out a grain that merely
        // jitters back and forth without the ring actually turning.
        if (cwTurn < 6)
            throw new AssertionException(
                $"the clockwise ring turned only {cwTurn} eighths in 32 ticks, expected a lap\n{report}");
        if (ccwTurn > -6)
            throw new AssertionException(
                $"the counter-clockwise ring turned only {ccwTurn} eighths in 32 ticks, expected a lap\n{report}");
    }

    /// <summary>Packs all eight cells, marks one grain, and follows it for 32 ticks.</summary>
    private static List<int> Lap(Region r, int cx, int cy, string machine)
    {
        r.Fill(cx - 4, cy - 4, 9, 9, "Granite");
        for (var dy = -1; dy <= 1; dy++)
        for (var dx = -1; dx <= 1; dx++)
            r.SetAir(cx + dx, cy + dy);

        r.Set(cx, cy, machine);

        var offsets = new[] { (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1) };
        foreach (var (dx, dy) in offsets)
            r.Set(cx + dx, cy + dy, "Sand");
        r.Set(cx, cy - 1, "Iron");        // the marked grain, starting due north

        var path = new List<int> { Angle(r, cx, cy, "Iron") };
        for (var i = 0; i < 32; i++)
        {
            r.Ticks(1);
            path.Add(Angle(r, cx, cy, "Iron"));
        }
        return path;
    }

    /// <summary>Net rotation in eighths of a turn, taking the shorter way around each step.</summary>
    private static int Turn(List<int> angles)
    {
        var total = 0;
        for (var i = 1; i < angles.Count; i++)
        {
            if (angles[i] < 0 || angles[i - 1] < 0)
                continue;
            total += ((angles[i] - angles[i - 1] + 4) % 8 + 8) % 8 - 4;
        }
        return total;
    }

    private readonly record struct SpinResult(List<int> Sand, List<int> Iron, bool ControlsHeld);

    /// <summary>
    /// Carves the chamber, seeds it, and records where each grain sits after every tick for
    /// seven ticks. Seven covers more than a half turn while staying short enough to read.
    /// </summary>
    private static SpinResult Spin(Region r, int cx, int cy, string machine)
    {
        // Rock with a 3x3 hollow, so the eight cells around the machine are the ONLY air.
        // A wider chamber lets gravity drag a grain out of the ring within two ticks, and
        // what gets measured is falling rather than spinning.
        r.Fill(cx - 4, cy - 4, 9, 9, "Granite");
        for (var dy = -1; dy <= 1; dy++)
        for (var dx = -1; dx <= 1; dx++)
            r.SetAir(cx + dx, cy + dy);

        // Controls: sealed single-cell pockets three out, well beyond the eight-cell
        // neighborhood. Nothing can move them, so if they move the machine reached too far.
        r.SetAir(cx - 3, cy);
        r.SetAir(cx + 3, cy);
        r.Set(cx - 3, cy, "Compost");
        r.Set(cx + 3, cy, "Compost");

        r.Set(cx, cy, machine);
        r.Set(cx, cy + 1, "Sand");        // south, index 4
        r.Set(cx + 1, cy, "Iron");        // east,  index 2, a quarter turn away

        // Let gravity seat the grains first. Measuring from the initial placement measures
        // the drop, not the rotation; the machine only acts every other tick anyway.
        r.Ticks(8);

        var sand = new List<int> { Angle(r, cx, cy, "Sand") };
        var iron = new List<int> { Angle(r, cx, cy, "Iron") };
        for (var i = 0; i < 8; i++)
        {
            r.Ticks(1);
            sand.Add(Angle(r, cx, cy, "Sand"));
            iron.Add(Angle(r, cx, cy, "Iron"));
        }

        var held = r.At(cx - 3, cy) == "Compost" && r.At(cx + 3, cy) == "Compost";
        return new SpinResult(sand, iron, held);
    }

    /// <summary>
    /// Position around the machine as an index into Utils.AdjacentOffsets, which runs
    /// clockwise from north: 0 N, 1 NE, 2 E, 3 SE, 4 S, 5 SW, 6 W, 7 NW. -1 if not adjacent.
    /// </summary>
    private static int Angle(Region r, int cx, int cy, string material)
    {
        var offsets = new[] { (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1) };
        for (var i = 0; i < offsets.Length; i++)
            if (r.At(cx + offsets[i].Item1, cy + offsets[i].Item2) == material)
                return i;
        return -1;
    }

    /// <summary>
    /// Mean displacement from straight down, in eighths of a turn. Positive means the grain
    /// spends its time on the clockwise side of bottom, negative the counter-clockwise side.
    /// </summary>
    private static double Lean(List<int> angles)
    {
        var seen = angles.Where(a => a >= 0).ToList();
        if (seen.Count == 0)
            return 0;
        return seen.Average(a => (double)(((a - 4 + 4) % 8 + 8) % 8 - 4));
    }

    private static string Path(List<int> angles) =>
        string.Join("->", angles.Select(a => a < 0 ? "-" : a.ToString()));


}
