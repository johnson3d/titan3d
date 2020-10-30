using System;
using System.Collections.Generic;
using System.Text;
using ImGuiNET;

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
            {
                return result;
            }

            var mtl = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(CEngine.Instance.RenderContext, name);

            AllMaterials.Add(name, result);

            return result;
        }
        private Dictionary<RName, THandle<Graphics.CGfxMaterialInstance>> AllMaterials = new Dictionary<RName, THandle<Graphics.CGfxMaterialInstance>>();
        public async System.Threading.Tasks.Task<THandle<Graphics.CGfxMaterialInstance>> GetFontMaterial(RName fontname, GuiContext context)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            IntPtr pixels;
            int width, height, bytesPerPixel;
            THandle<Graphics.CGfxMaterialInstance> result;
            if(AllMaterials.TryGetValue(fontname, out result))
            {
                io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height, out bytesPerPixel);
                io.Fonts.ClearTexData();
                return result;
            }
            //从ImGui中获得TextAtlas数据，创建纹理，然后创建材质
            // Build
            io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height, out bytesPerPixel);

            var rc = CEngine.Instance.RenderContext;
            CTexture2DDesc desc = new CTexture2DDesc();
            desc.Init();
            desc.Width = (uint)width;
            desc.Height = (uint)height;
            desc.MipLevels = 1;
            desc.InitData.SysMemPitch = (uint)(bytesPerPixel * width);
            var imageBlob = new Support.CBlobObject();
            imageBlob.PushData(pixels, (uint)(bytesPerPixel * width * height));
            desc.InitData.pSysMem = imageBlob.CoreObject;
            desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            var tex2d = rc.CreateTexture2D(desc);
            CShaderResourceViewDesc srvDesc = new CShaderResourceViewDesc();
            srvDesc.mFormat = desc.Format;
            srvDesc.mTexture2D = tex2d.CoreObject;
            var srv = rc.CreateShaderResourceView(srvDesc);
            srv.ResourceState.StreamState = EStreamingState.SS_Valid;
            io.Fonts.ClearTexData();

            var mtl = await CEngine.Instance.MaterialManager.GetMaterialAsync(rc, RName.GetRName("material/defaultfontmaterial.material"), true);
            var mtlInst = CEngine.Instance.MaterialInstanceManager.NewMaterialInstance(rc, mtl);
            var idxTex = mtlInst.FindSRVIndex("txDiffuse");
            mtlInst.SetSRV(idxTex, srv);
            
            result = new THandle<Graphics.CGfxMaterialInstance>(mtlInst);
            AllMaterials.Add(fontname, result);
            return result;
        }
    }
}
