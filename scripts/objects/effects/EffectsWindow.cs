using Godot;
using System;
using global::desktoppet.scripts.global;

namespace desktoppet.scripts.objects.effects;

public partial class EffectsWindow : Window
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GlobalManager.EffectsWindow = this;
		
		InitWindow();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void InitWindow()
	{
		Title = "";
		
		Transparent = true;
	}
}
