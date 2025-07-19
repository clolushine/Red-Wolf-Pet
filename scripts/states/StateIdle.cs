using global::desktoppet.scripts.global;
using Godot;

namespace desktoppet.scripts.states;

public partial class StateIdle : State
{
    private State _moveState;
    private State _fireState;
    
    public override void _Ready()
    {
        _moveState = GetNode<State>("../move");
        _fireState = GetNode<State>("../fire");
    }

    public override void Enter()
    {
        Pet.UpdateAnimationPlayer("idle");
    }
    
    public override void Exit()
    {

    }
    
    public override State Process(double delta)
    {
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