using Atomcraft;
using Godot;

namespace Contracts;

public partial class ContractsWindow : UIWindow
{
    private static readonly StyleBoxFlat SeparatorStyle = new StyleBoxFlat
    {
        BgColor = new Color(1f, 1f, 1f, 0.8f),
        ContentMarginLeft = 1,
        ContentMarginRight = 1
    };

    public override WindowId ID => (WindowId) 13;
    
    private TextureButton CloseButton = null!;
    private RichTextLabel Description = null!;
    private Label StarsLabel = null!;
    private TextureRect Star = null!;
    private HBoxContainer ButtonsContainer = null!;
    private ScrollContainer ScrollContainer = null!;
    private HBoxContainer ChainsContainer = null!;
    
    private ScrollBar VScroll = null!;
    private ScrollBar HScroll = null!;

    private bool _isDragging;
    private readonly List<ContractChainUIElement> ChainUIElements = [];

    public override UIWindow Init()
    {
        CloseButton = GetNode<TextureButton>("%CloseButton");
        Description = GetNode<RichTextLabel>("%Description");
        ButtonsContainer = GetNode<HBoxContainer>("%ButtonsContainer");
        ScrollContainer = GetNode<ScrollContainer>("%ScrollContainer");
        VScroll = GetNode<VScrollBar>("%VScrollBar");
        HScroll = GetNode<HScrollBar>("%HScrollBar");
        ChainsContainer = GetNode<HBoxContainer>("%ChainsContainer");
        StarsLabel = GetNode<Label>("%StarsLabel");
        Star = GetNode<TextureRect>("%Star");

        Star.Texture = UICache.StarFullTexture;
        
        StyleBoxTexture bgStyle = (StyleBoxTexture) GetThemeStylebox("panel");
        bgStyle.Texture = UICache.WindowBGTexture;
        
        
        StyleBoxTexture innerBorderStyle = (StyleBoxTexture) ((PanelContainer) ScrollContainer.GetParent()).GetThemeStylebox("panel");
        innerBorderStyle.Texture = UICache.WindowInnerBorderTexture;
        
        ScrollContainer.GuiInput += OnScrollGuiInput;
        
        VScroll.ValueChanged += (v) => ScrollContainer.ScrollVertical = (int)v;
        HScroll.ValueChanged += (v) => ScrollContainer.ScrollHorizontal = (int)v;
        ScrollContainer.GetVScrollBar().ValueChanged += (v) => VScroll.Value = v;
        ScrollContainer.GetHScrollBar().ValueChanged += (v) => HScroll.Value = v;
        ScrollContainer.GetVScrollBar().Changed += UpdateVScrollBar;
        ScrollContainer.GetHScrollBar().Changed += UpdateHScrollBar;
        
        
        BuildChains();

        
        CloseButton.Pressed += OnCloseButtonPressed;

        Description.Text = "This is a  description for the contracts";
        return this;
    }

    public override void OnOpen()
    {
        Refresh();
    }

    public override void OnClose()
    {
    }
    
    private void OnCloseButtonPressed()
    {
        Gameplay.CloseCurrentWindow();
    }

    public override void OnLocalizationChanged()
    {
        if (Contracts.ContractsData.Count == 0)
        {
            return;
        }
        foreach (ContractChainUIElement chainUIElement in ChainUIElements)
        {
            chainUIElement.OnLocalizationChanged();
        }
    }

    public override void Process(float delta)
    {
        if (Contracts.ContractsData.Count > 0)
        {
            return;
        }
        
        foreach (ContractChainUIElement chainUIElement in ChainUIElements)
        {
            chainUIElement.Process(delta);
        }
    }
    
    
    private void OnScrollGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left)
        {
            _isDragging = inputEventMouseButton.Pressed;
        }
        else if (@event is InputEventMouseMotion inputEventMouseMotion && _isDragging)
        {
            ScrollContainer.ScrollHorizontal -= (int)inputEventMouseMotion.Relative.X;
            ScrollContainer.ScrollVertical -= (int)inputEventMouseMotion.Relative.Y;
            HScroll.SetValueNoSignal(ScrollContainer.ScrollHorizontal);
            VScroll.SetValueNoSignal(ScrollContainer.ScrollVertical);
        }
    }
    
    private void UpdateVScrollBar()
    {
        VScroll.Step = ScrollContainer.GetVScrollBar().Step;
        VScroll.MinValue = ScrollContainer.GetVScrollBar().MinValue;
        VScroll.MaxValue = ScrollContainer.GetVScrollBar().MaxValue;
        VScroll.Page = ScrollContainer.GetVScrollBar().Page;
    }
    
    private void UpdateHScrollBar()
    {
        HScroll.Step = ScrollContainer.GetHScrollBar().Step;
        HScroll.MinValue = ScrollContainer.GetHScrollBar().MinValue;
        HScroll.MaxValue = ScrollContainer.GetHScrollBar().MaxValue;
        HScroll.Page = ScrollContainer.GetHScrollBar().Page;
    }

    private void BuildChains()
    {
        
        foreach (Node child in ChainsContainer.GetChildren())
        {
            child.QueueFree();
        }

        ChainUIElements.Clear();

        PackedScene chainUIElementScene = GD.Load<PackedScene>("res://Contracts/Resources/UI/ContractChainUIElement.tscn");
        int i = 0;
        foreach (KeyValuePair<string, Contracts.ContractChain> chainPair in Contracts.Chains)
        {
            ContractChainUIElement chainUIElement = chainUIElementScene.Instantiate<ContractChainUIElement>();
            
            ChainsContainer.AddChild(
                chainUIElement,
                forceReadableName: false);

            chainUIElement.Init(chainPair.Key, chainPair.Value);
            ChainUIElements.Add(chainUIElement);

            i++;
            if (i < Contracts.Chains.Count)
            {
                var separator = new VSeparator()
                {
                    CustomMinimumSize = new Vector2(4, 0),
                };

                separator.AddThemeStyleboxOverride("separator", SeparatorStyle);

                ChainsContainer.AddChild(separator);
            }
        }
        
    }

    public void Refresh()
    {
        
        StarsLabel.Text = Contracts.GetCurrentStars().ToString();
        
        foreach (ContractChainUIElement chainUIElement in ChainUIElements)
        {
            chainUIElement.Refresh();
        }
        
    }
}