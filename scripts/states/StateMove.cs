using Godot;
using desktoppet.scripts.global;

namespace desktoppet.scripts.states;

public partial class StateMove : State
{
    private State _idleState;
    private State _fireState;
    
    private bool _isOnMove;
    
    // 定义基础速度单位：速度等级 1 对应的每秒移动像素数
    private const float BaseSpeedUnit = 16.0f; // 例如：16 像素/秒
    
    // !!! 关键：_currentExactPosition 必须是静态的，并且在 Enter 或 Ready 中初始化一次 !!!
    private Vector2 _currentExactPosition; 
    
    // 用于记录上一次实际应用到窗口的整数位置，用于避免重复设置相同位置
    private Vector2I _lastAppliedPosition; 
    
    public override void _Ready()
    {
        _idleState = GetNode<State>("../idle");
        _fireState = GetNode<State>("../fire");
    }

    public override void Enter()
    {
        StartMove();
        
        // !!! 关键：_currentExactPosition 在进入移动状态时，只初始化一次 !!!
        // 它会从窗口当前实际的整数位置开始，但之后会以浮点数形式累积
        _currentExactPosition = (Vector2)DisplayServer.WindowGetPosition();
        
        // 同时初始化 _lastAppliedPosition 为当前窗口的整数位置
        _lastAppliedPosition = DisplayServer.WindowGetPosition();
    }
    
    public override void Exit()
    {
        EndMove();
    }
    
    private void StartMove()
    {
        _isOnMove = true;
        Pet.UpdateAnimationPlayer("move");
        Pet.PlayPetMoveSound();
        
        // 隐藏提示窗
        GlobalManager.PopupPanelTip.Hide();
    }
    
    private void EndMove()
    {
        _isOnMove = false;
        Pet.StopPetMoveSound();
    }
    
    
    public override State Process(double delta)
    {
        // 执行移动
        MoveProcess(delta);
        
        return null;
    }

    private void MoveProcess(double delta)
    {
        if (_isOnMove && Pet.GetPetDirection() != Vector2.Zero)
        {
            // 1. 获取归一化的方向向量
            // 确保 Pet.PetDirection 是 Vector2。如果它被定义为 Vector2I，这里强制转换。
            Vector2 normalizedDirection = ((Vector2)Pet.GetPetDirection()).Normalized(); 
        
            // 2. 根据 Pet.MoveSpeed（速度等级）计算实际的每秒移动像素数
            // 假设 Pet.MoveSpeed 是 int 类型，表示速度等级 (1, 2, 3...)
            float actualPixelsPerSecond = Pet.MoveSpeed * BaseSpeedUnit; // 例如：2 * 16 = 32 像素/秒
        
            // 3. 计算本帧应该移动的精确像素量
            // 将 "每秒移动像素数" 乘以 "本帧持续的秒数 (delta)"，得到本帧的实际位移
            Vector2 movementThisFrame = normalizedDirection * actualPixelsPerSecond * (float)delta;
        
            // 4. 将本帧的移动量累加到精确浮点位置上
            // _currentExactPosition 是 Vector2，movementThisFrame 也是 Vector2，所以这里可以正常相加。
            _currentExactPosition += movementThisFrame;
            
            // --- 新增：边界检查（Clamp）逻辑 ---

            // 1. 获取屏幕尺寸（主屏幕）
            // 如果你的游戏需要支持多屏幕，这里需要更复杂的逻辑来获取所有屏幕的边界
            int currentScreen = DisplayServer.WindowGetCurrentScreen(); 
            
            Vector2I screenSize =  DisplayServer.ScreenGetUsableRect(currentScreen).Size; // 1. 获取窗口当前在哪个屏幕
        
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
            _currentExactPosition.X = Mathf.Clamp(_currentExactPosition.X, minX, maxX);
            _currentExactPosition.Y = Mathf.Clamp(_currentExactPosition.Y, minY, maxY);

            // --- 边界检查逻辑结束 ---
        
            // 计算新的目标整数位置
            Vector2I newTargetPosition = new Vector2I(
                (int)Mathf.Round(_currentExactPosition.X),
                (int)Mathf.Round(_currentExactPosition.Y)
            );
        
            // 5. 将精确浮点位置转换为整数像素，并移动窗口
            // !!!! 关键：只有当新的目标整数位置与上次应用的整数位置不同时才移动窗口 !!!!
            if (newTargetPosition != _lastAppliedPosition)
            {
                GlobalManager.MoveWindow(newTargetPosition.X, newTargetPosition.Y);
                _lastAppliedPosition = newTargetPosition; // 更新上次应用的位置
            }
        }
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