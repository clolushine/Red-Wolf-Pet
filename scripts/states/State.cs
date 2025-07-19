using Godot;
using System;
using desktoppet.scripts.objects.pet;

namespace desktoppet.scripts.states;


public partial class State : Node
{
	public Pet Pet;
	
	public PetStateMachine PetStateMachine;
	
	public virtual void Init()
	{
	}
    
	public virtual void Enter()
	{
	}
    
	public virtual void Exit()
	{
	}
    
	public virtual State Process(double delta)
	{
		return null;
	}
    
	public virtual State PhysicsProcess(double delta)
	{
		return null;
	}
    
	public virtual State UnHandleInput(InputEvent _event)
	{
		return null;
	}
}
