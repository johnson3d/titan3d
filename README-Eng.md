# Architecture Highlights (Industry Comparison)

> **TitanEngine's Render Dependency Graph (`TtRenderGraph` / `TtAttachmentCache`) is benchmarked against UE5 RDG / Unity URP-HDRP Render Graph / Frostbite FrameGraph / Granite / O3DE Atom / Stride GraphicsCompositor / Falcor / Bevy bevy_render.**
>
> **One-line positioning: Tier-1 RDG capabilities (topology pruning / refcount / transient pooling / auto barrier all in place) + a rare visual editor for the *main* rendering pipeline + Permutation / RenderPolicy as first-class editable assets — fewer than 5 engines worldwide combine all three.** Both UE5 and Unity expose their RDG as pure C++/C# API; the main pipeline is *not* visually editable.
>
> Full feature matrix, roadmap, and design-philosophy comparison: → **[Documents/engine/RenderGraph.Industry.Compare.md](Documents/engine/RenderGraph.Industry.Compare.md)**

# Compilation and Runtime Environment
- Titan3D Launches!
- ![Overview](Documents/picture/main_edtor.png)
- 1. Install Visual Studio 2022
- 2. Install C# development environment
- 3. Recommended to install C# mobile development environment

# Engine Features
- 1. C++/C# hybrid programming with C++ as the underlying layer and C# as the upper layer. Automatic glue code generation enables complete C++ functionality calls from C#
- 2. Coroutine-based multithreading architecture that eliminates Callback Hell
- 3. Macro-based visual scripting architecture allowing users to implement complex game logic without coding
- 4. Fully customizable rendering pipeline - edit RenderGraph via editor and extend RenderGraphNode through C# for truly customized effects
- 5. Built-in multi-process server cluster architecture suitable for large MMO projects
- 6. HLSL-based shading development with unified cross-platform shader reflection for rendering resource binding, following native DX development conventions

# Supported Features
- 1. Supports DX11, DX12, Vulkan, OpenGLES (deprecated)
- 2. Supports Windows and Android platforms
- 3. Amplification Shader, Mesh Shader, RayTracing Shader
- 4. Visual editor for configuring RenderGraph assets with built-in DeferredShading and MobileShading templates
- 5. Visual Material Shader editor
- 6. InGame UI editor supporting 2D/3D game UI editing
- 7. GPU-driven framework based on GpuScene+IndirectDraw+Bindless
- 8. Particle system with visual logic editing supporting CPU/GPU particle switching
- 9. SDF fonts with scaling-friendly design
- 10. Node-based scene editor where C# scripts can be attached to any Node via world outline
- 11. Visual state machine editor working with macros to implement game logic
- 12. Visual script editor - Macross system can implement nearly all engine functionalities
- 13. Built-in RenderDoc support (configurable)
- 14. TCP/IP-based RPC networking with built-in large-scale server cluster architecture
- 15. Double-precision coordinates supporting CDLOD terrain for infinite worlds
- 16. Automatic Excel data mapping to data structures with auto read/write
- 17. Dynamic plugin loading/unloading for extending both game and engine features
- 18. Dedicated prefab editor

# Default RenderGraph Node List
![Overview](Documents/editor/RenderPolicyEditor/rpolicy_editor.png)
All rendering pipelines are configured through node connections. Therefore, don't ask what rendering features the engine supports - expanding and combining nodes determines the final rendering pipeline effects.

- 1. CullClusterNode, SwRastererizeNode, QuakResolveNode, etc. for software rasterization (WIP)
- 2. Hzb and depth clip map for depth culling
- 3. CpuCulling for generating visible Node lists on CPU
- 4. DeferredBassPass for deferred rendering (3-4 RT outputs with configurable velocity output)
- 5. ShadowMap for classic CSM shadows
- 6. ScreenTiling for screen partitioning (currently tracking point light influences)
- 7. AdvShadow for large-scale shadow processing based on QTree Clip Map
- 8. DirLighting for PBR deferred shading of directional lights
- 9. Forward node for transparent rendering
- 10. Particle node for particle system driving and rendering
- 11. AvgBright/HDR for dynamic lighting tonemapping and eye adaptation
- 12. Picked, PickBlur, PickHollow, PickHollowBlend, HitProxy for editor pixel selection
- 13. ScreenSpaceUI for screen-space UI rendering (3D UI handled in base pass/forward)
- 14. Other effect nodes:
   - VoxelNode: Creates sparse voxels from viewport
   - FogNode: Height-based fog
   - LuminanceThredhole: Extracts luminance range
   - Bloom: Self-explanatory
   - Additive: Color blending
   - SunShaftDepthThreshole/SunShaftRadialBlur: God Ray effect nodes
   - TAA: Temporal anti-aliasing (works with velocity output)
- 15. Realtime Global Illumination & Denoising Nodes (ReSTIR GI Pipeline)
   - ReSTIRGI (`TtReSTIRGINode`): Realtime GI based on the [ReSTIR GI](https://research.nvidia.com/publication/2021-06_restir-gi-path-resampling-real-time-path-tracing) paper. A single node hosts 4 compute passes: Initial Sampling / Temporal Reuse / Spatial Reuse / Resolve. Supports hardware RT (DXR) and software RT (SDF / BVH) via Permutation switch, plus SkyCube / constant-sky miss shading modes.
   - Denoise (`TtDenoiseNode`): Combined spatial + temporal denoiser in one node:
     - **Spatial**: à-trous wavelet edge-aware bilateral filter, multi-iteration ping-pong (4 iters by default, StepSize = 1/2/4/8), edge-stopping driven by GBuffer Normal + Depth, with built-in firefly luminance clamp to suppress ReSTIR resolve outliers.
     - **Temporal**: motion-vector reprojection + history ping-pong + normal/depth consistency validation. Critical for eliminating frame-to-frame flicker under low-spp ReSTIR / SSR. Toggleable via `EnableTemporal` (auto-falls back to pure spatial if MotionVector pin is left unconnected).

# Compilation and Building
## Windows Engine Compilation
1. **First-time compilation often requires manually running CppWeavingTools and CSharpCodeTools projects once to generate necessary temporary cpp/cs files in codegen/NativeBinder and Cs2Cpp directories**
2. If NativeBinder generation fails initially, LLVM installation may be required
3. Compile Core.Window project (C++)
4. Compile Engine.Window project (C#)
5. Compile MainEditor project (C#)
6. **First-time may require manual compilation of these plugin projects:**
   - SourceGit
   - DataCopyer
7. **Due to GitHub LFS limitations, manual decompression may be needed for:**
   - binaries\Tools\net8.0\libclang.7z

## Android APK Compilation
1. Compile Core.Android project (C++)
2. Compile Engine.Android project (C#)

# Debugging and Execution
1. Set MainEditor as startup project
2. Debug command line arguments: config=$(SolutionDir)content\EngineConfig.cfg use_renderdoc=false
3. Debug working directory: $(SolutionDir)binaries\
4. For runtime and debugging, read [**Engine Configuration and Editor Usage Documentation**](Documents/Index.md)
5. For unexpected IO-related crashes/exceptions, try deleting local cache directory

# Developer Notes
1. Avoid committing large files (>20MB) to prevent LFS usage
2. [Common Code Snippets](CodeLib.md)
3. **New C++ Bricks must have corresponding macros added** - otherwise C# won't find C++ functions (see Note 2)

# Console Programs
## Special Parameters
- 1. ExeCmd=Command to execute
- 2. ExtraCmd={n} where n determines the number of console input parameters after launch

## Save Assets to Latest Version
- Resolves MetaVersion explosion by saving specified assets to latest version:

## License
- Licensed under [LGPL v3](LICENSE.md)
