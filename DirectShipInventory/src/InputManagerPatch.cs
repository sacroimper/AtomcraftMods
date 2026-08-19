using Atomcraft;
using Godot;
using HarmonyLib;

namespace DirectShipInventory;

[HarmonyPatch(typeof(InputManager), nameof(InputManager.Process))]
public static class InputManagerPatch
{   
    public static void Postfix(float elapsed)
    {
        bool isInteractWithBackgroundPressed = Input.IsActionJustPressed("KB_InteractWithBackground") || Input.IsActionJustPressed("Joypad_InteractWithBackground");
        bool canEnter = Avatars.LocalAvatar != null && !Avatars.LocalAvatar.SpaceshipActive &&
                        Game.ALLOW_ENTERING_SPACESHIP && !Game.World.Spaceship.IsTakingOff; // removed UI.CurrentPageId == PageId.Gameplay
        if (UI.CurrentPageId == PageId.SpaceshipInterior && canEnter && GodotObject.IsInstanceValid(Avatars.LocalAvatar) && isInteractWithBackgroundPressed && Avatars.LocalAvatar.NearSpaceship)
        {
            Game.UI.SetUIPage(PageId.SpaceshipInventory); // SpaceshipInterior
        }
    }
}