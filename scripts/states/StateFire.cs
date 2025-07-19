using desktoppet.scripts.events;
using global::desktoppet.scripts.global;
using Godot;

namespace desktoppet.scripts.states;

public partial class StateFire : State
{
    private State _idleState;
    private State _moveState;
    
    private bool _isOnFire;
    
    public override void _Ready()
    {
        _idleState = GetNode<State>("../idle");
        _moveState = GetNode<State>("../move");
    }

    public override void Enter()
    {
        StartFire();
    }
    
    public override void Exit()
    {
        EndFire();
    }

    private async void StartFire()
    {
        if (Pet.CurrentShells > 0 && !_isOnFire)
        {
            _isOnFire = true;
            // 减少弹药数量
            Pet.CurrentShells--;
            GD.Print("剩余炮弹数量：" + Pet.CurrentShells);
            Pet.UpdateFireAnimationPlayer("fire");
            Pet.PetFireAnimationPlayer.Connect("animation_finished", new Callable(this, nameof(EndOnFire)));
            
            // 延迟
            await ToSignal(GetTree().CreateTimer(0.35f), "timeout");
            
            // 发送信号给子窗口
            EventBus.Publish(new PetStartFireEvent{ CurrentShells = Pet.CurrentShells });
        }
    }
    
    private void EndFire()
    {
        if (Pet.PetFireAnimationPlayer.IsConnected("animation_finished", new Callable(this, nameof(EndOnFire))))
        {
            Pet.PetFireAnimationPlayer.Disconnect("animation_finished", new Callable(this, nameof(EndOnFire)));
        }
    }
    
    private void EndOnFire(string animationName = null)
    {
        _isOnFire = false;
    }
    
    public override State Process(double delta)
    {
        if (_isOnFire == false)
        {
            return _idleState;
        }
        return null;
    }
    
    public override State PhysicsProcess(double delta)
    {
        return null;
    }
    
    public override State UnHandleInput(InputEvent _event)
    {
        return null;
    }
}