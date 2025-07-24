using Godot;

namespace desktoppet.scripts.objects.effects;

public partial class EffectNone : Effect
{
    public override void _Ready()
    {
    }

    public override void Start()
    {
        // 隐藏窗口
        SetHideWindow(GetWindow());
    }
    
    public override void End()
    {
    }
    
    public override Effect Process(double delta)
    {
        return null;
    }
    
    public override Effect PhysicsProcess(double delta)
    {
        return null;
    }
    
    public override Effect UnHandleInput(InputEvent _event)
    {
        return null;
    }
}