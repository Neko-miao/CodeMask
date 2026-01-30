===============================================
GameFramework - Unity游戏框架
===============================================

一、框架概述
--------------------------------------------
GameFramework是一个完整的Unity游戏开发框架，提供了游戏开发所需的基础功能和架构支持。

主要特性：
- 统一的组件化架构（所有Mgr和Mdl继承自GameComponent）
- 接口与实现分离设计
- 游戏状态管理
- 时间控制系统
- 单局/关卡/规则系统
- MVC UI框架
- Entity组件系统

二、目录结构
--------------------------------------------
Assets/
├── GameFramework/
│   ├── Core/                    # 核心系统
│   │   ├── GameInstance.cs      # 游戏实例总控制器
│   │   ├── GameLauncher.cs      # 游戏启动器
│   │   ├── GameState.cs         # 游戏状态定义
│   │   ├── Component/           # 组件系统
│   │   ├── Time/                # 时间控制器
│   │   ├── World/               # 世界上下文
│   │   └── Session/             # 单局系统
│   ├── Components/
│   │   └── Managers/            # 管理器组件
│   │       ├── Event/           # 事件系统
│   │       ├── Timer/           # 定时器系统
│   │       ├── Resource/        # 资源加载
│   │       ├── Config/          # 配置系统
│   │       ├── Audio/           # 音频系统
│   │       ├── Input/           # 输入系统
│   │       ├── Pool/            # 对象池
│   │       ├── Log/             # 日志系统
│   │       └── UI/              # UI管理
│   ├── UI/                      # UI框架
│   │   └── Core/                # MVC基类
│   └── Entity/                  # 实体框架
│       ├── Core/                # 实体核心
│       └── Components/          # 实体组件
│
└── Game/                        # 游戏逻辑
    ├── Modules/                 # 功能模块
    └── Events/                  # 事件定义

三、快速开始
--------------------------------------------
1. 创建场景并添加一个空GameObject
2. 挂载GameLauncher组件（或继承它创建自定义启动器）
3. 运行游戏

示例代码：

// 获取管理器
var eventMgr = GameInstance.Instance.GetComp<IEventMgr>();
var audioMgr = GameInstance.Instance.GetComp<IAudioMgr>();

// 订阅事件
eventMgr.Subscribe<PlayerDeadEvent>(OnPlayerDead);

// 发布事件
eventMgr.Publish(new PlayerDeadEvent { PlayerId = 1 });

// 播放音效
audioMgr.PlaySFX("Hit");

// 使用定时器
var timerMgr = GameInstance.Instance.GetComp<ITimerMgr>();
timerMgr.Schedule(2f, () => Debug.Log("2秒后执行"));

四、扩展框架
--------------------------------------------
1. 创建自定义模块：

// 接口定义
public interface IMyMdl : IGameComponent
{
    void DoSomething();
}

// 实现
[ComponentInfo(Type = ComponentType.Module, Priority = 200, RequiredStates = new[] { GameState.Playing })]
public class MyMdl : GameComponent, IMyMdl
{
    public void DoSomething() { }
}

2. 注册模块（在GameLauncher子类中）：

protected override void RegisterCustomComponents(IComponentRegistry registry)
{
    registry.RegisterForState<IMyMdl, MyMdl>(GameState.Playing, priority: 200);
}

五、命名规范
--------------------------------------------
- 管理器: XXXMgr (如 EventMgr, AudioMgr)
- 功能模块: XXXMdl (如 PlayerMdl, BagMdl)
- 接口: IXXXMgr / IXXXMdl
- 实体组件: XXXComp (如 HealthComp, MoveComp)

六、组件生命周期
--------------------------------------------
OnRegister() → OnInit() → OnStart() → [OnTick/OnFixedTick] → OnShutdown() → OnUnregister()

七、单局系统使用
--------------------------------------------
// 开始单局
var session = GameInstance.Instance.GetComp<IGameSession>();

var config = new SessionConfig
{
    StartLevelId = 1,
    Rules = new ISessionRule[]
    {
        new TimeLimitRule(300f),
        new ScoreTargetRule(10000),
        new LivesLimitRule(3)
    }
};

await session.StartSession(config);

// 监听单局事件
session.OnRuleTriggered += rule =>
{
    if (rule is TimeLimitRule)
        session.EndSession(SessionEndReason.TimeUp);
};

八、联系方式
--------------------------------------------
如有问题，请联系框架维护者。

===============================================

