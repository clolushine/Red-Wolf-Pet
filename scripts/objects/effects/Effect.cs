using Godot;
using System;

namespace desktoppet.scripts.objects.effects;


public partial class Effect : Node
{
    public Effects Effects;
	
    public EffectsManager EffectsManager;
	
    public virtual void Init()
    {
    }
    
    public virtual void Start()
    {
    }
    
    public virtual void End()
    {
    }
    
    public virtual Effect Process(double delta)
    {
        return null;
    }
    
    public virtual Effect PhysicsProcess(double delta)
    {
        return null;
    }
    
    public virtual Effect UnHandleInput(InputEvent _event)
    {
        return null;
    }
    
    // 防止影响屏幕点击
    public static void SetHideWindow(Window window)
    {
        var size = window.GetSize();
        
        window.SetPosition(new Vector2I(1, -size.Y + 1));
    }
}