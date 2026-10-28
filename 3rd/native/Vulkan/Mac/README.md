# Vulkan loader + MoltenVK（macOS，随仓库自带）

引擎在 Mac 上不再依赖 `brew install vulkan-loader molten-vk`：Core.Mac 链接本目录的
`libvulkan.1.dylib`，构建时整目录拷到 `binaries/net8.0/vulkan/`，运行时由 loader 的
可执行文件同级 `vulkan/icd.d` 搜索（`<exe目录>/vulkan/icd.d`）发现 `MoltenVK_icd.json`。

```
binaries/net8.0/
  MainEditor
  vulkan/
    libvulkan.1.dylib          # @rpath/libvulkan.1.dylib
    libMoltenVK.dylib          # @rpath/libMoltenVK.dylib
    icd.d/MoltenVK_icd.json    # library_path 相对本文件: ../libMoltenVK.dylib
```

Core.Mac 的 rpath 为 `@loader_path/../net8.0/vulkan`（Core.Mac 在 `binaries/debug|release/`）。

## 来源与版本

| 文件 | 来源 | 版本 |
| --- | --- | --- |
| `libvulkan.1.dylib` | Homebrew Cellar `vulkan-loader` 的 `libvulkan.1.4.363.dylib` | 1.4.363.0 |
| `libMoltenVK.dylib` | Homebrew Cellar `molten-vk` | 1.4.2 |

两者都是 arm64 单架构，与 Core.Mac（只支持 arm64）一致。

两个 dylib 的 LC_ID 已改为 `@rpath/...`，改动后用 `codesign -f -s -` 重新 ad-hoc 签名。
升级时替换文件后需重做这两步（否则 dyld 会拒绝加载或记录绝对路径）。

## 许可证

- Vulkan-Loader：Apache-2.0，见 [LICENSE-vulkan-loader.txt](LICENSE-vulkan-loader.txt)
- MoltenVK：Apache-2.0，见 [LICENSE-MoltenVK.txt](LICENSE-MoltenVK.txt)
