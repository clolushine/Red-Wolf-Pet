using System;
using System.Collections.Generic;
using Godot;
using desktoppet.scripts.global;
using desktoppet.scripts.states;

namespace desktoppet.scripts.objects.pet;

public partial class Pet : CharacterBody2D
{
	[Export]
	private AudioStream _petMoveSound; // 移动音效
	
	// 属性相关
	[Export(PropertyHint.Range,"16,128,1")]
	public int MaxShells { get; set; } = 32; // 最大炮弹数量
	
	// 属性相关
	[Export(PropertyHint.Range,"1,32,1")]
	public int MoveSpeed { get; set; } = 12; // 移动速度
	
	public int CurrentShells; // 当前炮弹数量

	private bool _isRefillShellsWait = false; // 是否在填充炮弹冷却时间
	
	// 行为相关，新增随机移动相关变量
	private float _moveTimer = 0f;
	private float _idleTimer = 0f;
	// 移动持续时间
	private float _moveDuration = 2f;
	// 静止持续时间
	private float _idleDuration = 3f;
	// 使用指数曲线使音高变化更自然
	private float _basePitch = 0.8f;
	
	private Random _random = new Random();
	private List<Vector2> _possibleDirections = new List<Vector2>
	{
		Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right
	};
	
	private bool _isDragging = false;
	private Vector2 _dragStartPosition;
	
	// 检测点击是否在精灵范围内
	private Sprite2D _petSprite;
	// 碰撞体
	private CollisionShape2D _petCollision;
	// 点击穿透区域
	// private Polygon2D _petPolygon;
	
	// 状态机
	private PetStateMachine _petStateMachine;
	private static State _idleState;
	private static State _moveState;
	private static State _fireState;
	
	public static Vector2 PetDirection = Vector2.Zero;
	
	public AnimationPlayer PetAnimationPlayer;
	
	public AnimationPlayer PetFireAnimationPlayer;
	
	public AudioStreamPlayer PetAudioStreamPlayer;


	public Pet()
	{
		// 炮弹数量
		CurrentShells = MaxShells;
	}
	
	public override void _Ready()
	{
		_petSprite = GetNode<Sprite2D>("vehicle"); // 目标精灵
		_petCollision = GetNode<CollisionShape2D>("CollisionShape2D"); // 碰撞体
		// _petPolygon = GetNode<Polygon2D>("Polygon2D"); // 
		_petStateMachine = GetNode<PetStateMachine>("statemachine"); // 状态机
		_idleState = GetNode<State>("statemachine/idle");
		_moveState = GetNode<State>("statemachine/move");
		_fireState = GetNode<State>("statemachine/fire");
		
		PetAnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		PetAudioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		PetFireAnimationPlayer = GetNode<AnimationPlayer>("vehicle/fireEffect/AnimationPlayer");
		
		SetProcessInput(true); // 启用输入
		
		// 设置光标为手型
		Input.SetDefaultCursorShape(Input.CursorShape.PointingHand);
		
		_Init(); // 执行初始化
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// 检查炮弹是否在等待填充
		CheckAndRefillShells();
		
		// 执行随机移动
		HandleRandomMovement((float)delta);
	}
	
	private void _Init()
	{
		// 全局引用
		GlobalManager.Pet = this;
		// 初始化状态机
		_petStateMachine.Initialize(this);
		// 设置点击穿透点位
		// SetMousePassthroughPoints();
	}

	// private void SetMousePassthroughPoints()
	// {
	// 	// 获取全局变换矩阵
	// 	Transform2D globalTransform = GetGlobalTransform();
	// 	// 转换多边形坐标
	// 	Vector2[] transformedPoints = new Vector2[_petPolygon.Polygon.Length];
	// 	for (var i = 0; i < _petPolygon.Polygon.Length; i++)
	// 	{
	// 		// 应用缩放、旋转和位移
	// 		transformedPoints[i] = globalTransform * _petPolygon.Polygon[i];
	// 	}
	// 	// 设置点击穿透
	// 	GetWindow().SetMousePassthroughPolygon(transformedPoints);
	// }
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			// 停止精灵移动
			if (_petStateMachine.GetCurrentState() == _moveState)
			{
				_petStateMachine.ChangeState(_idleState);
			}
			
			if (mouseEvent.ButtonIndex == MouseButton.Left)
			{
				if (mouseEvent.Pressed)
				{
					if (_petSprite != null && IsClickOnSprite(mouseEvent.Position))
					{
						_isDragging = true;
						// 跟随鼠标移动
						_dragStartPosition = DisplayServer.MouseGetPosition();
					}
				}
				else
				{
					_isDragging = false;
				}
			}
			else if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				// 鼠标右键点击开炮
				if (GlobalManager.Pet.CurrentShells <= 0)
				{
					GlobalManager.ShowWindowTip(GetWindow(),default, default);
				}
				if (_petStateMachine.GetCurrentState() == _idleState)
				{
					_petStateMachine.ChangeState(_fireState);
				}
				
			}
		}
		
		if (_isDragging && @event is InputEventMouseMotion && canMoveWindow())
		{
			// 计算新窗口位置
			Vector2 currentPos = DisplayServer.MouseGetPosition();
			Vector2 delta = currentPos - _dragStartPosition;
			Vector2 newWindowPos = DisplayServer.WindowGetPosition()+ delta;
			
			// --- 新增：边界检查（Clamp）逻辑 ---

			// 1. 获取屏幕尺寸（主屏幕）
			// 如果你的游戏需要支持多屏幕，这里需要更复杂的逻辑来获取所有屏幕的边界
			Vector2I screenSize = DisplayServer.ScreenGetUsableRect().Size; 
        
			// 2. 获取宠物窗口的尺寸
			Vector2I windowSize = DisplayServer.WindowGetSize();
        
			// 3. 计算窗口的最小允许 XY 坐标（通常是 0,0）
			float minX = 0.0f;
			float minY = 0.0f;

			// 4. 计算窗口的最大允许 XY 坐标（这确保窗口的右下角不会超出屏幕）
			// 最大X = 屏幕宽度 - 窗口宽度
			// 最大Y = 屏幕高度 - 窗口高度
			float maxX = screenSize.X - windowSize.X;
			float maxY = screenSize.Y - windowSize.Y;
        
			// 5. 使用 Mathf.Clamp 限制 _currentExactPosition 在有效范围内
			newWindowPos.X = Mathf.Clamp(newWindowPos.X, minX, maxX);
			newWindowPos.Y = Mathf.Clamp(newWindowPos.Y, minY, maxY);
			
			var newPosition = new Vector2I((int)newWindowPos.X, (int)newWindowPos.Y);
			
			// 转换为整数坐标，应用新位置
			GlobalManager.MoveWindow(newPosition);
			// 更新位置
			_dragStartPosition = currentPos;
		}
	}
	
	private bool IsClickOnSprite(Vector2 mouseGlobalPos)
	{
		if (_petSprite == null || _petSprite.Texture == null) 
			return false;
            
		Transform2D transform = _petSprite.GetGlobalTransform();
		Vector2 textureSize = _petSprite.Texture.GetSize() * _petSprite.Scale;
		Rect2 rect = new Rect2(-textureSize/2, textureSize);
		Vector2 localPos = transform.AffineInverse() * mouseGlobalPos;
        
		return rect.HasPoint(localPos);
	}

	private static bool canMoveWindow()
	{
		return DisplayServer.GetName() != "headless" && DisplayServer.GetName() != "HTML5";
	}
	
	public void UpdateAnimationPlayer(string animation)
	{
		PetAnimationPlayer.Play(animation + "_" + PetAnimationDirection());
	}
	
	public void UpdateFireAnimationPlayer(string animation)
	{
		PetFireAnimationPlayer.Play(animation + "_" + PetAnimationDirection());
	}
	
	public void PlayPetMoveSound()
	{
		if (_petMoveSound != null && !PetAudioStreamPlayer.Playing)
		{
			// 设置音频流循环属性
			if (_petMoveSound is AudioStreamWav wav)
			{
				wav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
			}
			else if (_petMoveSound is AudioStreamOggVorbis ogg)
			{
				ogg.Loop = true;
			}
			else if (_petMoveSound is AudioStreamMP3 mp3)
			{
				mp3.Loop = true;
			}
			
			PetAudioStreamPlayer.Stream = _petMoveSound;
			PetAudioStreamPlayer.PitchScale = _basePitch * Mathf.Pow(1.25f / _basePitch, (MoveSpeed - 1) / 15f);
			PetAudioStreamPlayer.Play();
		}
	}
	
	public void StopPetMoveSound()
	{
		if (_petMoveSound != null && PetAudioStreamPlayer.Playing)
		{
			PetAudioStreamPlayer.Stop();
		}
	}
	
	private void HandleRandomMovement(float delta)
	{
		if (!_isDragging)
		{
			State currentState = _petStateMachine.GetCurrentState();
			if (currentState == _moveState)
			{
				_moveTimer += delta;
				if (_moveTimer >= _moveDuration)
				{
					StopMoving();
					_moveTimer = 0f;
				}
			}
			else if (currentState == _idleState)
			{
				_idleTimer += delta;
				if (_idleTimer >= _idleDuration)
				{
					StartRandomMove();
					_idleTimer = 0f;
				}
			}
		}
	}
	
	private void StartRandomMove()
	{
		// 随机选择方向
		int index = _random.Next(_possibleDirections.Count);
		PetDirection = _possibleDirections[index];
        
		// 随机移动持续时间 (1-3秒)
		_moveDuration = (float)(1 + _random.NextDouble() * 2);

		_petStateMachine.ChangeState(_moveState);
	}

	private void StopMoving()
	{
		// 随机静止持续时间 (2-5秒)
		_idleDuration = (float)(2 + _random.NextDouble() * 3);
		
		_petStateMachine.ChangeState(_idleState);
	}
	
	private string PetAnimationDirection()
	{
		Dictionary<Vector2, string> directionMap = new()
		{
			[Vector2.Up] = "up",
			[Vector2.Down] = "down",
			[Vector2.Right] = "right",
			[Vector2.Left] = "left"
		};
		return directionMap.GetValueOrDefault(PetDirection, "up");
	}

	public Vector2 GetPetDirection()
	{
		return PetDirection == Vector2.Zero ? Vector2.Up : PetDirection;
	}
	
	private async void CheckAndRefillShells()
	{
		if (CurrentShells <= 0 && !_isRefillShellsWait)
		{
			_isRefillShellsWait = true;
			
			// 等待10分钟（600秒）
			await ToSignal(GetTree().CreateTimer(600), "timeout");
            
			// 恢复原始数量
			CurrentShells = MaxShells;

			_isRefillShellsWait = false;
		}
	}
}
