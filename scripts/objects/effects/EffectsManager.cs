using Godot;
using System;
using System.Collections.Generic;

namespace desktoppet.scripts.objects.effects;

public partial class EffectsManager : Node
{
	private static readonly List<Effect> Effects = new List<Effect>();
	
	public Effect CurrentEffect = null;
	private Effect PrevEffect = null;
	
	public override void _Ready()
	{
		// ProcessModeEnum.Disabled表示该节点将不参与物理处理
		// 不接收_Process和_PhysicsProcess回调，但仍会渲染（如果可见）
		ProcessMode = ProcessModeEnum.Disabled;
	}
	
	public override void _Process(double delta)
	{
		StartEffect(CurrentEffect.Process(delta));
	}

	public override void _PhysicsProcess(double delta)
	{
		StartEffect(CurrentEffect.Process(delta));
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
	}
	
	public void Initialize(Effects effects)
	{
		Effects.Clear();
		foreach (var n in GetChildren())
		{
			if (n is Effect effect) Effects.Add(effect);
		}
		if (Effects.Count == 0) return;
		foreach (var effect in Effects)
		{
			effect.Effects = effects;
			effect.EffectsManager = this;
			effect.Init();
		}
		StartEffect(Effects[0]);
		
		ProcessMode = ProcessModeEnum.Inherit;
	}
	
	
	public void StartEffect(Effect newEffect)
	{
		if (newEffect == null) return;

		if (IsInstanceValid(CurrentEffect))
		{
			CurrentEffect.End();
		}
		
		PrevEffect = CurrentEffect;
		CurrentEffect = newEffect;

		CurrentEffect.Start();
	}
	
	public Effect GetCurrentEffect()
	{
		return CurrentEffect;
	}
}