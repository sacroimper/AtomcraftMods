using Godot;

namespace Contracts;

public partial class ContractTooltip : PanelContainer
{
    private Contracts.ContractType Contract = null!;

    private Label StarsRequired = null!;
    private Label TimesDone = null!;
    private VBoxContainer RewardsContainer = null!;
    private VBoxContainer CostsContainer = null!;
    
    private static PackedScene MaterialAmountUIElementScene = GD.Load<PackedScene>("res://Contracts/Resources/UI/MaterialAmountUIElement.tscn");

    public ContractTooltip Init(Contracts.ContractType contract)
    {
        Contract = contract;

        RewardsContainer = GetNode<VBoxContainer>("%RewardsContainer");
        CostsContainer = GetNode<VBoxContainer>("%CostsContainer");
        StarsRequired = GetNode<Label>("%StarsRequired");
        TimesDone = GetNode<Label>("%TimesDone");
        
        foreach (var cost in Contract.Cost)
        {
            CostsContainer.AddChild(MaterialAmountUIElementScene.Instantiate<MaterialAmountUIElement>().Init(cost));
        }
        
        foreach (var reward in Contract.Reward)
        {
            RewardsContainer.AddChild(MaterialAmountUIElementScene.Instantiate<MaterialAmountUIElement>().Init(reward));
        }
        
        UpdateTooltip();
        
        return this;
    }

    public void Refresh()
    {
        UpdateTooltip();
    }
    
    public void OnLocalizationChanged()
    {
        UpdateTooltip();
    }

    private void UpdateTooltip()
    {
        if (!Contracts.IsUnlocked(Contract.ContractTypeId))
        {
            StarsRequired.AddThemeColorOverride(
                "font_color",
                Colors.IndianRed);
            if (Contracts.GetCurrentStars() < Contract.StarsRequired)
            {
                StarsRequired.Text = $"{Contracts.GetCurrentStars()} out of {Contract.StarsRequired} stars.\n" +
                                     $"(Gain more stars to unlock)";
            }
            else
            {
                StarsRequired.Text = $"{Contracts.GetCurrentStars()} out of {Contract.StarsRequired} stars.\n" +
                                     $"(Gain at least one star with the previous contract)";
            }
            TimesDone.Hide();
        }
        else
        {
            if (Contracts.IsActive(Contract.ContractTypeId))
            {
                StarsRequired.AddThemeColorOverride(
                    "font_color",
                    Colors.LimeGreen);
                StarsRequired.Text = $"Unlocked since {Contract.StarsRequired} Stars.\n" +
                                     $"(Click to deactivate)";
            }
            else
            {
                StarsRequired.AddThemeColorOverride(
                    "font_color",
                    Colors.Yellow);
                StarsRequired.Text = $"Unlocked since {Contract.StarsRequired} Stars.\n" +
                                     $"(Click to activate)";
            }

            int timesDone = Contracts.GetData(Contract.ContractTypeId)?.TimesDone ?? 0;
            if (timesDone < 50)
            {
                int nextStar = timesDone switch
                {
                    < 1 => 1,
                    < 5 => 5,
                    < 10 => 10,
                    < 25 => 25,
                    _ => 50
                };
                TimesDone.Text = $"Times done: {timesDone}/{nextStar}";
            }
            else
            {
                TimesDone.Text = $"Times done: {timesDone}";
            }
            TimesDone.Show();
        }
    }
}