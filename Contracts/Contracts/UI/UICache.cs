using Godot;

namespace Contracts;

public static class UICache
{
    public static readonly Texture2D StarFullTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://Contracts/Resources/Art/star_full.png"));
    public static readonly Texture2D StarEmptyTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://Contracts/Resources/Art/star_empty.png"));
    public static readonly Texture2D ActiveStyleBGTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://Contracts/Resources/Art/active_bg.png"));
    public static readonly Texture2D InactiveStyleBGTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://Contracts/Resources/Art/inactive_bg.png"));
    public static readonly Texture2D LockedStyleBGTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://Contracts/Resources/Art/locked_bg.png"));
    public static readonly Texture2D WindowBGTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://Contracts/Resources/Art/screen_bg.png"));
    public static readonly Texture2D WindowInnerBorderTexture = ImageTexture.CreateFromImage(Image.LoadFromFile("res://Contracts/Resources/Art/inner_border.png"));
}