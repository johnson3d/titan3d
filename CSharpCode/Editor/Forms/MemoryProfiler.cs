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

        private List<Rtti.TtTypeDesc> MacrossGetterTypes = new List<Rtti.TtTypeDesc>();
        public void OnDraw()
        {
            var size = new Vector2(800, 600);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_FirstUseEver);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(TtLocalization.Label("MemProfiler", "###MemProfiler"), this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                if (ImGuiAPI.BeginTabBar("Memory", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                {
                    bool showTab = true;
                    if (ImGuiAPI.BeginTabItem("Engine", ref showTab, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                    {
                        if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("Object"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                        {
                            ImGuiAPI.Text(TtLocalization.Format("ActiveThread = {0};", Thread.Async.TtContextThreadManager.AliveThread));
                            ImGuiAPI.Text(TtLocalization.Format("TtGameInstance Count = {0};", GamePlay.TtGameInstance.NodeAliveNumber));
                            ImGuiAPI.Text(TtLocalization.Format("TtMacrossGetter Count = {0};", TtEngine.Instance.MacrossModule.mGetters.Count));
                            ImGuiAPI.Text(TtLocalization.Format("TtWorld Count = {0};", GamePlay.TtWorld.NodeAliveNumber));
                            ImGuiAPI.Text(TtLocalization.Format("TtNode Count = {0};", GamePlay.Scene.TtNode.NodeAliveNumber));
                        }
                        if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("Macross"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                        {
                            if (MacrossGetterTypes.Count == 0)
                            {
                                Rtti.TtTypeDescManager.Instance.GetInheritTypes(Rtti.TtTypeDescGetter<Macross.TtMacrossGetterBase>.TypeDesc, MacrossGetterTypes);
                            }
                            foreach (var i in MacrossGetterTypes)
                            {
                                var prop = i.SystemType.GetProperty("NumOfMacrossGetter");
                                if (prop == null)
                                    continue;
                                ImGuiAPI.Text(TtLocalization.Format("{0} = {1};", i.SystemType.GetGenericArguments()[0].FullName, prop.GetValue(null)));
                            }
                        }
                        if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("Drawcall"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                        {
                            ImGuiAPI.Text(TtLocalization.Format("GraphicsDrawcall = {0} / {1}", TtStatistic.Instance.GraphicsDrawcall.Value, TtStatistic.Instance.NativeGraphicsDrawcall));
                            ImGuiAPI.Text(TtLocalization.Format("ComputeDrawcall = {0} / {1}", TtStatistic.Instance.ComputeDrawcall.Value, TtStatistic.Instance.NativeComputeDrawcall));
                            ImGuiAPI.Text(TtLocalization.Format("TransferDrawcall = {0} / {1}", TtStatistic.Instance.TransferDrawcall.Value, TtStatistic.Instance.NativeTransferDrawcall));

                            var stats = TtStatistic.Instance.RenderCmdQueue;
                            ImGuiAPI.Text(TtLocalization.Format("CmdList = {0};Drawcall = {1};Primitive = {2};", stats.NumOfCmdlist, stats.NumOfDrawcall, stats.NumOfPrimitive));
                        }

                        if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("NativeMemory"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                        {
                            ImGuiAPI.Text(TtLocalization.Format("UseMemory = {0}k,;MaxMemory = {1}k;AllocTimes = {2};", CoreSDK.NativeMemoryUsed() / (1024), CoreSDK.NativeMemoryMax() / (1024), CoreSDK.NativeMemoryAllocTimes()));
                            ImGuiAPI.Text(TtLocalization.Format("Texture Alive = {0};", NxRHI.ITexture.GetAliveCount()));
                            ImGuiAPI.Text(TtLocalization.Format("Srv Alive = {0}/{1};", EngineNS.NxRHI.TtSrView.NumOfInstance, EngineNS.NxRHI.TtSrView.NumOfGCHandle));
                            ImGuiAPI.Text(TtLocalization.Format("Texture Alive AttachBuffer = {0};", NxRHI.ITexture.GetAliveAttachBufferCount()));
                            ImGuiAPI.Text(TtLocalization.Format("IGraphicDraw Count = {0};", NxRHI.IGraphicDraw.GetNumOfInstance()));
                            ImGuiAPI.Text(TtLocalization.Format("IComputeDraw Count = {0};", NxRHI.IComputeDraw.GetNumOfInstance()));
                            ImGuiAPI.Text(TtLocalization.Format("ICopyDraw Count = {0};", NxRHI.ICopyDraw.GetNumOfInstance()));
                        }

                        if (TtEngine.Instance.GfxDevice.RenderContext.RhiType == NxRHI.ERhiType.RHI_D3D12)
                        {
                            if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("DX12"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                            {
                                var dx12 = TtEngine.Instance.GfxDevice.RenderContext.AsDX12Deivce;
                                ImGuiAPI.Separator();
                                ImGuiAPI.Text(TtLocalization.Format("CmdAllocator = {0};", dx12.GetCommandAllocatorManager().GetNumOfAllocators()));
                                ImGuiAPI.Text(TtLocalization.Format("CmdAllocator Recycles= {0};", dx12.GetCommandAllocatorManager().GetNumOfRecycles()));
                                ImGuiAPI.Text(TtLocalization.Format("CmdAllocator RecycleRefs= {0};", dx12.GetCommandAllocatorManager().GetNumOfRecyclesRefs()));

                                ImGuiAPI.Text(TtLocalization.Format("CBuffer Pool = {0};", dx12.GetCBufferMemAllocator().GetPoolsCount()));
                                ImGuiAPI.Text(TtLocalization.Format("CBuffer PoolTotalSize = {0};", dx12.GetCBufferMemAllocator().GetTotalPoolSize()));

                                ImGuiAPI.Text(TtLocalization.Format("Upload Alloc/Free = {0} / {1};", dx12.GetUploadBufferMemAllocator().GetAllocSize(), dx12.GetUploadBufferMemAllocator().GetFreeSize()));
                                ImGuiAPI.Text(TtLocalization.Format("UAV Alloc/Free = {0} / {1};", dx12.GetUavBufferMemAllocator().GetAllocSize(), dx12.GetUavBufferMemAllocator().GetFreeSize()));

                                ImGuiAPI.Text(TtLocalization.Format("Rtv Count/Size = {0} / {1};", dx12.GetRtvAllocator().GetAliveCount(), dx12.GetRtvAllocator().GetTotalSize()));
                                ImGuiAPI.Text(TtLocalization.Format("Dsv Count/Size = {0} / {1};", dx12.GetDsvAllocator().GetAliveCount(), dx12.GetDsvAllocator().GetTotalSize()));
                                ImGuiAPI.Text(TtLocalization.Format("Sampler Count/Size = {0} / {1};", dx12.GetSamplerAllocator().GetAliveCount(), dx12.GetSamplerAllocator().GetTotalSize()));
                                ImGuiAPI.Text(TtLocalization.Format("CbvSrvUav Count/Size = {0} / {1};", dx12.GetCbvSrvUavAllocator().GetAliveCount(), dx12.GetCbvSrvUavAllocator().GetTotalSize()));

                                ImGuiAPI.Text(TtLocalization.Format("Begin Heap = {0}", dx12.GetDescriptorSetAllocator().GetAllocatorCount()));
                                dx12.GetDescriptorSetAllocator().IterateAllocator(static (key, value) =>
                                {
                                    uint type = (uint)((key >> 32) & 0xffffffff);
                                    uint size = (uint)(key & 0xffffffff);
                                    ImGuiAPI.Text(TtLocalization.Format("{0}:{1} = {2}", type, size, value.GetTotalSize()));
                                });
                                ImGuiAPI.Text(TtLocalization.Tr("End Heap"));
                            }
                        }

                        if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("AttachCache"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                        {
                            foreach (var i in TtEngine.Instance.GfxDevice.AttachBufferManager.Pools)
                            {
                                ImGuiAPI.Text(TtLocalization.Format("{0} X {1} => Max({2}) / Alloc({3})", i.Key.ToString(), i.Value.PoolSize, i.Value.FrameMaxLiveCount, i.Value.FrameAllocCount));
                            }
                        }

                        if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("Assembly"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                        {
                            ImGuiAPI.Text(TtLocalization.Format("TtAssemblyDesc Count = {0};", Rtti.TtAssemblyDesc.GetNumOfInstance()));
                            foreach (var s in Rtti.TtTypeDescManager.Instance.Services)
                            {
                                if (ImGuiAPI.CollapsingHeader(s.Key, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet))
                                {
                                    foreach (var a in s.Value.Assemblies)
                                    {
                                        ImGuiAPI.Text(TtLocalization.Format("{0}({1}):{2} = {3}", a.Value.Name, a.Value.Platform, a.Value.Description, a.Value.Version));
                                    }
                                }
                            }
                        }

                        if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("ObjectPool"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                        {
                            foreach (var i in TtObjectPoolManager.Instance.Pools)
                            {
                                var pool = i as EngineNS.TtObjectPoolBase;
                                if (pool == null)
                                    continue;
                                ImGuiAPI.Text(TtLocalization.Format("{0}: Alive = {1}; PoolSize = {2}", pool.ShowName, pool.AliveNumber, pool.PoolSize));
                            }
                        }
                        ImGuiAPI.EndTabItem();
                    }

                    var game = TtEngine.Instance.GameInstance;
                    if (game != null)
                    {
                        if (ImGuiAPI.BeginTabItem("Game", ref showTab, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                        {
                            if (ImGuiAPI.CollapsingHeader(TtLocalization.Tr("PrefabPool"), ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                            {
                                foreach (var p in game.PrefabPoolManager.Pools)
                                {
                                    ImGuiAPI.Text(TtLocalization.Format("{0} = {1} / {2};", p.Key, p.Value.AliveNumber, p.Value.PoolSize));
                                }
                            }
                            ImGuiAPI.EndTabItem();
                        }
                    }
                    ImGuiAPI.EndTabBar();
                }
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
    }
}
