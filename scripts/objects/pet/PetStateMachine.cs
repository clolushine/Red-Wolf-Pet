using Godot;
using System;
using System.Collections.Generic;
using desktoppet.scripts.states;

namespace desktoppet.scripts.objects.pet;


public partial class PetStateMachine : Node
{
	
	private static readonly List<State> States = new List<State>();
    
	private State _currentState = null;
	private State _prevState = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// ProcessModeEnum.Disabled表示该节点将不参与物理处理
		// 不接收_Process和_PhysicsProcess回调，但仍会渲染（如果可见）
		ProcessMode = ProcessModeEnum.Disabled;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ChangeState(_currentState.Process(delta));
	}

	public override void _PhysicsProcess(double delta)
	{
		ChangeState(_currentState.PhysicsProcess(delta));
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		ChangeState(_currentState.UnHandleInput(@event));
	}
	
	public void Initialize(Pet pet)
	{
		States.Clear();
		foreach (var n in GetChildren())
		{
			if (n is State state) States.Add(state);
		}
		if (States.Count == 0) return;
		foreach (var state in States)
		{
			state.Pet = pet;
			state.PetStateMachine = this;
			state.Init();
		}
		ChangeState(States[0]);
		
		ProcessMode = ProcessModeEnum.Inherit;
	}
    
	public void ChangeState(State newState)
	{
		if (newState == null || newState == _currentState) return;

		if (IsInstanceValid(_currentState))
		{
			_currentState.Exit();
		}
		
		_prevState = _currentState;
		_currentState = newState;
        
		_currentState.Enter();
	}
	
	public State GetCurrentState()
	{
		return _currentState;
	}
}
