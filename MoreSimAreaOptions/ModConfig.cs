using Godot;
using GodotMonoModLoader;
using Newtonsoft.Json;

namespace MoreSimAreaOptions;

[Serializable]
public class ModConfig : IModConfig
{
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<Vector2I> SimAreaOptions =
    [
        new Vector2I(12, 12),
        new Vector2I(14, 14),
        new Vector2I(16, 16),
        new Vector2I(18, 18),
        new Vector2I(20, 20),
        new Vector2I(25, 25)
    ];

    public static IModConfig Default()
    {
        return new ModConfig();
    }
}