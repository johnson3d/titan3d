using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Windows
{
    public class TtClrProfiler : IRootForm
    {
        public TtClrProfiler()
        {
            TtEngine.RootFormManager.RegRootForm(this);
        }

        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return true;
        }

        public unsafe void Dispose()
        {
            StrName.NativeSuper.Release();
            while (mObjectAllocLogs.Count > 0)
            {
                var d = mObjectAllocLogs.Dequeue();
                CoreSDK.Free(d.ToPointer());
            }
        }

        public static CoreCLRManager ClrManager
        {
            get => CoreCLRManager.GetInstance();
        }

        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public Queue<IntPtr> mObjectAllocLogs = new ();
        [Category("Option")]
        public int MaxAllocLog { get; set; } = 64;
        ClrString StrName = ClrString.CreateInstance("System.String");
        public unsafe void UpdateLogs()
        {
            var clrStr = ClrManager.PopLog();
            
            while (clrStr.IsValidPointer)
            {
                switch(clrStr.mType )
                {
                    case EClrLogStringType.ObjectAlloc:
                        {
                            var s = (sbyte*)clrStr.GetStringPtr();
                            if (CoreSDK.SDK_StrCmp(StrName.GetStringPtr(), s) != 0)
                            {
                                var len = CoreSDK.SDK_StrLen(s);
                                if (len > 0)
                                {
                                    var p = (byte*)CoreSDK.Alloc(len + 1, null, 0);
                                    //s[len] = 0;
                                    CoreSDK.SDK_StrCpy(p, s, len + 1);
                                    mObjectAllocLogs.Enqueue((IntPtr)p);
                                    if (mObjectAllocLogs.Count >= 256)
                                    {
                                        var d = mObjectAllocLogs.Dequeue();
                                        CoreSDK.Free(d.ToPointer());
                                    }
                                }
                            }
                        }
                        break;
                    case EClrLogStringType.ObjectReferences:
                        {
                            var s = (sbyte*)clrStr.GetStringPtr();
                        }
                        break;
                }
                clrStr.NativeSuper.Release();
                clrStr = ClrManager.PopLog();
            }
        }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

            var clrMgr = CoreCLRManager.GetInstance();
            var bPause = clrMgr.PauseLog;

            Vector2 size = new Vector2(0, 0);
            var result = EGui.UIProxy.DockProxy.BeginMainForm("ClrProfiler", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                if (ImGuiAPI.BeginTabBar("CLR", ImGuiTabBarFlags_.ImGuiTabBarFlags_None))
                {
                    if (ImGuiAPI.BeginTabItem("ObjectAlloc", null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                    {
                        ImGuiAPI.Checkbox("PauseLog", ref bPause);
                        clrMgr.PauseLog = bPause;
                        foreach (var i in mObjectAllocLogs)
                        {
                            ImGuiAPI.TextAsPointer((sbyte*)i.ToPointer());
                        }
                        ImGuiAPI.EndTabItem();
                    }
                    if (ImGuiAPI.BeginTabItem("CachedClasses", null, ImGuiTabItemFlags_.ImGuiTabItemFlags_None))
                    {
                        var num = ClrManager.GetCachedClassNum();
                        var ptr = ClrManager.GetCachedClassPtr();
                        for (int i = 0; i < num; i++)
                        {
                            ImGuiAPI.TextAsPointer((sbyte*)ptr[i]->m_Name.GetStrPtr());
                            //ImGuiAPI.SameLine(0, -1);
                            //ImGuiAPI.Text(ptr[i].m_Id);
                        }
                        ImGuiAPI.EndTabItem();
                    }
                    ImGuiAPI.EndTabBar();
                }   
            }
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }
    }
}

namespace EngineNS.Editor
{
    public partial class TtMainEditorApplication
    {
        public EngineNS.Windows.TtClrProfiler mClrProfiler = new Windows.TtClrProfiler();
    }
}

