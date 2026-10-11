<div align="center">

# YueToJapanesePhonemizer

**让日语音源唱粤语**

[![OpenUtau](https://img.shields.io/badge/OpenUtau-v0.1.570%2B-blue)](https://github.com/stakira/OpenUtau)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)
[![Version](https://img.shields.io/badge/version-v1.0.0-orange)](CHANGELOG.md)

一个 OpenUtau 音素器插件，把粤语歌词自动转换为日语 mora 序列，
让任何日语音源（CV / VCV / CVVC）直接演唱粤语歌曲。

</div>

---

## ✨ 特性

- **粤语音系** —— 完整支持 Jyutping 声母、韵母、声调
  - 19 个声母（含 `ng` / `gw` / `kw`）
  - 50+ 个韵母（含 `aa` / `oeng` / `eoi` / `eon` 等粤语特有声韵）
  - 入声韵尾 `-p` / `-t` / `-k`
- **六声调** —— 粤拼尾部数字 1–6，影响末尾 mora 时长
- **粤语数字读法** —— `jat / ji / saam / sei / ng / luk / cat / baat / gau`
- **日期时间识别** —— `2024-01-15`、`12:30`、`3.14`、`50%`、`1,000`
- **YAML 配置** —— 自定义映射、多音字修正、别名覆盖
- **鼻音三档处理** —— `none` / `short` / `full`
- **通配符 `*`** —— 改善 Defoko 等纯 CV 音源的连音
- **离线编辑器** —— 单文件 HTML，可视化调参（[`tools/yue2ja-editor.html`](tools/yue2ja-editor.html)）

---

## 📋 系统要求

| 项目 | 要求 |
| ---- | ---- |
| OpenUtau | v0.1.570 或更高（官方 win-x64 编译版） |
| 操作系统 | Windows 10 / 11（x64） |
| 音源 | 任何日语音源（CV、VCV、CVVC） |
| 运行时 | .NET 10 Desktop Runtime（官方 OpenUtau 已自带） |

> 本插件针对 OpenUtau 官方 .NET 10 版本编译。旧版 OpenUtau（.NET 8 时代）无法加载。

---

## 📦 安装

### 方式一：安装包（推荐）

1. 从 [Releases](https://github.com/styisme/openutau-yue-to-ja/releases) 下载 `MyYUtoJAPlugin-v1.0.0-Setup.exe`
2. 双击运行，按提示完成
3. 安装程序会自动探测 OpenUtau 的 Plugins 目录

### 方式二：手动

1. **完全关闭 OpenUtau**（任务管理器确认无 `OpenUtau.exe`）
2. 复制 `MyYUtoJAPlugin.dll` 到：
   ```
   <文档目录>\OpenUtau\Plugins\MyYUtoJAPlugin.dll
   ```
3. （可选）复制 `yue2ja.yaml` 到同一目录
4. 启动 OpenUtau → 音轨设置 → 音素器 → 选择 **"YUE to JA"**

> ⚠️ OpenUtau 运行时 DLL 会被锁定，必须完全关闭后再复制。

---

## 🎤 使用方法

### 1. 输入粤语歌词

在钢琴卷帘中双击音符，直接输入汉字：

| 音符 | 歌词 |
| ---- | ---- |
| 1 | 你 |
| 2 | 好 |
| 3 | 世 |
| 4 | 界 |

**一个音符 = 一个字**，不要把一句话塞进同一个音符。

### 2. 也可以用粤拼输入

多音字读错时，直接写粤拼更可靠：

```
nei5  hou2  sai3  gaai3
```

声调数字可省略：`nei hou sai gaai`

### 3. 特殊符号

| 符号 | 作用 |
| ---- | ---- |
| `-` | 起始标记（放在句首音符） |
| `R` | 尾音收尾 |
| `+` | 延音 |
| `吸入` | 换气声 |

### 4. 多音字处理

右键音符 → **音素提示** → 输入粤拼，覆盖默认转换。

### 5. 更换 DLL / YAML 后必做

1. 完全关闭 OpenUtau
2. 复制新文件
3. **清空** `<文档目录>\OpenUtau\Cache\`
4. 重启 OpenUtau 重新渲染

---

## 🗣️ 粤拼速查表

### 声母

| 粤拼 | 近似发音 | 例字 |
| ---- | -------- | ---- |
| `b` `p` `m` `f` | 爸 怕 妈 花 | baa paa maa faa |
| `d` `t` `n` `l` | 打 他 那 啦 | daa taa naa laa |
| `g` `k` `h` | 家 卡 哈 | gaa kaa haa |
| `gw` `kw` `w` | 瓜 夸 蛙 | gwaa kwaa waa |
| `z` `c` `s` | 渣 叉 沙 | zaa caa saa |
| `j` | 也 | jaa |
| `ng` | 牙 | ngaa |

### 韵母

| 粤拼 | 例字 | 粤拼 | 例字 |
| ---- | ---- | ---- | ---- |
| `aa` | 沙 saa | `a` | 心 sam |
| `e` | 写 se | `i` | 诗 si |
| `o` | 梳 so | `u` | 夫 fu |
| `yu` | 书 syu | `oe` | 靴 hoe |
| `eo` | 需 seoi | `ai` | 大 daai |
| `au` | 走 zau | `ei` | 你 nei |
| `ou` | 好 hou | `iu` | 小 siu |
| `ui` | 水 seoi | `aai` | 街 gaai |
| `aau` | 搞 gaau | `oeng` | 香 hoeng |
| `eon` | 春 ceon | `ing` | 星 sing |
| `ung` | 中 zung | `yun` | 冤 jyun |

### 声调

| 数字 | 调值 | 例字 | 时长效果 |
| ---- | ---- | ---- | -------- |
| 1 | 高平 | 诗 si1 | 正常 |
| 2 | 高升 | 史 si2 | 略短 |
| 3 | 中平 | 试 si3 | 正常 |
| 4 | 低降 | 时 si4 | 略短 |
| 5 | 低升 | 市 si5 | 略长 |
| 6 | 低平 | 事 si6 | 最短 |

> 声调可省略，插件会用默认时长。

---

## ⚙️ 配置

插件启动时按以下顺序读取 `yue2ja.yaml`：

1. `<文档目录>\OpenUtau\Plugins\yue2ja.yaml`（全局）
2. `<音源目录>\yue2ja.yaml`（该音源专属，优先级更高）

没有文件时使用内置默认值。

### 示例

```yaml
# yue2ja.yaml
nasal_mode: short       # none / short / full
nasal_ms: 120           # short 模式「ん」占末尾毫秒
use_wildcard: true      # 启用 * 过渡音素

disable_vcv: false
disable_light_tone: false
disable_tone_timing: false
disable_double_char: false

# 自定义粤拼 → 日语 mora 映射
# full_jyutping:
#   nei: [ne, i]
#   hou: [ho, u]

# 多音字修正
# polyphones:
#   "ngan haang": "ngan hang"

# 别名覆盖
# alias_overrides:
#   "ん": ["ん -", "n -", "n"]
```

### 参数说明

| 参数 | 默认 | 说明 |
| ---- | ---- | ---- |
| `nasal_mode` | `short` | 鼻音韵尾处理：`none` 省略 / `short` 固定毫秒 / `full` 完整 |
| `nasal_ms` | `120` | `short` 模式下「ん」占末尾毫秒数（30–500） |
| `use_wildcard` | `true` | 优先尝试 `*` 过渡音素 |
| `disable_vcv` | `false` | 禁用 VCV 过渡 |
| `disable_light_tone` | `false` | 禁用叠字缩短 |
| `disable_tone_timing` | `false` | 禁用声调时长微调 |
| `disable_double_char` | `false` | 禁用叠字处理 |

---

## 🔊 发音特点

由于粤语和日语音系差异，转换结果带有"日式粤语"特色，属于正常现象：

| 粤语 | 粤拼 | 日语近似 | 说明 |
| ---- | ---- | -------- | ---- |
| 早 | zou | じょう | 复韵母合并 |
| 我 | ngo | ご | 鼻音声母近似 |
| 香 | hoeng | へ ん | 元音 + 鼻音韵尾 |
| 入 | jap | や | 入声尾音丢弃 |

**总结：听起来像日本人唱粤语。** 这是跨语言音源的固有限制。

---

## ❓ 常见问题

<details>
<summary><b>音素器列表里找不到 "YUE to JA"？</b></summary>

1. 确认 DLL 在 `<文档目录>\OpenUtau\Plugins\` 下
2. 确认文件名是 `MyYUtoJAPlugin.dll`（不能带 `.txt` 或 `(1)` 后缀）
3. 菜单栏「帮助」→「显示日志」，搜索 `YUE to JA`
4. 完全退出 OpenUtau 再启动

</details>

<details>
<summary><b>改了 YAML 效果没变化？</b></summary>

OpenUtau 有渲染缓存。必须：

1. 完全关闭 OpenUtau
2. 清空 `<文档目录>\OpenUtau\Cache\`
3. 重启并重新渲染

</details>

<details>
<summary><b>渲染时报 Oto not found for "ゃ"？</b></summary>

正常警告。「ゃ」是拗音后缀，不是独立 mora。OpenUtau 做边界探测时会拆开查找 OTO，可忽略。

</details>

<details>
<summary><b>ん 听起来太长？</b></summary>

音源本身的「ん」采样就很长（常见于弱音源、气声音源）。方案：

1. `nasal_mode: none` 直接省略
2. 用两个音符写 `a` + `n`，精确控制时长
3. 调小 `nasal_ms`（如 60）

</details>

<details>
<summary><b>某些字发音很奇怪？</b></summary>

1. **直接用粤拼输入** —— 最可靠
2. 右键音符 → 音素提示 → 手动指定
3. 换音源测试
4. 反馈问题（附上字 + 音源型号）

</details>

---

## 🛠️ 开发

### 环境

- .NET 10 SDK
- Windows（推荐）/ Linux / macOS
- （可选）Inno Setup 6 —— 用于打包安装程序

### 编译

```powershell
# 拉取依赖 DLL（OpenUtau 官方 Release）
powershell -ExecutionPolicy Bypass -File build\fetch-deps.ps1

# 编译
dotnet build openutau-yue-to-ja.slnx -c Release

# 运行单元测试
dotnet test openutau-yue-to-ja.slnx
```

产物：`src/YueToJapanesePhonemizer/bin/Release/net10.0/MyYUtoJAPlugin.dll`

### 打包安装程序

```powershell
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" build\installer.iss
```

### 目录结构

| 目录 | 职责 |
| ---- | ---- |
| `src/YueToJapanesePhonemizer/` | 插件源码（`Yue2JaCore.cs` 为纯逻辑，可单测） |
| `tests/` | xunit 单元测试 |
| `build/` | 构建、依赖拉取、安装包脚本 |
| `docs/` | Markdown 文档 |
| `tools/` | 离线工具（配置编辑器） |

### 核心文件

| 文件 | 说明 |
| ---- | ---- |
| `Yue2JaCore.cs` | 数据表 + 纯逻辑（不依赖 OpenUtau 运行时） |
| `YueToJapanesePhonemizer.cs` | 主类，实现 `SyllableBasedPhonemizer` |
| `yue2ja.yaml` | 默认配置模板 |

---

## 📄 许可

[MIT License](LICENSE) © 2026 styisme

---

<div align="center">

**祝调教愉快 🎤**

</div>
