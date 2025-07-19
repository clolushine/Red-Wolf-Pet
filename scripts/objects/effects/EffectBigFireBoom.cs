using System.Collections.Generic;
using Godot;

namespace desktoppet.scripts.objects.effects;

public partial class EffectBigFireBoom : Effect
{
    private Effect _noneEffect;
    
    private List<AnimationPlayer> _copyPlayers = new List<AnimationPlayer>();
    
    public override void _Ready()
    {
        _noneEffect = GetNode<Effect>("../none");
    }

    public override void Start()
    {
        StartEffect();
    }
    
    public override void End()
    {
        EndEffect();
    }
    
    private void StartEffect()
    {
        AnimationPlayer copyPlayer = Effects.GetCopyPlayer();
        
        _copyPlayers.Add(copyPlayer);
        
        copyPlayer.Play("bigfireboom");

        if (Effects.EffectAudioPlayer.Stream == null)
        {
            Effects.EffectAudioPlayer.Stream = Effects.BigFireBoomSound;
        }
        
        Effects.EffectAudioPlayer.Play();
        
        copyPlayer.Connect("animation_finished", Callable.From<string>((name) =>
        {
            copyPlayer.QueueFree();
            _copyPlayers.Remove(copyPlayer);
            
            // 延迟
            GetTree().ProcessFrame += () =>
            {
                if (_copyPlayers.Count == 0 && EffectsManager.GetCurrentEffect() != this)
                {
                    // 设置点击穿透，不然该图片区域无法点击
                    SetMouseThoughWindow(GetWindow());
                }
            };
        }));
    }
    
    private void EndEffect()
    {
       
    }
    
    public override Effect Process(double delta)
    {
        if (_copyPlayers.Count == 0)
        {
            return _noneEffect;
        }
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