using Godot;
using System;
using global::desktoppet.scripts.global;
using global::desktoppet.scripts.utils;

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

	private async void InitWindow()
	{
		Title = "";
		
		Transparent = true;

		Visible = true;
		
		// 延迟
		// await Utils.WaitFrames(this, 6); // 等待6帧
		await ToSignal(GetTree(), "process_frame");
		// 完全穿透（所有区域）
		SetMouseThoughWindow();
	}
	
	private void SetMouseThoughWindow()
	{
		var size = GetSize();
        
		var position = GetPosition();
		
		Vector2[] polygon = new Vector2[] {
			new Vector2(position.X, position.Y),
			new Vector2(position.X + size.X, position.Y),
			new Vector2(position.X + size.X, position.Y + size.Y),
			new Vector2(position.X,  position.Y + size.Y)
		};
		
		MousePassthroughPolygon = polygon;
	}
}
