using Godot;
using System;
using desktoppet.scripts.events;
using global::desktoppet.scripts.global;

namespace desktoppet.scripts.objects.effects;

public partial class Effects : Node2D
{
	[Export]
	public AudioStream BigFireBoomSound; // 大爆炸音效
	
	// 精灵
	public Sprite2D EffectSprite;
	// 动画
	public AnimationPlayer EffectAnimationPlayer;
	
	public AudioStreamPlayer EffectAudioPlayer;
	
	// 效果
	private static EffectsManager _effectsManager;
	private Effect _noneEffect;
	private Effect _bigFireBoomEffect;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		EffectSprite = GetNode<Sprite2D>("Sprite2D");
		EffectAnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		EffectAudioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		
		_effectsManager = GetNode<EffectsManager>("effectsmanager");
		_noneEffect = GetNode<Effect>("effectsmanager/none");
		_bigFireBoomEffect = GetNode<Effect>("effectsmanager/bigfireboom");
		
		_Init();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void _Init()
	{
		// 全局引用
		GlobalManager.Effects = this;
		
		_effectsManager.Initialize(this);
		
		// 事件监听
		EventBus.Subscribe<PetStartFireEvent>(OnPetStartFire);
	}
	
	public override void _ExitTree()
	{
		EventBus.Unsubscribe<PetStartFireEvent>(OnPetStartFire);
	}

	private void OnPetStartFire(PetStartFireEvent e)
	{
		// 爆炸效果
		_effectsManager.StartEffect(_bigFireBoomEffect);
	}

	public AnimationPlayer GetCopyPlayer()
	{
		var copyEffectAnimationPlayer = (AnimationPlayer)EffectAnimationPlayer.Duplicate();
		
		CallDeferred("add_child",copyEffectAnimationPlayer);

		return copyEffectAnimationPlayer;
	}
	
	private void ShowEffectWindow()
	{
		// 计算动画精灵的步长
		var w = (int)(EffectSprite.Texture.GetWidth() / EffectSprite.Vframes * Scale.X);
		var h = (int)(EffectSprite.Texture.GetHeight() / EffectSprite.Hframes * Scale.Y);

		GlobalManager.EffectsWindow.SetSize(new Vector2I(w, h));
		
		// 3. 获取“宠物”主窗口的当前信息
		Vector2I petWindowPosition = DisplayServer.WindowGetPosition(); // 主窗口的左上角
		Vector2I petWindowSize = DisplayServer.WindowGetSize();         // 主窗口的尺寸
		
		// 计算“宠物”主窗口的中心点
		// 这是你的“精灵”的视觉中心点
		Vector2 petWindowCenter = petWindowPosition + (Vector2)petWindowSize / 2f;
        
		// 4. 获取宠物朝向的标准化向量
		Vector2 normalizedDir = GlobalManager.Pet.GetPetDirection().Normalized(); // 确保 GetPetDirection() 返回的是 Vector2
		
				
		// 5. 计算特效窗口的中心点目标位置
		float distanceInFront = 256f; 
		Vector2 targetEffectCenterScreenPosition = petWindowCenter; // 初始目标点为宠物中心

		// 根据主导方向，只在那个轴上位移，强制对齐精灵的中心线
		if (Mathf.Abs(normalizedDir.X) > Mathf.Abs(normalizedDir.Y)) 
		{   // 主要为水平移动 (左/右)
			targetEffectCenterScreenPosition.X += Mathf.Sign(normalizedDir.X) * (distanceInFront + w / 2) ;
			// 调整 Y 值：因为特效从底部向上变化，Y 需要向上移动半个身高，以使底部近似对齐中心。
			targetEffectCenterScreenPosition.Y -= (h / 2f); 
		}
		else // 主要为垂直移动 (上/下) 或对角线
		{   
			targetEffectCenterScreenPosition.Y += Mathf.Sign(normalizedDir.Y) * distanceInFront;
			// X 轴保持与宠物中心线对齐 (即 X 坐标不变)
		}
        
		// 6. 计算特效窗口的左上角位置
		// 因为 GlobalManager.EffectsWindow.SetPosition 设置的是窗口的左上角，
		// 所以我们需要从目标中心点减去特效窗口自身大小的一半。
		Vector2 effectWindowVisualSize = new Vector2(w, h); 
		Vector2 topLeftScreenPosition = targetEffectCenterScreenPosition - (effectWindowVisualSize / 2.0f);
        
		// 7. 设置特效窗口的位置
		GlobalManager.EffectsWindow.SetPosition(
			new Vector2I((int)topLeftScreenPosition.X, (int)topLeftScreenPosition.Y));
	}
}
