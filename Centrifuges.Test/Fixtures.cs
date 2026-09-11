using Atomcraft;
using Atomcraft.TestHarness;

namespace Centrifuges.Test;

/// <summary>
/// Structures the tests build to hold a machine and its inputs in place.
/// </summary>
internal static class Fixtures
{
    /// <summary>
    /// Walls a pocket on the floor and fills it, so liquid stays in contact with the
    /// machine. Returns the cell the centrifuge should occupy.
    /// </summary>
    internal static (int X, int Y) BuildPocket(Region r, int centerX, int width, int height,
        string fill, short? kelvin = null)
    {
        var floorY = r.Height - r.WallThickness - 1;
        var left = centerX - width / 2;

        for (var y = floorY - height; y <= floorY; y++)
        {
            r.Set(left - 1, y, "Granite");
            r.Set(left + width, y, "Granite");
        }

        for (var y = floorY - height + 1; y <= floorY; y++)
        for (var x = left; x < left + width; x++)
            r.Set(x, y, fill, kelvin);

        return (centerX, floorY);
    }
}
