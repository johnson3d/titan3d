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
                if (ImGuiAPI.BeginTabBar("RHI", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                {
                    ImGuiAPI.Text($"GraphicsDrawcall = {TtStatistic.Instance.GraphicsDrawcall.Value} / {TtStatistic.Instance.NativeGraphicsDrawcall}");
                    ImGuiAPI.Text($"ComputeDrawcall = {TtStatistic.Instance.ComputeDrawcall.Value} / {TtStatistic.Instance.NativeComputeDrawcall}");
                    ImGuiAPI.Text($"TransferDrawcall = {TtStatistic.Instance.TransferDrawcall.Value} / {TtStatistic.Instance.NativeTransferDrawcall}");

                    var stats = TtStatistic.Instance.RenderCmdQueue;
                    ImGuiAPI.Text($"CmdList = {stats.NumOfCmdlist};Drawcall = {stats.NumOfDrawcall};Primitive = {stats.NumOfPrimitive};");
                    
                    ImGuiAPI.Separator();
                    ImGuiAPI.Text($"UseMemory = {CoreSDK.NativeMemoryUsed()};MaxMemory = {CoreSDK.NativeMemoryMax()};AllocTimes = {CoreSDK.NativeMemoryAllocTimes()};");
                    ImGuiAPI.Text($"Texture Alive = {NxRHI.ITexture.GetAliveCount()};");
                    ImGuiAPI.Text($"Texture Alive AttachBuffer = {NxRHI.ITexture.GetAliveAttachBufferCount()};");
                    if (TtEngine.Instance.GfxDevice.RenderContext.RhiType == NxRHI.ERhiType.RHI_D3D12)
                    {
                        var dx12 = TtEngine.Instance.GfxDevice.RenderContext.AsDX12Deivce;
                        ImGuiAPI.Separator();
                        ImGuiAPI.Text($"DX12 CmdAllocator = {dx12.GetCommandAllocatorManager().GetNumOfAllocators()};");
                        ImGuiAPI.Text($"DX12 CmdAllocator Recycles= {dx12.GetCommandAllocatorManager().GetNumOfRecycles()};");
                        ImGuiAPI.Text($"DX12 CmdAllocator RecycleRefs= {dx12.GetCommandAllocatorManager().GetNumOfRecyclesRefs()};");
                        ImGuiAPI.Separator();
                    }
                    

                    ImGuiAPI.Separator();
                    ImGuiAPI.Text("Begin AttachBuffer");
                    foreach (var i in TtEngine.Instance.GfxDevice.AttachBufferManager.Pools)
                    {
                        ImGuiAPI.Text($"{i.Key.ToString()} X {i.Value.PoolSize} => Max({i.Value.FrameMaxLiveCount}) / Alloc({i.Value.FrameAllocCount})");
                    }
                    ImGuiAPI.Text("End AttachBuffer");
                    ImGuiAPI.EndTabBar();
                }
                
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
    }
}
