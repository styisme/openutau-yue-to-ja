# Changelog

本项目的重要变更都记录在此文件。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，
版本号遵循 [Semantic Versioning 2.0.0](https://semver.org/lang/zh-CN/)。

> **来源**：本项目移植自 [openutau-cn-to-ja](https://github.com/styisme/openutau-cn-to-ja)
> v2.3.0（普通话版）。粤语版自 v1.0.0 起独立记录版本历史。
>
> **`## [Latest] X.Y.Z`**：CI 构建安装包时读取的唯一版本来源。
>   发新版时把 `[Latest]` 头衔移到新条目上，旧条目改为 `## X.Y.Z`。
>   两个 `[Latest]` 同时存在时，CI 只取文件中靠上的那一个。

## [Latest] 1.0.0

粤语版首个正式版本。

### Added

- 让日语音源唱粤语的 OpenUtau 音素器，显示名 "YUE to JA"
- 粤拼（Jyutping）音系数据表：
  - 声母 19 个（含 `ng` / `gw` / `kw`）
  - 韵母 50+ 个（含 `aa` / `oeng` / `eoi` / `eon` 等粤语特有声韵）
  - 入声韵尾 `-p` / `-t` / `-k`（日语无对应，丢弃尾音处理）
- 粤语六声调支持（粤拼尾部数字 1–6），影响末尾 mora 时长
- 粤语数字读法：`jat / ji / saam / sei / ng / luk / cat / baat / gau`
- 量词读法：年 = `nin`、月 = `jyut`、日 = `jat`、点 = `dim`、分 = `fan`、秒 = `miu`
- 日期、时间、小数、百分比、千位逗号的识别与读法
- YAML 配置（`yue2ja.yaml`），支持自定义映射、多音字、别名覆盖
- 鼻音韵尾三档处理：`nasal_mode` = none / short / full
- 通配符 `*` 过渡音素支持（Defoko 等 CV 音源）
- xunit 单元测试：粤拼解析、假名转换、数字读法、文本处理、YAML 配置
- GitHub Actions CI：编译 → 测试 → Inno Setup 安装包
- 离线配置编辑器 `tools/yue2ja-editor.html`

### Changed

- 插件显示名、DLL 名、配置文件名、目录结构相对源项目全部重命名
- 移除普通话特有逻辑：儿化音合并、一/不变调、`LightToneChars` 语法助词表

### Removed

- 普通话拼音数据表（`ParsePinyin`、`FullPinyinMap` 等）

[Unreleased]: https://github.com/styisme/openutau-yue-to-ja/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/styisme/openutau-yue-to-ja/releases/tag/v1.0.0