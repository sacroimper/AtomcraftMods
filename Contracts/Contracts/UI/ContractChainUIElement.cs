using System.Collections.Generic;
using Atomcraft;
using Godot;

namespace Contracts;

public partial class ContractChainUIElement : VBoxContainer
{
    private string ChainId = null!;
    private Contracts.ContractChain Chain = null!;

    private Label ChainName = null!;
    private VBoxContainer ContractsContainer = null!;

    private readonly List<ContractUIElement> ContractUIElements = new();

    public ContractChainUIElement Init(string chainId, Contracts.ContractChain chain)
    {
        ChainId = chainId;
        Chain = chain;

        ChainName = GetNode<Label>("%ChainName");
        ContractsContainer = GetNode<VBoxContainer>("%ContractsContainer");

        ChainName.Text = chain.Name;

        BuildContracts();

        return this;
    }
    
    public void Refresh()
    {
        for (int i = 0; i < ContractUIElements.Count; i++)
        {
            ContractUIElements[i].Refresh();
        }
    }

    public void Process(float delta)
    {
        foreach (ContractUIElement contractUIElement in ContractUIElements)
        {
            contractUIElement._Process(delta);
        }
    }

    public void OnLocalizationChanged()
    {
        foreach (ContractUIElement contractUIElement in ContractUIElements)
        {
            contractUIElement.OnLocalizationChanged();
        }
    }

    private void BuildContracts()
    {
        foreach (Node child in ContractsContainer.GetChildren())
        {
            child.QueueFree();
        }

        ContractUIElements.Clear();


        PackedScene contractUIElementScene = GD.Load<PackedScene>("res://Contracts/Resources/UI/ContractUIElement.tscn");
        foreach (Contracts.ContractType contract in Chain.Contracts)
        {
            ContractUIElement contractUIElement = contractUIElementScene.Instantiate<ContractUIElement>();

            contractUIElement.Init(contract);
            
            ContractsContainer.AddChild(
                contractUIElement,
                forceReadableName: false);

            ContractUIElements.Add(contractUIElement);
        }
    }
}