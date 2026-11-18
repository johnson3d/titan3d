# RenderDoc MCP 环境与项目配置

RenderDoc MCP 用于让支持 MCP 的 IDE/助手打开 `.rdc`、读取渲染管线状态并分析抓帧。它与引擎自身的抓帧 API、RenderDoc 图形界面是不同的入口；MCP 环境搭建成功不代表引擎菜单抓帧问题已经解决。

仓库提供初始化脚本：[3rd/native/renderdoc/setup-mcp.ps1](../../3rd/native/renderdoc/setup-mcp.ps1)。以下命令均在仓库根目录的 PowerShell 中执行。

## 新机器一键初始化

前置条件：

- 已取得包含初始化脚本的完整仓库，运行平台为 Windows。
- 有可用的 x64 Python 3.13；如果没有，脚本使用 `winget` 安装当前用户版。两者都没有时，需要先安装 Windows“应用安装程序”（提供 `winget`），或从 [Python 官方 Windows 下载页](https://www.python.org/downloads/windows/) 安装 x64 Python 3.13。
- 首次安装依赖需要联网。无需预装系统 RenderDoc，也无需 Vulkan SDK 来加载 MCP 包内的 pyd；实际 GPU 抓帧重放仍需要可用的图形驱动。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\3rd\native\renderdoc\setup-mcp.ps1 -ConfigureMcp
```

`-ExecutionPolicy Bypass` 仅作用于这次启动的 PowerShell 进程，不修改系统持久执行策略。

脚本依次完成：

1. 查找并验证 x64 Python 3.13，支持 `py` 启动器、Python 注册表安装路径、默认目录及 PATH；不使用 Windows Store 的 Python 占位程序。
2. 缺少 Python 时，通过 `winget` 安装 `Python.Python.3.13` 的 x64 当前用户版。
3. 在 `cache/renderdoc-mcp-venv` 创建独立虚拟环境；已有有效环境则复用，不向全局 Python 安装 MCP 依赖。
4. 安装固定版本 `renderdoc-mcp==0.2.7` 与 `mcp[cli]==1.30.0`，执行 `pip check`。
5. 验证版本、包内 `renderdoc.pyd` 加载，以及 MCP `initialize` / `tools/list` 握手；握手检查设有 30 秒超时。
6. 仅在验证通过且指定 `-ConfigureMcp` 时，更新仓库根 `.mcp.json` 的 `renderdoc` 项。其他服务和未指定字段保留，原文件备份到 `cache/renderdoc-mcp-backups/`。

脚本不会安装或替换系统 RenderDoc，不会修改引擎使用的 `renderdoc.dll`。运行失败返回非零退出码。

## 参数与常用操作

| 参数 | 行为 |
| --- | --- |
| `-ConfigureMcp` | 环境验证通过后，备份并合并项目 `.mcp.json`；默认不修改配置。 |
| `-PythonPath` | 指定已有的 x64 Python 3.13 `python.exe`，支持自定义安装目录；路径不存在或版本不符时直接报错。 |
| `-SkipPythonInstall` | 禁止自动安装 Python；仍会安装虚拟环境中的 MCP 依赖，不是离线模式。 |
| `-CheckOnly` | 只检查基础 Python 并输出目标路径，不创建环境、不安装依赖、不修改配置；不等于 MCP 握手验证。 |

只搭建环境，不改当前已连接的 MCP 配置：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\3rd\native\renderdoc\setup-mcp.ps1
```

指定自定义安装的 Python，并配置 MCP：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\3rd\native\renderdoc\setup-mcp.ps1 -PythonPath 'C:\Tools\Python313\python.exe' -SkipPythonInstall -ConfigureMcp
```

只做预检查：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\3rd\native\renderdoc\setup-mcp.ps1 -CheckOnly
```

无需执行虚拟环境的激活脚本。安装、验证和 MCP 启动均直接使用独立环境中的 `Scripts/python.exe`。

## 项目 MCP 配置

脚本生成的配置形如下面的 JSON。该示例只展示 `renderdoc` 项；手动配置时应合并到已有 `mcpServers`，不要覆盖其他服务。

```json
{
  "mcpServers": {
    "renderdoc": {
      "command": "D:/GitHub/titanengine/cache/renderdoc-mcp-venv/Scripts/python.exe",
      "args": ["-I", "-m", "renderdoc_mcp"]
    }
  }
}
```

- 示例中的仓库位置仅用于说明；`-ConfigureMcp` 会写入本机实际绝对路径，不硬编码用户名。
- `-I` 使用 Python 隔离模式，避免用户级包和 `PYTHONPATH` 等影响模块加载。
- 本流程不依赖 `${workspaceFolder}` 的客户端变量展开。
- 仓库位置或 Python 安装位置变化后需要重新检查环境，不要把另一台机器的虚拟环境直接复制过来。环境仍有效时可再次运行 `-ConfigureMcp` 更新路径；损坏或版本不符的已有目录需要先人工处理，脚本不会自动清空它。
- `.mcp.json` 中包含机器相关绝对路径，共享或提交配置前应核对路径适用范围。

## 为什么不需要配置 renderdoc.pyd 路径

`renderdoc-mcp 0.2.7` 严格要求 Python 3.13。它自带 Python 3.13 ABI 的 RenderDoc 库；当前实测包内 RenderDoc 版本为 **1.43**。

加载顺序是：Python 导入 `renderdoc_mcp` → 包初始化将自身 `lib/` 加入模块和 DLL 搜索路径 → 从 `renderdoc_mcp/lib/renderdoc.pyd` 加载 RenderDoc。

因此，当前版本不需要 `RENDERDOC_MODULE_PATH`，也不读取该变量来选择仓库中的 pyd。不要把系统 RenderDoc DLL 或仓库 `Development` 目录的 pyd/DLL 拷入虚拟环境混用。仓库 `Development` 当前的 RenderDoc 1.44 / Python 3.14 离线分析链路与此 MCP 环境是两套独立环境。

同时固定 `mcp[cli]==1.30.0`，是因为 `renderdoc-mcp 0.2.7` 的依赖声明仅为 `mcp[cli]>=1.0.0`，没有限制大版本；只安装 `renderdoc-mcp` 不能保证获得这里验证过的 SDK 组合。

## 验证 IDE 连接与抓帧重放

1. 完整运行脚本后，确认输出包版本、pyd 路径及 `MCP initialize / tools/list: OK`，且最终退出码为 0。
2. 在当前仓库的 Qoder IDE 中打开“设置 → MCP 服务”，确认项目 `renderdoc` 服务连接为绿色、工具可用。配置保存不等于当前 IDE 已刷新；未刷新时可重启 Qoder 后再检查。
3. 使用 MCP 打开实际 `.rdc`，确认能重放并读取事件或资源。

必须区分三个结果：依赖安装成功、MCP 连接成功、抓帧重放成功。绿色连接不保证任意 RenderDoc 版本生成的抓帧都兼容，也不证明某个渲染 pass 正确。

仓库抓帧默认放在 `cache/renderdoc/`，分析产物放在 `cache/renderdoc/analysis/`。通过项目 RenderDoc MCP 传入抓帧路径时，使用相对仓库根目录的路径，例如 `cache/renderdoc/22.rdc`。

## 常见失败

| 现象 | 检查方式 |
| --- | --- |
| 找不到 Python 3.13，或指定解释器版本不符 | 安装 x64 Python 3.13，或用 `-PythonPath` 指向正确的 `python.exe`；Python 3.14 不能替代当前 MCP 包要求的 3.13。 |
| 找不到 `winget` | 安装 Windows“应用安装程序”，或手动安装 Python 3.13 后重试。 |
| 禁止运行 PowerShell 脚本 | 使用本文含 `-ExecutionPolicy Bypass` 的进程级命令；组织策略强制限制时，联系管理员，不修改全局策略。 |
| pip 安装失败、`pip check` 报错 | 检查网络和包源，并保留完整错误输出；不要通过取消版本约束或修改全局 Python 来掩盖问题。 |
| 已有目录不是完整虚拟环境 | 人工检查 `cache/renderdoc-mcp-venv`；脚本会停止，不覆盖或自动删除该目录。 |
| MCP 握手超时、模块或 DLL 加载失败 | 检查脚本输出的 Python、包版本和 pyd 路径；配置更新只会在验证成功后执行。 |
| `.mcp.json` 无效、重复键或已有远程 `renderdoc` 定义 | 先人工核对配置；脚本会拒绝改写，避免覆盖不兼容结构。 |
| MCP 绿色但 `.rdc` 无法重放 | 分别检查捕获版本、重放库版本、驱动与设备要求，不把连接成功当作重放兼容性结论。 |

[返回文档索引](../Index.md)
