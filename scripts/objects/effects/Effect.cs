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
    
    public static void SetMouseThoughWindow(Window window)
    {
        var size = window.GetSize();
        
        var position = window.GetPosition();
		  
        Vector2[] polygon = new Vector2[] {
            new Vector2(position.X, position.Y),
            new Vector2(position.X + size.X, position.Y),
            new Vector2(position.X + size.X, position.Y + size.Y),
            new Vector2(position.X,  position.Y + size.Y)
        };
        
        window.MousePassthroughPolygon = polygon;
    }
}