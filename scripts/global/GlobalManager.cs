using Godot;
using System;
using desktoppet.scripts.objects;
using desktoppet.scripts.objects.effects;
using desktoppet.scripts.objects.pet;

namespace desktoppet.scripts.global;

public partial class GlobalManager : Node
{
	public static Pet Pet { get; set; }
	
	public static Effects Effects { get; set; }
	
	public static EffectsWindow EffectsWindow { get; set; }
	
	public static PopupPanelTip PopupPanelTip { get; set; }
	
	private static readonly int InitWindowHeight = 108;
	private static readonly int InitWindowWidth = 108;
	
	private static int _audioBusIndex;
	
	public override void _Ready()
	{
		// 获取总线的索引（默认"Master"总线索引为0）
		_audioBusIndex = AudioServer.GetBusIndex("Master");
		
		SetWindowAlwaysOnTop();
		
		SetAudioVolumeDb();
	}
	
	public static void SetWindowSize(int width, int height)
	{
		DisplayServer.WindowSetSize(new Vector2I(width,height));
		DisplayServer.WindowSetMinSize(new Vector2I(width,height));
	}
	
	public static void SetWindowAlwaysOnTop()
	{
		DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.AlwaysOnTop, true);
	}
	
	public static void ReSetWindowSize()
	{
		DisplayServer.WindowSetSize(new Vector2I(InitWindowWidth,InitWindowHeight));
		DisplayServer.WindowSetMinSize(new Vector2I(InitWindowWidth,InitWindowHeight));
	}
	
	
	public static void MoveWindow(int x, int y)
	{
		DisplayServer.WindowSetPosition(new Vector2I(x, y));
	}

	public static void SetAudioMute(bool mute = true)
	{
		// 静音总线
		AudioServer.SetBusMute(_audioBusIndex, mute);
	}
	
	public static void SetAudioVolumeDb(float volume = 0.6f)
	{
		// 设置音量（线性模式 0.0-1.0）
		float dbVolume = Mathf.LinearToDb(volume);
		// 设置音量总线
		AudioServer.SetBusVolumeDb(_audioBusIndex, dbVolume);
	}
	
	public static void ShowWindowTip(string tip = default, Vector2I position = default)
	{
		if (tip != default) PopupPanelTip.SetLabel(tip);
		
		// 获取窗口位置
		// 获取窗口信息
		Vector2I windowPos = DisplayServer.WindowGetPosition();
		Vector2I windowSize = DisplayServer.WindowGetSize();

		Vector2I targetPos;
		
		if (position == default)
		{
			// 计算居中上方位置
			targetPos = new Vector2I(
				windowPos.X + (windowSize.X - PopupPanelTip.Size.X) / 2,
				windowPos.Y - PopupPanelTip.Size.Y - 10  // 上方留10像素边距
			);
		}
		else
		{
			targetPos = position;
		}
		
		PopupPanelTip.SetPosition(targetPos);
		
		PopupPanelTip.Popup();
	}

}
