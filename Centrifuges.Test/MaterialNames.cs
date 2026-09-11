using Atomcraft;
using Atomcraft.TestHarness;

namespace Centrifuges.Test;

/// <summary>
/// The mod's material names, as the game knows them. Strings rather than ids, because ids
/// shift with whatever else is installed and only names are stable.
///
/// Not called Materials: that is the game's own static class, used throughout these tests,
/// and a local type of the same name would win name resolution over it.
/// </summary>
internal static class MaterialNames
{
    internal const string Clockwise = "Centrifuge (Clockwise)";
    internal const string CounterClockwise = "Centrifuge (Counter-clockwise)";
    internal const string Bits = "Bits of Centrifuge";
}
