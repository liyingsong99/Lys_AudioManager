# Lys.AudioManager

Unity 音效管理器包，提供完整的音效管理功能，包括 AudioBank 管理、AudioGroup 配置、可扩展的播放条件系统、以及可视化编辑器界面。

## 安装方式

### 通过 Unity Package Manager (Git URL)

1. 打开 Unity 编辑器
2. 菜单 `Window -> Package Manager`
3. 点击左上角 `+` 按钮，选择 `Add package from git URL...`
4. 输入以下地址：
```
https://github.com/liyingsong/Lys_AudioManager.git?path=cn.lys.audiomanager
```

### 手动安装

将 `cn.lys.audiomanager` 文件夹复制到项目的 `Packages` 目录下。

## 依赖项

### 必需依赖

- **YooAsset** (2.0.0+) - 资源加载框架
  - 可通过 UPM 安装：`https://github.com/tuyoogame/YooAsset.git`
  - 或使用其他资源加载方式（需实现 `IAudioAssetLoader` 接口）

### 可选依赖

- **Odin Inspector** - 用于增强编辑器体验
  - 如果没有安装 Odin，部分编辑器功能可能受限
  - 推荐安装以获得最佳编辑体验

## 功能特性

- **AudioBank**: 音效库管理，支持单个音效和文件夹索引批量分类
- **AudioGroup**: 音效组配置，每个组关联一个或多个 AudioBank，支持按需加载和缓存类型配置
- **AudioMixerGroup**: 每个 AudioGroup 可配置默认的 AudioMixerGroup
- **播放组系统**: 集中管理音效播放组
  - 随机模式：从组内随机选择一个音效播放
  - 顺序模式：按顺序循环播放组内音效
  - 互斥模式：同组只能播放一个音效
- **播放条件系统**: 可扩展的播放条件系统
  - ConcurrentLimitCondition: 同时播放上限
  - CooldownCondition: 冷却时间条件
  - ProbabilityCondition: 概率播放条件
  - DistanceCondition: 距离条件
- **PlayAudioEvent 组件**: 支持 GameObject 生命周期控制的音效播放
- **对象池**: AudioSource 对象池管理，减少 GC
- **可视化编辑器**: 四面板布局的音效编辑器界面

## 快速开始

### 1. 创建 AudioBank

在 Project 窗口右键：`Create -> Audio -> Audio Bank`

### 2. 创建 AudioGroup

在 Project 窗口右键：`Create -> Audio -> Audio Group`

### 3. 注册资源

```csharp
using Lys.Audio;

// 注册 AudioGroup（推荐，会自动注册关联的所有 Bank）
AudioAPI.RegisterGroup(myAudioGroup);

// 或单独注册 AudioBank
AudioAPI.RegisterBank(myAudioBank);
```

### 4. 播放音效

```csharp
using Lys.Audio;

// 播放 2D 音效
var instance = AudioAPI.Play2D("ui_click");

// 在指定位置播放 3D 音效
var instance = AudioAPI.PlayAt("explosion", transform.position);

// 播放跟随目标的音效
var instance = AudioAPI.PlayFollow("footstep", characterTransform);

// 停止音效
AudioAPI.Stop("bgm");
AudioAPI.StopAll();

// 暂停/恢复
AudioAPI.Pause("bgm");
AudioAPI.Resume("bgm");
```

### 5. 打开音效编辑器

菜单：`Tools -> Audio -> Audio Editor`

## 自定义资源加载器

如果不使用 YooAsset，可以实现 `IAudioAssetLoader` 接口：

```csharp
public class MyCustomAudioLoader : IAudioAssetLoader
{
    public void LoadClipAsync(string assetPath, Action<AudioClip> onComplete) { ... }
    public AudioClip LoadClipSync(string assetPath) { ... }
    public void UnloadClip(string assetPath) { ... }
    public void UnloadAll() { ... }
    public bool IsLoaded(string assetPath) { ... }
    public AudioClip GetLoadedClip(string assetPath) { ... }
}
```

## 目录结构

```
Runtime/
├── Core/               # 核心组件
├── Bank/               # 音效库
├── Group/              # 音效组
├── Condition/          # 播放条件
├── Parameter/          # 参数配置
├── Loader/             # 资源加载
├── Components/         # 组件
└── API/                # 对外 API

Editor/
├── Windows/            # 编辑器窗口
├── Inspectors/         # Inspector 扩展
├── Preview/            # 预览功能
└── Utils/              # 工具类
```

## 版本历史

### 1.0.0

- 初始版本
- 完整的 AudioBank/AudioGroup 管理
- 可扩展的播放条件系统
- 可视化音效编辑器
- YooAsset 集成

## 作者

liyingsong

## 许可证

MIT License
