using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Editor.Forms
{
    public class TtMemoryProfiler : IRootForm
    {
        public TtMemoryProfiler()
        {
            TtEngine.RootFormManager.RegRootForm(this);
        }
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public void Dispose() { }

        public void OnDraw()
        {
            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm("MemProfiler", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                if (ImGuiAPI.BeginTabBar("Memory", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                {
                    if (ImGuiAPI.CollapsingHeader("Object", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                    {
                        ImGuiAPI.Text($"TtGameInstance Count = {GamePlay.TtGameInstance.NodeAliveNumber};");
                        ImGuiAPI.Text($"TtMacrossGetter Count = {TtEngine.Instance.MacrossModule.mGetters.Count};");
                        ImGuiAPI.Text($"TtWorld Count = {GamePlay.TtWorld.NodeAliveNumber};");
                        ImGuiAPI.Text($"TtNode Count = {GamePlay.Scene.TtNode.NodeAliveNumber};");
                    }
                    if (ImGuiAPI.CollapsingHeader("Drawcall", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                    {
                        ImGuiAPI.Text($"GraphicsDrawcall = {TtStatistic.Instance.GraphicsDrawcall.Value} / {TtStatistic.Instance.NativeGraphicsDrawcall}");
                        ImGuiAPI.Text($"ComputeDrawcall = {TtStatistic.Instance.ComputeDrawcall.Value} / {TtStatistic.Instance.NativeComputeDrawcall}");
                        ImGuiAPI.Text($"TransferDrawcall = {TtStatistic.Instance.TransferDrawcall.Value} / {TtStatistic.Instance.NativeTransferDrawcall}");

                        var stats = TtStatistic.Instance.RenderCmdQueue;
                        ImGuiAPI.Text($"CmdList = {stats.NumOfCmdlist};Drawcall = {stats.NumOfDrawcall};Primitive = {stats.NumOfPrimitive};");
                    }
                    
                    if(ImGuiAPI.CollapsingHeader("NativeMemory", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                    {
                        ImGuiAPI.Text($"UseMemory = {CoreSDK.NativeMemoryUsed() / (1024)}k,;MaxMemory = {CoreSDK.NativeMemoryMax() / (1024)}k;AllocTimes = {CoreSDK.NativeMemoryAllocTimes()};");
                        ImGuiAPI.Text($"Texture Alive = {NxRHI.ITexture.GetAliveCount()};");
                        ImGuiAPI.Text($"Texture Alive AttachBuffer = {NxRHI.ITexture.GetAliveAttachBufferCount()};");
                        ImGuiAPI.Text($"IGraphicDraw Count = {NxRHI.IGraphicDraw.GetNumOfInstance()};");
                        ImGuiAPI.Text($"IComputeDraw Count = {NxRHI.IComputeDraw.GetNumOfInstance()};");
                        ImGuiAPI.Text($"ICopyDraw Count = {NxRHI.ICopyDraw.GetNumOfInstance()};");
                    }
                    
                    if (TtEngine.Instance.GfxDevice.RenderContext.RhiType == NxRHI.ERhiType.RHI_D3D12)
                    {
                        if(ImGuiAPI.CollapsingHeader("DX12", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                        {
                            var dx12 = TtEngine.Instance.GfxDevice.RenderContext.AsDX12Deivce;
                            ImGuiAPI.Separator();
                            ImGuiAPI.Text($"CmdAllocator = {dx12.GetCommandAllocatorManager().GetNumOfAllocators()};");
                            ImGuiAPI.Text($"CmdAllocator Recycles= {dx12.GetCommandAllocatorManager().GetNumOfRecycles()};");
                            ImGuiAPI.Text($"CmdAllocator RecycleRefs= {dx12.GetCommandAllocatorManager().GetNumOfRecyclesRefs()};");

                            ImGuiAPI.Text($"CBuffer Pool = {dx12.GetCBufferMemAllocator().GetPoolsCount()};");
                            ImGuiAPI.Text($"CBuffer PoolTotalSize = {dx12.GetCBufferMemAllocator().GetTotalPoolSize()};");

                            ImGuiAPI.Text($"Upload Alloc/Free = {dx12.GetUploadBufferMemAllocator().GetAllocSize()} / {dx12.GetUploadBufferMemAllocator().GetFreeSize()};");
                            ImGuiAPI.Text($"UAV Alloc/Free = {dx12.GetUavBufferMemAllocator().GetAllocSize()} / {dx12.GetUavBufferMemAllocator().GetFreeSize()};");

                            ImGuiAPI.Text($"Rtv Count/Size = {dx12.GetRtvAllocator().GetAliveCount()} / {dx12.GetRtvAllocator().GetTotalSize()};");
                            ImGuiAPI.Text($"Dsv Count/Size = {dx12.GetDsvAllocator().GetAliveCount()} / {dx12.GetDsvAllocator().GetTotalSize()};");
                            ImGuiAPI.Text($"Sampler Count/Size = {dx12.GetSamplerAllocator().GetAliveCount()} / {dx12.GetSamplerAllocator().GetTotalSize()};");
                            ImGuiAPI.Text($"CbvSrvUav Count/Size = {dx12.GetCbvSrvUavAllocator().GetAliveCount()} / {dx12.GetCbvSrvUavAllocator().GetTotalSize()};");

                            ImGuiAPI.Text($"Begin Heap = {dx12.GetDescriptorSetAllocator().GetAllocatorCount()}");
                            dx12.GetDescriptorSetAllocator().IterateAllocator(static (key, value) =>
                            {
                                uint type = (uint)((key >> 32) & 0xffffffff);
                                uint size = (uint)(key & 0xffffffff);
                                ImGuiAPI.Text($"{type}:{size} = {value.GetTotalSize()}");
                            });
                            ImGuiAPI.Text($"End Heap");
                        }
                    }

                    if(ImGuiAPI.CollapsingHeader("AttachCache", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                    {
                        foreach (var i in TtEngine.Instance.GfxDevice.AttachBufferManager.Pools)
                        {
                            ImGuiAPI.Text($"{i.Key.ToString()} X {i.Value.PoolSize} => Max({i.Value.FrameMaxLiveCount}) / Alloc({i.Value.FrameAllocCount})");
                        }
                    }

                    if (ImGuiAPI.CollapsingHeader("Assembly", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                    {
                        ImGuiAPI.Text($"TtAssemblyDesc Count = {Rtti.TtAssemblyDesc.GetNumOfInstance()};");
                        foreach(var s in Rtti.TtTypeDescManager.Instance.Services)
                        {
                            if (ImGuiAPI.CollapsingHeader(s.Key, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet))
                            {
                                foreach (var a in s.Value.Assemblies)
                                {
                                    ImGuiAPI.Text($"{a.Value.Name}({a.Value.Platform}):{a.Value.Description} = {a.Value.Version}");
                                }
                            }   
                        }
                    }

                    if (ImGuiAPI.CollapsingHeader("ObjectPool", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                    {
                        foreach (var i in TtObjectPoolManager.Instance.Pools)
                        {
                            var pool = i as EngineNS.TtObjectPoolBase;
                            if (pool == null)
                                continue;
                            ImGuiAPI.Text($"{pool.ShowName}: Alive = {pool.AliveNumber}; PoolSize = {pool.PoolSize}");
                        }
                    }   
                    ImGuiAPI.EndTabBar();
                }
                
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
    }
}
