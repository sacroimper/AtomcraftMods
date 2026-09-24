using Atomcraft;
using Godot;

namespace Contracts;

public partial class MaterialAmountUIElement : HBoxContainer
{
    private Contracts.MaterialAmount MaterialAmount = null!;
    
    private ColorRect ColorRect = null!;
    private Label MaterialName = null!;
    private Label Amount = null!;
    
    public MaterialAmountUIElement Init(Contracts.MaterialAmount materialAmount)
    {
        MaterialAmount = materialAmount;

        ColorRect = GetNode<ColorRect>("%ColorRect");
        MaterialName = GetNode<Label>("%MaterialName");
        Amount = GetNode<Label>("%Amount");
        
        StyleBoxTexture innerBorderStyle = (StyleBoxTexture) ((PanelContainer) ColorRect.GetParent()).GetThemeStylebox("panel");
        innerBorderStyle.Texture = UICache.WindowInnerBorderTexture;
        
        UpdateMaterialAmount();
        
        return this;
    }

    private void UpdateMaterialAmount()
    {
        BaseMaterial m = MaterialAmount.MaterialTypeId.ToMaterial();
        ColorRect.Modulate = m.Color;
        MaterialName.Text = m.GetLocalizedNameWithBackup();
        Amount.Text = MaterialAmount.Amount.ToString();
    }
}