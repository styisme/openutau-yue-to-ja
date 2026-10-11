# 贡献指南

感谢参与 YueToJapanesePhonemizer 的开发！本项目采用轻量但明确的源代码管理规范：Conventional Commits + SemVer 2.0.0 + CHANGELOG，CI 全自动验证。

## 本地开发

```powershell
# 1. 首次：拉取编译依赖（约 160MB，来自 OpenUtau 官方 Release，之后命中缓存秒完成）
powershell -ExecutionPolicy Bypass -File build\fetch-deps.ps1

# 2. 编译
powershell -ExecutionPolicy Bypass -File build\build.ps1
# 或直接
dotnet build openutau-yue-to-ja.slnx -c Release

# 3. 运行单元测试
dotnet test openutau-yue-to-ja.slnx

# 4. 打包安装包（需要 Inno Setup 6）
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" build\installer.iss
```

需要 .NET 10 SDK。依赖 DLL（OpenUtau.Core / OpenUtau.Plugin.Builtin / WanaKanaNet / Serilog / YamlDotNet）不入库，由 `build/fetch-deps.ps1` 下载。

## 目录结构

| 目录 | 职责 |
|---|---|
| `src/YueToJapanesePhonemizer/` | 插件源码与项目文件（`Yue2JaCore.cs` 为可单测的纯逻辑） |
| `tests/` | xunit 单元测试 |
| `build/` | 构建、依赖拉取、安装包脚本 |
| `docs/` | Markdown 文档 |
| `local-archive/` | 本地历史二进制归档（已 gitignore，不入库） |

## 提交信息规范（Conventional Commits）

提交信息必须符合 [Conventional Commits](https://www.conventionalcommits.org/zh-hans/)：

```
<type>(<scope>): <描述>

[可选正文]
```

常用 type：

| type | 用途 | 示例 |
|---|---|---|
| `feat` | 新功能（用户可见） | `feat: 加入通配符 * 过渡音素支持` |
| `fix` | 缺陷修复 | `fix: 修复 nasal_mode=none 不生效` |
| `docs` | 文档 | `docs: 更新 README 安装说明` |
| `refactor` | 重构（不改行为） | `refactor: 重构 FullJyutpingMap` |
| `test` | 测试 | `test: 补充拼音解析用例` |
| `ci` | CI/CD | `ci: 流水线加入安装包编译` |
| `build` | 构建脚本 | `build: 更新 build.ps1` |
| `perf` | 性能 | `perf: 缓存 Oto 查询` |
| `chore` | 杂项 | `chore: 更新 .gitignore` |

规则：

* 描述用中文即可，首字母不加句号
* 破坏性变更必须带 `!`（如 `feat!:`）并在正文写明影响
* `feat`/`fix`/`perf` 会成为 CHANGELOG 的素材；`chore`/`refactor` 等通常不记录

## 语义化版本（SemVer 2.0.0）

版本格式 `MAJOR.MINOR.PATCH`：

| 递增 | 何时 | 本项目示例 |
|---|---|---|
| **MAJOR** | 不兼容的公共行为变更 | 删除 YAML 配置键、改变默认音素输出、提高 OpenUtau/.NET 最低版本要求 |
| **MINOR** | 向后兼容的新功能 | 新增 `use_wildcard` 等配置项、新增音源支持 |
| **PATCH** | 向后兼容的修复 | 修复拼音映射、修复 bug |

预发布：`2.2.0-beta.1`；Git tag：`v2.2.0`。

本项目的「公共行为」指以下内容，改动时按上表判断版本递增：

* `yue2ja.yaml` 配置键及默认值
* `nasal_mode`、`nasal_ms`、`use_wildcard` 等开关行为
* 音素别名输出结果
* 支持的 OpenUtau / .NET 版本要求
* 安装方式与 DLL 路径

> v2.1.7 及以前的历史版本号未严格遵循 SemVer，历史 tag 不重写；自 v2.2.0 起严格执行。

## 发版检查单

1. **更新版本号**（三处）：
   * `src/YueToJapanesePhonemizer/YueToJapanesePhonemizer.cs` — 文件头第 2 行与运行日志中的 `vX.Y.Z`
   * `build/installer.iss` — `MyAppVersion`
   * `README.md` — 版本徽章
2. **更新 CHANGELOG.md**：把 `[Unreleased]` 内容归入新版本段并加日期
3. **提交**：`release: vX.Y.Z - 摘要`
4. **打 tag**：`git tag vX.Y.Z && git push origin vX.Y.Z`
5. **创建 GitHub Release**，附上 CI 产出的安装包与 DLL artifact

## PR / 推送流程

* 直接推送 `main` 或发起 PR 均可，但 **CI（编译 + 测试 + 安装包）必须通过**
* 提交前本地至少运行 `dotnet test`
* 新功能应附带单元测试（`Yue2JaCore` 纯逻辑均可测）
