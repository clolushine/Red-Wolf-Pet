using Godot;
using System;
using global::desktoppet.scripts.global;

namespace desktoppet.scripts.objects;

public partial class PopupPanelTip : PopupPanel
{
	public Label LabelContainer;

	private static readonly string DefaultLabelText = "弹药打光了，请稍等片刻等待装弹完成";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GlobalManager.PopupPanelTip = this;
		
		LabelContainer = GetNode<Label>("Label");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetLabel(string text = default)
	{
		LabelContainer.SetText(text ?? DefaultLabelText);
	}
}
