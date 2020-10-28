using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GuiWrapper
{
    public struct THandle<T> where T : class
    {
        private System.Runtime.InteropServices.GCHandle Handle;
        public THandle(T obj) 
        {
            Handle = System.Runtime.InteropServices.GCHandle.Alloc(obj);
        }
        public THandle(IntPtr p)
        {
            Handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(p);
        }
        public void Free()
        {
            Handle.Free();
        }
        public IntPtr Ptr
        {
            get
            {
                return System.Runtime.InteropServices.GCHandle.ToIntPtr(Handle);
            }
        }
        public T Get()
        {
            return Handle.Target as T;
        }
    }

    public class GuiManager
    {
        public static GuiManager Instance = new GuiManager();
        public void Cleanup()
        {
            foreach(var i in AllMaterials)
            {
                i.Value.Free();
            }
            AllMaterials.Clear();
        }
        public async System.Threading.Tasks.Task<THandle<Graphics.CGfxMaterialInstance>> GetMaterialRHI(RName name)
        {
            THandle<Graphics.CGfxMaterialInstance> result;
            if (AllMaterials.TryGetValue(name, out result))
                return result;

            var rsv = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(CEngine.Instance.RenderContext, name);
            result = new THandle<Graphics.CGfxMaterialInstance>(rsv);
            AllMaterials.Add(name, result);
            return result;
        }
        private Dictionary<RName, THandle<Graphics.CGfxMaterialInstance>> AllMaterials = new Dictionary<RName, THandle<Graphics.CGfxMaterialInstance>>();
        public THandle<Graphics.CGfxMaterialInstance> GetFontMaterial(RName fontname, GuiContext context)
        {
            THandle<Graphics.CGfxMaterialInstance> result;
            if (AllMaterials.TryGetValue(fontname, out result))
                return result;
            //从ImGui中获得TextAtlas数据，创建纹理，然后创建材质

            AllMaterials.Add(fontname, result);
            return result;
        }
    }
}
