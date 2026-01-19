# Lys.AudioManager 最佳实践指南

本文档基于实际项目配置，介绍 AudioManager 的推荐目录结构和配置方式。

## 目录结构

```
Assets/GameRes/Audios/
├── AudioMixer.mixer              # Unity AudioMixer 配置
└── AudioClips/
    ├── AudioBanks/               # Bank 和 Group 配置文件
    │   ├── AudioBank_Music.asset
    │   ├── AudioBank_Sound.asset
    │   ├── AudioBank_UI.asset
    │   ├── AudioGroup_Music.asset
    │   ├── AudioGroup_Sound.asset
    │   └── AudioGroup_UI.asset
    ├── BGM/                      # 背景音乐文件夹
    │   ├── menu_music.wav
    │   ├── game_music.wav
    │   └── ...
    ├── Sounds/                   # 游戏音效文件夹
    │   ├── arrow_hit1.wav
    │   ├── sword_hit1.wav
    │   └── ...
    └── UIAudio/                  # UI 音效文件夹
        ├── CommonClick.wav
        ├── levelup.wav
        └── ...
```

## AudioBank 配置详解

### 1. AudioBank_Music (背景音乐)

| 配置项 | 推荐值 | 说明 |
|--------|--------|------|
| **缓存类型** | `按需加载 (OnDemand)` | BGM 文件较大，按需加载节省内存 |
| **循环播放** | `true` | 背景音乐通常需要循环 |
| **淡入时间** | `1.0s` | 平滑过渡，避免突兀 |
| **淡出时间** | `1.0s` | 切换 BGM 时平滑淡出 |
| **播放组** | `BgmGroup` | 使用互斥模式，确保同时只有一首 BGM |

**默认参数配置：**
```yaml
defaultParameters:
  volume: 1
  pitch: 1
  fadeInTime: 1        # 1秒淡入
  fadeOutTime: 1       # 1秒淡出
  loop: true           # 循环播放
  spatialBlend: 0      # 2D 音效
  priority: 128
```

**文件夹索引配置：**
- 文件夹路径: `Assets/GameRes/Audios/AudioClips/BGM`
- 包含子文件夹: `true`
- 所有音效设置播放组: `BgmGroup`

---

### 2. AudioBank_UI (UI 音效)

| 配置项 | 推荐值 | 说明 |
|--------|--------|------|
| **缓存类型** | `常驻内存 (Persistent)` | UI 音效频繁使用，常驻内存避免加载延迟 |
| **循环播放** | `false` | UI 音效通常是一次性播放 |
| **淡入时间** | `0` | UI 音效需要即时响应 |
| **淡出时间** | `0` | 无需淡出 |

**默认参数配置：**
```yaml
defaultParameters:
  volume: 1
  pitch: 1
  fadeInTime: 0        # 无淡入
  fadeOutTime: 0       # 无淡出
  loop: false          # 不循环
  spatialBlend: 0      # 2D 音效
  priority: 128
```

**文件夹索引配置：**
- 文件夹路径: `Assets/GameRes/Audios/AudioClips/UIAudio`
- 包含子文件夹: `true`

**事件名配置示例：**
可以为常用音效设置事件名，方便代码调用：
- `Adv_Go_Btn` → 事件名: `UI_BtnClick`

---

### 3. AudioBank_Sound (游戏音效)

| 配置项 | 推荐值 | 说明 |
|--------|--------|------|
| **缓存类型** | `预加载 (Preload)` | 战斗音效需要即时播放，进入场景时预加载 |
| **循环播放** | `false` | 音效通常是一次性播放 |
| **淡入时间** | `0` | 音效需要即时响应 |
| **淡出时间** | `0` | 无需淡出 |

**默认参数配置：**
```yaml
defaultParameters:
  volume: 1
  pitch: 1
  fadeInTime: 0
  fadeOutTime: 0
  loop: false
  spatialBlend: 0      # 可根据需要设置为 1 (3D 音效)
  priority: 128
```

**文件夹索引配置：**
- 文件夹路径: `Assets/GameRes/Audios/AudioClips/Sounds`
- 包含子文件夹: `true`

---

## AudioGroup 配置详解

### AudioGroup_Music

| 配置项 | 值 | 说明 |
|--------|-----|------|
| 组名称 | `AudioGroup_Music` | |
| Mixer Group | `Music` | 关联 AudioMixer 的 Music 通道 |
| 关联 Bank | `AudioBank_Music` | |
| 组音量 | `1.0` | |
| 最大同时播放数 | `0` (不限制) | 由播放组控制互斥 |

### AudioGroup_UI

| 配置项 | 值 | 说明 |
|--------|-----|------|
| 组名称 | `AudioGroup_UI` | |
| Mixer Group | `UI` | 关联 AudioMixer 的 UI 通道 |
| 关联 Bank | `AudioBank_UI` | |
| 组音量 | `1.0` | |
| 最大同时播放数 | `0` (不限制) | UI 音效可以叠加播放 |

### AudioGroup_Sound

| 配置项 | 值 | 说明 |
|--------|-----|------|
| 组名称 | `AudioGroup_Sound` | |
| Mixer Group | `Sound` | 关联 AudioMixer 的 Sound 通道 |
| 关联 Bank | `AudioBank_Sound` | |
| 组音量 | `1.0` | |
| 最大同时播放数 | `0` (不限制) | |

---

## 播放组配置

### BgmGroup (背景音乐互斥组)

| 配置项 | 值 | 说明 |
|--------|-----|------|
| 组名称 | `BgmGroup` | |
| 播放模式 | `互斥 (Exclusive)` | 同时只能播放一首 BGM |
| 互斥行为 | `淡出旧的 (FadeOutOld)` | 切换时淡出当前 BGM |

**使用方式：**
在 AudioBank_Music 的文件夹索引中，将所有 BGM 的 `playGroupName` 设置为 `BgmGroup`。

---

## 缓存类型选择指南

| 缓存类型 | 适用场景 | 内存占用 | 加载延迟 |
|----------|----------|----------|----------|
| **按需加载 (OnDemand)** | 大文件、不常用音效 | 低 | 首次播放有延迟 |
| **预加载 (Preload)** | 场景音效、战斗音效 | 中 | 无延迟 |
| **常驻内存 (Persistent)** | UI 音效、高频音效 | 高 | 无延迟 |

---

## 代码使用示例

### 初始化注册

```csharp
using Lys.Audio;

// 游戏启动时注册 UI 音效组（常驻内存）
AudioAPI.RegisterGroup(audioGroupUI);

// 进入场景时注册场景相关音效组
AudioAPI.RegisterGroup(audioGroupMusic);
AudioAPI.RegisterGroup(audioGroupSound);
```

### 播放音效

```csharp
// 播放 UI 音效
AudioAPI.Play2D("CommonClick");

// 通过事件名播放
AudioAPI.Play2D("UI_BtnClick");

// 播放背景音乐（自动互斥，淡入淡出）
AudioAPI.Play2D("menu_music");

// 切换背景音乐（旧的会自动淡出）
AudioAPI.Play2D("game_music");

// 播放 3D 音效
AudioAPI.PlayAt("arrow_hit1", targetPosition);
```

### 控制音效

```csharp
// 停止背景音乐（使用配置的淡出时间）
AudioAPI.Stop("menu_music");

// 立即停止
AudioAPI.Stop("menu_music", 0f);

// 指定淡出时间停止
AudioAPI.Stop("menu_music", 2f);

// 暂停/恢复所有音效
AudioAPI.PauseAll();
AudioAPI.ResumeAll();
```

---

## 配置检查清单

### 新建 AudioBank 时

- [ ] 设置合适的缓存类型
- [ ] 配置默认参数（循环、淡入淡出等）
- [ ] 添加文件夹索引或单个音效
- [ ] 扫描文件夹更新音效列表

### 新建 AudioGroup 时

- [ ] 设置组名称
- [ ] 关联 AudioMixer Group
- [ ] 添加相关的 AudioBank
- [ ] 配置组级别设置（音量、最大同时播放数等）

### 背景音乐配置

- [ ] 缓存类型设为 `按需加载`
- [ ] 默认参数开启 `循环播放`
- [ ] 设置 `淡入时间` 和 `淡出时间`
- [ ] 所有 BGM 设置相同的 `播放组`
- [ ] 播放组模式设为 `互斥`，行为设为 `淡出旧的`

### UI 音效配置

- [ ] 缓存类型设为 `常驻内存`
- [ ] 默认参数关闭 `循环播放`
- [ ] 淡入淡出时间设为 `0`
- [ ] 为常用音效设置 `事件名`

---

## 常见问题

### Q: 为什么 BGM 切换时没有淡出效果？

检查以下配置：
1. AudioBank_Music 的 `defaultParameters.fadeOutTime` 是否大于 0
2. BGM 是否都设置了相同的 `playGroupName`
3. 播放组的 `互斥行为` 是否设为 `淡出旧的`

### Q: UI 音效播放有延迟？

将 AudioBank_UI 的缓存类型改为 `常驻内存 (Persistent)`，确保音效在游戏启动时就加载到内存。

### Q: 如何让同一音效不重叠播放？

为该音效添加 `ConcurrentLimitCondition` 播放条件，设置 `maxConcurrent = 1`。
