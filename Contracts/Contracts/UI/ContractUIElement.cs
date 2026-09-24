
using Atomcraft;
using Godot;

namespace Contracts;

public partial class ContractUIElement : PanelContainer
{
    
    private static readonly StyleBoxTexture LockedStyle = new()
    {
        Texture = UICache.LockedStyleBGTexture,
        TextureMarginLeft = 8,
        TextureMarginRight = 8,
        TextureMarginTop = 8,
        TextureMarginBottom = 8,
        
    };

    private static readonly StyleBoxTexture InactiveStyle = new()
    {
        Texture = UICache.InactiveStyleBGTexture,
        TextureMarginLeft = 8,
        TextureMarginRight = 8,
        TextureMarginTop = 8,
        TextureMarginBottom = 8,
    };

    private static readonly StyleBoxTexture ActiveStyle = new()
    {
        Texture = UICache.ActiveStyleBGTexture,
        TextureMarginLeft = 8,
        TextureMarginRight = 8,
        TextureMarginTop = 8,
        TextureMarginBottom = 8,
    };

    
    private Contracts.ContractType Contract = null!;

    private RichTextLabel LabelName = null!;
    private HBoxContainer StarsContainer = null!;
    
    private static readonly PackedScene TooltipScene = GD.Load<PackedScene>("res://Contracts/Resources/UI/ContractTooltip.tscn");
    private ContractTooltip? _tooltip;

    public ContractUIElement Init(Contracts.ContractType contract)
    {
        Contract = contract;

        LabelName = GetNode<RichTextLabel>("%LabelContractName");
        StarsContainer = GetNode<HBoxContainer>("%StarsContainer");

        
        MouseEntered += OnMouseEntered;
        Pressed += OnPressed;
        MouseExited += OnMouseExited;

        return this;
    }
    
    [Signal]
    public delegate void PressedEventHandler();
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton
            {
                ButtonIndex: MouseButton.Left,
                Pressed: true
            })
        {
            EmitSignal(SignalName.Pressed);
            AcceptEvent(); // Prevents the click from propagating to parent controls.
        }
    }
    
    public override void _Process(double delta)
    {
        if (_tooltip != null)
        {
            _tooltip.GlobalPosition = GetGlobalMousePosition() + new Vector2(16,16);
        }
    }

    public void Refresh()
    {
        UpdateContract();

        if (_tooltip != null)
        {
            _tooltip.Refresh();
        }
    }

    public void OnLocalizationChanged()
    {
        UpdateContract();
    }

    private void UpdateContract()
    {

        LabelName.Text = Contract.Name;

        var color = SelfModulate;
        if (!IsContractUnlocked())
        {
            color.A = 0.5f;
            AddThemeStyleboxOverride("panel", LockedStyle);
        }
        else if (IsContractActive())
        {
            color.A = 1f;
            AddThemeStyleboxOverride("panel", ActiveStyle);
        }
        else
        {
            color.A = 0.8f;
            AddThemeStyleboxOverride("panel", InactiveStyle);
        }
        SelfModulate = color; 

        foreach (var child in StarsContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (IsContractUnlocked())
        {
            int stars = Contracts.GetData(Contract.ContractTypeId)?.GetStars() ?? 0;
            for (int i = 1; i <= 5; i++)
            {
                StarsContainer.AddChild(new TextureRect()
                {
                    Texture = i > stars ? UICache.StarEmptyTexture : UICache.StarFullTexture,
                    CustomMinimumSize = new Vector2(24, 24)
                });
            }
        }
        else
        {
            StarsContainer.AddChild(new Label()
            {
                Text = $"{Contracts.GetCurrentStars()}/{Contract.StarsRequired}"
            });
            StarsContainer.AddChild(new TextureRect()
            {
                Texture = UICache.StarFullTexture,
                CustomMinimumSize = new Vector2(24, 24)
            });
        }
    }

    private bool IsContractActive()
    {
        return Contracts.IsActive(Contract.ContractTypeId);
    }

    private bool IsContractUnlocked()
    {
        return Contracts.IsUnlocked(Contract.ContractTypeId);
    }

    private ContractTooltip GetOrCreateTooltip()
    {
        if (_tooltip == null) {
            _tooltip = TooltipScene.Instantiate<ContractTooltip>().Init(Contract);
            Gameplay.Instance.AddChild(_tooltip);
        }
        
        return _tooltip;
    }
    
    private void OnMouseEntered()
    {
        ContractTooltip tooltip = GetOrCreateTooltip();
        tooltip.GlobalPosition = GetGlobalMousePosition() + new Vector2(16, 16);
        tooltip.Visible = true;
    }

    private void OnMouseExited()
    {
        
        _tooltip?.QueueFree();
        _tooltip = null;
    }

    private void OnPressed()
    {

        if (!IsContractUnlocked())
        {
            return;
        }
        
        // TODO Multiplayer support
        Contracts.ToggleContractActive(Contract.ContractTypeId);
        
        Refresh();
    }
}